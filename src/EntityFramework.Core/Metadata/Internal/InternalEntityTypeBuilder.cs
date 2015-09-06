// Copyright (c) .NET Foundation. All rights reserved.
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
    public class InternalEntityTypeBuilder : InternalMetadataItemBuilder<EntityType>
    {
        private readonly LazyRef<MetadataDictionary<Index, InternalIndexBuilder>> _indexBuilders =
            new LazyRef<MetadataDictionary<Index, InternalIndexBuilder>>(() => new MetadataDictionary<Index, InternalIndexBuilder>());

        private readonly MetadataDictionary<Key, InternalKeyBuilder> _keyBuilders = new MetadataDictionary<Key, InternalKeyBuilder>();
        private readonly MetadataDictionary<Property, InternalPropertyBuilder> _propertyBuilders = new MetadataDictionary<Property, InternalPropertyBuilder>();

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _ignoredProperties =
            new LazyRef<Dictionary<string, ConfigurationSource>>(() => new Dictionary<string, ConfigurationSource>());

        private readonly LazyRef<MetadataDictionary<ForeignKey, InternalRelationshipBuilder>> _relationshipBuilders =
            new LazyRef<MetadataDictionary<ForeignKey, InternalRelationshipBuilder>>(() => new MetadataDictionary<ForeignKey, InternalRelationshipBuilder>());

        private ConfigurationSource? _baseTypeConfigurationSource;

        public InternalEntityTypeBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        public virtual InternalKeyBuilder PrimaryKey([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var oldPrimaryKey = Metadata.FindDeclaredPrimaryKey();
            var newPrimaryKey = Metadata.FindDeclaredKey(properties);
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
                ModelBuilder.ConventionDispatcher.OnKeyAdded(Key(properties, ConfigurationSource.Convention));
            }

            var keyBuilder = _keyBuilders.GetOrAdd(
                () => newPrimaryKey,
                () => Metadata.SetPrimaryKey(properties),
                key => new InternalKeyBuilder(key, ModelBuilder),
                ModelBuilder.ConventionDispatcher.OnKeyAdded,
                configurationSource,
                configurationSource);

            ReplaceConventionShadowKeys(keyBuilder.Metadata);

            return keyBuilder;
        }

        private void UpdateReferencingForeignKeys(Key keyToReplace, Key newKey, ConfigurationSource configurationSource)
        {
            var newProperties = newKey.Properties;

            var allForeignKeysReplaced = true;
            foreach (var referencingForeignKey in ModelBuilder.Metadata.FindReferencingForeignKeys(keyToReplace).ToList())
            {
                allForeignKeysReplaced &= Relationship(
                    referencingForeignKey,
                    existingForeignKey: true,
                    configurationSource: ConfigurationSource.Convention)
                    .UpdatePrincipalKey(newProperties, configurationSource) != null;
            }

            if (allForeignKeysReplaced)
            {
                RemoveKey(keyToReplace, ConfigurationSource.Convention);
            }
        }

        private void ReplaceConventionShadowKeys(Key newKey)
        {
            foreach (var key in Metadata.GetKeys().ToList())
            {
                if (key != newKey
                    && _keyBuilders.GetConfigurationSource(key) == ConfigurationSource.Convention
                    && key.Properties.All(p => ((IProperty)p).IsShadowProperty))
                {
                    UpdateReferencingForeignKeys(key, newKey, ConfigurationSource.Convention);
                }
            }
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => Key(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => Key(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

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
                () => Metadata.FindDeclaredKey(properties),
                () => Metadata.AddKey(properties),
                key => new InternalKeyBuilder(key, ModelBuilder),
                ModelBuilder.ConventionDispatcher.OnKeyAdded,
                configurationSource,
                configurationSource);
        }

        public virtual ConfigurationSource? RemoveKey([NotNull] Key key, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            var removedConfigurationSource = _keyBuilders.Remove(key, configurationSource, canOverrideSameSource);
            if (!removedConfigurationSource.HasValue)
            {
                return null;
            }

            foreach (var foreignKey in ModelBuilder.Metadata.FindReferencingForeignKeys(key).ToList())
            {
                var removed = ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            var removedKey = Metadata.RemoveKey(key);
            Debug.Assert(removedKey == key);

            RemoveShadowPropertiesIfUnused(key.Properties);

            return removedConfigurationSource;
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName, [NotNull] Type propertyType, ConfigurationSource configurationSource)
            => InternalProperty(propertyName, propertyType, /*shadowProperty:*/ null, configurationSource);

        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName, ConfigurationSource configurationSource)
            => InternalProperty(propertyName, null, /*shadowProperty:*/ null, configurationSource);

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
            => InternalProperty(clrProperty.Name, clrProperty.PropertyType, /*shadowProperty:*/ false, configurationSource);

        private InternalPropertyBuilder InternalProperty(string propertyName, Type propertyType, bool? shadowProperty, ConfigurationSource configurationSource)
        {
            if (CanAdd(propertyName, isNavigation: false, configurationSource: configurationSource))
            {
                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                }

                var builder = _propertyBuilders.GetOrAdd(
                    () => Metadata.FindDeclaredProperty(propertyName),
                    () => Metadata.AddProperty(propertyName),
                    property => new InternalPropertyBuilder(property, ModelBuilder),
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource,
                    configurationSource);

                if (builder == null)
                {
                    return null;
                }

                if (propertyType != null
                    && !builder.ClrType(propertyType, configurationSource))
                {
                    return null;
                }

                if (shadowProperty.HasValue
                    && !builder.Shadow(shadowProperty.Value, configurationSource))
                {
                    return null;
                }

                return builder;
            }

            return null;
        }

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource)
               && (Metadata.FindNavigation(navigationName) == null || CanRemove(Metadata.FindNavigation(navigationName).ForeignKey, configurationSource, true));

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
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(foreignKey, configurationSource, canOverrideSameSource);
            }

            return _relationshipBuilders.Value.CanRemove(foreignKey, configurationSource, canOverrideSameSource);
        }

        public virtual InternalRelationshipBuilder Navigation(
            [CanBeNull] string navigationName,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource) =>
                Navigation(
                    navigationName,
                    foreignKey,
                    pointsToPrincipal,
                    configurationSource,
                    canOverrideSameSource: true);

        private InternalRelationshipBuilder Navigation(
            [CanBeNull] string navigationName,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource)
        {
            var existingNavigation = pointsToPrincipal
                ? foreignKey.DeclaringEntityType == Metadata ? foreignKey.DependentToPrincipal : null
                : foreignKey.PrincipalEntityType == Metadata ? foreignKey.PrincipalToDependent : null;

            var fkOwner = foreignKey.DeclaringEntityType == Metadata
                ? this
                : ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);

            var builder = fkOwner.Relationship(foreignKey, true, ConfigurationSource.Convention);

            if (navigationName == existingNavigation?.Name)
            {
                fkOwner._relationshipBuilders.Value.UpdateConfigurationSource(foreignKey, configurationSource);
                return builder;
            }

            if (!CanSetNavigation(navigationName, foreignKey, configurationSource, canOverrideSameSource))
            {
                return null;
            }

            var removedNavigation = existingNavigation?.DeclaringEntityType.RemoveNavigation(existingNavigation);
            Debug.Assert(removedNavigation == existingNavigation);

            var conflictingNavigation = navigationName == null
                ? null
                : Metadata.FindNavigation(navigationName);

            if (conflictingNavigation != null
                && conflictingNavigation.ForeignKey != foreignKey)
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

                if (!pointsToPrincipal)
                {
                    var navigationPropertyInfo = Metadata.ClrType?.GetPropertiesInHierarchy(navigationName).FirstOrDefault();
                    if (navigationPropertyInfo != null)
                    {
                        var elementType = navigationPropertyInfo.PropertyType.TryGetSequenceType();
                        if (elementType == null)
                        {
                            builder = builder.Unique(true, configurationSource) ?? builder;
                        }
                        else if (elementType.GetTypeInfo().IsAssignableFrom(foreignKey.DeclaringEntityType.ClrType.GetTypeInfo()))
                        {
                            builder = builder.Unique(false, configurationSource) ?? builder;
                        }
                        pointsToPrincipal = builder.Metadata.DeclaringEntityType != fkOwner.Metadata;
                    }
                }
                fkOwner._relationshipBuilders.Value.UpdateConfigurationSource(builder.Metadata, configurationSource);
                var navigation = Metadata.AddNavigation(navigationName, builder.Metadata, pointsToPrincipal);
                return ModelBuilder.ConventionDispatcher.OnNavigationAdded(builder, navigation);
            }

            return builder;
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
                : Metadata.FindNavigation(navigationName);

            if (conflictingNavigation != null
                && conflictingNavigation.ForeignKey != foreignKey
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
            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredProperties.Value.TryGetValue(propertyName, out ignoredConfigurationSource))
            {
                if (ignoredConfigurationSource.Overrides(configurationSource))
                {
                    return true;
                }
            }

            _ignoredProperties.Value[propertyName] = configurationSource;

            var property = Metadata.FindDeclaredProperty(propertyName);
            if (property != null)
            {
                if (!RemoveProperty(property, configurationSource, canOverrideSameSource: true).HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                    return false;
                }
            }

            var navigation = Metadata.FindDeclaredNavigation(propertyName);
            if (navigation != null)
            {
                if (Navigation(null, navigation.ForeignKey, navigation.PointsToPrincipal(), configurationSource, canOverrideSameSource: true) == null)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                    return false;
                }

                RemoveForeignKeyIfUnused(navigation.ForeignKey, configurationSource);
                ModelBuilder.RemoveEntityTypesUnreachableByNavigations(configurationSource);
            }

            // Ignoring a navigation or property might have fixed an ambiguity that prevented a convention from proceeding
            ModelBuilder.ConventionDispatcher.OnEntityTypeAdded(this);
            return true;
        }

        public virtual InternalEntityTypeBuilder BaseType([CanBeNull] Type baseEntityType, ConfigurationSource configurationSource)
        {
            if (baseEntityType == null)
            {
                return BaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityType, configurationSource);
            return baseType == null
                ? null
                : BaseType(baseType.Metadata, configurationSource);
        }

        public virtual InternalEntityTypeBuilder BaseType([CanBeNull] string baseEntityTypeName, ConfigurationSource configurationSource)
        {
            if (baseEntityTypeName == null)
            {
                return BaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityTypeName, configurationSource);
            return baseType == null
                ? null
                : BaseType(baseType.Metadata, configurationSource);
        }

        public virtual InternalEntityTypeBuilder BaseType([CanBeNull] EntityType baseEntityType, ConfigurationSource configurationSource)
        {
            if (_baseTypeConfigurationSource == null)
            {
                _baseTypeConfigurationSource = Metadata.BaseType != null
                    ? ConfigurationSource.Explicit
                    : ConfigurationSource.Convention;
            }

            if (Metadata.BaseType == baseEntityType)
            {
                _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);
                return this;
            }

            if (!configurationSource.Overrides(_baseTypeConfigurationSource.Value))
            {
                return null;
            }

            var detachedRelationships = new HashSet<RelationshipBuilderSnapshot>();
            var baseRelationshipsToBeRemoved = new HashSet<ForeignKey>();
            var originalBaseType = Metadata.BaseType;

            if (baseEntityType != null)
            {
                Metadata.BaseType = null;

                var duplicatedProperties = baseEntityType.Properties
                    .Select(p => Metadata.FindProperty(p.Name))
                    .Where(p => p != null)
                    .ToList();

                if (duplicatedProperties.Any(p => !_propertyBuilders.CanRemove(p, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    Metadata.BaseType = originalBaseType;
                    return null;
                }

                if (Metadata.GetKeys().Any(k => !_keyBuilders.CanRemove(k, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    Metadata.BaseType = originalBaseType;
                    return null;
                }

                var relationshipsToBeRemoved = new HashSet<ForeignKey>();
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: true);
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: false);

                if (baseRelationshipsToBeRemoved.Any(relationshipToBeRemoved =>
                    !CanRemove(relationshipToBeRemoved, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    Metadata.BaseType = originalBaseType;
                    return null;
                }

                if (relationshipsToBeRemoved.Any(relationshipToBeRemoved =>
                    !CanRemove(relationshipToBeRemoved, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    Metadata.BaseType = originalBaseType;
                    return null;
                }

                foreach (var relationshipToBeRemoved in baseRelationshipsToBeRemoved)
                {
                    var removedConfigurationSource = RemoveRelationship(relationshipToBeRemoved, configurationSource);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
                {
                    var removedConfigurationSource = RemoveRelationship(relationshipToBeRemoved, configurationSource);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                foreach (var key in Metadata.GetKeys().ToList())
                {
                    foreach (var referencingForeignKey in ModelBuilder.Metadata.FindReferencingForeignKeys(key).ToList())
                    {
                        detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                    }
                }

                foreach (var duplicatedProperty in duplicatedProperties)
                {
                    foreach (var relationship in duplicatedProperty.FindContainingForeignKeys(Metadata).ToList())
                    {
                        detachedRelationships.Add(DetachRelationship(relationship));
                    }

                    // TODO: Detach indexes that contain non-duplicate properties
                    // Issue #2514
                }

                foreach (var key in Metadata.GetKeys().ToList())
                {
                    if (Metadata.FindKey(key.Properties) != null)
                    {
                        var removedConfigurationSource = RemoveKey(key, configurationSource);
                        Debug.Assert(removedConfigurationSource.HasValue);
                    }
                }

                foreach (var duplicatedProperty in duplicatedProperties)
                {
                    if (Metadata.FindProperty(duplicatedProperty.Name) != null)
                    {
                        var removedConfigurationSource = RemoveProperty(duplicatedProperty, configurationSource);
                        Debug.Assert(removedConfigurationSource.HasValue);
                    }
                }

                ModelBuilder.Entity(baseEntityType.Name, configurationSource);
            }

            _baseTypeConfigurationSource = configurationSource;
            Metadata.BaseType = baseEntityType;

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            if (baseRelationshipsToBeRemoved.Any())
            {
                // Try to readd the removed relationships to the derived types
                var basestType = baseEntityType;
                foreach (var baseTypeFromRelationship in baseRelationshipsToBeRemoved.Select(r => r.ResolveEntityType(baseEntityType)))
                {
                    if (baseTypeFromRelationship.IsAssignableFrom(baseEntityType))
                    {
                        basestType = baseTypeFromRelationship;
                    }
                    Debug.Assert(baseEntityType.IsAssignableFrom(baseTypeFromRelationship));
                }

                var affectedDerivedTypes = new Queue<EntityType>();
                affectedDerivedTypes.Enqueue(basestType);
                while (affectedDerivedTypes.Count > 0)
                {
                    var affectedDerivedType = affectedDerivedTypes.Dequeue();
                    foreach (var moreDerivedType in affectedDerivedType.GetDirectlyDerivedTypes().Where(t => t != Metadata))
                    {
                        affectedDerivedTypes.Enqueue(moreDerivedType);
                    }
                    if (affectedDerivedType != baseEntityType)
                    {
                        ModelBuilder.ConventionDispatcher.OnEntityTypeAdded(
                            ModelBuilder.Entity(affectedDerivedType.Name, ConfigurationSource.Convention));
                    }
                }
            }

            ModelBuilder.ConventionDispatcher.OnBaseEntityTypeSet(this, originalBaseType);

            return this;
        }

        private void FindConflictingRelationships(
            EntityType baseEntityType,
            HashSet<ForeignKey> baseRelationshipsToBeRemoved,
            HashSet<ForeignKey> relationshipsToBeRemoved,
            bool whereDependent)
        {
            var baseRelationshipsByTargetType = GroupForeignKeysByTargetType(baseEntityType, whereDependent);
            var relationshipsByTargetType = GroupForeignKeysByTargetType(Metadata, whereDependent);

            foreach (var relatedEntityType in relationshipsByTargetType.Keys)
            {
                if (!baseRelationshipsByTargetType.ContainsKey(relatedEntityType))
                {
                    continue;
                }

                foreach (var baseRelationship in baseRelationshipsByTargetType[relatedEntityType])
                {
                    foreach (var relationship in relationshipsByTargetType[relatedEntityType])
                    {
                        if (!relationship.ConflictsWith(baseRelationship, whereDependent))
                        {
                            continue;
                        }

                        if (baseRelationship.NavigationTo == null
                            && relationship.NavigationTo != null)
                        {
                            baseRelationshipsToBeRemoved.Add(baseRelationship.ForeignKey);
                        }
                        else
                        {
                            relationshipsToBeRemoved.Add(relationship.ForeignKey);
                        }
                    }
                }
            }
        }

        private Dictionary<EntityType, List<RelationshipSnapshot>> GroupForeignKeysByTargetType(EntityType entityType, bool whereDependent)
        {
            var foreignKeys = whereDependent
                ? entityType.GetForeignKeys()
                : entityType.FindReferencingForeignKeys().Where(foreignKey => !foreignKey.IsSelfReferencing());
            return foreignKeys
                .GroupBy(foreignKey => whereDependent ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType)
                .ToDictionary(g => g.Key, g => g.Select(foreignKey =>
                    new RelationshipSnapshot(foreignKey,
                        whereDependent ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent,
                        whereDependent ? foreignKey.PrincipalToDependent : foreignKey.DependentToPrincipal)).ToList());
        }

        private ConfigurationSource? RemoveProperty(Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            var removedConfigurationSource = _propertyBuilders.Remove(property, configurationSource, canOverrideSameSource);
            if (!removedConfigurationSource.HasValue)
            {
                return null;
            }

            foreach (var index in Metadata.Indexes.Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveIndex(index, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            var detachedRelationships = property.FindContainingForeignKeys(Metadata).ToList()
                .Select(DetachRelationship).ToList();

            foreach (var key in Metadata.GetKeys().Where(i => i.Properties.Contains(property)).ToList())
            {
                detachedRelationships.AddRange(ModelBuilder.Metadata.FindReferencingForeignKeys(key).ToList()
                    .Select(DetachRelationship));
                var removed = RemoveKey(key, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            if (Metadata.Properties.Contains(property))
            {
                var removedProperty = Metadata.RemoveProperty(property);
                Debug.Assert(removedProperty == property);
            }

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            return removedConfigurationSource;
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] string principalEntityTypeName, [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotNull(propertyNames, nameof(propertyNames));

            var principalType = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            if (principalType == null)
            {
                return null;
            }

            return ForeignKey(principalType.Metadata, GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type principalClrType, [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            ConfigurationSource configurationSource)
        {
            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : ForeignKey(principalType.Metadata, GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalRelationshipBuilder ForeignKey(EntityType principalType, IReadOnlyList<Property> dependentProperties, ConfigurationSource configurationSource)
            => dependentProperties == null
                ? null
                : Relationship(principalType, Metadata, null, null, configurationSource, strictPrincipal: false)
                    ?.ForeignKey(dependentProperties, configurationSource);

        private RelationshipBuilderSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
        {
            var navigationToPrincipalName = foreignKey.DependentToPrincipal?.Name;
            var navigationToDependentName = foreignKey.PrincipalToDependent?.Name;
            var relationship = Relationship(foreignKey, true, ConfigurationSource.Convention);
            var relationshipConfigurationSource = RemoveRelationship(foreignKey, ConfigurationSource.Explicit);
            Debug.Assert(relationshipConfigurationSource != null);

            return new RelationshipBuilderSnapshot(relationship, navigationToPrincipalName, navigationToDependentName, relationshipConfigurationSource.Value);
        }

        private bool RemoveRelationships(ConfigurationSource configurationSource, params ForeignKey[] foreignKeys)
        {
            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey != null)
                {
                    var relationshipConfigurationSource = ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
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
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .RemoveRelationship(foreignKey, configurationSource);
            }

            var removedConfigurationSource = _relationshipBuilders.Value.Remove(foreignKey, configurationSource);
            if (removedConfigurationSource == null)
            {
                return null;
            }

            var principalEntityTypeBuilder = ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);

            var navigationToDependent = foreignKey.PrincipalToDependent;
            var removedNavigation = navigationToDependent?.DeclaringEntityType.RemoveNavigation(navigationToDependent);
            Debug.Assert(removedNavigation == navigationToDependent);

            var navigationToPrincipal = foreignKey.DependentToPrincipal;
            removedNavigation = navigationToPrincipal?.DeclaringEntityType.RemoveNavigation(navigationToPrincipal);
            Debug.Assert(removedNavigation == navigationToPrincipal);

            var removedForeignKey = Metadata.RemoveForeignKey(foreignKey);
            Debug.Assert(removedForeignKey == foreignKey);

            ModelBuilder.ConventionDispatcher.OnForeignKeyRemoved(this, foreignKey);
            RemoveShadowPropertiesIfUnused(foreignKey.Properties);
            principalEntityTypeBuilder.RemoveKeyIfUnused(foreignKey.PrincipalKey);

            return removedConfigurationSource;
        }

        private void RemoveKeyIfUnused(Key key)
        {
            if (Metadata.FindPrimaryKey() == key)
            {
                return;
            }

            if (ModelBuilder.Metadata.FindReferencingForeignKeys(key).Any())
            {
                return;
            }

            RemoveKey(key, ConfigurationSource.Convention);
        }

        private void RemoveForeignKeyIfUnused(ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.PrincipalToDependent == null
                && foreignKey.DependentToPrincipal == null)
            {
                RemoveRelationship(foreignKey, configurationSource);
            }
        }

        public virtual void RemoveShadowPropertiesIfUnused([NotNull] IReadOnlyList<Property> properties)
        {
            foreach (var property in properties.ToList())
            {
                if (((IProperty)property).IsShadowProperty)
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

            if (Metadata.GetForeignKeys().Any(i => i.Properties.Contains(property)))
            {
                return;
            }

            if (Metadata.GetKeys().Any(i => i.Properties.Contains(property)))
            {
                return;
            }

            if (!_propertyBuilders.Remove(property, ConfigurationSource.Convention).HasValue)
            {
                return;
            }

            if (Metadata.Properties.Contains(property))
            {
                var removedProperty = Metadata.RemoveProperty(property);
                Debug.Assert(removedProperty == property);
            }
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => Index(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => Index(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        private InternalIndexBuilder Index(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => properties == null
                ? null
                : _indexBuilders.Value.GetOrAdd(
                    () => Metadata.FindDeclaredIndex(properties),
                    () => Metadata.AddIndex(properties),
                    index => new InternalIndexBuilder(index, ModelBuilder),
                    configurationSource);

        public virtual ConfigurationSource? RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            var removedConfigurationSource = _indexBuilders.Value.Remove(index, configurationSource);
            if (!removedConfigurationSource.HasValue)
            {
                return null;
            }

            var removedIndex = Metadata.RemoveIndex(index);
            Debug.Assert(removedIndex == index);

            RemoveShadowPropertiesIfUnused(index.Properties);

            return removedConfigurationSource;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] ForeignKey foreignKey, bool existingForeignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
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
            [NotNull] InternalEntityTypeBuilder sourceBuilder,
            [NotNull] PropertyInfo navigationToTarget,
            [CanBeNull] PropertyInfo navigationToSource,
            ConfigurationSource configurationSource)
        {
            var isToTargetNavigationCollection = navigationToTarget.PropertyType.TryGetSequenceType() != null;

            if (isToTargetNavigationCollection)
            {
                if (navigationToSource?.PropertyType.TryGetSequenceType() != null)
                {
                    // TODO: Support many to many
                    return null;
                }

                return Relationship(
                    sourceBuilder,
                    this,
                    navigationToSource?.Name,
                    navigationToTarget.Name,
                    configurationSource: configurationSource, isUnique: false, strictPrincipal: true);
            }
            else
            {
                if (navigationToSource == null)
                {
                    return Relationship(
                        this,
                        sourceBuilder,
                        navigationToTarget.Name,
                        navigationToDependentName: null,
                        configurationSource: configurationSource, isUnique: null, strictPrincipal: false);
                }
                else
                {
                    if (navigationToSource.PropertyType.TryGetSequenceType() == null)
                    {
                        return Relationship(
                            sourceBuilder,
                            this,
                            navigationToSource.Name,
                            navigationToTarget.Name,
                            configurationSource: configurationSource, isUnique: true, strictPrincipal: false);
                    }
                }
            }
            return null;
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
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool strictPrincipal = true) =>
                Relationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    null,
                    null,
                    configurationSource,
                    isUnique: isUnique,
                    strictPrincipal: strictPrincipal);

        // If strictPrincipal is true then principalEntityTypeBuilder will always be the principal end,
        // otherwise an existing foreign key going the other way could be used
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool? isRequired = null,
            bool strictPrincipal = true,
            [CanBeNull] Func<InternalRelationshipBuilder, InternalRelationshipBuilder> onRelationshipAdding = null)
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
                    foreignKeyProperties,
                    principalProperties,
                    configurationSource,
                    isUnique,
                    isRequired,
                    strictPrincipal,
                    onRelationshipAdding);
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
                : dependentEntityType.FindDeclaredNavigation(navigationToPrincipalName);

            if (navigationToPrincipal != null
                && navigationToPrincipal.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)true : null, isUnique))
            {
                return Relationship(navigationToPrincipal, configurationSource, navigationToDependentName);
            }

            var navigationToDependent = string.IsNullOrEmpty(navigationToDependentName)
                ? null
                : principalEntityType.FindDeclaredNavigation(navigationToDependentName);

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

            if (foreignKeyProperties != null
                && foreignKeyProperties.Count == 0)
            {
                foreignKeyProperties = null;
            }

            if (principalProperties != null
                && principalProperties.Count == 0)
            {
                principalProperties = null;
            }

            var foreignKey = foreignKeyProperties == null
                ? null
                : dependentEntityType.FindForeignKey(
                    principalEntityType,
                    null,
                    null,
                    foreignKeyProperties,
                    principalProperties,
                    isUnique);

            var existingForeignKey = foreignKey != null;
            if (!existingForeignKey)
            {
                if (foreignKeyProperties != null)
                {
                    var conflictingForeignKey = dependentEntityType.FindForeignKey(foreignKeyProperties);
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
                    principalProperties,
                    isUnique,
                    isRequired,
                    configurationSource);

                if (foreignKey == null)
                {
                    return null;
                }
            }

            var builder = Relationship(foreignKey, existingForeignKey, configurationSource);
            Debug.Assert(builder != null);

            if (isRequired.HasValue)
            {
                if (!builder.CanSetRequired(isRequired.Value, configurationSource))
                {
                    if (!existingForeignKey)
                    {
                        var removedForeignKey = dependentEntityType.RemoveForeignKey(foreignKey);
                        Debug.Assert(removedForeignKey == foreignKey);
                    }

                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        // TODO: throw for explicit
                        throw new InvalidOperationException();
                    }

                    return null;
                }

                var properties = foreignKey.Properties;
                var nullableTypeProperties = properties.Where(p => ((IProperty)p).ClrType.IsNullableType()).ToList();
                if (nullableTypeProperties.Any())
                {
                    properties = nullableTypeProperties;
                }
                // If no properties can be made nullable, let it fail

                foreach (var property in properties)
                {
                    var requiredSet = ModelBuilder.Entity(property.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .Property(property.Name, ConfigurationSource.Convention)
                        .Required(isRequired.Value, configurationSource);
                    if (requiredSet
                        && !isRequired.Value)
                    {
                        break;
                    }
                    Debug.Assert(requiredSet || !isRequired.Value);
                }

                Debug.Assert(foreignKey.IsRequired == isRequired);
                Debug.Assert(((IForeignKey)foreignKey).IsRequired == isRequired);
            }

            builder = dependentEntityTypeBuilder
                .Navigation(navigationToPrincipalName, builder.Metadata, pointsToPrincipal: true, configurationSource: configurationSource)
                      ?? _relationshipBuilders.Value.TryGetValue(builder.Metadata, configurationSource);
            if (builder == null)
            {
                return null;
            }

            builder = principalEntityTypeBuilder
                .Navigation(navigationToDependentName, builder.Metadata, pointsToPrincipal: builder.Metadata.DeclaringEntityType != Metadata, configurationSource: configurationSource)
                      ?? _relationshipBuilders.Value.TryGetValue(builder.Metadata, configurationSource);
            if (builder == null)
            {
                return null;
            }

            Debug.Assert(builder != null);

            if (onRelationshipAdding != null)
            {
                builder = onRelationshipAdding(builder);
            }
            else
            {
                if (isUnique.HasValue)
                {
                    builder = builder.Unique(foreignKey.IsUnique, configurationSource);
                }
                if (isRequired.HasValue)
                {
                    builder = builder.Required(foreignKey.IsRequired, configurationSource);
                }
                if (foreignKeyProperties != null)
                {
                    builder = builder.ForeignKey(foreignKey.Properties, configurationSource);
                }
                if (principalProperties != null)
                {
                    builder = builder.PrincipalKey(foreignKey.PrincipalKey.Properties, configurationSource);
                }
            }

            if (!existingForeignKey)
            {
                builder = ModelBuilder.ConventionDispatcher.OnForeignKeyAdded(builder);
            }

            return builder;
        }

        private ForeignKey CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            ConfigurationSource configurationSource)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var dependentType = dependentEntityTypeBuilder.Metadata;

            if (foreignKeyProperties != null
                && dependentType.FindForeignKey(foreignKeyProperties) != null)
            {
                return null;
            }

            if (foreignKeyProperties != null
                && principalProperties != null)
            {
                Entity.Metadata.Property.EnsureCompatible(principalProperties, foreignKeyProperties, principalType, dependentType);
            }

            var principalBaseEntityTypeBuilder = ModelBuilder.Entity(principalType.RootType().Name, ConfigurationSource.Convention);
            Key principalKey;
            if (principalProperties != null)
            {
                var keyBuilder = principalBaseEntityTypeBuilder.Key(principalProperties, configurationSource);
                if (keyBuilder == null)
                {
                    return null;
                }
                principalKey = keyBuilder.Metadata;
            }
            else
            {
                principalKey = principalType.FindPrimaryKey();
            }

            if (foreignKeyProperties != null)
            {
                if (principalKey == null
                    || !Entity.Metadata.Property.AreCompatible(principalKey.Properties, foreignKeyProperties))
                {
                    var principalKeyProperties = new Property[foreignKeyProperties.Count];
                    for (var i = 0; i < foreignKeyProperties.Count; i++)
                    {
                        IProperty foreignKeyProperty = foreignKeyProperties[i];
                        principalKeyProperties[i] = CreateUniqueProperty(
                            foreignKeyProperty.Name,
                            foreignKeyProperty.ClrType,
                            principalBaseEntityTypeBuilder,
                            isRequired);
                    }

                    var keyBuilder = principalBaseEntityTypeBuilder.Key(principalKeyProperties, ConfigurationSource.Convention);

                    principalKey = keyBuilder.Metadata;
                }
            }
            else
            {
                var baseName = (string.IsNullOrEmpty(navigationToPrincipal) ? principalType.DisplayName() : navigationToPrincipal);

                if (principalKey == null)
                {
                    var principalKeyProperty = CreateUniqueProperty(
                        "TempId",
                        isRequired ?? false ? typeof(int) : typeof(int?),
                        principalBaseEntityTypeBuilder,
                        isRequired);

                    principalKey = principalBaseEntityTypeBuilder.Key(new[] { principalKeyProperty }, ConfigurationSource.Convention).Metadata;
                }

                var fkProperties = new Property[principalKey.Properties.Count];
                for (var i = 0; i < principalKey.Properties.Count; i++)
                {
                    IProperty keyProperty = principalKey.Properties[i];
                    fkProperties[i] = CreateUniqueProperty(
                        baseName + keyProperty.Name,
                        keyProperty.ClrType.MakeNullable(),
                        dependentEntityTypeBuilder,
                        isRequired);
                }

                foreignKeyProperties = fkProperties;
            }

            var newForeignKey = dependentType.AddForeignKey(foreignKeyProperties, principalKey, principalType);
            newForeignKey.IsUnique = isUnique;

            foreach (var foreignKeyProperty in foreignKeyProperties)
            {
                var propertyBuilder = ModelBuilder.Entity(foreignKeyProperty.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .Property(foreignKeyProperty.Name, ConfigurationSource.Convention);

                propertyBuilder.UseValueGenerator(null, ConfigurationSource.Convention);
                propertyBuilder.ValueGenerated(null, ConfigurationSource.Convention);
            }

            return newForeignKey;
        }

        private Property CreateUniqueProperty(string baseName, Type propertyType, InternalEntityTypeBuilder entityTypeBuilder, bool? isRequired = null)
        {
            var index = -1;
            while (true)
            {
                var name = baseName + (++index > 0 ? index.ToString() : "");
                if (entityTypeBuilder.Metadata.FindProperty(name) != null
                    || entityTypeBuilder.Metadata.FindDerivedProperties(new[] { name }).Any())
                {
                    continue;
                }

                var propertyBuilder = entityTypeBuilder.Property(name, ConfigurationSource.Convention);
                if (propertyBuilder != null)
                {
                    var clrTypeSet = propertyBuilder.ClrType(propertyType, ConfigurationSource.Convention);
                    Debug.Assert(clrTypeSet);

                    if (isRequired.HasValue
                        && propertyType.IsNullableType())
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

                relationship = navigation.PointsToPrincipal()
                    ? relationship.PrincipalToDependent(inverseNavigationName, configurationSource)
                    : relationship.DependentToPrincipal(inverseNavigationName, configurationSource);
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
                var property = Metadata.FindProperty(propertyName);
                if (property == null)
                {
                    if (Metadata.ClrType == null)
                    {
                        throw new ModelItemNotFoundException(Strings.PropertyNotFound(propertyName, Metadata.Name));
                    }

                    var clrProperty = Metadata.ClrType.GetPropertiesInHierarchy(propertyName).FirstOrDefault();
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
                    Property(property.Name, configurationSource);
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

        private struct RelationshipSnapshot
        {
            public readonly ForeignKey ForeignKey;
            public readonly Navigation NavigationFrom;
            public readonly Navigation NavigationTo;

            public RelationshipSnapshot(ForeignKey foreignKey, Navigation navigationFrom, Navigation navigationTo)
            {
                ForeignKey = foreignKey;
                NavigationFrom = navigationFrom;
                NavigationTo = navigationTo;
            }

            public bool ConflictsWith(RelationshipSnapshot baseRelationship, bool whereDependent) =>
                (NavigationFrom != null && baseRelationship.NavigationFrom?.Name == NavigationFrom.Name)
                || (whereDependent && PropertyListComparer.Instance.Equals(baseRelationship.ForeignKey.Properties, ForeignKey.Properties));
        }

        private class RelationshipBuilderSnapshot
        {
            public RelationshipBuilderSnapshot(
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
                var inverted = Relationship.Metadata.DeclaringEntityType != newRelationship.Metadata.DeclaringEntityType;
                Debug.Assert(inverted
                             || (Relationship.Metadata.DeclaringEntityType == newRelationship.Metadata.DeclaringEntityType
                                 && Relationship.Metadata.PrincipalEntityType == newRelationship.Metadata.PrincipalEntityType));
                Debug.Assert(!inverted
                             || (Relationship.Metadata.DeclaringEntityType == newRelationship.Metadata.PrincipalEntityType
                                 && Relationship.Metadata.PrincipalEntityType == newRelationship.Metadata.DeclaringEntityType));

                if (NavigationToPrincipalName != null)
                {
                    newRelationship = inverted
                        ? newRelationship.PrincipalToDependent(NavigationToPrincipalName, RelationshipConfigurationSource)
                        : newRelationship.DependentToPrincipal(NavigationToPrincipalName, RelationshipConfigurationSource);
                }

                if (NavigationToDependentName != null)
                {
                    newRelationship = inverted
                        ? newRelationship.DependentToPrincipal(NavigationToDependentName, RelationshipConfigurationSource)
                        : newRelationship.PrincipalToDependent(NavigationToDependentName, RelationshipConfigurationSource);
                }

                return newRelationship;
            }
        }
    }
}
