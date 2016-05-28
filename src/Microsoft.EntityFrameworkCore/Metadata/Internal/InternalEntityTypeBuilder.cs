// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{Metadata,nq}")]
    public class InternalEntityTypeBuilder : InternalMetadataItemBuilder<EntityType>
    {
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
                Metadata.SetPrimaryKey(properties, configurationSource);
                return HasKeyInternal(properties, configurationSource);
            }

            var primaryKeyConfigurationSource = Metadata.GetPrimaryKeyConfigurationSource();
            if (primaryKeyConfigurationSource.HasValue
                && !configurationSource.Overrides(primaryKeyConfigurationSource.Value))
            {
                return null;
            }

            var keyBuilder = HasKeyInternal(properties, configurationSource);
            if (keyBuilder == null)
            {
                return null;
            }

            var previousPrimaryKey = Metadata.FindPrimaryKey();
            Metadata.SetPrimaryKey(keyBuilder.Metadata.Properties, configurationSource, runConventions: false);
            UpdateReferencingForeignKeys(keyBuilder.Metadata);

            keyBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnPrimaryKeySet(keyBuilder, previousPrimaryKey);

            if (previousPrimaryKey != null)
            {
                RemoveKeyIfUnused(previousPrimaryKey);
            }

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

                var detachedRelationships = key.GetReferencingForeignKeys().ToList()
                    .Select(DetachRelationship).ToList();
                RemoveKey(key, ConfigurationSource.DataAnnotation);
                foreach (var relationshipSnapshot in detachedRelationships)
                {
                    relationshipSnapshot.Attach();
                }
            }
        }

        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => HasKeyInternal(properties, configurationSource);

        private InternalKeyBuilder HasKeyInternal(IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = GetActualProperties(properties, configurationSource);
            var key = Metadata.FindDeclaredKey(actualProperties);
            if (key == null)
            {
                if ((configurationSource != ConfigurationSource.Explicit) // let it throw for explicit
                    && (configurationSource == null
                        || actualProperties.Any(p => p.GetContainingForeignKeys().Any(k => k.DeclaringEntityType != Metadata))
                        || actualProperties.Any(p => !p.Builder.CanSetRequired(true, configurationSource))))
                {
                    return null;
                }

                foreach (var actualProperty in actualProperties)
                {
                    actualProperty.Builder.IsRequired(true, configurationSource.Value);
                }

                key = Metadata.AddKey(actualProperties, configurationSource.Value);
            }
            else if (configurationSource.HasValue)
            {
                key.UpdateConfigurationSource(configurationSource.Value);
            }

            return key?.Builder;
        }

        public virtual ConfigurationSource? RemoveKey(
            [NotNull] Key key, ConfigurationSource configurationSource, bool runConventions = true)
        {
            var currentConfigurationSource = key.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            foreach (var foreignKey in key.GetReferencingForeignKeys().ToList())
            {
                var removed = foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource, runConventions);
                Debug.Assert(removed.HasValue);
            }

            var removedKey = Metadata.RemoveKey(key.Properties, runConventions);
            if (removedKey == null)
            {
                return null;
            }
            Debug.Assert(removedKey == key);

            RemoveShadowPropertiesIfUnused(key.Properties);
            foreach (var property in key.Properties)
            {
                if (property.ClrType.IsNullableType())
                {
                    property.Builder?.IsRequired(false, configurationSource);
                }
            }

            return currentConfigurationSource;
        }

        private class KeyBuildersSnapshot
        {
            public KeyBuildersSnapshot(
                IReadOnlyList<Tuple<InternalKeyBuilder, ConfigurationSource>> keys,
                Tuple<InternalKeyBuilder, ConfigurationSource> primaryKey)
            {
                Keys = keys;
                PrimaryKey = primaryKey;
            }

            private IReadOnlyList<Tuple<InternalKeyBuilder, ConfigurationSource>> Keys { get; }
            private Tuple<InternalKeyBuilder, ConfigurationSource> PrimaryKey { get; }

            public void Attach()
            {
                foreach (var keyTuple in Keys)
                {
                    var detachedKeyBuilder = keyTuple.Item1;
                    var detachedConfigurationSource = keyTuple.Item2;
                    if (detachedKeyBuilder.Attach(detachedConfigurationSource) == null)
                    {
                        detachedKeyBuilder.ModelBuilder.Metadata.ConventionDispatcher
                            .OnKeyRemoved(detachedKeyBuilder.Metadata.DeclaringEntityType.Builder, detachedKeyBuilder.Metadata);
                    }
                    else if (PrimaryKey != null
                             && PrimaryKey.Item1 == detachedKeyBuilder)
                    {
                        var rootType = detachedKeyBuilder.Metadata.DeclaringEntityType.RootType();
                        var primaryKeyConfigurationSource = rootType.GetPrimaryKeyConfigurationSource();
                        if (primaryKeyConfigurationSource == null
                            || !primaryKeyConfigurationSource.Value.Overrides(PrimaryKey.Item2))
                        {
                            rootType.Builder.PrimaryKey(detachedKeyBuilder.Metadata.Properties, PrimaryKey.Item2);
                        }
                    }
                }
            }
        }

        private static KeyBuildersSnapshot DetachKeys(IEnumerable<Key> keysToDetach)
        {
            var keysToDetachList = keysToDetach.ToList();
            if (keysToDetachList.Count == 0)
            {
                return null;
            }

            var detachedKeys = new List<Tuple<InternalKeyBuilder, ConfigurationSource>>();
            Tuple<InternalKeyBuilder, ConfigurationSource> primaryKey = null;
            foreach (var keyToDetach in keysToDetachList)
            {
                var entityTypeBuilder = keyToDetach.DeclaringEntityType.Builder;
                var keyBuilder = keyToDetach.Builder;
                if (keyToDetach.IsPrimaryKey())
                {
                    var primaryKeyConfigurationSource = entityTypeBuilder.Metadata.GetPrimaryKeyConfigurationSource();
                    Debug.Assert(primaryKeyConfigurationSource.HasValue);
                    primaryKey = Tuple.Create(keyBuilder, primaryKeyConfigurationSource.Value);
                }
                var removedConfigurationSource = entityTypeBuilder.RemoveKey(keyToDetach, ConfigurationSource.Explicit, runConventions: false);
                Debug.Assert(removedConfigurationSource != null);

                detachedKeys.Add(Tuple.Create(keyBuilder, removedConfigurationSource.Value));
            }

            return new KeyBuildersSnapshot(detachedKeys, primaryKey);
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName, [NotNull] Type propertyType, ConfigurationSource configurationSource)
            => Property(propertyName, propertyType, clrProperty: null, configurationSource: configurationSource);

        public virtual InternalPropertyBuilder Property([NotNull] string propertyName, ConfigurationSource configurationSource)
            => Property(propertyName, null, clrProperty: null, configurationSource: configurationSource);

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty, ConfigurationSource configurationSource)
            => Property(clrProperty.Name, clrProperty.PropertyType, clrProperty: clrProperty, configurationSource: configurationSource);

        private InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] PropertyInfo clrProperty,
            [CanBeNull] ConfigurationSource? configurationSource)
        {
            if (IsIgnored(propertyName, configurationSource))
            {
                return null;
            }

            PropertyBuildersSnapshot detachedProperties = null;
            var existingProperty = Metadata.FindProperty(propertyName);
            if (existingProperty == null)
            {
                var derivedProperties = Metadata.FindDerivedProperties(propertyName);
                detachedProperties = DetachProperties(derivedProperties);
            }
            else if (existingProperty.DeclaringEntityType != Metadata)
            {
                return existingProperty.DeclaringEntityType.Builder
                    .Property(existingProperty, propertyName, propertyType, clrProperty, configurationSource);
            }

            var builder = Property(existingProperty, propertyName, propertyType, clrProperty, configurationSource);

            detachedProperties?.Attach(this);

            return builder;
        }

        private InternalPropertyBuilder Property(
            [CanBeNull] Property existingProperty,
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] PropertyInfo clrProperty,
            [CanBeNull] ConfigurationSource? configurationSource)
        {
            var property = existingProperty;
            if (existingProperty == null)
            {
                if (!configurationSource.HasValue)
                {
                    return null;
                }

                Unignore(propertyName);

                if (clrProperty != null)
                {
                    property = Metadata.AddProperty(clrProperty, configurationSource.Value);
                }
                else
                {
                    property = Metadata.AddProperty(propertyName, propertyType, configurationSource: configurationSource.Value);
                }
            }
            else
            {
                if ((propertyType != null
                     && propertyType != existingProperty.ClrType)
                    || (clrProperty != null
                        && existingProperty.IsShadowProperty))
                {
                    if (!configurationSource.HasValue
                        || !configurationSource.Value.Overrides(existingProperty.GetConfigurationSource()))
                    {
                        return null;
                    }

                    var detachedProperties = DetachProperties(new[] { existingProperty });

                    if (clrProperty != null)
                    {
                        property = Metadata.AddProperty(clrProperty, configurationSource.Value);
                    }
                    else
                    {
                        property = Metadata.AddProperty(propertyName, propertyType, configurationSource: configurationSource.Value);
                    }

                    detachedProperties.Attach(this);
                }
                else if (configurationSource.HasValue)
                {
                    property.UpdateConfigurationSource(configurationSource.Value);
                }
            }

            return property?.Builder;
        }

        private bool CanRemoveProperty(
            [NotNull] Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            Check.NotNull(property, nameof(property));
            Debug.Assert(property.DeclaringEntityType == Metadata);

            var currentConfigurationSource = property.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource)
                   && (canOverrideSameSource || (configurationSource != currentConfigurationSource));
        }

        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && !Metadata.FindNavigationsInHierarchy(navigationName).Any();

        public virtual bool CanAddOrReplaceNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && Metadata.FindNavigationsInHierarchy(navigationName).All(n =>
                   n.ForeignKey.Builder.CanSetNavigation((string)null, n.IsDependentToPrincipal(), configurationSource));

        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource? configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue
                && ignoredConfigurationSource.Value.Overrides(configurationSource))
            {
                return true;
            }

            if (Metadata.BaseType != null)
            {
                return Metadata.BaseType.Builder.IsIgnored(name, configurationSource);
            }

            return false;
        }

        public virtual bool CanRemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource);
        }

        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                if (configurationSource.Overrides(ignoredConfigurationSource)
                    && (configurationSource != ignoredConfigurationSource))
                {
                    Metadata.Ignore(name, configurationSource);
                }
                return true;
            }

            Metadata.Ignore(name, configurationSource);
            var navigation = Metadata.FindNavigation(name);
            if (navigation != null)
            {
                var foreignKey = navigation.ForeignKey;
                if (foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource) == null)
                {
                    Metadata.Unignore(name);
                    return false;
                }
            }
            else
            {
                var property = Metadata.FindProperty(name);
                if ((property != null)
                    && (property.DeclaringEntityType.Builder.RemoveProperty(property, configurationSource) == null))
                {
                    Metadata.Unignore(name);
                    return false;
                }
            }

            return true;
        }

        public virtual void Unignore([NotNull] string memberName)
        {
            var entityType = Metadata;
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                derivedType.Unignore(memberName);
            }

            while (entityType != null)
            {
                entityType.Unignore(memberName);
                entityType = entityType.BaseType;
            }
        }

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
            if (Metadata.BaseType == baseEntityType)
            {
                Metadata.HasBaseType(baseEntityType, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource()))
            {
                return null;
            }

            var detachedRelationships = new HashSet<RelationshipBuilderSnapshot>();
            PropertyBuildersSnapshot detachedProperties = null;
            KeyBuildersSnapshot detachedKeys = null;
            var changedRelationships = new List<InternalRelationshipBuilder>();
            IReadOnlyList<RelationshipSnapshot> relationshipsToBeRemoved = new List<RelationshipSnapshot>();
            // We use at least DataAnnotation as ConfigurationSource while removing to allow us
            // to remove metadata object which were defined in derived type
            // while corresponding annotations were present on properties in base type.
            var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
            if (baseEntityType != null)
            {
                if (Metadata.GetDeclaredKeys().Any(k => !configurationSourceForRemoval.Overrides(k.GetConfigurationSource())))
                {
                    return null;
                }

                relationshipsToBeRemoved = FindConflictingRelationships(baseEntityType, configurationSourceForRemoval);
                if (relationshipsToBeRemoved == null)
                {
                    return null;
                }

                var foreignKeysUsingKeyProperties = Metadata.GetDeclaredForeignKeys()
                    .Where(fk => relationshipsToBeRemoved.All(r => r.ForeignKey != fk)
                                 && fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true)).ToList();

                if (foreignKeysUsingKeyProperties.Any(fk =>
                    !configurationSourceForRemoval.Overrides(fk.GetForeignKeyPropertiesConfigurationSource())))
                {
                    return null;
                }

                changedRelationships.AddRange(
                    foreignKeysUsingKeyProperties.Select(foreignKeyUsingKeyProperties =>
                        foreignKeyUsingKeyProperties.Builder.HasForeignKey(null, configurationSourceForRemoval, runConventions: false)));

                foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
                {
                    var removedConfigurationSource = relationshipToBeRemoved.ForeignKey.DeclaringEntityType.Builder
                        .RemoveForeignKey(relationshipToBeRemoved.ForeignKey, configurationSourceForRemoval, runConventions: false);
                    Debug.Assert(removedConfigurationSource.HasValue);
                }

                foreach (var key in Metadata.GetDeclaredKeys().ToList())
                {
                    foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                    {
                        detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                    }
                }

                detachedKeys = DetachKeys(Metadata.GetDeclaredKeys());

                var duplicatedProperties = baseEntityType.GetProperties()
                    .Select(p => Metadata.FindDeclaredProperty(p.Name))
                    .Where(p => p != null);

                detachedProperties = DetachProperties(duplicatedProperties);

                baseEntityType.UpdateConfigurationSource(configurationSource);
            }

            var originalBaseType = Metadata.BaseType;
            Metadata.HasBaseType(baseEntityType, configurationSource, runConventions: false);

            detachedProperties?.Attach(this);

            detachedKeys?.Attach();

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            foreach (var changedRelationship in changedRelationships)
            {
                ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(changedRelationship);
            }

            foreach (var relationshipToBeRemoved in relationshipsToBeRemoved)
            {
                var dependentEntityType = relationshipToBeRemoved.ForeignKey.DeclaringEntityType.Builder;
                var principalEntityType = relationshipToBeRemoved.ForeignKey.PrincipalEntityType.Builder;
                var source = relationshipToBeRemoved.IsDependent ? dependentEntityType : principalEntityType;
                var target = relationshipToBeRemoved.IsDependent ? principalEntityType : dependentEntityType;

                var from = relationshipToBeRemoved.NavigationFrom;
                if (from != null)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnNavigationRemoved(source, target, from.Name, from.PropertyInfo);
                }

                var to = relationshipToBeRemoved.NavigationTo;
                if (to != null)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnNavigationRemoved(target, source, to.Name, to.PropertyInfo);
                }

                ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(dependentEntityType, relationshipToBeRemoved.ForeignKey);
            }

            ModelBuilder.Metadata.ConventionDispatcher.OnBaseEntityTypeSet(this, originalBaseType);

            return this;
        }

        private static PropertyBuildersSnapshot DetachProperties(IEnumerable<Property> propertiesToDetach)
        {
            var propertiesToDetachList = propertiesToDetach.ToList();
            if (propertiesToDetachList.Count == 0)
            {
                return null;
            }

            var detachedRelationships = new List<RelationshipBuilderSnapshot>();
            foreach (var propertyToDetach in propertiesToDetachList)
            {
                foreach (var relationship in propertyToDetach.GetContainingForeignKeys().ToList())
                {
                    detachedRelationships.Add(DetachRelationship(relationship));
                }
            }

            var detachedIndexes = DetachIndexes(propertiesToDetachList.SelectMany(p => p.GetContainingIndexes()).Distinct());

            var detachedKeys = DetachKeys(propertiesToDetachList.SelectMany(p => p.GetContainingKeys()).Distinct());

            var detachedProperties = new List<Tuple<InternalPropertyBuilder, ConfigurationSource>>();
            foreach (var propertyToDetach in propertiesToDetachList)
            {
                var property = propertyToDetach.DeclaringEntityType.FindDeclaredProperty(propertyToDetach.Name);
                if (property != null)
                {
                    var entityTypeBuilder = propertyToDetach.DeclaringEntityType.Builder;
                    var propertyBuilder = propertyToDetach.Builder;
                    var removedConfigurationSource = entityTypeBuilder
                        .RemoveProperty(propertyToDetach, ConfigurationSource.Explicit);
                    Debug.Assert(removedConfigurationSource.HasValue);
                    detachedProperties.Add(Tuple.Create(propertyBuilder, removedConfigurationSource.Value));
                }
            }

            return new PropertyBuildersSnapshot(detachedProperties, detachedIndexes, detachedKeys, detachedRelationships);
        }

        private class PropertyBuildersSnapshot
        {
            public PropertyBuildersSnapshot(
                IReadOnlyList<Tuple<InternalPropertyBuilder, ConfigurationSource>> properties,
                IndexBuildersSnapshot indexes,
                KeyBuildersSnapshot keys,
                IReadOnlyList<RelationshipBuilderSnapshot> relationships)
            {
                Properties = properties;
                Indexes = indexes;
                Keys = keys;
                Relationships = relationships;
            }

            private IReadOnlyList<Tuple<InternalPropertyBuilder, ConfigurationSource>> Properties { get; }
            private IReadOnlyList<RelationshipBuilderSnapshot> Relationships { get; }
            private IndexBuildersSnapshot Indexes { get; }
            private KeyBuildersSnapshot Keys { get; }

            public void Attach(InternalEntityTypeBuilder entityTypeBuilder)
            {
                foreach (var propertyTuple in Properties)
                {
                    propertyTuple.Item1.Attach(entityTypeBuilder, propertyTuple.Item2);
                }

                Indexes?.Attach();

                Keys?.Attach();

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
                            || ((relationship.NavigationFrom != null)
                                && (baseRelationship.NavigationFrom?.Name == relationship.NavigationFrom.Name)))
                        {
                            if (relationship.ForeignKey.DeclaringEntityType.Builder
                                .CanRemoveForeignKey(relationship.ForeignKey, configurationSource))
                            {
                                relationshipsToBeRemoved.Add(relationship);
                            }
                            else if (baseRelationship.ForeignKey.DeclaringEntityType.Builder
                                .CanRemoveForeignKey(baseRelationship.ForeignKey, configurationSource))
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

        private static Dictionary<EntityType, List<RelationshipSnapshot>> GroupRelationshipsByTargetType(EntityType entityType)
            => entityType.GetForeignKeys()
                .Select(foreignKey =>
                    new RelationshipSnapshot(foreignKey,
                        foreignKey.DependentToPrincipal,
                        foreignKey.PrincipalToDependent,
                        isDependent: true))
                .Concat(entityType.GetReferencingForeignKeys().Where(foreignKey => !foreignKey.IsSelfReferencing())
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
            var currentConfigurationSource = property.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource)
                || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
            {
                return null;
            }

            var detachedRelationships = property.GetContainingForeignKeys().ToList()
                .Select(DetachRelationship).ToList();

            foreach (var key in Metadata.GetKeys().Where(i => i.Properties.Contains(property)).ToList())
            {
                detachedRelationships.AddRange(key.GetReferencingForeignKeys().ToList()
                    .Select(DetachRelationship));
                var removed = RemoveKey(key, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            foreach (var index in Metadata.GetIndexes().Where(i => i.Properties.Contains(property)).ToList())
            {
                var removed = RemoveIndex(index, configurationSource);
                Debug.Assert(removed.HasValue);
            }

            if (Metadata.GetProperties().Contains(property))
            {
                var removedProperty = Metadata.RemoveProperty(property.Name);
                Debug.Assert(removedProperty == property);
            }

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
            }

            return currentConfigurationSource;
        }

        private static RelationshipBuilderSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
        {
            var relationshipBuilder = foreignKey.Builder;
            var relationshipConfigurationSource = foreignKey.DeclaringEntityType.Builder
                .RemoveForeignKey(foreignKey, ConfigurationSource.Explicit, runConventions: false);
            Debug.Assert(relationshipConfigurationSource != null);

            return new RelationshipBuilderSnapshot(relationshipBuilder, relationshipConfigurationSource.Value);
        }

        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
            => RemoveForeignKey(foreignKey, configurationSource, runConventions: true);

        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource, bool runConventions)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedForeignKey = Metadata.RemoveForeignKey(
                foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType, runConventions);

            if (removedForeignKey == null)
            {
                return null;
            }
            Debug.Assert(removedForeignKey == foreignKey);

            var index = Metadata.FindIndex(foreignKey.Properties);
            if (index != null
                && !index.IsInUse())
            {
                // Remove index if created by convention
                index.DeclaringEntityType.Builder.RemoveIndex(index, ConfigurationSource.Convention);
            }

            RemoveShadowPropertiesIfUnused(foreignKey.Properties.Where(p => p.DeclaringEntityType.FindDeclaredProperty(p.Name) != null).ToList());
            foreignKey.PrincipalKey.DeclaringEntityType.Builder?.RemoveKeyIfUnused(foreignKey.PrincipalKey);

            return currentConfigurationSource;
        }

        private void RemoveKeyIfUnused(Key key)
        {
            if (Metadata.FindPrimaryKey() == key)
            {
                return;
            }

            if (key.GetReferencingForeignKeys().Any())
            {
                return;
            }

            RemoveKey(key, ConfigurationSource.Convention);
        }

        public virtual void RemoveShadowPropertiesIfUnused([NotNull] IReadOnlyList<Property> properties)
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
            if (!property.DeclaringEntityType.Builder.CanRemoveProperty(property, ConfigurationSource.Convention))
            {
                return;
            }

            if (property.GetContainingIndexes().Any())
            {
                return;
            }

            if (property.GetContainingForeignKeys().Any())
            {
                return;
            }

            if (property.GetContainingKeys().Any())
            {
                return;
            }

            var removedProperty = property.DeclaringEntityType.RemoveProperty(property.Name);
            Debug.Assert(removedProperty == property);
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

            IndexBuildersSnapshot detachedIndexes = null;
            var existingIndex = Metadata.FindIndex(properties);
            if (existingIndex == null)
            {
                var derivedIndexes = Metadata.FindDerivedIndexes(properties);
                detachedIndexes = DetachIndexes(derivedIndexes);
            }
            else if (existingIndex.DeclaringEntityType != Metadata)
            {
                return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, configurationSource);
            }

            var indexBuilder = HasIndex(existingIndex, properties, configurationSource);

            detachedIndexes?.Attach();

            return indexBuilder;
        }

        private InternalIndexBuilder HasIndex(
            Index existingIndex, IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (existingIndex == null)
            {
                var index = Metadata.AddIndex(properties, configurationSource);
                return index.Builder;
            }

            existingIndex.UpdateConfigurationSource(configurationSource);
            return existingIndex.Builder;
        }

        public virtual ConfigurationSource? RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = index.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedIndex = Metadata.RemoveIndex(index.Properties);
            Debug.Assert(removedIndex == index);

            RemoveShadowPropertiesIfUnused(index.Properties);

            return currentConfigurationSource;
        }

        private class IndexBuildersSnapshot
        {
            public IndexBuildersSnapshot(IReadOnlyList<Tuple<InternalIndexBuilder, ConfigurationSource>> indexes)
            {
                Indexes = indexes;
            }

            private IReadOnlyList<Tuple<InternalIndexBuilder, ConfigurationSource>> Indexes { get; }

            public void Attach()
            {
                foreach (var indexTuple in Indexes)
                {
                    indexTuple.Item1.Attach(indexTuple.Item2);
                }
            }
        }

        private static IndexBuildersSnapshot DetachIndexes(IEnumerable<Index> indexesToDetach)
        {
            var indexesToDetachList = indexesToDetach.ToList();
            if (indexesToDetachList.Count == 0)
            {
                return null;
            }

            var detachedIndexes = new List<Tuple<InternalIndexBuilder, ConfigurationSource>>();
            foreach (var indexToDetach in indexesToDetachList)
            {
                var entityTypeBuilder = indexToDetach.DeclaringEntityType.Builder;
                var indexBuilder = indexToDetach.Builder;
                var removedConfigurationSource = entityTypeBuilder.RemoveIndex(indexToDetach, ConfigurationSource.Explicit);
                Debug.Assert(removedConfigurationSource != null);

                detachedIndexes.Add(Tuple.Create(indexBuilder, removedConfigurationSource.Value));
            }

            return new IndexBuildersSnapshot(detachedIndexes);
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
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(propertyNames, configurationSource, principalType.Metadata.FindPrimaryKey()?.Properties),
                    null,
                    configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalType = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(propertyNames, configurationSource, principalKey.Properties),
                    principalKey,
                    configurationSource);
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
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(clrProperties, configurationSource),
                    null,
                    configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(clrProperties, configurationSource),
                    principalKey,
                    configurationSource);
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(principalEntityTypeBuilder,
                GetActualProperties(dependentProperties, configurationSource),
                null,
                configurationSource);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(principalEntityTypeBuilder,
                GetActualProperties(dependentProperties, configurationSource),
                principalKey,
                configurationSource);

        private InternalRelationshipBuilder HasForeignKeyInternal(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            if (dependentProperties == null)
            {
                return null;
            }

            var newRelationship = RelationshipInternal(principalEntityTypeBuilder, principalKey, configurationSource);
            var relationship = newRelationship.HasForeignKey(dependentProperties, configurationSource);
            if (relationship == null
                && newRelationship.Metadata.Builder != null)
            {
                RemoveForeignKey(newRelationship.Metadata, configurationSource);
            }

            return relationship;
        }

        private InternalRelationshipBuilder CreateRelationshipBuilder(
            EntityType principalType,
            IReadOnlyList<Property> dependentProperties,
            Key principalKey,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            var key = Metadata.AddForeignKey(dependentProperties, principalKey, principalType, configurationSource: null, runConventions: false);
            key.UpdateConfigurationSource(configurationSource);
            principalType.UpdateConfigurationSource(configurationSource);

            HasIndex(dependentProperties, ConfigurationSource.Convention);

            var value = key.Builder;
            if (runConventions)
            {
                value = ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(value);
            }

            return value;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationToTargetName,
            [CanBeNull] string inverseNavigationName,
            ConfigurationSource configurationSource)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationToTargetName),
                PropertyIdentity.Create(inverseNavigationName),
                configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] PropertyInfo navigationToTarget,
            [CanBeNull] PropertyInfo inverseNavigation,
            ConfigurationSource configurationSource)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationToTarget),
                PropertyIdentity.Create(inverseNavigation),
                configurationSource);

        private InternalRelationshipBuilder Relationship(
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            PropertyIdentity? navigationToTarget,
            PropertyIdentity? inverseNavigation,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder));

            Debug.Assert(navigationToTarget != null
                         || inverseNavigation != null);

            var inverseProperty = inverseNavigation?.Property;
            var navigationProperty = navigationToTarget?.Property;
            if (inverseNavigation == null
                && navigationProperty != null
                && !navigationProperty.PropertyType.GetTypeInfo().IsAssignableFrom(
                    targetEntityTypeBuilder.Metadata.ClrType.GetTypeInfo()))
            {
                // Only one nav specified and it can't be the nav to principal
                return targetEntityTypeBuilder.Relationship(this, null, navigationToTarget, configurationSource);
            }

            var existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                targetEntityTypeBuilder.Metadata,
                Metadata,
                navigationToTarget,
                inverseNavigation,
                null,
                null);
            if (existingRelationship != null)
            {
                if (navigationToTarget != null)
                {
                    existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                }
                if (inverseNavigation != null)
                {
                    existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                }
                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);
                return existingRelationship;
            }

            existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                Metadata,
                targetEntityTypeBuilder.Metadata,
                inverseNavigation,
                navigationToTarget,
                null,
                null);
            if (existingRelationship != null)
            {
                if (navigationToTarget != null)
                {
                    existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                }
                if (inverseNavigation != null)
                {
                    existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                }
                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);
                return existingRelationship;
            }

            var relationship = CreateForeignKey(
                targetEntityTypeBuilder,
                null,
                null,
                null,
                null,
                configurationSource,
                runConventions: false);

            var newRelationship = relationship;

            if (inverseNavigation == null)
            {
                newRelationship = navigationProperty != null
                    ? newRelationship.DependentToPrincipal(navigationProperty, configurationSource)
                    : newRelationship.DependentToPrincipal(navigationToTarget.Value.Name, configurationSource);
            }
            else if (navigationToTarget == null)
            {
                newRelationship = inverseProperty != null
                    ? newRelationship.PrincipalToDependent(inverseProperty, configurationSource)
                    : newRelationship.PrincipalToDependent(inverseNavigation.Value.Name, configurationSource);
            }
            else
            {
                newRelationship = navigationProperty != null || inverseProperty != null
                    ? newRelationship.Navigations(navigationProperty, inverseProperty, configurationSource)
                    : newRelationship.Navigations(navigationToTarget.Value.Name, inverseNavigation.Value.Name, configurationSource);
            }

            if (newRelationship == null)
            {
                if (relationship.Metadata.Builder != null)
                {
                    relationship.Metadata.DeclaringEntityType.Builder.RemoveForeignKey(relationship.Metadata, configurationSource);
                }
                return null;
            }

            return newRelationship;
        }

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, null, configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, principalKey, configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityTypeBuilder, null, configurationSource);

        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityTypeBuilder, principalKey, configurationSource);

        private InternalRelationshipBuilder RelationshipInternal(
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            var relationship = CreateForeignKey(
                targetEntityTypeBuilder,
                null,
                principalKey,
                null,
                null,
                configurationSource,
                runConventions: true);

            if (principalKey == null)
            {
                return relationship;
            }

            var newRelationship = relationship?.RelatedEntityTypes(targetEntityTypeBuilder.Metadata, Metadata, configurationSource)
                ?.HasPrincipalKey(principalKey.Properties, configurationSource);

            if (newRelationship == null
                && relationship?.Metadata.Builder != null)
            {
                relationship.Metadata.DeclaringEntityType.Builder.RemoveForeignKey(relationship.Metadata, configurationSource);
                return null;
            }

            return newRelationship;
        }

        public virtual InternalRelationshipBuilder Navigation(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationName,
            ConfigurationSource configurationSource)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationName),
                null,
                configurationSource);

        public virtual InternalRelationshipBuilder Navigation(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] PropertyInfo navigationProperty,
            ConfigurationSource configurationSource,
            bool strictPrincipalEnd = false)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationProperty),
                null,
                configurationSource);

        public virtual InternalRelationshipBuilder CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            [CanBeNull] string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var principalBaseEntityTypeBuilder = principalType.RootType().Builder;
            if (principalKey == null)
            {
                principalKey = principalType.FindPrimaryKey();
                if (principalKey != null
                    && dependentProperties != null
                    && (!ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalType,
                        Metadata,
                        shouldThrow: false)
                        || Metadata.FindForeignKeysInHierarchy(dependentProperties, principalKey, principalType).Any()))
                {
                    principalKey = null;
                }
            }

            if (dependentProperties != null)
            {
                dependentProperties = GetActualProperties(dependentProperties, ConfigurationSource.Convention);
                if (principalKey == null)
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

                    var keyBuilder = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention);

                    principalKey = keyBuilder.Metadata;
                }
                else
                {
                    Debug.Assert(Metadata.FindForeignKey(dependentProperties, principalKey, principalType) == null);
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

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(new[] { principalKeyProperty }, ConfigurationSource.Convention).Metadata;
                }

                var baseName = string.IsNullOrEmpty(navigationToPrincipalName) ? principalType.DisplayName() : navigationToPrincipalName;
                var fkProperties = new Property[principalKey.Properties.Count];
                for (var i = 0; i < principalKey.Properties.Count; i++)
                {
                    IProperty keyProperty = principalKey.Properties[i];
                    var propertyName = (keyProperty.Name.StartsWith(baseName, StringComparison.OrdinalIgnoreCase) ? "" : baseName)
                                       + keyProperty.Name;
                    fkProperties[i] = CreateUniqueProperty(
                        propertyName,
                        isRequired ?? false ? keyProperty.ClrType : keyProperty.ClrType.MakeNullable(),
                        this,
                        isRequired);
                }

                dependentProperties = fkProperties;
            }

            return CreateRelationshipBuilder(principalType, dependentProperties, principalKey, configurationSource, runConventions);
        }

        private static Property CreateUniqueProperty(string baseName, Type propertyType, InternalEntityTypeBuilder entityTypeBuilder, bool? isRequired = null)
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

        public virtual IReadOnlyList<Property> GetOrCreateProperties(
            [CanBeNull] IEnumerable<string> propertyNames,
            ConfigurationSource configurationSource,
            [CanBeNull] IEnumerable<Property> referencedProperties = null)
        {
            if (propertyNames == null)
            {
                return null;
            }

            var list = new List<Property>();
            var propertyNamesList = propertyNames.ToList();
            var referencedPropertiesList = referencedProperties?.ToList();
            if (referencedPropertiesList != null
                && referencedPropertiesList.Count != propertyNamesList.Count)
            {
                referencedPropertiesList = null;
            }
            var typesList = referencedPropertiesList?.Select(p => p.IsShadowProperty ? null : p.ClrType).ToList();
            for (var i = 0; i < propertyNamesList.Count; i++)
            {
                var propertyName = propertyNamesList[i];
                var property = Metadata.FindProperty(propertyName);
                if (property == null)
                {
                    var clrProperty = Metadata.ClrType?.GetPropertiesInHierarchy(propertyName).FirstOrDefault();
                    var type = typesList?[i];
                    InternalPropertyBuilder propertyBuilder;
                    if (clrProperty != null)
                    {
                        propertyBuilder = Property(clrProperty, configurationSource);
                    }
                    else if (type != null)
                    {
                        // TODO: Log that shadow property is created by convention
                        propertyBuilder = Property(propertyName, type.MakeNullable(), ConfigurationSource.Convention);
                    }
                    else
                    {
                        throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                    }

                    if (propertyBuilder == null)
                    {
                        return null;
                    }
                    property = propertyBuilder.Metadata;
                }
                else
                {
                    property.DeclaringEntityType.UpdateConfigurationSource(configurationSource);
                    property = property.DeclaringEntityType.Builder.Property(property.Name, configurationSource).Metadata;
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

        public virtual IReadOnlyList<Property> GetActualProperties([CanBeNull] IEnumerable<Property> properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = new List<Property>();
            foreach (var property in properties)
            {
                var builder = property.Builder != null && property.DeclaringEntityType.IsAssignableFrom(Metadata)
                    ? property.Builder
                    : Metadata.FindProperty(property.Name)?.Builder
                      ?? Property(property.Name, property.ClrType, property.PropertyInfo, configurationSource);
                if (builder == null)
                {
                    return null;
                }

                actualProperties.Add(builder.Metadata);
            }
            return actualProperties;
        }

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
                ConfigurationSource relationshipConfigurationSource)
            {
                Relationship = relationship;
                RelationshipConfigurationSource = relationshipConfigurationSource;
            }

            private InternalRelationshipBuilder Relationship { get; }
            private ConfigurationSource RelationshipConfigurationSource { get; }

            public void Attach()
                => Relationship.Attach(RelationshipConfigurationSource);
        }
    }
}
