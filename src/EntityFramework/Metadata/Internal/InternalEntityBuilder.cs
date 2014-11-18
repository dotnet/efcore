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

            return Key(GetExistingProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(clrProperties, "clrProperties");

            return Key(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalKeyBuilder Key(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            Debug.Assert(properties != null);

            return _keyBuilders.GetOrReplace(
                () => Metadata.TryGetPrimaryKey(properties),
                () => Metadata.TryGetPrimaryKey(),
                () => Metadata.SetPrimaryKey(properties),
                (key, isNew) => new InternalKeyBuilder(key, ModelBuilder),
                configurationSource);
        }

        public virtual bool RemoveKey([NotNull] Key key, ConfigurationSource configurationSource)
        {
            Check.NotNull(key, "key");

            if (!_keyBuilders.Remove(key, configurationSource, canOverrideSameSource: false))
            {
                return false;
            }

            Metadata.RemoveKey(key);

            return true;
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] Type propertyType, [NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotEmpty(name, "name");

            return InternalProperty(propertyType, name, /*shadowProperty:*/ true, configurationSource);
        }

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperty, "clrProperty");

            return InternalProperty(clrProperty.PropertyType, clrProperty.Name, /*shadowProperty:*/ false, configurationSource);
        }

        private InternalPropertyBuilder InternalProperty(Type propertyType, string propertyName, bool shadowProperty, ConfigurationSource configurationSource)
        {
            if (!CanAdd(propertyName, configurationSource))
            {
                return null;
            }

            return _propertyBuilders.GetOrAdd(
                () => Metadata.TryGetProperty(propertyName),
                () => Metadata.AddProperty(propertyName, propertyType, shadowProperty),
                (property, isNew) => new InternalPropertyBuilder(property, ModelBuilder, configurationSource),
                configurationSource);
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

                _ignoredProperties.Value.Remove(propertyName);
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
                if (!_propertyBuilders.Remove(property, configurationSource, canOverrideSameSource: false))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.PropertyAddedExplicitly(propertyName, Metadata.Name));
                    }

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
            }

            _ignoredProperties.Value[propertyName] = configurationSource;

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

            return ForeignKey(principalType.Metadata, GetExistingProperties(propertyNames, configurationSource), configurationSource);
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
            return _foreignKeyBuilders.Value.GetOrAdd(
                () => Metadata.TryGetForeignKey(dependentProperties),
                () => Metadata.AddForeignKey(dependentProperties, principalType.GetPrimaryKey()),
                (foreignKey, isNew) => new InternalForeignKeyBuilder(foreignKey, ModelBuilder),
                configurationSource);
        }

        public virtual bool RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (!_foreignKeyBuilders.Value.Remove(foreignKey, configurationSource, canOverrideSameSource: false))
            {
                return false;
            }

            Metadata.RemoveForeignKey(foreignKey);

            return true;
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Index(GetExistingProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperties, "clrProperties");

            return Index(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalIndexBuilder Index(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            return _indexBuilders.Value.GetOrAdd(
                () => Metadata.TryGetIndex(properties),
                () => Metadata.AddIndex(properties),
                (index, isNew) => new InternalIndexBuilder(index, ModelBuilder),
                configurationSource);
        }

        public virtual bool RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            Check.NotNull(index, "index");

            if (!_indexBuilders.Value.Remove(index, configurationSource, canOverrideSameSource: false))
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
            var foreignKey = navToDependent != null
                ? navToDependent.ForeignKey
                : navToPrincipal != null
                    ? navToPrincipal.ForeignKey
                    : new ForeignKeyConvention()
                        .FindOrCreateForeignKey(principalEntityType, dependentEntityType, navNameToPrincipal, navNameToDependent, oneToOne);

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
                navToDependent = principalEntityType.AddNavigation(navNameToDependent, foreignKey, pointsToPrincipal: false);
            }

            if (navNameToPrincipal != null
                && navToPrincipal == null)
            {
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
                (fk, isNew) => new InternalRelationshipBuilder(
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
            entityType.RemoveForeignKey(relationshipBuilder.Metadata);

            var newForeignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                relationshipBuilder.PrincipalType,
                relationshipBuilder.DependentType,
                relationshipBuilder.NavigationToPrincipal != null ? relationshipBuilder.NavigationToPrincipal.Name : null,
                relationshipBuilder.NavigationToDependent != null ? relationshipBuilder.NavigationToDependent.Name : null,
                dependentProperties,
                principalProperties,
                ((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            // TODO: Remove principal key only if it was added by convention
            // Issue #213
            var currentPrincipalKey = relationshipBuilder.Metadata.ReferencedKey;
            if (currentPrincipalKey != newForeignKey.ReferencedKey
                && currentPrincipalKey != currentPrincipalKey.EntityType.TryGetPrimaryKey()
                && currentPrincipalKey.Properties.All(p => p.IsShadowProperty))
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

            InternalRelationshipBuilder builder;
            var owner = this;
            if (Metadata != newForeignKey.EntityType)
            {
                owner = ModelBuilder.Entity(newForeignKey.EntityType.Name, configurationSource);
            }

            builder = owner._relationshipBuilders.Value.TryGetValue(newForeignKey, configurationSource);
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

        private IReadOnlyList<Property> GetExistingProperties(IEnumerable<string> propertyNames, ConfigurationSource configurationSource)
        {
            var list = new List<Property>();
            foreach (var propertyName in propertyNames)
            {
                var property = Metadata.GetProperty(propertyName);
                InternalProperty(property.PropertyType, property.Name, property.IsShadowProperty, configurationSource);
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

                var property = propertyBuilder == null
                    ? Metadata.GetProperty(propertyInfo)
                    : propertyBuilder.Metadata;

                list.Add(property);
            }
            return list;
        }
    }
}
