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

        private readonly LazyRef<Dictionary<string, ConfigurationSource>> _ignoredMembers =
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
                return HasKey(properties, configurationSource);
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

            var keyBuilder = HasKey(properties, configurationSource);
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

        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasKey(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        private InternalKeyBuilder HasKey(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
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
                    .RemoveForeignKey(foreignKey, configurationSource);
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
            if (!IsIgnored(propertyName, configurationSource: configurationSource))
            {
                Unignore(propertyName);

                PropertyBuildersSnapshot detachedProperties = null;
                var existingProperty = Metadata.FindProperty(propertyName);
                if (existingProperty == null)
                {
                    var derivedProperties = Metadata.FindDerivedProperties(propertyName).ToList();
                    detachedProperties = DetachProperties(derivedProperties);
                }

                var builder = _propertyBuilders.GetOrAdd(
                    () => existingProperty,
                    () => Metadata.AddProperty(clrProperty),
                    property => new InternalPropertyBuilder(property, ModelBuilder, existing: existingProperty != null),
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource);

                if (detachedProperties != null)
                {
                    detachedProperties.Attach(this);
                }

                return ConfigureProperty(builder, clrProperty.PropertyType, /*shadowProperty:*/ false, configurationSource);
            }

            return null;
        }

        private InternalPropertyBuilder InternalProperty(string propertyName, Type propertyType, ConfigurationSource configurationSource)
        {
            if (!IsIgnored(propertyName, configurationSource: configurationSource))
            {
                Unignore(propertyName);

                PropertyBuildersSnapshot detachedProperties = null;
                var existingProperty = Metadata.FindProperty(propertyName);
                if (existingProperty == null)
                {
                    var derivedProperties = Metadata.FindDerivedProperties(propertyName).ToList();
                    detachedProperties = DetachProperties(derivedProperties);
                }

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

                if (detachedProperties != null)
                {
                    detachedProperties.Attach(this);
                }

                return ConfigureProperty(builder, propertyType, /*shadowProperty:*/ null, configurationSource);
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

        public virtual bool CanRemoveProperty([NotNull] Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            Check.NotNull(property, nameof(property));
            if (property.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(property.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .CanRemoveProperty(property, configurationSource, canOverrideSameSource);
            }

            return _propertyBuilders.CanRemove(property, configurationSource, canOverrideSameSource);
        }

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && !Metadata.FindNavigationsInHierarchy(navigationName).Any();

        public virtual bool CanAddOrReplaceNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && Metadata.FindNavigationsInHierarchy(navigationName).All(n =>
                   Relationship(n.ForeignKey, true, ConfigurationSource.Convention)
                       .CanSetNavigation(null, n.PointsToPrincipal(), configurationSource));

        private bool IsIgnored(string name, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredMembers.HasValue
                && _ignoredMembers.Value.TryGetValue(name, out ignoredConfigurationSource))
            {
                if (ignoredConfigurationSource.Overrides(configurationSource))
                {
                    return true;
                }
            }

            if (Metadata.BaseType != null)
            {
                return ModelBuilder.Entity(Metadata.BaseType.Name, ConfigurationSource.Convention)
                    .IsIgnored(name, configurationSource);
            }

            return false;
        }

        public virtual bool CanRemove([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(foreignKey, configurationSource);
            }

            return _relationshipBuilders.Value.CanRemove(foreignKey, configurationSource, canOverrideSameSource: true);
        }

        public virtual InternalRelationshipBuilder Navigation(
            [CanBeNull] string navigationName,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource,
            bool runConventions = true)
        {
            var existingNavigation = pointsToPrincipal
                ? foreignKey.DeclaringEntityType == Metadata ? foreignKey.DependentToPrincipal : null
                : foreignKey.PrincipalEntityType == Metadata ? foreignKey.PrincipalToDependent : null;

            var fkOwner = foreignKey.DeclaringEntityType == Metadata
                ? this
                : ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);

            if (navigationName == existingNavigation?.Name)
            {
                var existingBuilder = fkOwner.Relationship(foreignKey, true, configurationSource);
                return pointsToPrincipal
                    ? existingBuilder.DependentToPrincipal(navigationName, configurationSource, runConventions: false)
                    : existingBuilder.PrincipalToDependent(navigationName, configurationSource, runConventions: false);
            }

            var builder = fkOwner.Relationship(foreignKey, true, ConfigurationSource.Convention);
            if (!CanSetNavigation(navigationName, builder, pointsToPrincipal, configurationSource))
            {
                return null;
            }

            if (existingNavigation != null)
            {
                var removedNavigation = existingNavigation.DeclaringEntityType.RemoveNavigation(existingNavigation);
                Debug.Assert(removedNavigation == existingNavigation);

                ModelBuilder.ConventionDispatcher.OnNavigationRemoved(builder, existingNavigation.Name, existingNavigation.PointsToPrincipal());
            }

            if (navigationName != null)
            {
                var conflictingNavigation = Metadata.FindNavigation(navigationName);

                if (conflictingNavigation != null
                    && conflictingNavigation.ForeignKey != foreignKey)
                {
                    var removed = RemoveForeignKey(conflictingNavigation.ForeignKey, configurationSource);
                    Debug.Assert(removed.HasValue);
                }

                Unignore(navigationName);

                if (!pointsToPrincipal)
                {
                    var canBeUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName, Metadata, foreignKey.DeclaringEntityType, shouldBeCollection: false, shouldThrow: false);
                    var canBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName, Metadata, foreignKey.DeclaringEntityType, shouldBeCollection: true, shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        builder = builder.IsUnique(canBeUnique, configurationSource);
                        Debug.Assert(builder != null);
                        pointsToPrincipal = builder.Metadata.DeclaringEntityType != fkOwner.Metadata;
                    }
                }

                var navigation = Metadata.AddNavigation(navigationName, builder.Metadata, pointsToPrincipal);
                builder = fkOwner.Relationship(builder.Metadata, true, configurationSource);
                builder = pointsToPrincipal
                    ? builder.DependentToPrincipal(navigationName, configurationSource, runConventions: false)
                    : builder.PrincipalToDependent(navigationName, configurationSource, runConventions: false);
                if (runConventions)
                {
                    return ModelBuilder.ConventionDispatcher.OnNavigationAdded(builder, navigation);
                }
            }

            return builder;
        }

        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            if (!relationshipBuilder.CanSetNavigation(navigationName, pointsToPrincipal, configurationSource))
            {
                return false;
            }

            var conflictingNavigation = navigationName == null
                ? null
                : Metadata.FindNavigation(navigationName);

            if (conflictingNavigation != null)
            {
                if (conflictingNavigation.ForeignKey == relationshipBuilder.Metadata)
                {
                    if (!relationshipBuilder.CanSetNavigation(null, conflictingNavigation.PointsToPrincipal(), configurationSource))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!ModelBuilder.Entity(conflictingNavigation.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .CanRemove(conflictingNavigation.ForeignKey, configurationSource))
                    {
                        return false;
                    }
                }
            }

            if (navigationName != null)
            {
                if (IsIgnored(navigationName, configurationSource: configurationSource))
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

        public virtual bool Ignore([NotNull] string memberName, ConfigurationSource configurationSource)
        {
            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredMembers.Value.TryGetValue(memberName, out ignoredConfigurationSource))
            {
                _ignoredMembers.Value[memberName] = configurationSource.Max(ignoredConfigurationSource);
                return true;
            }

            if (IsIgnored(memberName, ConfigurationSource.Convention))
            {
                _ignoredMembers.Value[memberName] = configurationSource;
                return true;
            }

            _ignoredMembers.Value[memberName] = configurationSource;

            var property = Metadata.FindPropertiesInHierarchy(memberName).SingleOrDefault();
            if (property != null
                && !ModelBuilder.Entity(property.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .RemoveProperty(property, configurationSource).HasValue)
            {
                _ignoredMembers.Value.Remove(memberName);
                return false;
            }

            var navigation = Metadata.FindNavigationsInHierarchy(memberName).SingleOrDefault();
            if (navigation != null)
            {
                if (ModelBuilder.Entity(navigation.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .Navigation(null, navigation.ForeignKey, navigation.PointsToPrincipal(), configurationSource) == null)
                {
                    _ignoredMembers.Value.Remove(memberName);
                    return false;
                }

                RemoveForeignKeyIfUnused(navigation.ForeignKey, configurationSource);
            }

            ModelBuilder.ConventionDispatcher.OnEntityTypeMemberIgnored(this, memberName);
            return true;
        }

        private void Unignore(string memberName)
        {
            var entityType = Metadata;
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                Unignore(memberName, derivedType);
            }

            while (entityType != null)
            {
                Unignore(memberName, entityType);
                entityType = entityType.BaseType;
            }
        }

        private void Unignore(string memberName, EntityType entityType)
            => ModelBuilder?.Entity(entityType.Name, ConfigurationSource.Convention)
                ?._ignoredMembers?.Value.Remove(memberName);

        public virtual InternalEntityTypeBuilder HasBaseType([CanBeNull] Type baseEntityType, ConfigurationSource configurationSource)
        {
            if (baseEntityType == null)
            {
                return HasBaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityType, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        public virtual InternalEntityTypeBuilder HasBaseType([CanBeNull] string baseEntityTypeName, ConfigurationSource configurationSource)
        {
            if (baseEntityTypeName == null)
            {
                return HasBaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityTypeName, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        public virtual InternalEntityTypeBuilder HasBaseType([CanBeNull] EntityType baseEntityType, ConfigurationSource configurationSource)
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
            PropertyBuildersSnapshot detachedProperties = null;
            var baseRelationshipsToBeRemoved = new HashSet<ForeignKey>();

            if (baseEntityType != null)
            {
                if (Metadata.GetKeys().Any(k => !_keyBuilders.CanRemove(k, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    return null;
                }

                // TODO: Find conflicting navigations if different principal end
                var relationshipsToBeRemoved = new HashSet<ForeignKey>();
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: true);
                FindConflictingRelationships(baseEntityType, baseRelationshipsToBeRemoved, relationshipsToBeRemoved, whereDependent: false);

                // TODO: Try to remove on derived if this fails
                if (baseRelationshipsToBeRemoved.Any(relationshipToBeRemoved =>
                    !CanRemove(relationshipToBeRemoved, configurationSource)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    return null;
                }

                // TODO: Try to remove on base if this fails
                if (relationshipsToBeRemoved.Any(relationshipToBeRemoved =>
                    !CanRemove(relationshipToBeRemoved, configurationSource)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    return null;
                }

                foreach (var relationshipToBeRemoved in baseRelationshipsToBeRemoved)
                {
                    var removedConfigurationSource = RemoveForeignKey(relationshipToBeRemoved, configurationSource);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
                {
                    var removedConfigurationSource = RemoveForeignKey(relationshipToBeRemoved, configurationSource);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                foreach (var key in Metadata.GetKeys().ToList())
                {
                    foreach (var referencingForeignKey in ModelBuilder.Metadata.FindReferencingForeignKeys(key).ToList())
                    {
                        detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                    }
                }

                // TODO: Detach and reattach keys
                // Issue #2611
                foreach (var key in Metadata.GetKeys().ToList())
                {
                    var removedConfigurationSource = RemoveKey(key, configurationSource);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                var duplicatedProperties = baseEntityType.Properties
                    .Select(p => Metadata.FindDeclaredProperty(p.Name))
                    .Where(p => p != null)
                    .ToList();

                detachedProperties = DetachProperties(duplicatedProperties);

                ModelBuilder.Entity(baseEntityType.Name, configurationSource);
            }

            _baseTypeConfigurationSource = configurationSource;
            var originalBaseType = Metadata.BaseType;
            Metadata.BaseType = baseEntityType;

            if (detachedProperties != null)
            {
                detachedProperties.Attach(this);
            }

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

        private PropertyBuildersSnapshot DetachProperties(
            IReadOnlyList<Property> propertiesToDetach)
        {
            var detachedRelationships = new List<RelationshipBuilderSnapshot>();
            foreach (var propertyToDetach in propertiesToDetach)
            {
                foreach (var relationship in propertyToDetach.FindContainingForeignKeysInHierarchy().ToList())
                {
                    detachedRelationships.Add(DetachRelationship(relationship));
                }
            }

            // TODO: Detach and reattach keys and the referencing FKs
            // Issue #2611

            // TODO: Detach and reattach indexes
            // Issue #2514

            var detachedProperties = new List<Tuple<InternalPropertyBuilder, ConfigurationSource>>();
            foreach (var propertyToDetach in propertiesToDetach)
            {
                var property = propertyToDetach.DeclaringEntityType.FindDeclaredProperty(propertyToDetach.Name);
                if (property != null)
                {
                    var entityTypeBuilder = ModelBuilder
                        .Entity(propertyToDetach.DeclaringEntityType.Name, ConfigurationSource.Convention);
                    var propertyBuilder = entityTypeBuilder.Property(propertyToDetach.Name, ConfigurationSource.Convention);
                    var removedConfigurationSource = entityTypeBuilder
                        .RemoveProperty(propertyToDetach, ConfigurationSource.Explicit);
                    detachedProperties.Add(Tuple.Create(propertyBuilder, removedConfigurationSource.Value));
                }
            }

            return new PropertyBuildersSnapshot(detachedProperties, detachedRelationships);
        }

        private class PropertyBuildersSnapshot
        {
            public PropertyBuildersSnapshot(
                IReadOnlyList<Tuple<InternalPropertyBuilder, ConfigurationSource>> properties,
                IReadOnlyList<RelationshipBuilderSnapshot> relationships)
            {
                Properties = properties;
                Relationships = relationships;
            }

            private IReadOnlyList<Tuple<InternalPropertyBuilder, ConfigurationSource>> Properties { get; }
            private IReadOnlyList<RelationshipBuilderSnapshot> Relationships { get; }

            public void Attach(InternalEntityTypeBuilder entityTypeBuilder)
            {
                foreach (var propertyTuple in Properties)
                {
                    propertyTuple.Item1.Attach(entityTypeBuilder, propertyTuple.Item2);
                }

                foreach (var detachedRelationship in Relationships)
                {
                    detachedRelationship.Attach();
                }
            }
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

        private ConfigurationSource? RemoveProperty(
            Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
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

            var detachedRelationships = property.FindContainingForeignKeysInHierarchy().ToList()
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

        public virtual InternalRelationshipBuilder HasForeignKey(
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

            return HasForeignKey(principalType, GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type principalClrType, [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            ConfigurationSource configurationSource)
        {
            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : HasForeignKey(principalType, GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        private InternalRelationshipBuilder HasForeignKey(InternalEntityTypeBuilder principalType, IReadOnlyList<Property> dependentProperties, ConfigurationSource configurationSource)
            => dependentProperties == null
                ? null
                : Relationship(principalType, this, null, null, dependentProperties, null, configurationSource);

        private RelationshipBuilderSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
        {
            var navigationToPrincipalName = foreignKey.DependentToPrincipal?.Name;
            var navigationToDependentName = foreignKey.PrincipalToDependent?.Name;
            var relationship = Relationship(foreignKey, true, ConfigurationSource.Convention);
            var relationshipConfigurationSource = RemoveForeignKey(foreignKey, ConfigurationSource.Explicit);
            Debug.Assert(relationshipConfigurationSource != null);

            return new RelationshipBuilderSnapshot(relationship, navigationToPrincipalName, navigationToDependentName, relationshipConfigurationSource.Value);
        }

        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .RemoveForeignKey(foreignKey, configurationSource);
            }

            var removedConfigurationSource = _relationshipBuilders.Value.Remove(foreignKey, configurationSource);
            if (removedConfigurationSource == null)
            {
                return null;
            }

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
            ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention)
                ?.RemoveKeyIfUnused(foreignKey.PrincipalKey);

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
                RemoveForeignKey(foreignKey, configurationSource);
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

        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        private InternalIndexBuilder HasIndex(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var existingIndex = Metadata.FindIndex(properties);
            if (existingIndex != null
                && existingIndex.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(existingIndex.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .HasIndex(properties, configurationSource);
            }

            // TODO: Lift indexes from derived types
            // Issue #2514

            return _indexBuilders.Value.GetOrAdd(
                () => existingIndex,
                () => Metadata.AddIndex(properties),
                index => new InternalIndexBuilder(index, ModelBuilder),
                configurationSource);
        }

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
                ?.IsUnique(true, ConfigurationSource.Convention);
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
            Debug.Assert(strictPrincipal
                         || (dependentProperties == null
                             && principalProperties == null));

            var dependentEntityType = dependentEntityTypeBuilder.Metadata;
            var principalEntityType = principalEntityTypeBuilder.Metadata;

            if (!InternalRelationshipBuilder.AreCompatible(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                ModelBuilder,
                configurationSource))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(navigationToPrincipalName)
                && dependentEntityTypeBuilder.IsIgnored(navigationToPrincipalName, configurationSource))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(navigationToDependentName)
                && principalEntityTypeBuilder.IsIgnored(navigationToDependentName, configurationSource))
            {
                return null;
            }

            var existingRelationships = new List<InternalRelationshipBuilder>();
            if (!string.IsNullOrEmpty(navigationToPrincipalName))
            {
                existingRelationships.AddRange(dependentEntityType
                    .FindNavigationsInHierarchy(navigationToPrincipalName)
                    .Select(n => Relationship(n.ForeignKey, true, ConfigurationSource.Convention)));
            }

            if (!string.IsNullOrEmpty(navigationToDependentName))
            {
                existingRelationships.AddRange(principalEntityType
                    .FindNavigationsInHierarchy(navigationToDependentName)
                    .Select(n => Relationship(n.ForeignKey, true, ConfigurationSource.Convention)));
            }

            if (dependentProperties != null)
            {
                existingRelationships.AddRange(dependentEntityType
                    .FindForeignKeysInHierarchy(dependentProperties)
                    .Select(fk => Relationship(fk, true, ConfigurationSource.Convention)));
            }

            // TODO: Try to use the least derived ones first
            var existingInverted = false;
            existingRelationships = existingRelationships.Distinct().ToList();
            var relationshipBuilder = existingRelationships.FirstOrDefault(r =>
                r.CanSet(principalEntityType,
                    dependentEntityType,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    dependentProperties,
                    principalProperties,
                    isUnique,
                    isRequired,
                    deleteBehavior,
                    strictPrincipal,
                    configurationSource,
                    out existingInverted));

            var conflictingForeignKeys = existingRelationships.Where(r => r != relationshipBuilder).Select(r => r.Metadata).ToList();
            if (conflictingForeignKeys.Any(foreignKey => !CanRemove(foreignKey, configurationSource)))
            {
                return null;
            }

            foreach (var foreignKey in conflictingForeignKeys)
            {
                var removed = RemoveForeignKey(foreignKey, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            if (relationshipBuilder == null)
            {
                var foreignKey = dependentEntityTypeBuilder.CreateForeignKey(
                    principalEntityTypeBuilder,
                    navigationToPrincipalName,
                    dependentProperties,
                    principalProperties,
                    isUnique,
                    isRequired,
                    deleteBehavior);
                relationshipBuilder = Relationship(foreignKey, false, configurationSource);
            }
            else if (existingInverted)
            {
                if (strictPrincipal)
                {
                    relationshipBuilder = relationshipBuilder.Invert(configurationSource);
                }
                else
                {
                    var entityTypeBuilder = principalEntityTypeBuilder;
                    principalEntityTypeBuilder = dependentEntityTypeBuilder;
                    dependentEntityTypeBuilder = entityTypeBuilder;

                    var navigationName = navigationToPrincipalName;
                    navigationToPrincipalName = navigationToDependentName;
                    navigationToDependentName = navigationName;

                    var properties = dependentProperties;
                    dependentProperties = principalProperties;
                    principalProperties = properties;
                }
            }

            if (onRelationshipAdding == null)
            {
                relationshipBuilder = relationshipBuilder.DependentType(dependentEntityTypeBuilder.Metadata, configurationSource);
                relationshipBuilder = relationshipBuilder.PrincipalType(principalEntityTypeBuilder.Metadata, configurationSource);

                if (strictPrincipal)
                {
                    relationshipBuilder = relationshipBuilder.PrincipalEnd(
                        relationshipBuilder.Metadata.PrincipalEntityType, configurationSource, runConventions: false);
                }
                if (dependentProperties != null)
                {
                    relationshipBuilder = relationshipBuilder.HasForeignKey(
                        dependentProperties, configurationSource, runConventions: false);
                }
                if (principalProperties != null)
                {
                    relationshipBuilder = relationshipBuilder.HasPrincipalKey(
                        principalProperties, configurationSource, runConventions: false);
                }
                if (isUnique.HasValue)
                {
                    relationshipBuilder = relationshipBuilder.IsUnique(
                        isUnique.Value, configurationSource, runConventions: false);
                }
                if (isRequired.HasValue)
                {
                    relationshipBuilder = relationshipBuilder.IsRequired(
                        isRequired.Value, configurationSource, runConventions: false);
                }
                if (deleteBehavior.HasValue)
                {
                    relationshipBuilder = relationshipBuilder.DeleteBehavior(
                        deleteBehavior.Value, configurationSource, runConventions: false);
                }
                if (navigationToPrincipalName != null)
                {
                    relationshipBuilder = relationshipBuilder.DependentToPrincipal(
                        navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                        configurationSource, runConventions: false);
                }
                if (navigationToDependentName != null)
                {
                    relationshipBuilder = relationshipBuilder.PrincipalToDependent(
                        navigationToDependentName == "" ? null : navigationToDependentName,
                        configurationSource, runConventions: false);
                }
            }
            else
            {
                relationshipBuilder = onRelationshipAdding(relationshipBuilder);
            }

            if (runConventions)
            {
                relationshipBuilder = ModelBuilder.ConventionDispatcher.OnForeignKeyAdded(relationshipBuilder);
                if (relationshipBuilder == null)
                {
                    return null;
                }

                if (relationshipBuilder.Metadata.DependentToPrincipal != null)
                {
                    relationshipBuilder = ModelBuilder.ConventionDispatcher.OnNavigationAdded(
                        relationshipBuilder, relationshipBuilder.Metadata.DependentToPrincipal);
                    if (relationshipBuilder == null)
                    {
                        return null;
                    }
                }

                if (relationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    relationshipBuilder = ModelBuilder.ConventionDispatcher.OnNavigationAdded(relationshipBuilder, relationshipBuilder.Metadata.PrincipalToDependent);
                }
            }

            return relationshipBuilder;
        }

        private ForeignKey CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            DeleteBehavior? deleteBehavior)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var dependentType = Metadata;

            Debug.Assert(dependentProperties == null
                         || dependentType.FindForeignKey(dependentProperties) == null);

            var principalBaseEntityTypeBuilder = ModelBuilder.Entity(principalType.RootType().Name, ConfigurationSource.Convention);
            Key principalKey;
            if (principalProperties != null)
            {
                var keyBuilder = principalBaseEntityTypeBuilder.HasKey(principalProperties, ConfigurationSource.Convention);
                principalKey = keyBuilder.Metadata;
            }
            else
            {
                principalKey = principalType.FindPrimaryKey();
            }

            if (dependentProperties != null)
            {
                dependentProperties = GetOrCreateProperties(dependentProperties, ConfigurationSource.Convention);
                if (principalKey == null
                    || !Entity.Metadata.ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalType,
                        dependentType,
                        shouldThrow: false))
                {
                    var principalKeyProperties = new Property[dependentProperties.Count];
                    for (var i = 0; i < dependentProperties.Count; i++)
                    {
                        IProperty foreignKeyProperty = dependentProperties[i];
                        principalKeyProperties[i] = CreateUniqueProperty(
                            foreignKeyProperty.Name,
                            foreignKeyProperty.ClrType,
                            principalBaseEntityTypeBuilder,
                            isRequired: true);
                    }

                    var keyBuilder = principalBaseEntityTypeBuilder.HasKey(principalKeyProperties, ConfigurationSource.Convention);

                    principalKey = keyBuilder.Metadata;
                }
            }
            else
            {
                if (principalKey == null)
                {
                    var principalKeyProperty = CreateUniqueProperty(
                        "TempId",
                        typeof(int),
                        principalBaseEntityTypeBuilder,
                        isRequired: true);

                    principalKey = principalBaseEntityTypeBuilder.HasKey(new[] { principalKeyProperty }, ConfigurationSource.Convention).Metadata;
                }

                var baseName = (string.IsNullOrEmpty(navigationToPrincipal) ? principalType.DisplayName() : navigationToPrincipal);
                var fkProperties = new Property[principalKey.Properties.Count];
                for (var i = 0; i < principalKey.Properties.Count; i++)
                {
                    IProperty keyProperty = principalKey.Properties[i];
                    fkProperties[i] = CreateUniqueProperty(
                        baseName + keyProperty.Name,
                        isRequired ?? false ? keyProperty.ClrType : keyProperty.ClrType.MakeNullable(),
                        this,
                        isRequired);
                }

                dependentProperties = fkProperties;
            }

            var newForeignKey = dependentType.AddForeignKey(dependentProperties, principalKey, principalType);
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

            foreach (var foreignKeyProperty in dependentProperties)
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
                if (entityType.FindPropertiesInHierarchy(name).Any()
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
                        propertyBuilder.IsRequired(isRequired.Value, ConfigurationSource.Convention);
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
                        throw new ModelItemNotFoundException(CoreStrings.PropertyNotFound(propertyName, Metadata.Name));
                    }

                    var clrProperty = Metadata.ClrType.GetPropertiesInHierarchy(propertyName).FirstOrDefault();
                    if (clrProperty == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NoClrProperty(propertyName, Metadata.Name));
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
            [CanBeNull] IEnumerable<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = new List<Property>();
            foreach (var builder in properties.Select(property => Property(property.Name, configurationSource)))
            {
                if (builder == null)
                {
                    return null;
                }
                actualProperties.Add(builder.Metadata);
            }
            return actualProperties;
        }

        public static IEnumerable<InternalPropertyBuilder> GetPropertyBuilders(
            [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] IEnumerable<Property> properties,
            ConfigurationSource configurationSource)
            => properties.Select(property =>
                modelBuilder.Entity(property.DeclaringEntityType.Name, configurationSource)
                    ?.Property(property.Name, configurationSource));

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
                => Relationship.Attach(
                    NavigationToPrincipalName,
                    NavigationToDependentName,
                    RelationshipConfigurationSource);
        }
    }
}
