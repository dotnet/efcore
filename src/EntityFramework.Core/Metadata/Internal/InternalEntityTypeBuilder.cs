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
                    var derivedProperties = Metadata.FindDerivedProperties(propertyName);
                    detachedProperties = DetachProperties(derivedProperties);
                }
                else if (existingProperty.DeclaringEntityType != Metadata)
                {
                    return ModelBuilder.Entity(existingProperty.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .InternalProperty(clrProperty, configurationSource);
                }

                var builder = _propertyBuilders.GetOrAdd(
                    () => existingProperty,
                    () => Metadata.AddProperty(clrProperty),
                    property => new InternalPropertyBuilder(property, ModelBuilder, existing: existingProperty != null),
                    ModelBuilder.ConventionDispatcher.OnPropertyAdded,
                    configurationSource);

                detachedProperties?.Attach(this);

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
                    var derivedProperties = Metadata.FindDerivedProperties(propertyName);
                    detachedProperties = DetachProperties(derivedProperties);
                }
                else if(existingProperty.DeclaringEntityType != Metadata)
                {
                    return ModelBuilder.Entity(existingProperty.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .InternalProperty(propertyName, propertyType, configurationSource);
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

                detachedProperties?.Attach(this);

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
                   ModelBuilder.Entity(n.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                       .Relationship(n.ForeignKey, ConfigurationSource.Convention)
                       .CanSetNavigation(null, n.PointsToPrincipal(), configurationSource));

        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

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
                var ownerBuilder = ModelBuilder.Entity(navigation.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var removedConfigurationSource = ownerBuilder.RemoveForeignKey(navigation.ForeignKey, configurationSource);

                if (removedConfigurationSource == null)
                {
                    _ignoredMembers.Value.Remove(memberName);
                    return false;
                }
            }

            ModelBuilder.ConventionDispatcher.OnEntityTypeMemberIgnored(this, memberName);
            return true;
        }

        public virtual void Unignore([NotNull] string memberName)
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

            IReadOnlyList<RelationshipSnapshot> relationshipsToBeRemoved = new List<RelationshipSnapshot>();
            if (baseEntityType != null)
            {
                if (Metadata.GetKeys().Any(k => !_keyBuilders.CanRemove(k, configurationSource, canOverrideSameSource: true)))
                {
                    Debug.Assert(configurationSource != ConfigurationSource.Explicit);

                    return null;
                }

                relationshipsToBeRemoved = FindConflictingRelationships(baseEntityType, configurationSource);
                if (relationshipsToBeRemoved == null)
                {
                    return null;
                }

                foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
                {
                    var removedConfigurationSource = RemoveForeignKey(relationshipToBeRemoved.ForeignKey, configurationSource, runConventions: false);
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
                    .Where(p => p != null);

                // TODO: Detach base property if shadow and derived non-shadow
                detachedProperties = DetachProperties(duplicatedProperties);

                ModelBuilder.Entity(baseEntityType.Name, configurationSource);
            }

            _baseTypeConfigurationSource = configurationSource;
            var originalBaseType = Metadata.BaseType;
            Metadata.BaseType = baseEntityType;

            detachedProperties?.Attach(this);

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
            {
                var dependentEntityType = ModelBuilder.Entity(
                    relationshipToBeRemoved.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var principalEntityType = ModelBuilder.Entity(
                    relationshipToBeRemoved.ForeignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);
                var source = relationshipToBeRemoved.IsDependent ? dependentEntityType : principalEntityType;
                var target = relationshipToBeRemoved.IsDependent ? principalEntityType : dependentEntityType;

                if (relationshipToBeRemoved.NavigationFrom != null)
                {
                    ModelBuilder.ConventionDispatcher.OnNavigationRemoved(source, target, relationshipToBeRemoved.NavigationFrom.Name);
                }
                if (relationshipToBeRemoved.NavigationTo != null)
                {
                    ModelBuilder.ConventionDispatcher.OnNavigationRemoved(target, source, relationshipToBeRemoved.NavigationTo.Name);
                }

                ModelBuilder.ConventionDispatcher.OnForeignKeyRemoved(dependentEntityType, relationshipToBeRemoved.ForeignKey);
            }

            ModelBuilder.ConventionDispatcher.OnBaseEntityTypeSet(this, originalBaseType);

            return this;
        }

        private PropertyBuildersSnapshot DetachProperties(IEnumerable<Property> propertiesToDetach)
        {
            var propertiesToDetachList = propertiesToDetach.ToList();
            if (propertiesToDetachList.Count == 0)
            {
                return null;
            }

            var detachedRelationships = new List<RelationshipBuilderSnapshot>();
            foreach (var propertyToDetach in propertiesToDetachList)
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
            foreach (var propertyToDetach in propertiesToDetachList)
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

        private IReadOnlyList<RelationshipSnapshot> FindConflictingRelationships(
            EntityType baseEntityType,
            ConfigurationSource configurationSource)
        {
            var relationshipsToBeRemoved = new List<RelationshipSnapshot>();
            var baseRelationshipsByTargetType = GroupRelationshipsByTargetType(baseEntityType);
            var relationshipsByTargetType = GroupRelationshipsByTargetType(Metadata);

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
                        if ((baseRelationship.IsDependent
                             && relationship.IsDependent
                             && PropertyListComparer.Instance.Equals(
                                 baseRelationship.ForeignKey.Properties,
                                 relationship.ForeignKey.Properties))
                            || (relationship.NavigationFrom != null
                                && baseRelationship.NavigationFrom?.Name == relationship.NavigationFrom.Name))
                        {
                            if (CanRemove(relationship.ForeignKey, configurationSource))
                            {
                                relationshipsToBeRemoved.Add(relationship);
                            }
                            else if (CanRemove(baseRelationship.ForeignKey, configurationSource))
                            {
                                relationshipsToBeRemoved.Add(baseRelationship);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            return relationshipsToBeRemoved;
        }

        private Dictionary<EntityType, List<RelationshipSnapshot>> GroupRelationshipsByTargetType(EntityType entityType)
            => entityType.GetForeignKeys()
                .Select(foreignKey =>
                    new RelationshipSnapshot(foreignKey,
                        foreignKey.DependentToPrincipal,
                        foreignKey.PrincipalToDependent,
                        isDependent: true))
                .Concat(entityType.FindReferencingForeignKeys().Where(foreignKey => !foreignKey.IsSelfReferencing())
                    .Select(foreignKey =>
                        new RelationshipSnapshot(foreignKey,
                            foreignKey.PrincipalToDependent,
                            foreignKey.DependentToPrincipal,
                            isDependent: false)))
                .GroupBy(relationship => relationship.IsDependent
                    ? relationship.ForeignKey.PrincipalEntityType
                    : relationship.ForeignKey.DeclaringEntityType)
                .ToDictionary(g => g.Key, g => g.ToList());

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

        private RelationshipBuilderSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
        {
            var navigationToPrincipalName = foreignKey.DependentToPrincipal?.Name;
            var navigationToDependentName = foreignKey.PrincipalToDependent?.Name;
            var relationship = ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                .Relationship(foreignKey, ConfigurationSource.Convention);
            var relationshipConfigurationSource = RemoveForeignKey(foreignKey, ConfigurationSource.Explicit, runConventions: false);
            Debug.Assert(relationshipConfigurationSource != null);

            return new RelationshipBuilderSnapshot(relationship, navigationToPrincipalName, navigationToDependentName, relationshipConfigurationSource.Value);
        }

        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
            => RemoveForeignKey(foreignKey, configurationSource, runConventions: true);

        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource, bool runConventions)
        {
            if (foreignKey.DeclaringEntityType != Metadata)
            {
                return ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .RemoveForeignKey(foreignKey, configurationSource, runConventions);
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

            RemoveShadowPropertiesIfUnused(foreignKey.Properties);
            ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention)
                ?.RemoveKeyIfUnused(foreignKey.PrincipalKey);

            if (runConventions)
            {
                var principalEntityBuilder = ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);
                if (principalEntityBuilder != null)
                {
                    if (navigationToPrincipal != null)
                    {
                        ModelBuilder.ConventionDispatcher.OnNavigationRemoved(this, principalEntityBuilder, navigationToPrincipal.Name);
                    }

                    if (navigationToDependent != null)
                    {
                        ModelBuilder.ConventionDispatcher.OnNavigationRemoved(principalEntityBuilder, this, navigationToDependent.Name);
                    }
                }

                ModelBuilder.ConventionDispatcher.OnForeignKeyRemoved(this, foreignKey);
            }

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

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalType = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(principalType, GetOrCreateProperties(propertyNames, configurationSource), configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(principalType, GetOrCreateProperties(clrProperties, configurationSource), configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(principalEntityTypeBuilder,
                GetOrCreateProperties(dependentProperties, configurationSource),
                configurationSource);

        private InternalRelationshipBuilder HasForeignKeyInternal(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
        {
            if (dependentProperties == null)
            {
                return null;
            }

            InternalRelationshipBuilder relationship;
            InternalRelationshipBuilder newRelationship = null;
            var existingForeignKey = Metadata.FindForeignKeysInHierarchy(dependentProperties).FirstOrDefault();
            if (existingForeignKey == null
                || existingForeignKey.DeclaringEntityType != Metadata)
            {
                newRelationship = Relationship(principalEntityTypeBuilder, configurationSource);
                relationship = newRelationship;
            }
            else
            {
                relationship = Relationship(existingForeignKey, configurationSource);
            }

            relationship = relationship.HasForeignKey(dependentProperties, configurationSource);
            if (relationship == null
                && newRelationship != null)
            {
                RemoveForeignKey(newRelationship.Metadata, configurationSource);
            }

            return relationship;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Debug.Assert(Metadata.GetDeclaredForeignKeys().Contains(foreignKey));

            return _relationshipBuilders.Value.GetOrAdd(
                () => foreignKey,
                () => foreignKey,
                fk => new InternalRelationshipBuilder(
                    foreignKey, ModelBuilder, ConfigurationSource.Explicit),
                configurationSource);
        }

        public virtual IReadOnlyList<InternalRelationshipBuilder> GetRelationshipBuilders(
            [NotNull] EntityType principalEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties)
        {
            var existingRelationships = new List<InternalRelationshipBuilder>();
            if (!string.IsNullOrEmpty(navigationToPrincipalName))
            {
                existingRelationships.AddRange(Metadata
                    .FindNavigationsInHierarchy(navigationToPrincipalName)
                    .Select(n => ModelBuilder.Entity(n.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .Relationship(n.ForeignKey, ConfigurationSource.Convention)));
            }

            if (!string.IsNullOrEmpty(navigationToDependentName))
            {
                existingRelationships.AddRange(principalEntityType
                    .FindNavigationsInHierarchy(navigationToDependentName)
                    .Select(n => ModelBuilder.Entity(n.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .Relationship(n.ForeignKey, ConfigurationSource.Convention)));
            }

            if (dependentProperties != null)
            {
                existingRelationships.AddRange(Metadata
                    .FindForeignKeysInHierarchy(dependentProperties)
                    .Select(fk => ModelBuilder.Entity(fk.DeclaringEntityType.Name, ConfigurationSource.Convention)
                        .Relationship(fk, ConfigurationSource.Convention)));
            }

            return existingRelationships;
        }

        private InternalRelationshipBuilder CreateRelationshipBuilder(
            EntityType principalType,
            IReadOnlyList<Property> dependentProperties,
            Key principalKey,
            ConfigurationSource configurationSource,
            bool runConventions)
            => _relationshipBuilders.Value.GetOrAdd(
                () => null,
                () => Metadata.AddForeignKey(dependentProperties, principalKey, principalType),
                fk => new InternalRelationshipBuilder(fk, ModelBuilder, null),
                runConventions
                    ? ModelBuilder.ConventionDispatcher.OnForeignKeyAdded
                    : (Func<InternalRelationshipBuilder, InternalRelationshipBuilder>)null,
                configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationToTargetName,
            [CanBeNull] string inverseNavigationName,
            ConfigurationSource configurationSource)
        {
            PropertyInfo navigationToTarget = null;
            if (navigationToTargetName != null)
            {
                if (!Navigation.IsCompatible(navigationToTargetName, Metadata, targetEntityTypeBuilder.Metadata, shouldBeCollection: null, shouldThrow: true))
                {
                    return null;
                }
                navigationToTarget = Metadata.ClrType.GetPropertiesInHierarchy(navigationToTargetName).First();
            }

            PropertyInfo inverseNavigation = null;
            if (inverseNavigationName != null)
            {
                if (!Navigation.IsCompatible(inverseNavigationName, targetEntityTypeBuilder.Metadata, Metadata, shouldBeCollection: null, shouldThrow: true))
                {
                    return null;
                }
                inverseNavigation = targetEntityTypeBuilder.Metadata.ClrType.GetPropertiesInHierarchy(inverseNavigationName).First();
            }

            return Relationship(targetEntityTypeBuilder, navigationToTarget, inverseNavigation, configurationSource);
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] PropertyInfo navigationToTarget,
            [CanBeNull] PropertyInfo inverseNavigation,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder));

            if (inverseNavigation == null)
            {
                if (navigationToTarget == null)
                {
                    return Relationship(targetEntityTypeBuilder, configurationSource)
                        .Navigations(null, null, configurationSource);
                }

                return targetEntityTypeBuilder.Relationship(
                    this,
                    inverseNavigation,
                    navigationToTarget,
                    configurationSource);
            }

            var toTargetCanBeUnique = Navigation.IsCompatible(
                inverseNavigation.Name, targetEntityTypeBuilder.Metadata, Metadata, shouldBeCollection: false, shouldThrow: false);
            var toTargetCanBeNonUnique = Navigation.IsCompatible(
                inverseNavigation.Name, targetEntityTypeBuilder.Metadata, Metadata, shouldBeCollection: true, shouldThrow: false);
            if (!toTargetCanBeUnique
                && !toTargetCanBeNonUnique)
            {
                return null;
            }

            if (navigationToTarget == null)
            {
                if (!toTargetCanBeUnique)
                {
                    return Navigations(
                        Relationship(targetEntityTypeBuilder, configurationSource)
                            .PrincipalEntityType(targetEntityTypeBuilder, configurationSource)
                            .IsUnique(false, configurationSource),
                        null,
                        inverseNavigation.Name,
                        configurationSource);
                }

                return Navigations(
                    targetEntityTypeBuilder.Relationship(this, configurationSource),
                    inverseNavigation.Name,
                    null,
                    configurationSource);
            }

            var toSourceCanBeUnique = Navigation.IsCompatible(
                navigationToTarget.Name, Metadata, targetEntityTypeBuilder.Metadata, shouldBeCollection: false, shouldThrow: false);
            var toSourceCanBeNonUnique = Navigation.IsCompatible(
                navigationToTarget.Name, Metadata, targetEntityTypeBuilder.Metadata, shouldBeCollection: true, shouldThrow: false);
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

                return Navigations(
                    Relationship(targetEntityTypeBuilder, configurationSource)
                        .PrincipalEntityType(targetEntityTypeBuilder, configurationSource)
                        .IsUnique(false, configurationSource),
                    navigationToTarget.Name,
                    inverseNavigation.Name,
                    configurationSource);
            }

            if (!toSourceCanBeUnique)
            {
                return Navigations(
                    targetEntityTypeBuilder.Relationship(this, configurationSource)
                        .PrincipalEntityType(this, configurationSource)
                        .IsUnique(false, configurationSource),
                    inverseNavigation.Name,
                    navigationToTarget.Name,
                    configurationSource);
            }

            var relationship = Relationship(targetEntityTypeBuilder, configurationSource);
            if (!toTargetCanBeNonUnique
                && !toSourceCanBeNonUnique)
            {
                relationship = relationship.IsUnique(true, configurationSource);
            }
            else
            {
                relationship = relationship.IsUnique(true, ConfigurationSource.Convention);
            }

            return Navigations(relationship, navigationToTarget.Name, inverseNavigation.Name, configurationSource);
        }

        private InternalRelationshipBuilder Navigations(
            InternalRelationshipBuilder relationship,
            string navigationToPrincipalName,
            string navigationToDependentName,
            ConfigurationSource configurationSource)
        {
            var relationshipWithNavigations = relationship.Navigations(navigationToPrincipalName, navigationToDependentName, configurationSource);
            if (relationshipWithNavigations == null)
            {
                RemoveForeignKey(relationship.Metadata, configurationSource);
            }

            return relationshipWithNavigations;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
            => Relationship(ModelBuilder.Entity(principalEntityType.Name, configurationSource), configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            ConfigurationSource configurationSource)
            => CreateForeignKey(
                principalEntityTypeBuilder,
                null,
                null,
                null,
                null,
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            [CanBeNull] string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            Debug.Assert(dependentProperties == null
                         || Metadata.FindForeignKey(dependentProperties) == null);

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
                    || !ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalType,
                        Metadata,
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

                var baseName = (string.IsNullOrEmpty(navigationToPrincipalName) ? principalType.DisplayName() : navigationToPrincipalName);
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

            ModelBuilder.Entity(principalType.Name, configurationSource);
            return CreateRelationshipBuilder(principalType, dependentProperties, principalKey, configurationSource, runConventions);
        }

        private Property CreateUniqueProperty(string baseName, Type propertyType, InternalEntityTypeBuilder entityTypeBuilder, bool? isRequired = null)
        {
            var index = -1;
            while (true)
            {
                var name = baseName + (++index > 0 ? index.ToString() : "");
                var entityType = entityTypeBuilder.Metadata;
                if (entityType.FindPropertiesInHierarchy(name).Any()
                    || (entityType.ClrType?.GetRuntimeProperties().FirstOrDefault(p => p.Name == name) != null))
                {
                    continue;
                }

                var propertyBuilder = entityTypeBuilder.Property(name, propertyType, ConfigurationSource.Convention);
                if (propertyBuilder != null)
                {
                    if (isRequired.HasValue
                        && propertyType.IsNullableType())
                    {
                        propertyBuilder.IsRequired(isRequired.Value, ConfigurationSource.Convention);
                    }
                    return propertyBuilder.Metadata;
                }
            }
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
            => properties.Where(property => property.DeclaringEntityType.FindProperty(property.Name) != null)
                .Select(property => modelBuilder.Entity(property.DeclaringEntityType.Name, configurationSource)
                    ?.Property(property.Name, configurationSource));

        private struct RelationshipSnapshot
        {
            public readonly ForeignKey ForeignKey;
            public readonly Navigation NavigationFrom;
            public readonly Navigation NavigationTo;
            public readonly bool IsDependent;

            public RelationshipSnapshot(ForeignKey foreignKey, Navigation navigationFrom, Navigation navigationTo, bool isDependent)
            {
                ForeignKey = foreignKey;
                NavigationFrom = navigationFrom;
                NavigationTo = navigationTo;
                IsDependent = isDependent;
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
