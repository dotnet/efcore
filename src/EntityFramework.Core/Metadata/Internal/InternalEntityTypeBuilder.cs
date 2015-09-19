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
        private ConfigurationSource? _primaryKeyConfigurationSource;

        public InternalEntityTypeBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        private InternalKeyBuilder PrimaryKey(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            if (Metadata.FindPrimaryKey(properties) != null)
            {
                _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);
                return Key(properties, configurationSource);
            }

            if (!_primaryKeyConfigurationSource.HasValue
                && Metadata.FindDeclaredPrimaryKey() != null)
            {
                _primaryKeyConfigurationSource = ConfigurationSource.Explicit;
            }

            if (_primaryKeyConfigurationSource.HasValue
                && !configurationSource.Overrides(_primaryKeyConfigurationSource.Value))
            {
                return null;
            }

            var keyBuilder = Key(properties, configurationSource);
            if (keyBuilder == null)
            {
                return null;
            }

            var previousPrimaryKey = Metadata.FindPrimaryKey();
            _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);
            Metadata.SetPrimaryKey(keyBuilder.Metadata.Properties);
            UpdateReferencingForeignKeys(keyBuilder.Metadata);

            keyBuilder = ModelBuilder.ConventionDispatcher.OnPrimaryKeySet(keyBuilder, previousPrimaryKey);

            return keyBuilder;
        }

        private void UpdateReferencingForeignKeys(Key newKey)
        {
            foreach (var key in Metadata.GetDeclaredKeys().ToList())
            {
                if (key == newKey)
                {
                    continue;
                }

                var detachedRelationships = ModelBuilder.Metadata.FindReferencingForeignKeys(key).ToList()
                    .Select(DetachRelationship).ToList();
                RemoveKey(key, ConfigurationSource.DataAnnotation);
                foreach (var relationshipSnapshot in detachedRelationships)
                {
                    relationshipSnapshot.Attach();
                }
            }
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => Key(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => Key(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        private InternalKeyBuilder Key(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = GetOrCreateProperties(properties, configurationSource);

            return _keyBuilders.GetOrAdd(
                () => Metadata.FindDeclaredKey(actualProperties),
                () => Metadata.AddKey(actualProperties),
                key => new InternalKeyBuilder(key, ModelBuilder),
                ModelBuilder.ConventionDispatcher.OnKeyAdded,
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
            => InternalProperty(propertyName, propertyType, configurationSource);

        public virtual InternalPropertyBuilder Property([NotNull] string propertyName, ConfigurationSource configurationSource)
            => InternalProperty(propertyName, null, configurationSource);

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
            => InternalProperty(clrProperty, configurationSource);

        private InternalPropertyBuilder InternalProperty(PropertyInfo clrProperty, ConfigurationSource configurationSource)
        {
            var propertyName = clrProperty.Name;
            if (CanAdd(propertyName, isNavigation: false, configurationSource: configurationSource))
            {
                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                }

                var existingProperty = Metadata.FindDeclaredProperty(propertyName);
                var builder = _propertyBuilders.GetOrAdd(
                    () => existingProperty,
                    () => Metadata.AddProperty(clrProperty),
                    property => new InternalPropertyBuilder(property, ModelBuilder, existing: existingProperty != null),
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource);

                return ConfigureProperty(builder, clrProperty.PropertyType, false, configurationSource);
            }

            return null;
        }

        private InternalPropertyBuilder InternalProperty(string propertyName, Type propertyType, ConfigurationSource configurationSource)
        {
            if (CanAdd(propertyName, isNavigation: false, configurationSource: configurationSource))
            {
                if (_ignoredProperties.HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                }

                var existingProperty = Metadata.FindDeclaredProperty(propertyName);
                var builder = _propertyBuilders.GetOrAdd(
                    () => existingProperty,
                    () =>
                        {
                            var property = Metadata.AddProperty(propertyName);
                            if (propertyType != null)
                            {
                                property.ClrType = propertyType;
                            }
                            return property;
                        },
                    property => new InternalPropertyBuilder(property, ModelBuilder, existing: existingProperty != null),
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource);

                return ConfigureProperty(builder, propertyType, /*shadowProperty*/ null, configurationSource);
            }

            return null;
        }

        private InternalPropertyBuilder ConfigureProperty(InternalPropertyBuilder builder, Type propertyType, bool? shadowProperty, ConfigurationSource configurationSource)
        {
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

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource)
               && Metadata.FindNavigation(navigationName) == null;

        public virtual bool CanAddOrReplaceNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource)
               && (Metadata.FindNavigation(navigationName) == null
                   || CanRemove(Metadata.FindNavigation(navigationName).ForeignKey, configurationSource, true));

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
            ConfigurationSource configurationSource)
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
                return fkOwner.Relationship(builder.Metadata, true, configurationSource);
            }

            if (!CanSetNavigation(navigationName, builder, pointsToPrincipal, configurationSource))
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
                    var canBeUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName, Metadata, foreignKey.DeclaringEntityType, shouldBeCollection: false, shouldThrow: false);
                    var canBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName, Metadata, foreignKey.DeclaringEntityType, shouldBeCollection: true, shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        builder = builder.Unique(canBeUnique, configurationSource);
                        Debug.Assert(builder != null);
                        pointsToPrincipal = builder.Metadata.DeclaringEntityType != fkOwner.Metadata;
                    }
                }

                builder = fkOwner.Relationship(builder.Metadata, true, configurationSource);
                var navigation = Metadata.AddNavigation(navigationName, builder.Metadata, pointsToPrincipal);
                return ModelBuilder.ConventionDispatcher.OnNavigationAdded(builder, navigation);
            }

            return builder;
        }

        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            if (!CanRemove(relationshipBuilder.Metadata, configurationSource, canOverrideSameSource: true))
            {
                return false;
            }

            var conflictingNavigation = navigationName == null
                ? null
                : Metadata.FindNavigation(navigationName);

            if (conflictingNavigation != null
                && conflictingNavigation.ForeignKey != relationshipBuilder.Metadata
                && !CanRemove(conflictingNavigation.ForeignKey, configurationSource, canOverrideSameSource: true))
            {
                return false;
            }

            if (navigationName != null)
            {
                if (!CanAdd(navigationName, isNavigation: true, configurationSource: configurationSource))
                {
                    return false;
                }

                if (pointsToPrincipal)
                {
                    return Entity.Metadata.Navigation.IsCompatible(
                        navigationName, relationshipBuilder.Metadata.DeclaringEntityType, relationshipBuilder.Metadata.PrincipalEntityType, shouldBeCollection: false, shouldThrow: false);
                }

                var canBeUnique = Entity.Metadata.Navigation.IsCompatible(
                    navigationName, Metadata, relationshipBuilder.Metadata.DeclaringEntityType, shouldBeCollection: false, shouldThrow: false);
                var canBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                    navigationName, Metadata, relationshipBuilder.Metadata.DeclaringEntityType, shouldBeCollection: true, shouldThrow: false);

                if (canBeUnique != canBeNonUnique)
                {
                    return relationshipBuilder.CanSetUnique(canBeUnique, configurationSource);
                }

                return canBeUnique;
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
                if (!RemoveProperty(property, configurationSource).HasValue)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                    return false;
                }
            }

            var navigation = Metadata.FindDeclaredNavigation(propertyName);
            if (navigation != null)
            {
                if (Navigation(null, navigation.ForeignKey, navigation.PointsToPrincipal(), configurationSource) == null)
                {
                    _ignoredProperties.Value.Remove(propertyName);
                    return false;
                }

                RemoveForeignKeyIfUnused(navigation.ForeignKey, configurationSource);
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

                // TODO: Find conflicting navigations if different principal end
                var relationshipsToBeRemoved = new HashSet<ForeignKey>();
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: true);
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: false);

                // TODO: Try to remove on derived if this fails
                if (baseRelationshipsToBeRemoved.Any(relationshipToBeRemoved =>
                    !CanRemove(relationshipToBeRemoved, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    Metadata.BaseType = originalBaseType;
                    return null;
                }

                // TODO: Try to remove on base if this fails
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
                        ModelBuilder.ConventionDispatcher.OnBaseEntityTypeSet(
                            ModelBuilder.Entity(affectedDerivedType.Name, ConfigurationSource.Convention),
                            affectedDerivedType.BaseType);
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
                        if ((whereDependent
                             && PropertyListComparer.Instance.Equals(
                                 baseRelationship.ForeignKey.Properties,
                                 relationship.ForeignKey.Properties))
                            || (relationship.NavigationFrom != null
                                && baseRelationship.NavigationFrom?.Name == relationship.NavigationFrom.Name))
                        {
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

        private bool RemoveRelationships(ConfigurationSource configurationSource, IReadOnlyList<ForeignKey> foreignKeys)
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
            Check.NotNull(sourceBuilder, nameof(sourceBuilder));
            Check.NotNull(navigationToTarget, nameof(navigationToTarget));

            var toTargetCanBeUnique = Entity.Metadata.Navigation.IsCompatible(
                navigationToTarget.Name, sourceBuilder.Metadata, Metadata, shouldBeCollection: false, shouldThrow: false);
            var toTargetCanBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                navigationToTarget.Name, sourceBuilder.Metadata, Metadata, shouldBeCollection: true, shouldThrow: false);
            if (!toTargetCanBeUnique
                && !toTargetCanBeNonUnique)
            {
                return null;
            }

            if (navigationToSource == null)
            {
                if (!toTargetCanBeUnique)
                {
                    return Relationship(
                        sourceBuilder,
                        this,
                        null,
                        navigationToTarget.Name,
                        configurationSource: configurationSource,
                        isUnique: false,
                        strictPrincipal: true);
                }

                return Relationship(
                    this,
                    sourceBuilder,
                    navigationToTarget.Name,
                    navigationToDependentName: null,
                    configurationSource: configurationSource,
                    isUnique: null,
                    strictPrincipal: false);
            }

            var toSourceCanBeUnique = Entity.Metadata.Navigation.IsCompatible(
                navigationToSource.Name, Metadata, sourceBuilder.Metadata, shouldBeCollection: false, shouldThrow: false);
            var toSourceCanBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                navigationToSource.Name, Metadata, sourceBuilder.Metadata, shouldBeCollection: true, shouldThrow: false);
            if (!toSourceCanBeUnique
                && !toSourceCanBeNonUnique)
            {
                return null;
            }

            if (!toTargetCanBeUnique)
            {
                if (!toSourceCanBeUnique)
                {
                    // TODO: Support many to many
                    return null;
                }

                return Relationship(
                    sourceBuilder,
                    this,
                    navigationToSource.Name,
                    navigationToTarget.Name,
                    configurationSource: configurationSource,
                    isUnique: false,
                    strictPrincipal: true);
            }

            if (!toSourceCanBeUnique)
            {
                return Relationship(
                    this,
                    sourceBuilder,
                    navigationToTarget.Name,
                    navigationToSource.Name,
                    configurationSource: configurationSource,
                    isUnique: false,
                    strictPrincipal: true);
            }

            if (!toTargetCanBeNonUnique
                && !toSourceCanBeNonUnique)
            {
                return Relationship(
                    sourceBuilder,
                    this,
                    navigationToSource.Name,
                    navigationToTarget.Name,
                    configurationSource: configurationSource,
                    isUnique: true,
                    strictPrincipal: false);
            }

            return Relationship(
                sourceBuilder,
                this,
                navigationToSource.Name,
                navigationToTarget.Name,
                configurationSource: configurationSource,
                isUnique: null,
                strictPrincipal: false)
                ?.Unique(true, ConfigurationSource.Convention);
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
            bool strictPrincipal = true)
            => Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                null,
                null,
                configurationSource,
                isUnique: isUnique,
                strictPrincipal: strictPrincipal);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            ConfigurationSource configurationSource,
            bool? isUnique = null,
            bool? isRequired = null,
            DeleteBehavior? deleteBehavior = null,
            bool strictPrincipal = true,
            [CanBeNull] Func<InternalRelationshipBuilder, InternalRelationshipBuilder> onRelationshipAdding = null,
            bool runConventions = true)
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
                    dependentProperties,
                    principalProperties,
                    configurationSource,
                    isUnique,
                    isRequired,
                    deleteBehavior,
                    strictPrincipal,
                    onRelationshipAdding,
                    runConventions);
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
            var relationshipsToRemove = new List<ForeignKey>();

            if (!string.IsNullOrEmpty(navigationToPrincipalName))
            {
                var navigationToPrincipal = dependentEntityType.FindNavigation(navigationToPrincipalName);
                if (navigationToPrincipal != null)
                {
                    if (navigationToPrincipal.DeclaringEntityType != dependentEntityType)
                    {
                        if (configurationSource == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(Strings.DuplicateNavigation(navigationToPrincipalName, dependentEntityType.DisplayName()));
                        }
                        return null;
                    }

                    if (navigationToPrincipal.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)true : null, isUnique))
                    {
                        return Relationship(navigationToPrincipal, configurationSource, navigationToDependentName);
                    }

                    relationshipsToRemove.Add(navigationToPrincipal.ForeignKey);
                }
            }

            if (!string.IsNullOrEmpty(navigationToDependentName))
            {
                var navigationToDependent = principalEntityType.FindNavigation(navigationToDependentName);
                if (navigationToDependent != null)
                {
                    if (navigationToDependent.DeclaringEntityType != principalEntityType)
                    {
                        if (configurationSource == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(Strings.DuplicateNavigation(navigationToDependentName, principalEntityType.DisplayName()));
                        }
                        return null;
                    }

                    if (navigationToDependent.IsCompatible(principalEntityType, dependentEntityType, strictPrincipal ? (bool?)false : null, isUnique))
                    {
                        return Relationship(navigationToDependent, configurationSource, navigationToPrincipalName);
                    }

                    relationshipsToRemove.Add(navigationToDependent.ForeignKey);
                }
            }

            navigationToPrincipalName = navigationToPrincipalName == "" ? null : navigationToPrincipalName;
            navigationToDependentName = navigationToDependentName == "" ? null : navigationToDependentName;

            if (!InternalRelationshipBuilder.AreCompatible(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                configurationSource))
            {
                return null;
            }

            var foreignKey = dependentProperties == null
                ? null
                : dependentEntityType.FindForeignKey(
                    principalEntityType,
                    null,
                    null,
                    dependentProperties,
                    principalProperties,
                    isUnique);

            var existingForeignKey = foreignKey != null;
            if (!existingForeignKey
                && dependentProperties != null)
            {
                var conflictingForeignKey = dependentEntityType.FindForeignKey(dependentProperties);
                if (conflictingForeignKey != null)
                {
                    relationshipsToRemove.Add(conflictingForeignKey);
                }
            }

            if (!RemoveRelationships(configurationSource, relationshipsToRemove.Distinct().ToList()))
            {
                return null;
            }

            if (!existingForeignKey)
            {
                foreignKey = CreateForeignKey(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    dependentProperties,
                    principalProperties,
                    isUnique,
                    isRequired,
                    deleteBehavior);
                Debug.Assert(foreignKey != null);
            }

            var builder = Relationship(foreignKey, existingForeignKey, configurationSource);

            if (onRelationshipAdding != null)
            {
                builder = onRelationshipAdding(builder);
            }
            else
            {
                if (strictPrincipal)
                {
                    builder = builder.SetPrincipalEndIfCompatible(principalEntityType, configurationSource);
                    Debug.Assert(builder != null);
                }
                if (isUnique.HasValue)
                {
                    builder = builder.SetUniqueIfCompatible(isUnique.Value, configurationSource);
                    Debug.Assert(builder != null);
                }
                if (isRequired.HasValue)
                {
                    builder = builder.SetRequiredIfCompatible(isRequired.Value, configurationSource);
                    Debug.Assert(builder != null);
                }
                if (deleteBehavior.HasValue)
                {
                    builder = builder.SetDeleteBehaviorIfCompatible(deleteBehavior.Value, configurationSource);
                    Debug.Assert(builder != null);
                }
                if (dependentProperties != null)
                {
                    builder = builder.SetForeignKeyIfCompatible(dependentProperties, configurationSource);
                    Debug.Assert(builder != null);
                }
                if (principalProperties != null)
                {
                    builder = builder.SetPrincipalKeyIfCompatible(principalProperties, configurationSource);
                    Debug.Assert(builder != null);
                }
            }

            builder = dependentEntityTypeBuilder
                .Navigation(navigationToPrincipalName, builder.Metadata, pointsToPrincipal: true, configurationSource: configurationSource);
            if (builder == null)
            {
                return null;
            }

            builder = principalEntityTypeBuilder
                .Navigation(navigationToDependentName, builder.Metadata, pointsToPrincipal: builder.Metadata.DeclaringEntityType != Metadata, configurationSource: configurationSource);
            if (builder == null)
            {
                return null;
            }

            if (runConventions)
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
            DeleteBehavior? deleteBehavior)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var dependentType = dependentEntityTypeBuilder.Metadata;

            if (foreignKeyProperties != null
                && dependentType.FindForeignKey(foreignKeyProperties) != null)
            {
                return null;
            }

            var principalBaseEntityTypeBuilder = ModelBuilder.Entity(principalType.RootType().Name, ConfigurationSource.Convention);
            Key principalKey;
            if (principalProperties != null)
            {
                var keyBuilder = principalBaseEntityTypeBuilder.Key(principalProperties, ConfigurationSource.Convention);
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
                foreignKeyProperties = GetOrCreateProperties(foreignKeyProperties, ConfigurationSource.Convention);
                if (principalKey == null
                    || !Entity.Metadata.ForeignKey.AreCompatible(
                        principalKey.Properties,
                        foreignKeyProperties,
                        principalType,
                        dependentType,
                        shouldThrow: false))
                {
                    var principalKeyProperties = new Property[foreignKeyProperties.Count];
                    for (var i = 0; i < foreignKeyProperties.Count; i++)
                    {
                        IProperty foreignKeyProperty = foreignKeyProperties[i];
                        principalKeyProperties[i] = CreateUniqueProperty(
                            foreignKeyProperty.Name,
                            foreignKeyProperty.ClrType,
                            principalBaseEntityTypeBuilder,
                            isRequired: true);
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
                        typeof(int),
                        principalBaseEntityTypeBuilder,
                        isRequired: true);

                    principalKey = principalBaseEntityTypeBuilder.Key(new[] { principalKeyProperty }, ConfigurationSource.Convention).Metadata;
                }

                var fkProperties = new Property[principalKey.Properties.Count];
                for (var i = 0; i < principalKey.Properties.Count; i++)
                {
                    IProperty keyProperty = principalKey.Properties[i];
                    fkProperties[i] = CreateUniqueProperty(
                        baseName + keyProperty.Name,
                        isRequired ?? false ? keyProperty.ClrType : keyProperty.ClrType.MakeNullable(),
                        dependentEntityTypeBuilder,
                        isRequired);
                }

                foreignKeyProperties = fkProperties;
            }

            var newForeignKey = dependentType.AddForeignKey(foreignKeyProperties, principalKey, principalType);
            if (isUnique.HasValue)
            {
                newForeignKey.IsUnique = isUnique.Value;
            }
            if (isRequired.HasValue)
            {
                newForeignKey.IsRequired = isRequired.Value;
            }
            if (deleteBehavior.HasValue)
            {
                newForeignKey.DeleteBehavior = deleteBehavior.Value;
            }

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
                var entityType = entityTypeBuilder.Metadata;
                if (entityType.FindProperty(name) != null
                    || entityType.FindDerivedProperties(new[] { name }).Any()
                    || (entityType.ClrType?.GetRuntimeProperty(name) != null))
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
                    property = ModelBuilder.Entity(property.DeclaringEntityType.Name, configurationSource)
                        .Property(property.Name, configurationSource).Metadata;
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
                var property = Metadata.FindProperty(propertyInfo);
                if (property == null)
                {
                    var propertyBuilder = Property(propertyInfo, configurationSource);
                    if (propertyBuilder == null)
                    {
                        return null;
                    }
                    property = propertyBuilder.Metadata;
                }

                list.Add(property);
            }
            return list;
        }

        public virtual IReadOnlyList<Property> GetOrCreateProperties(
            [NotNull] IEnumerable<Property> properties, ConfigurationSource configurationSource)
            => GetPropertyBuilders(ModelBuilder, properties, configurationSource).Select(p => p.Metadata).ToList();

        public static IEnumerable<InternalPropertyBuilder> GetPropertyBuilders(
            [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] IEnumerable<Property> properties,
            ConfigurationSource configurationSource)
            => properties.Select(property =>
                modelBuilder.Entity(property.DeclaringEntityType.Name, configurationSource)
                    .Property(property.Name, configurationSource));

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
