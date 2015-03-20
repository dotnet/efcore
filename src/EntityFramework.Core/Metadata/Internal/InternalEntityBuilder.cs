// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
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

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            return PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            return PrimaryKey(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalKeyBuilder PrimaryKey(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var oldPrimaryKey = Metadata.TryGetPrimaryKey();
            var newPrimaryKey = Metadata.TryGetKey(properties);
            if (oldPrimaryKey != null
                && oldPrimaryKey != newPrimaryKey)
            {
                if (!configurationSource.Overrides(_keyBuilders.GetConfigurationSource(oldPrimaryKey)))
                {
                    return null;
                }

                if (newPrimaryKey == null)
                {
                    newPrimaryKey = Key(properties, ConfigurationSource.Convention).Metadata;
                }

                UpdateReferencingForeignKeys(oldPrimaryKey, newPrimaryKey, configurationSource);
            }

            if (newPrimaryKey != null)
            {
                Metadata.SetPrimaryKey(properties);
            }

            var keyBuilder = _keyBuilders.GetOrAdd(
                () => newPrimaryKey,
                () => Metadata.SetPrimaryKey(properties),
                key => new InternalKeyBuilder(key, ModelBuilder),
                ModelBuilder.ConventionDispatcher.OnKeyAdded,
                configurationSource);

            ReplaceConventionShadowKeys(keyBuilder.Metadata);

            return keyBuilder;
        }

        private void UpdateReferencingForeignKeys(Key keyToReplace, Key newKey, ConfigurationSource configurationSource)
        {
            var newProperties = newKey.Properties;

            var allForeignKeysReplaced = true;
            foreach (var referencingForeignKey in ModelBuilder.Metadata.GetReferencingForeignKeys(keyToReplace))
            {
                allForeignKeysReplaced &= Relationship(
                    referencingForeignKey,
                    existingForeignKey: true,
                    configurationSource: ConfigurationSource.Convention)
                    .UpdateReferencedKey(newProperties, configurationSource) != null;
            }

            if (allForeignKeysReplaced)
            {
                RemoveKey(keyToReplace, ConfigurationSource.Convention);
            }
        }

        private void ReplaceConventionShadowKeys(Key newKey)
        {
            foreach (var key in Metadata.Keys.ToList())
            {
                if (key != newKey
                    && _keyBuilders.GetConfigurationSource(key) == ConfigurationSource.Convention
                    && key.Properties.All(p => p.IsShadowProperty))
                {
                    UpdateReferencingForeignKeys(key, newKey, ConfigurationSource.Convention);
                }
            }
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            return Key(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            return Key(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        public virtual InternalKeyBuilder Key([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null
                || !properties.Any())
            {
                return null;
            }

            foreach (var property in properties)
            {
                _propertyBuilders.UpdateConfigurationSource(property, configurationSource);
            }

            return _keyBuilders.GetOrAdd(
                () => Metadata.TryGetKey(properties),
                () => Metadata.AddKey(properties),
                key => new InternalKeyBuilder(key, ModelBuilder),
                ModelBuilder.ConventionDispatcher.OnKeyAdded,
                configurationSource);
        }

        public virtual ConfigurationSource? RemoveKey([NotNull] Key key, ConfigurationSource configurationSource)
        {
            Check.NotNull(key, nameof(key));

            var removedConfigurationSource = _keyBuilders.Remove(key, configurationSource);
            if (!removedConfigurationSource.HasValue)
            {
                return null;
            }

            foreach (var foreignKey in ModelBuilder.Metadata.GetReferencingForeignKeys(key))
            {
                var removed = ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            Metadata.RemoveKey(key);

            RemoveShadowPropertiesIfUnused(key.Properties);

            return removedConfigurationSource;
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] Type propertyType, [NotNull] string propertyName, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyType, nameof(propertyType));
            Check.NotEmpty(propertyName, nameof(propertyName));

            return InternalProperty(propertyType, propertyName, /*shadowProperty:*/ true, configurationSource);
        }

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperty, nameof(clrProperty));

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
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource);
            }

            return null;
        }

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(navigationName, nameof(navigationName));

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

                _ignoredProperties.Value.Remove(propertyName);
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
            Check.NotNull(foreignKey, nameof(foreignKey));

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

            if (!CanSetNavigation(navigationName, foreignKey, configurationSource, canOverrideSameSource))
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
                Debug.Assert(removed.HasValue);
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
            ConfigurationSource configurationSource,
            bool canOverrideSameSource)
        {
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
            Check.NotEmpty(propertyName, nameof(propertyName));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredProperties.Value.TryGetValue(propertyName, out ignoredConfigurationSource))
            {
                if (ignoredConfigurationSource.Overrides(configurationSource))
                {
                    return true;
                }
            }

            _ignoredProperties.Value[propertyName] = configurationSource;

            var property = Metadata.TryGetProperty(propertyName);
            if (property != null)
            {
                if (!RemoveProperty(property, configurationSource))
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(Strings.PropertyAddedExplicitly(property.Name, Metadata.Name));
                    }

                    _ignoredProperties.Value.Remove(propertyName);
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

                    _ignoredProperties.Value.Remove(propertyName);
                    return false;
                }

                RemoveForeignKeyIfUnused(navigation.ForeignKey, configurationSource);
                ModelBuilder.RemoveEntityTypesUnreachableByNavigations(configurationSource);
            }

            return true;
        }

        private bool RemoveProperty(Property property, ConfigurationSource configurationSource)
        {
            if (!_propertyBuilders.Remove(property, configurationSource, canOverrideSameSource: false).HasValue)
            {
                return false;
            }

            foreach (var index in Metadata.Indexes.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveIndex(index, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            var detachedRelationships = new List<RelationshipSnapshot>();
            foreach (var foreignKey in Metadata.ForeignKeys.Where(i => i.Properties.Contains(property)).ToList())
            {
                detachedRelationships.Add(DetachRelationship(foreignKey, configurationSource));
            }

            foreach (var key in Metadata.Keys.Where(i => i.Properties.Contains(property)).ToList())
            {
                foreach (var foreignKey in ModelBuilder.Metadata.GetReferencingForeignKeys(key))
                {
                    detachedRelationships.Add(DetachRelationship(foreignKey, configurationSource));
                }
                var removed = RemoveKey(key, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            if (Metadata.Properties.Contains(property))
            {
                Metadata.RemoveProperty(property);
            }

            foreach (var removedRelationship in detachedRelationships)
            {
                removedRelationship.Attach();
            }

            return true;
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] string referencedEntityTypeName, [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(referencedEntityTypeName, nameof(referencedEntityTypeName));
            Check.NotNull(propertyNames, nameof(propertyNames));

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
            Check.NotNull(referencedType, nameof(referencedType));
            Check.NotNull(clrProperties, nameof(clrProperties));

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
                : Relationship(principalType, Metadata, null, null, configurationSource, false)
                    ?.ForeignKey(dependentProperties, configurationSource);
        }

        private RelationshipSnapshot DetachRelationship([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            var navigationToPrincipalName = foreignKey.GetNavigationToPrincipal()?.Name;
            var navigationToDependentName = foreignKey.GetNavigationToDependent()?.Name;
            var relationship = Relationship(foreignKey, true, ConfigurationSource.Convention);
            var relationshipConfigurationSource = RemoveRelationship(foreignKey, configurationSource);

            if (relationshipConfigurationSource == null)
            {
                return null;
            }

            return new RelationshipSnapshot(relationship, navigationToPrincipalName, navigationToDependentName, relationshipConfigurationSource.Value);
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
                    Debug.Assert(removed.HasValue);
                }
            }

            return true;
        }

        public virtual ConfigurationSource? RemoveRelationship([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.EntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
            }

            var removedConfigurationSource = _relationshipBuilders.Value.Remove(foreignKey, configurationSource);
            if (removedConfigurationSource == null)
            {
                return null;
            }

            var principalEntityBuilder = ModelBuilder.Entity(foreignKey.ReferencedEntityType.Name, ConfigurationSource.Convention);

            var navigationToDependent = foreignKey.GetNavigationToDependent();
            navigationToDependent?.EntityType.RemoveNavigation(navigationToDependent);

            var navigationToPrincipal = foreignKey.GetNavigationToPrincipal();
            navigationToPrincipal?.EntityType.RemoveNavigation(navigationToPrincipal);

            Metadata.RemoveForeignKey(foreignKey);
            ModelBuilder.ConventionDispatcher.OnForeignKeyRemoved(this, foreignKey);
            RemoveShadowPropertiesIfUnused(foreignKey.Properties);
            principalEntityBuilder.RemoveKeyIfUnused(foreignKey.ReferencedKey);

            return removedConfigurationSource;
        }

        private void RemoveKeyIfUnused(Key key)
        {
            if (Metadata.TryGetPrimaryKey() == key)
            {
                return;
            }

            if (ModelBuilder.Metadata.GetReferencingForeignKeys(key).Count > 0)
            {
                return;
            }

            RemoveKey(key, ConfigurationSource.Convention);
        }

        private void RemoveForeignKeyIfUnused(ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.GetNavigationToDependent() == null
                && foreignKey.GetNavigationToPrincipal() == null)
            {
                RemoveRelationship(foreignKey, configurationSource);
            }
        }

        private void RemoveShadowPropertiesIfUnused(IReadOnlyList<Property> properties)
        {
            foreach (var property in properties.ToList())
            {
                if (property.IsShadowProperty)
                {
                    RemovePropertyIfUnused(property);
                }
            }
        }

        private void RemovePropertyIfUnused(Property property)
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

            if (!_propertyBuilders.Remove(property, ConfigurationSource.Convention).HasValue)
            {
                return;
            }

            if (Metadata.Properties.Contains(property))
            {
                Metadata.RemoveProperty(property);
            }
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, nameof(propertyNames));

            return Index(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            Check.NotNull(clrProperties, nameof(clrProperties));

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

        public virtual ConfigurationSource? RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            Check.NotNull(index, nameof(index));

            var removedConfigurationSource = _indexBuilders.Value.Remove(index, configurationSource);
            if (!removedConfigurationSource.HasValue)
            {
                return null;
            }

            Metadata.RemoveIndex(index);

            RemoveShadowPropertiesIfUnused(index.Properties);

            return removedConfigurationSource;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] ForeignKey foreignKey, bool existingForeignKey, ConfigurationSource configurationSource)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            if (foreignKey.EntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.EntityType.Name, ConfigurationSource.Convention)
                    .Relationship(foreignKey, existingForeignKey, configurationSource);
            }

            return _relationshipBuilders.Value.GetOrAdd(
                () => existingForeignKey ? foreignKey : null,
                () => foreignKey,
                fk => new InternalRelationshipBuilder(
                    foreignKey, ModelBuilder, existingForeignKey ? (ConfigurationSource?)ConfigurationSource.Explicit : null),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] Type principalType,
            [NotNull] Type dependentType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

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
                isUnique,
                strictPrincipal);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

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
                isUnique,
                strictPrincipal);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool strictPrincipal = true)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));

            if (dependentEntityTypeBuilder != this)
            {
                return dependentEntityTypeBuilder.Relationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    configurationSource,
                    isUnique,
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

            var navigationToPrincipal = string.IsNullOrEmpty(navigationToPrincipalName)
                ? null
                : dependentEntityType.TryGetNavigation(navigationToPrincipalName);

            if (navigationToPrincipal != null
                && navigationToPrincipal.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)true : null, isUnique))
            {
                return Relationship(navigationToPrincipal, configurationSource, navigationToDependentName);
            }

            var navigationToDependent = string.IsNullOrEmpty(navigationToDependentName)
                ? null
                : principalEntityType.TryGetNavigation(navigationToDependentName);

            if (navigationToDependent != null
                && navigationToDependent.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)false : null, isUnique))
            {
                return Relationship(navigationToDependent, configurationSource, navigationToPrincipalName);
            }

            if (!RemoveRelationships(configurationSource, navigationToPrincipal?.ForeignKey, navigationToDependent?.ForeignKey))
            {
                return null;
            }

            navigationToPrincipalName = navigationToPrincipalName == "" ? null : navigationToPrincipalName;
            navigationToDependentName = navigationToDependentName == "" ? null : navigationToDependentName;

            return Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                null,
                null,
                configurationSource,
                isUnique);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool? isRequired = null,
            [CanBeNull] Func<InternalRelationshipBuilder, InternalRelationshipBuilder> onRelationshipAdding = null)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var dependentType = dependentEntityTypeBuilder.Metadata;

            if (foreignKeyProperties != null
                && foreignKeyProperties.Count == 0)
            {
                foreignKeyProperties = null;
            }

            if (referencedProperties != null
                && referencedProperties.Count == 0)
            {
                referencedProperties = null;
            }

            var foreignKey = foreignKeyProperties == null
                ? null
                : dependentType.TryGetForeignKey(
                    principalType,
                    null,
                    null,
                    foreignKeyProperties,
                    referencedProperties,
                    isUnique);

            var existingForeignKey = foreignKey != null;
            if (!existingForeignKey)
            {
                if (foreignKeyProperties != null)
                {
                    var conflictingForeignKey = dependentType.TryGetForeignKey(foreignKeyProperties);
                    if (conflictingForeignKey != null
                        && !dependentEntityTypeBuilder.RemoveRelationship(conflictingForeignKey, configurationSource).HasValue)
                    {
                        return null;
                    }
                }

                foreignKey = CreateForeignKey(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    foreignKeyProperties,
                    referencedProperties,
                    isUnique,
                    isRequired,
                    configurationSource);

                if (foreignKey == null)
                {
                    return null;
                }
            }

            if (isRequired.HasValue)
            {
                var properties = foreignKey.Properties;
                if (!isRequired.Value)
                {
                    var nullableTypeProperties = properties.Where(p => p.PropertyType.IsNullableType()).ToList();
                    if (nullableTypeProperties.Any())
                    {
                        properties = nullableTypeProperties;
                    }
                }

                foreach (var property in properties)
                {
                    if (!dependentEntityTypeBuilder.Property(property.PropertyType, property.Name, ConfigurationSource.Convention)
                        .CanSetRequired(isRequired.Value, configurationSource))
                    {
                        if (!existingForeignKey)
                        {
                            dependentType.RemoveForeignKey(foreignKey);
                        }

                        // TODO: throw for explicit
                        return null;
                    }
                }

                foreach (var property in properties)
                {
                    // TODO: Depending on resolution of #723 this may change
                    var requiredSet = dependentEntityTypeBuilder.Property(property.PropertyType, property.Name, ConfigurationSource.Convention)
                        .Required(isRequired.Value, configurationSource);
                    Debug.Assert(requiredSet);
                }

                foreignKey.IsRequired = isRequired.Value;
            }

            var builder = Relationship(foreignKey, existingForeignKey, configurationSource);
            Debug.Assert(builder != null);

            var navigationToPrincipalSet = dependentEntityTypeBuilder
                .Navigation(navigationToPrincipalName, foreignKey, pointsToPrincipal: true, configurationSource: configurationSource);
            Debug.Assert(navigationToPrincipalSet);

            var navigationToDependentSet = principalEntityTypeBuilder
                .Navigation(navigationToDependentName, foreignKey, pointsToPrincipal: false, configurationSource: configurationSource);
            Debug.Assert(navigationToDependentSet);

            if (onRelationshipAdding != null)
            {
                builder = onRelationshipAdding(builder);
            }
            else
            {
                if (isUnique.HasValue)
                {
                    builder = builder.Unique(isUnique.Value, configurationSource);
                }
                if (isRequired.HasValue)
                {
                    builder = builder.Required(isRequired.Value, configurationSource);
                }
                if (foreignKeyProperties != null)
                {
                    builder = builder.ForeignKey(foreignKeyProperties, configurationSource);
                }
                if (referencedProperties != null)
                {
                    builder = builder.ReferencedKey(referencedProperties, configurationSource);
                }
            }

            if (!existingForeignKey)
            {
                builder = ModelBuilder.ConventionDispatcher.OnForeignKeyAdded(builder);
            }

            return builder;
        }

        private ForeignKey CreateForeignKey(
            [NotNull] InternalEntityBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique,
            bool? isRequired,
            ConfigurationSource configurationSource)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var dependentType = dependentEntityTypeBuilder.Metadata;

            if (foreignKeyProperties != null
                && dependentType.TryGetForeignKey(foreignKeyProperties) != null)
            {
                return null;
            }

            if (foreignKeyProperties != null
                && referencedProperties != null)
            {
                Entity.Metadata.Property.EnsureCompatible(referencedProperties, foreignKeyProperties);
            }

            Key principalKey;
            if (referencedProperties != null)
            {
                var keyBuilder = principalEntityTypeBuilder.Key(referencedProperties, configurationSource);
                if (keyBuilder == null)
                {
                    return null;
                }
                principalKey = keyBuilder.Metadata;
            }
            else
            {
                principalKey = principalType.TryGetPrimaryKey();
            }

            if (foreignKeyProperties != null)
            {
                if (principalKey == null
                    || !Entity.Metadata.Property.AreCompatible(principalKey.Properties, foreignKeyProperties))
                {
                    var principalKeyProperties = new Property[foreignKeyProperties.Count];
                    for (var i = 0; i < foreignKeyProperties.Count; i++)
                    {
                        var foreignKeyProperty = foreignKeyProperties[i];
                        principalKeyProperties[i] = CreateUniqueProperty(
                            foreignKeyProperty.Name,
                            foreignKeyProperty.PropertyType,
                            principalEntityTypeBuilder,
                            isRequired);
                    }

                    var keyBuilder = principalEntityTypeBuilder.Key(principalKeyProperties, ConfigurationSource.Convention);

                    principalKey = keyBuilder.Metadata;
                }
            }
            else
            {
                var baseName = (string.IsNullOrEmpty(navigationToPrincipal) ? principalType.SimpleName : navigationToPrincipal);

                if (principalKey == null)
                {
                    var principalKeyProperty = CreateUniqueProperty(
                        "TempId",
                        typeof(int),
                        principalEntityTypeBuilder,
                        isRequired);

                    principalKey = principalEntityTypeBuilder.Key(new[] { principalKeyProperty }, ConfigurationSource.Convention).Metadata;
                }

                var fkProperties = new Property[principalKey.Properties.Count];
                for (var i = 0; i < principalKey.Properties.Count; i++)
                {
                    var keyProperty = principalKey.Properties[i];
                    fkProperties[i] = CreateUniqueProperty(
                        baseName + keyProperty.Name,
                        keyProperty.PropertyType.MakeNullable(),
                        dependentEntityTypeBuilder,
                        isRequired);
                }

                foreignKeyProperties = fkProperties;
            }

            var newForeignKey = dependentType.AddForeignKey(foreignKeyProperties, principalKey);
            newForeignKey.IsUnique = isUnique;

            foreach (var foreignKeyProperty in foreignKeyProperties)
            {
                dependentEntityTypeBuilder.Property(foreignKeyProperty.PropertyType, foreignKeyProperty.Name, ConfigurationSource.Convention)
                    .GenerateValueOnAdd(false, ConfigurationSource.Convention);
            }

            return newForeignKey;
        }

        private Property CreateUniqueProperty(string baseName, Type propertyType, InternalEntityBuilder entityTypeBuilder, bool? isRequired = null)
        {
            var index = -1;
            while (true)
            {
                var name = baseName + (++index > 0 ? index.ToString() : "");
                if (entityTypeBuilder.Metadata.TryGetProperty(name) != null)
                {
                    continue;
                }

                var propertyBuilder = entityTypeBuilder.Property(propertyType, name, ConfigurationSource.Convention);
                if (propertyBuilder != null)
                {
                    if (isRequired.HasValue)
                    {
                        propertyBuilder.Required(isRequired.Value, ConfigurationSource.Convention);
                    }
                    return propertyBuilder.Metadata;
                }
            }
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

        public virtual IReadOnlyList<Property> GetOrCreateProperties([CanBeNull] IEnumerable<string> propertyNames, ConfigurationSource configurationSource)
        {
            if (propertyNames == null)
            {
                return null;
            }

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

        public virtual IReadOnlyList<Property> GetOrCreateProperties([CanBeNull] IEnumerable<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
        {
            if (clrProperties == null)
            {
                return null;
            }

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

        private class RelationshipSnapshot
        {
            public RelationshipSnapshot(
                InternalRelationshipBuilder relationship,
                string navigationToPrincipalName,
                string navigationToDependentName,
                ConfigurationSource relationshipConfigurationSource)
            {
                Relationship = relationship;
                RelationshipConfigurationSource = relationshipConfigurationSource;
                NavigationToPrincipalName = navigationToPrincipalName;
                NavigationToDependentName = navigationToDependentName;
            }

            private InternalRelationshipBuilder Relationship { get; }
            private ConfigurationSource RelationshipConfigurationSource { get; }
            private string NavigationToPrincipalName { get; }
            private string NavigationToDependentName { get; }

            public InternalRelationshipBuilder Attach()
            {
                var newRelationship = Relationship.Attach(RelationshipConfigurationSource);
                var inverted = Relationship.Metadata.EntityType != newRelationship.Metadata.EntityType;
                if (NavigationToPrincipalName != null)
                {
                    newRelationship = inverted
                        ? newRelationship.NavigationToDependent(NavigationToPrincipalName, RelationshipConfigurationSource)
                        : newRelationship.NavigationToPrincipal(NavigationToPrincipalName, RelationshipConfigurationSource);
                }

                inverted = Relationship.Metadata.EntityType != newRelationship.Metadata.EntityType;
                if (NavigationToDependentName != null)
                {
                    newRelationship = inverted
                        ? newRelationship.NavigationToPrincipal(NavigationToDependentName, RelationshipConfigurationSource)
                        : newRelationship.NavigationToDependent(NavigationToDependentName, RelationshipConfigurationSource);
                }

                return newRelationship;
            }
        }
    }
}
