// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityBuilder : InternalMetadataItemBuilder<EntityType>
    {
        private readonly LazyRef<MetadataDictionary<ForeignKey, InternalForeignKeyBuilder>> _foreignKeyBuilders =
            new LazyRef<MetadataDictionary<ForeignKey, InternalForeignKeyBuilder>>(() => new MetadataDictionary<ForeignKey, InternalForeignKeyBuilder>());

        private readonly LazyRef<MetadataDictionary<Index, InternalIndexBuilder>> _indexBuilders =
            new LazyRef<MetadataDictionary<Index, InternalIndexBuilder>>(() => new MetadataDictionary<Index, InternalIndexBuilder>());

        private readonly MetadataDictionary<Key, InternalKeyBuilder> _keyBuilders = new MetadataDictionary<Key, InternalKeyBuilder>();
        private readonly MetadataDictionary<Property, InternalPropertyBuilder> _propertyBuilders = new MetadataDictionary<Property, InternalPropertyBuilder>();

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _ignoredProperties =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());

        private readonly LazyRef<MetadataDictionary<ForeignKey, InternalRelationshipBuilder>> _relationshipBuilders =
            new LazyRef<MetadataDictionary<ForeignKey, InternalRelationshipBuilder>>(() => new MetadataDictionary<ForeignKey, InternalRelationshipBuilder>());

        public InternalEntityBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(propertyNames, "propertyNames");

            return Key(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(clrProperties, "clrProperties");

            return Key(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalKeyBuilder Key(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }
            
            var keyToReplace = Metadata.TryGetPrimaryKey();
            var existingKey = Metadata.TryGetKey(properties);

            if (keyToReplace != null
                && keyToReplace != existingKey
                && !RemoveKey(keyToReplace, configurationSource))
            {
                return null;
            }

            if (existingKey != null)
            {
                Metadata.SetPrimaryKey(properties);
            }

            return _keyBuilders.GetOrAdd(
                    () => existingKey,
                    () => Metadata.SetPrimaryKey(properties),
                    key => new InternalKeyBuilder(key, ModelBuilder),
                    null,
                    configurationSource);
        }

        public virtual bool RemoveKey([NotNull] Key key, ConfigurationSource configurationSource)
        {
            Check.NotNull(key, "key");

            if (!_keyBuilders.Remove(key, configurationSource))
            {
                return false;
            }

            foreach (var foreignKey in ModelBuilder.Metadata.GetReferencingForeignKeys(key))
            {
                ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention).RemoveForeignKey(foreignKey, configurationSource);
            }

            Metadata.RemoveKey(key);

            return true;
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] Type propertyType, [NotNull] string propertyName, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotEmpty(propertyName, "propertyName");

            return InternalProperty(propertyType, propertyName, /*shadowProperty:*/ true, configurationSource);
        }

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperty, "clrProperty");

            return InternalProperty(clrProperty.PropertyType, clrProperty.Name, /*shadowProperty:*/ false, configurationSource);
        }

        private InternalPropertyBuilder InternalProperty(Type propertyType, string propertyName, bool shadowProperty, ConfigurationSource configurationSource)
        {
            if (CanAdd(propertyName, configurationSource))
            {
                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                }

                return _propertyBuilders.GetOrAdd(
                    () => Metadata.TryGetProperty(propertyName),
                    () => Metadata.AddProperty(propertyName, propertyType, shadowProperty),
                    property => new InternalPropertyBuilder(property, ModelBuilder, configurationSource),
                    configurationSource);
            }

            return null;
        }

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(navigationName, "navigationName");

            return CanAdd(navigationName, configurationSource)
                   && Metadata.TryGetNavigation(navigationName) == null;
        }

        private bool CanAdd(string propertyName, ConfigurationSource configurationSource)
        {
            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredProperties.HasValue
                && _ignoredProperties.Value.TryGetValue(propertyName, out ignoredConfigurationSource))
            {
                if (!configurationSource.Overrides(ignoredConfigurationSource))
                {
                    return false;
                }

                if (ignoredConfigurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(Strings.PropertyIgnoredExplicitly(propertyName, Metadata.Name));
                }
            }

            return true;
        }

        public virtual bool Ignore([NotNull] string propertyName, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(propertyName, "propertyName");

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredProperties.Value.TryGetValue(propertyName, out ignoredConfigurationSource))
            {
                if (!configurationSource.Overrides(ignoredConfigurationSource)
                    || configurationSource == ignoredConfigurationSource)
                {
                    return true;
                }
            }

            var property = Metadata.TryGetProperty(propertyName);
            if (property != null)
            {
                if (!Remove(property, configurationSource, canOverrideSameSource: false))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.PropertyAddedExplicitly(property.Name, Metadata.Name));
                    }

                    return false;
                }
            }

            _ignoredProperties.Value[propertyName] = configurationSource;

            return true;
        }

        private bool Remove(Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            if (!_propertyBuilders.Remove(property, configurationSource, canOverrideSameSource))
            {
                return false;
            }

            foreach (var index in Metadata.Indexes.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveIndex(index, configurationSource);

                Debug.Assert(removed);
            }

            foreach (var foreignKey in Metadata.ForeignKeys.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveForeignKey(foreignKey, configurationSource);

                Debug.Assert(removed);
            }

            foreach (var key in Metadata.Keys.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveKey(key, configurationSource);

                Debug.Assert(removed);
            }

            Metadata.RemoveProperty(property);

            return true;
        }

        public virtual InternalForeignKeyBuilder ForeignKey(
            [NotNull] string referencedEntityTypeName, [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(referencedEntityTypeName, "referencedEntityTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            var principalType = ModelBuilder.Entity(referencedEntityTypeName, configurationSource);
            if (principalType == null)
            {
                return null;
            }

            return ForeignKey(principalType.Metadata, GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalForeignKeyBuilder ForeignKey([NotNull] Type referencedType, [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(referencedType, "referencedType");
            Check.NotNull(clrProperties, "clrProperties");

            var principalType = ModelBuilder.Entity(referencedType, configurationSource);
            if (principalType == null)
            {
                return null;
            }

            return ForeignKey(principalType.Metadata, GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalForeignKeyBuilder ForeignKey(EntityType principalType, IReadOnlyList<Property> dependentProperties, ConfigurationSource configurationSource)
        {
            return dependentProperties == null
                ? null
                : _foreignKeyBuilders.Value.GetOrAdd(
                    () => Metadata.TryGetForeignKey(dependentProperties),
                    () => Metadata.AddForeignKey(dependentProperties, principalType.GetPrimaryKey()),
                    foreignKey => new InternalForeignKeyBuilder(foreignKey, ModelBuilder),
                    configurationSource);
        }

        public virtual bool RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (!_relationshipBuilders.Value.Remove(foreignKey, configurationSource)
                && !_foreignKeyBuilders.Value.Remove(foreignKey, configurationSource))
            {
                return false;
            }

            var navToDependent = (Navigation)foreignKey.GetNavigationToDependent();
            navToDependent?.EntityType.RemoveNavigation(navToDependent);

            var navToPrincipal = (Navigation)foreignKey.GetNavigationToPrincipal();
            navToPrincipal?.EntityType.RemoveNavigation(navToPrincipal);

            Metadata.RemoveForeignKey(foreignKey);

            // TODO: Remove this once conventions don't create shadow keys
            // Issue #1134
            if (foreignKey.ReferencedProperties.All(p => p.IsShadowProperty))
            {
                if (ModelBuilder.Metadata.EntityTypes.SelectMany(et => et.ForeignKeys).All(fk => fk.ReferencedKey != foreignKey.ReferencedKey))
                {
                    foreignKey.ReferencedEntityType.RemoveKey(foreignKey.ReferencedKey);
                    foreach (var property in foreignKey.ReferencedProperties)
                    {
                        foreignKey.ReferencedEntityType.RemoveProperty(property);
                    }
                }
            }

            foreach (var property in foreignKey.Properties)
            {
                if (property.IsShadowProperty)
                {
                    // TODO: Remove property only if it was added by convention
                    // Issue #1215
                    //Remove(property, configurationSource);
                    Metadata.RemoveProperty(property);
                }
            }

            return true;
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Index(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperties, "clrProperties");

            return Index(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalIndexBuilder Index(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            return properties == null
                ? null
                : _indexBuilders.Value.GetOrAdd(
                    () => Metadata.TryGetIndex(properties),
                    () => Metadata.AddIndex(properties),
                    index => new InternalIndexBuilder(index, ModelBuilder),
                    configurationSource);
        }

        public virtual bool RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            Check.NotNull(index, "index");

            if (!_indexBuilders.Value.Remove(index, configurationSource))
            {
                return false;
            }

            Metadata.RemoveIndex(index);

            return true;
        }

        public virtual InternalRelationshipBuilder BuildRelationship(
            [NotNull] Type principalType, [NotNull] Type dependentType,
            [CanBeNull] string navNameToPrincipal, [CanBeNull] string navNameToDependent, bool oneToOne,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var dependentEntityType = ModelBuilder.Entity(dependentType, configurationSource);
            if (dependentEntityType == null)
            {
                return null;
            }
            var principalEntityType = ModelBuilder.Entity(principalType, configurationSource);
            if (principalEntityType == null)
            {
                return null;
            }

            return BuildRelationship(principalEntityType.Metadata, dependentEntityType.Metadata, navNameToPrincipal, navNameToDependent, oneToOne, configurationSource);
        }

        public virtual InternalRelationshipBuilder BuildRelationship(
            [NotNull] EntityType principalEntityType, [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navNameToPrincipal, [CanBeNull] string navNameToDependent, bool oneToOne,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalEntityType, "principalEntityType");
            Check.NotNull(dependentEntityType, "dependentEntityType");

            var navToDependent = navNameToDependent == null ? null : principalEntityType.TryGetNavigation(navNameToDependent);
            var navToPrincipal = navNameToPrincipal == null ? null : dependentEntityType.TryGetNavigation(navNameToPrincipal);

            // Find the associated FK on an already existing navigation, or create one by convention
            // TODO: If FK isn't already specified, then creating the navigation should cause it to be found/created
            // by convention, but this part of conventions is not done yet, so we do it here instead--kind of h.acky
            // Issue #213
            var originalForeignKeys = dependentEntityType.ForeignKeys.ToList();
            var foreignKey = navToDependent?.ForeignKey ??
                             navToPrincipal?.ForeignKey;

            if (foreignKey != null
                && (foreignKey.ReferencedEntityType != principalEntityType
                    || foreignKey.IsUnique != oneToOne
                    || foreignKey.EntityType.Navigations.Any(n => n.ForeignKey == foreignKey && n.PointsToPrincipal && n.Name != navNameToPrincipal)
                    || foreignKey.ReferencedEntityType.Navigations.Any(n => n.ForeignKey == foreignKey && !n.PointsToPrincipal && n.Name != navNameToDependent)
                    || foreignKey.ReferencedKey != foreignKey.ReferencedEntityType.TryGetPrimaryKey()))
            {
                if (ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .RemoveForeignKey(foreignKey, configurationSource))
                {
                    if (navNameToPrincipal != null)
                    {
                        var otherFk = dependentEntityType.TryGetNavigation(navNameToPrincipal)?.ForeignKey;
                        if (otherFk != null
                            && !ModelBuilder.Entity(otherFk.EntityType.Name, ConfigurationSource.Convention)
                                .RemoveForeignKey(otherFk, configurationSource))
                        {
                            return null;
                        }
                    }

                    navToDependent = null;
                    navToPrincipal = null;
                    foreignKey = null;
                }
                else
                {
                    return null;
                }
            }

            if (foreignKey == null)
            {
                foreignKey = new ForeignKeyConvention()
                    .FindOrCreateForeignKey(principalEntityType, dependentEntityType, navNameToPrincipal, navNameToDependent, oneToOne);
            }

            Debug.Assert(principalEntityType == foreignKey.ReferencedEntityType);
            Debug.Assert(dependentEntityType == foreignKey.EntityType);
            Debug.Assert(oneToOne == foreignKey.IsUnique);

            var newForeignKey = (ForeignKey)null;
            if (originalForeignKeys.Count != dependentEntityType.ForeignKeys.Count)
            {
                foreach (var fk in dependentEntityType.ForeignKeys)
                {
                    var index = originalForeignKeys.IndexOf(fk);
                    if (index < 0)
                    {
                        newForeignKey = fk;
                        break;
                    }
                    originalForeignKeys.RemoveAt(index);
                }
            }

            if (navNameToDependent != null
                && navToDependent == null)
            {
                if (!CanAddNavigation(navNameToDependent, configurationSource))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        // Nav prop in use
                        throw new InvalidOperationException();
                    }
                    return null;
                }

                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(navNameToDependent);
                }

                navToDependent = principalEntityType.AddNavigation(navNameToDependent, foreignKey, pointsToPrincipal: false);
            }

            if (navNameToPrincipal != null
                && navToPrincipal == null)
            {
                if (!CanAddNavigation(navNameToPrincipal, configurationSource))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        // Nav prop in use
                        throw new InvalidOperationException();
                    }
                    return null;
                }

                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(navNameToPrincipal);
                }

                navToPrincipal = dependentEntityType.AddNavigation(navNameToPrincipal, foreignKey, pointsToPrincipal: true);
            }

            var owner = this;
            if (Metadata != foreignKey.EntityType)
            {
                owner = ModelBuilder.Entity(foreignKey.EntityType.Name, configurationSource);
            }

            return owner._relationshipBuilders.Value.GetOrAdd(
                () => newForeignKey == null ? foreignKey : null,
                () => newForeignKey,
                fk => new InternalRelationshipBuilder(
                    fk, ModelBuilder, principalEntityType, dependentEntityType, navToPrincipal, navToDependent),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] IReadOnlyList<Property> principalProperties,
            ConfigurationSource configurationSource)
        {
            if (!_relationshipBuilders.Value.Remove(relationshipBuilder.Metadata, configurationSource))
            {
                return null;
            }

            // TODO: avoid removing and readding the navigation property
            if (relationshipBuilder.NavigationToPrincipal != null)
            {
                relationshipBuilder.DependentType.RemoveNavigation(relationshipBuilder.NavigationToPrincipal);
            }

            if (relationshipBuilder.NavigationToDependent != null)
            {
                relationshipBuilder.PrincipalType.RemoveNavigation(relationshipBuilder.NavigationToDependent);
            }

            var entityType = relationshipBuilder.Metadata.EntityType;

            // TODO: Remove FK only if it was added by convention
            // Issue #213
            var fk = relationshipBuilder.Metadata;
            if (!relationshipBuilder.ModelBuilder.Entity(fk.EntityType.Name, ConfigurationSource.Convention).RemoveForeignKey(fk, configurationSource))
            {
                return null;
            }

            var newForeignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                relationshipBuilder.PrincipalType,
                relationshipBuilder.DependentType,
                relationshipBuilder.NavigationToPrincipal?.Name,
                relationshipBuilder.NavigationToDependent?.Name,
                dependentProperties,
                principalProperties,
                ((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            // TODO: Remove principal key only if it was added by convention
            // Issue #213
            var currentPrincipalKey = relationshipBuilder.Metadata.ReferencedKey;
            if (currentPrincipalKey != newForeignKey.ReferencedKey
                && currentPrincipalKey != currentPrincipalKey.EntityType.TryGetPrimaryKey()
                && currentPrincipalKey.Properties.All(p => p.IsShadowProperty)
                && ModelBuilder.Metadata.EntityTypes.SelectMany(e => e.ForeignKeys).All(k => k.ReferencedKey != currentPrincipalKey))
            {
                currentPrincipalKey.EntityType.RemoveKey(currentPrincipalKey);
            }

            var propertiesInUse = entityType.Keys.SelectMany(k => k.Properties)
                .Concat(entityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(relationshipBuilder.Metadata.ReferencedEntityType.Keys.SelectMany(k => k.Properties))
                .Concat(relationshipBuilder.Metadata.ReferencedEntityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(dependentProperties)
                .Concat(principalProperties)
                .Where(p => p.IsShadowProperty)
                .Distinct();

            var propertiesToRemove = Metadata.Properties
                .Concat(relationshipBuilder.Metadata.ReferencedKey.Properties)
                .Where(p => p.IsShadowProperty)
                .Distinct()
                .Except(propertiesInUse)
                .ToList();

            // TODO: Remove property only if it was added by convention
            // Issue #213
            foreach (var property in propertiesToRemove)
            {
                property.EntityType.RemoveProperty(property);
            }

            var navigationToPrincipal = relationshipBuilder.NavigationToPrincipal;
            if (navigationToPrincipal != null)
            {
                navigationToPrincipal = relationshipBuilder.DependentType.AddNavigation(
                    navigationToPrincipal.Name, newForeignKey, navigationToPrincipal.PointsToPrincipal);
            }

            var navigationToDependent = relationshipBuilder.NavigationToDependent;
            if (navigationToDependent != null)
            {
                navigationToDependent = relationshipBuilder.PrincipalType.AddNavigation(
                    navigationToDependent.Name, newForeignKey, navigationToDependent.PointsToPrincipal);
            }

            var owner = this;
            if (Metadata != newForeignKey.EntityType)
            {
                owner = ModelBuilder.Entity(newForeignKey.EntityType.Name, configurationSource);
            }

            var builder = owner._relationshipBuilders.Value.TryGetValue(newForeignKey, configurationSource);
            if (builder != null)
            {
                if (builder.PrincipalType == relationshipBuilder.PrincipalType
                    && builder.DependentType == relationshipBuilder.DependentType
                    && builder.NavigationToPrincipal == navigationToPrincipal
                    && builder.NavigationToDependent == navigationToDependent)
                {
                    return builder;
                }

                if (!owner._relationshipBuilders.Value.Remove(newForeignKey, configurationSource))
                {
                    return null;
                }
            }

            builder = new InternalRelationshipBuilder(
                relationshipBuilder,
                newForeignKey,
                navigationToPrincipal,
                navigationToDependent);
            owner._relationshipBuilders.Value.Add(newForeignKey, builder, configurationSource);
            return builder;
        }

        private IReadOnlyList<Property> GetOrCreateProperties(IEnumerable<string> propertyNames, ConfigurationSource configurationSource)
        {
            var list = new List<Property>();
            foreach (var propertyName in propertyNames)
            {
                var property = Metadata.TryGetProperty(propertyName);
                if (property == null)
                {
                    if (Metadata.Type == null)
                    {
                        throw new ModelItemNotFoundException(Strings.PropertyNotFound(propertyName, Metadata.Name));
                    }

                    var clrProperty = Metadata.Type.GetPropertiesInHierarchy(propertyName).FirstOrDefault();
                    if (clrProperty == null)
                    {
                        throw new InvalidOperationException(Strings.NoClrProperty(propertyName, Metadata.Name));
                    }

                    var propertyBuilder = Property(clrProperty, configurationSource);
                    if (propertyBuilder == null)
                    {
                        return null;
                    }
                    property = propertyBuilder.Metadata;
                }
                else
                {
                    InternalProperty(property.PropertyType, property.Name, property.IsShadowProperty, configurationSource);
                }
                list.Add(property);
            }
            return list;
        }

        private IReadOnlyList<Property> GetOrCreateProperties(IEnumerable<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            var list = new List<Property>();
            foreach (var propertyInfo in clrProperties)
            {
                var propertyBuilder = Property(propertyInfo, configurationSource);
                if (propertyBuilder == null)
                {
                    return null;
                }

                list.Add(propertyBuilder.Metadata);
            }
            return list;
        }
    }
}
