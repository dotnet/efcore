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
                ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
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
            if (CanAdd(propertyName, isNavigation: false, configurationSource: configurationSource))
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

            return CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource)
                   && Metadata.TryGetNavigation(navigationName) == null;
        }

        private bool CanAdd(string propertyName, bool isNavigation, ConfigurationSource configurationSource)
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
                    if (isNavigation)
                    {
                        throw new InvalidOperationException(Strings.NavigationIgnoredExplicitly(propertyName, Metadata.Name));
                    }
                    throw new InvalidOperationException(Strings.PropertyIgnoredExplicitly(propertyName, Metadata.Name));
                }
            }

            return true;
        }

        private bool CanRemove(ForeignKey foreignKey, ConfigurationSource configurationSource, bool canOverrideSameSource)
        {
            if (foreignKey.EntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(foreignKey, configurationSource, canOverrideSameSource);
            }

            var currentConfigurationSource = _relationshipBuilders.Value.GetConfigurationSource(foreignKey);
            return configurationSource.Overrides(currentConfigurationSource)
                   && (canOverrideSameSource || configurationSource != currentConfigurationSource);
        }

        public virtual bool Navigation(
            [CanBeNull] string navigationName,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            return Navigation(
                navigationName,
                foreignKey,
                pointsToPrincipal,
                configurationSource,
                canOverrideSameSource: true);
        }

        private bool Navigation(
            [CanBeNull] string navigationName,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var navigation = pointsToPrincipal
                ? foreignKey.GetNavigationToPrincipal()
                : foreignKey.GetNavigationToDependent();

            var fkOwner = foreignKey.EntityType == Metadata
                ? this
                : ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention);

            if (navigationName == navigation?.Name)
            {
                fkOwner._relationshipBuilders.Value.UpdateConfigurationSource(foreignKey, configurationSource);
                return true;
            }

            if (!CanSetNavigation(navigationName, foreignKey, pointsToPrincipal, configurationSource, canOverrideSameSource))
            {
                return false;
            }

            navigation?.EntityType.RemoveNavigation(navigation);

            var conflictingNavigation = navigationName == null
                ? null
                : Metadata.TryGetNavigation(navigationName);

            if (conflictingNavigation != null)
            {
                var removed = RemoveRelationship(conflictingNavigation.ForeignKey, configurationSource);
                Debug.Assert(removed);
            }

            if (navigationName != null)
            {
                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(navigationName);
                }

                fkOwner._relationshipBuilders.Value.UpdateConfigurationSource(foreignKey, configurationSource);
                Metadata.AddNavigation(navigationName, foreignKey, pointsToPrincipal);
                return true;
            }

            return true;
        }

        private bool CanSetNavigation(
            string navigationName,
            ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource)
        {
            var navigation = pointsToPrincipal
                ? foreignKey.GetNavigationToPrincipal()
                : foreignKey.GetNavigationToDependent();

            if (navigationName == navigation?.Name)
            {
                return true;
            }

            if (!CanRemove(foreignKey, configurationSource, canOverrideSameSource))
            {
                return false;
            }

            var conflictingNavigation = navigationName == null
                ? null
                : Metadata.TryGetNavigation(navigationName);

            if (conflictingNavigation != null
                && !CanRemove(conflictingNavigation.ForeignKey, configurationSource, canOverrideSameSource))
            {
                return false;
            }

            if (navigationName != null
                && !CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource))
            {
                return false;
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
                if (!RemoveProperty(property, configurationSource))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.PropertyAddedExplicitly(property.Name, Metadata.Name));
                    }

                    return false;
                }
            }

            var navigation = Metadata.TryGetNavigation(propertyName);
            if (navigation != null)
            {
                if (!Navigation(null, navigation.ForeignKey, navigation.PointsToPrincipal, configurationSource, canOverrideSameSource: false))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.NavigationAddedExplicitly(navigation.Name, Metadata.Name));
                    }

                    return false;
                }

                RemoveForeignKeyIfUnused(navigation.ForeignKey, configurationSource);
            }

            _ignoredProperties.Value[propertyName] = configurationSource;

            return true;
        }

        private void RemovePropertyIfUnused(Property property, ConfigurationSource configurationSource)
        {
            if (Metadata.Indexes.Any(i => i.Properties.Contains(property)))
            {
                return;
            }

            if (Metadata.ForeignKeys.Any(i => i.Properties.Contains(property)))
            {
                return;
            }

            if (Metadata.Keys.Any(i => i.Properties.Contains(property)))
            {
                return;
            }

            // TODO: Only remove if added by convention
            if (!_propertyBuilders.Remove(property, ConfigurationSource.Explicit))
            {
                return;
            }

            if (Metadata.Properties.Contains(property))
            {
                Metadata.RemoveProperty(property);
            }
        }

        private bool RemoveProperty(Property property, ConfigurationSource configurationSource)
        {
            if (!_propertyBuilders.Remove(property, configurationSource, canOverrideSameSource: false))
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
                var removed = RemoveRelationship(foreignKey, configurationSource);

                Debug.Assert(removed);
            }

            foreach (var key in Metadata.Keys.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveKey(key, configurationSource);

                Debug.Assert(removed);
            }

            if (Metadata.Properties.Contains(property))
            {
                Metadata.RemoveProperty(property);
            }

            return true;
        }

        public virtual InternalRelationshipBuilder ForeignKey(
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

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] Type referencedType, [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
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

        private InternalRelationshipBuilder ForeignKey(EntityType principalType, IReadOnlyList<Property> dependentProperties, ConfigurationSource configurationSource)
        {
            return dependentProperties == null
                ? null
                : Relationship(principalType, Metadata, null, null, configurationSource, false, true)
                    .ForeignKey(dependentProperties, configurationSource);
        }

        private bool RemoveRelationships(ConfigurationSource configurationSource, params ForeignKey[] foreignKeys)
        {
            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey != null)
                {
                    var relationshipConfigurationSource = ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                        ._relationshipBuilders.Value.GetConfigurationSource(foreignKey);

                    if (!configurationSource.Overrides(relationshipConfigurationSource))
                    {
                        return false;
                    }
                }
            }

            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey != null)
                {
                    var removed = RemoveRelationship(foreignKey, configurationSource);
                    Debug.Assert(removed);
                }
            }

            return true;
        }

        public virtual bool RemoveRelationship([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.EntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
            }

            if (!_relationshipBuilders.Value.Remove(foreignKey, configurationSource))
            {
                return false;
            }

            var navigationToDependent = foreignKey.GetNavigationToDependent();
            navigationToDependent?.EntityType.RemoveNavigation(navigationToDependent);

            var navigationToPrincipal = foreignKey.GetNavigationToPrincipal();
            navigationToPrincipal?.EntityType.RemoveNavigation(navigationToPrincipal);

            Metadata.RemoveForeignKey(foreignKey);

            RemoveShadowPropertiesIfUnused(foreignKey.Properties, configurationSource);

            return true;
        }

        private void RemoveShadowPropertiesIfUnused(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            foreach (var property in properties.ToList())
            {
                if (property.IsShadowProperty)
                {
                    RemovePropertyIfUnused(property, configurationSource);
                }
            }
        }

        private void RemoveForeignKeyIfUnused(ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.GetNavigationToDependent() == null
                && foreignKey.GetNavigationToPrincipal() == null)
            {
                RemoveRelationship(foreignKey, configurationSource);
            }
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

        public virtual InternalRelationshipBuilder Relationship([NotNull] ForeignKey foreignKey, bool existingForeignKey, ConfigurationSource configurationSource)
        {
            Check.NotNull(foreignKey, "foreignKey");

            if (foreignKey.EntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .Relationship(foreignKey, existingForeignKey, configurationSource);
            }

            return _relationshipBuilders.Value.GetOrAdd(
                () => existingForeignKey ? foreignKey : null,
                () => foreignKey,
                fk => new InternalRelationshipBuilder(foreignKey, ModelBuilder, existingForeignKey ? (ConfigurationSource?)ConfigurationSource.Explicit : null),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] Type principalType,
            [NotNull] Type dependentType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? oneToOne = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var principalEntityTypeBuilder = ModelBuilder.Entity(principalType, configurationSource);
            if (principalEntityTypeBuilder == null)
            {
                return null;
            }

            var dependentEntityTypeBuilder = ModelBuilder.Entity(dependentType, configurationSource);
            if (dependentEntityTypeBuilder == null)
            {
                return null;
            }

            return Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                configurationSource,
                oneToOne,
                strictPrincipal);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? oneToOne = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalEntityType, "principalEntityType");
            Check.NotNull(dependentEntityType, "dependentEntityType");

            var principalEntityTypeBuilder = ModelBuilder.Entity(principalEntityType.Name, configurationSource);
            Debug.Assert(principalEntityTypeBuilder != null);

            var dependentEntityTypeBuilder = ModelBuilder.Entity(dependentEntityType.Name, configurationSource);
            Debug.Assert(dependentEntityTypeBuilder != null);

            return Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                configurationSource,
                oneToOne,
                strictPrincipal);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? oneToOne = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalEntityTypeBuilder, "principalEntityTypeBuilder");
            Check.NotNull(dependentEntityTypeBuilder, "dependentEntityTypeBuilder");

            if (dependentEntityTypeBuilder != this)
            {
                return dependentEntityTypeBuilder.Relationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    configurationSource,
                    oneToOne,
                    strictPrincipal);
            }

            if (!string.IsNullOrEmpty(navigationToPrincipalName)
                && !dependentEntityTypeBuilder.CanAdd(navigationToPrincipalName, isNavigation: true, configurationSource: configurationSource))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(navigationToDependentName)
                && !principalEntityTypeBuilder.CanAdd(navigationToDependentName, isNavigation: true, configurationSource: configurationSource))
            {
                return null;
            }

            var principalEntityType = principalEntityTypeBuilder.Metadata;
            var dependentEntityType = dependentEntityTypeBuilder.Metadata;

            if (principalEntityType.TryGetPrimaryKey() == null)
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(Strings.PrincipalEntityTypeRequiresKey(principalEntityType.Name));
                }

                return null;
            }

            var navigationToPrincipal = string.IsNullOrEmpty(navigationToPrincipalName)
                ? null
                : dependentEntityType.TryGetNavigation(navigationToPrincipalName);

            if (navigationToPrincipal != null
                && navigationToPrincipal.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)true : null, oneToOne))
            {
                return Relationship(navigationToPrincipal, configurationSource, navigationToDependentName);
            }

            var navigationToDependent = string.IsNullOrEmpty(navigationToDependentName)
                ? null
                : principalEntityType.TryGetNavigation(navigationToDependentName);

            if (navigationToDependent != null
                && navigationToDependent.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal? (bool?)false : null, oneToOne))
            {
                return Relationship(navigationToDependent, configurationSource, navigationToPrincipalName);
            }

            if (!RemoveRelationships(configurationSource, navigationToPrincipal?.ForeignKey, navigationToDependent?.ForeignKey))
            {
                return null;
            }

            navigationToPrincipalName = navigationToPrincipalName == "" ? null : navigationToPrincipalName;
            navigationToDependentName = navigationToDependentName == "" ? null : navigationToDependentName;

            var foreignKey = new ForeignKeyConvention()
                .CreateForeignKeyByConvention(
                    principalEntityType,
                    dependentEntityType,
                    navigationToPrincipalName,
                    null,
                    null,
                    oneToOne);

            if (foreignKey == null)
            {
                return null;
            }

            var relationshipBuilder = new InternalRelationshipBuilder(foreignKey, dependentEntityTypeBuilder.ModelBuilder, foreignKeyConfigurationSource: null);
            Debug.Assert(relationshipBuilder.Metadata.EntityType == Metadata);

            _relationshipBuilders.Value.Add(foreignKey, relationshipBuilder, configurationSource);

            if (navigationToPrincipalName != null)
            {
                var navigationToPrincipalSet = dependentEntityTypeBuilder
                    .Navigation(navigationToPrincipalName, foreignKey, pointsToPrincipal: true, configurationSource: configurationSource);
                Debug.Assert(navigationToPrincipalSet);
            }

            if (navigationToDependentName != null)
            {
                var navigationToDependentSet = principalEntityTypeBuilder
                    .Navigation(navigationToDependentName, foreignKey, pointsToPrincipal: false, configurationSource: configurationSource);
                Debug.Assert(navigationToDependentSet);
            }

            return relationshipBuilder;
        }
        
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] Navigation navigation,
            ConfigurationSource configurationSource,
            [CanBeNull] string inverseNavigationName = null)
        {
            var relationship = Relationship(navigation.ForeignKey, existingForeignKey: true, configurationSource: configurationSource);
            
            if (inverseNavigationName != null)
            {
                inverseNavigationName = inverseNavigationName == "" ? null : inverseNavigationName;

                if (navigation.PointsToPrincipal)
                {
                    relationship = relationship.NavigationToDependent(inverseNavigationName, configurationSource);
                }
                else
                {
                    relationship = relationship.NavigationToPrincipal(inverseNavigationName, configurationSource);
                }
            }

            return relationship;
        }

        public virtual IReadOnlyList<Property> GetOrCreateProperties([NotNull] IEnumerable<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, "propertyNames");

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

        public virtual IReadOnlyList<Property> GetOrCreateProperties([NotNull] IEnumerable<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperties, "clrProperties");

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
