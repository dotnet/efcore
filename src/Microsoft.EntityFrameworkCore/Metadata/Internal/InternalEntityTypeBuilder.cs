// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    public class InternalEntityTypeBuilder : InternalMetadataItemBuilder<EntityType>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalEntityTypeBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

                var referencingForeignKeys = key.GetReferencingForeignKeys().Where(fk => fk.GetPrincipalKeyConfigurationSource() == null)
                    .ToList();
                if (referencingForeignKeys.Count == 0)
                {
                    continue;
                }

                var detachedRelationships = referencingForeignKeys.Select(DetachRelationship).ToList();
                foreach (var relationshipSnapshot in detachedRelationships)
                {
                    relationshipSnapshot.Attach();
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                if (configurationSource == null)
                {
                    return null;
                }

                var containingForeignKeys = actualProperties
                    .SelectMany(p => p.GetContainingForeignKeys().Where(k => k.DeclaringEntityType != Metadata))
                    .ToList();

                if (containingForeignKeys.Any(fk => !configurationSource.Overrides(fk.GetForeignKeyPropertiesConfigurationSource())))
                {
                    return null;
                }

                if (configurationSource != ConfigurationSource.Explicit // let it throw for explicit
                    && actualProperties.Any(p => !p.Builder.CanSetRequired(true, configurationSource)))
                {
                    return null;
                }

                var modifiedRelationships = containingForeignKeys
                    .Where(fk => fk.GetForeignKeyPropertiesConfigurationSource() != ConfigurationSource.Explicit)  // let it throw for explicit
                    .Select(foreignKey => foreignKey.Builder.HasForeignKey(null, configurationSource, runConventions: false))
                    .ToList();

                foreach (var actualProperty in actualProperties)
                {
                    actualProperty.Builder.IsRequired(true, configurationSource.Value);
                }

                key = Metadata.AddKey(actualProperties, configurationSource.Value);

                foreach (var foreignKey in containingForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(foreignKey.DeclaringEntityType.Builder, foreignKey);
                }

                foreach (var relationship in modifiedRelationships)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(relationship);
                }
            }
            else if (configurationSource.HasValue)
            {
                key.UpdateConfigurationSource(configurationSource.Value);
            }

            return key?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveKey(
            [NotNull] Key key, ConfigurationSource configurationSource, bool runConventions = true)
        {
            var currentConfigurationSource = key.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var referencingForeignKeys = key.GetReferencingForeignKeys().ToList();

            foreach (var foreignKey in referencingForeignKeys)
            {
                var removed = foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);
            }

            var removedKey = Metadata.RemoveKey(key.Properties, runConventions);
            if (removedKey == null)
            {
                return null;
            }
            Debug.Assert(removedKey == key);

            foreach (var foreignKey in referencingForeignKeys)
            {
                Metadata.Model.ConventionDispatcher.OnForeignKeyRemoved(foreignKey.DeclaringEntityType.Builder, foreignKey);
            }

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [NotNull] Type propertyType,
            ConfigurationSource configurationSource)
            => Property(propertyName, propertyType, configurationSource, typeConfigurationSource: configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [NotNull] Type propertyType,
            ConfigurationSource configurationSource,
            [CanBeNull] ConfigurationSource? typeConfigurationSource)
            => Property(propertyName, propertyType, memberInfo: null,
                configurationSource: configurationSource, typeConfigurationSource: typeConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] string propertyName, ConfigurationSource configurationSource)
            => Property(propertyName, propertyType: null, memberInfo: null, configurationSource: configurationSource, typeConfigurationSource: configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] MemberInfo clrProperty, ConfigurationSource configurationSource)
            => Property(clrProperty.Name, clrProperty.GetMemberType(), clrProperty, configurationSource, configurationSource);

        private InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] MemberInfo memberInfo,
            [CanBeNull] ConfigurationSource? configurationSource,
            [CanBeNull] ConfigurationSource? typeConfigurationSource)
        {
            if (IsIgnored(propertyName, configurationSource))
            {
                return null;
            }

            Metadata.Unignore(propertyName);

            PropertyBuildersSnapshot detachedProperties = null;
            var existingProperty = Metadata.FindProperty(propertyName);
            if (existingProperty == null)
            {
                detachedProperties = DetachProperties(Metadata.FindDerivedProperties(propertyName));
            }
            else if (existingProperty.DeclaringEntityType != Metadata)
            {
                if (memberInfo != null
                    && existingProperty.MemberInfo == null)
                {
                    detachedProperties = DetachProperties(new[] { existingProperty });
                }
                else
                {
                    return existingProperty.DeclaringEntityType.Builder
                        .Property(existingProperty, propertyName, propertyType, memberInfo, configurationSource, typeConfigurationSource);
                }
            }

            var builder = Property(existingProperty, propertyName, propertyType, memberInfo, configurationSource, typeConfigurationSource);

            detachedProperties?.Attach(this);

            if (builder != null
                && builder.Metadata.Builder == null)
            {
                return Metadata.FindProperty(propertyName)?.Builder;
            }

            return builder;
        }

        private InternalPropertyBuilder Property(
            [CanBeNull] Property existingProperty,
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] MemberInfo clrProperty,
            [CanBeNull] ConfigurationSource? configurationSource,
            [CanBeNull] ConfigurationSource? typeConfigurationSource)
        {
            var property = existingProperty;
            if (existingProperty == null)
            {
                if (!configurationSource.HasValue)
                {
                    return null;
                }

                var duplicateNavigation = Metadata.FindNavigationsInHierarchy(propertyName).FirstOrDefault();
                if (duplicateNavigation != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyCalledOnNavigation(propertyName, Metadata.DisplayName()));
                }

                if (clrProperty != null)
                {
                    property = Metadata.AddProperty(clrProperty, configurationSource.Value);
                }
                else
                {
                    property = Metadata.AddProperty(propertyName, propertyType,  configurationSource.Value, typeConfigurationSource);
                }
            }
            else
            {
                if ((propertyType != null
                     && propertyType != existingProperty.ClrType)
                    || (clrProperty != null
                        && existingProperty.PropertyInfo == null))
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
                        property = Metadata.AddProperty(propertyName, propertyType, configurationSource.Value, typeConfigurationSource.Value);
                    }

                    detachedProperties.Attach(this);
                }
                else
                {
                    if (configurationSource.HasValue)
                    {
                        property.UpdateConfigurationSource(configurationSource.Value);
                    }
                    if (typeConfigurationSource.HasValue)
                    {
                        property.UpdateConfigurationSource(typeConfigurationSource.Value);
                    }
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && !Metadata.FindNavigationsInHierarchy(navigationName).Any();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanAddOrReplaceNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource: configurationSource)
               && Metadata.FindNavigationsInHierarchy(navigationName).All(n =>
                   n.ForeignKey.Builder.CanSetNavigation((string)null, n.IsDependentToPrincipal(), configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource? configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(ignoredConfigurationSource))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanRemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue
                && ignoredConfigurationSource.Value.Overrides(configurationSource))
            {
                return true;
            }

            Metadata.Ignore(name, configurationSource, runConventions: false);

            var navigation = Metadata.FindNavigation(name);
            if (navigation != null)
            {
                var foreignKey = navigation.ForeignKey;
                if (navigation.DeclaringEntityType != Metadata)
                {
                    if (configurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(CoreStrings.InheritedPropertyCannotBeIgnored(
                            name, Metadata.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                    }
                    return false;
                }

                if (foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(
                    foreignKey, configurationSource, canOverrideSameSource: configurationSource == ConfigurationSource.Explicit) == null)
                {
                    Metadata.Unignore(name);
                    return false;
                }
            }
            else
            {
                var property = Metadata.FindProperty(name);
                if (property != null)
                {
                    if (property.DeclaringEntityType != Metadata)
                    {
                        if (configurationSource == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(CoreStrings.InheritedPropertyCannotBeIgnored(
                                name, Metadata.DisplayName(), property.DeclaringEntityType.DisplayName()));
                        }
                        return false;
                    }

                    if (property.DeclaringEntityType.Builder.RemoveProperty(
                        property, configurationSource, canOverrideSameSource: configurationSource == ConfigurationSource.Explicit) == null)
                    {
                        Metadata.Unignore(name);
                        return false;
                    }
                }
            }

            foreach (var derivedType in Metadata.GetDerivedTypes())
            {
                var derivedNavigation = derivedType.FindDeclaredNavigation(name);
                if (derivedNavigation != null)
                {
                    var foreignKey = derivedNavigation.ForeignKey;
                    foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource, canOverrideSameSource: false);
                }
                else
                {
                    var derivedProperty = derivedType.FindDeclaredProperty(name);
                    if (derivedProperty != null)
                    {
                        derivedType.Builder.RemoveProperty(derivedProperty, configurationSource, canOverrideSameSource: false);
                    }
                }

                var derivedIgnoredSource = derivedType.FindDeclaredIgnoredMemberConfigurationSource(name);
                if (derivedIgnoredSource.HasValue
                    && configurationSource.Overrides(derivedIgnoredSource))
                {
                    derivedType.Unignore(name);
                }
            }

            Metadata.Model.ConventionDispatcher.OnEntityTypeMemberIgnored(this, name);

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

            var detachedRelationships = new List<RelationshipBuilderSnapshot>();
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

                var foreignKeysUsingKeyProperties = Metadata.GetDerivedForeignKeysInclusive()
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
                    .SelectMany(p => Metadata.FindDerivedPropertiesInclusive(p.Name))
                    .Where(p => p != null);

                detachedProperties = DetachProperties(duplicatedProperties);

                var propertiesToRemove = Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties())
                    .Where(p => !p.GetConfigurationSource().Overrides(baseEntityType.FindIgnoredMemberConfigurationSource(p.Name))).ToList();
                foreach (var property in propertiesToRemove)
                {
                    property.DeclaringEntityType.Builder.RemoveProperty(property, ConfigurationSource.Explicit);
                }

                foreach (var ignoredMember in Metadata.GetIgnoredMembers().ToList())
                {
                    var ignoredSource = Metadata.FindDeclaredIgnoredMemberConfigurationSource(ignoredMember);
                    var baseIgnoredSource = baseEntityType.FindIgnoredMemberConfigurationSource(ignoredMember);

                    if (baseIgnoredSource.HasValue
                        && baseIgnoredSource.Value.Overrides(ignoredSource))
                    {
                        Metadata.Unignore(ignoredMember);
                    }
                }

                baseEntityType.UpdateConfigurationSource(configurationSource);
            }

            var detachedIndexes = new List<IndexBuilderSnapshot>();
            HashSet<Property> removedInheritedPropertiesToDuplicate = null;
            if (Metadata.BaseType != null)
            {
                var removedInheritedProperties = new HashSet<Property>(Metadata.BaseType.GetProperties()
                    .Where(p => baseEntityType == null || baseEntityType.FindProperty(p.Name) != p));
                if (removedInheritedProperties.Count != 0)
                {
                    removedInheritedPropertiesToDuplicate = new HashSet<Property>();
                    foreach (var foreignKey in Metadata.GetDerivedForeignKeysInclusive()
                        .Where(fk => fk.Properties.Any(p => removedInheritedProperties.Contains(p))).ToList())
                    {
                        foreach (var property in foreignKey.Properties)
                        {
                            if (removedInheritedProperties.Contains(property))
                            {
                                removedInheritedPropertiesToDuplicate.Add(property);
                            }
                        }
                        detachedRelationships.Add(DetachRelationship(foreignKey));
                    }

                    foreach (var index in Metadata.GetDerivedIndexesInclusive()
                        .Where(i => i.Properties.Any(p => removedInheritedProperties.Contains(p))).ToList())
                    {
                        foreach (var property in index.Properties)
                        {
                            if (removedInheritedProperties.Contains(property))
                            {
                                removedInheritedPropertiesToDuplicate.Add(property);
                            }
                        }
                        detachedIndexes.Add(DetachIndex(index));
                    }
                }
            }

            var originalBaseType = Metadata.BaseType;
            Metadata.HasBaseType(baseEntityType, configurationSource, runConventions: false);

            if (removedInheritedPropertiesToDuplicate != null)
            {
                foreach (var property in removedInheritedPropertiesToDuplicate)
                {
                    property.Builder?.Attach(this, property.GetConfigurationSource());
                }
            }

            detachedProperties?.Attach(this);

            detachedKeys?.Attach();

            foreach (var indexBuilderSnapshot in detachedIndexes)
            {
                indexBuilderSnapshot.Attach();
            }

            foreach (var detachedRelationship in detachedRelationships)
            {
                detachedRelationship.Attach();
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

                ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(
                    relationshipToBeRemoved.ForeignKey.DeclaringEntityType.Builder, relationshipToBeRemoved.ForeignKey);
            }

            foreach (var changedRelationship in changedRelationships)
            {
                ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(changedRelationship);
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

            var detachedIndexes = propertiesToDetachList.SelectMany(p => p.GetContainingIndexes()).Distinct().ToList()
                .Select(DetachIndex).ToList();

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
                IReadOnlyList<IndexBuilderSnapshot> indexes,
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
            private IReadOnlyList<IndexBuilderSnapshot> Indexes { get; }
            private KeyBuildersSnapshot Keys { get; }

            public void Attach(InternalEntityTypeBuilder entityTypeBuilder)
            {
                foreach (var propertyTuple in Properties)
                {
                    propertyTuple.Item1.Attach(entityTypeBuilder, propertyTuple.Item2);
                }

                Keys?.Attach();

                foreach (var indexBuilderSnapshot in Indexes)
                {
                    indexBuilderSnapshot.Attach();
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

            foreach (var ignoredNavigation in Metadata.GetDerivedTypesInclusive()
                .SelectMany(et => et.GetDeclaredNavigations())
                .Where(n => !n.ForeignKey.GetConfigurationSource().Overrides(baseEntityType.FindIgnoredMemberConfigurationSource(n.Name))))
            {
                var foreignKey = ignoredNavigation.ForeignKey;
                if (relationshipsToBeRemoved.Any(r => r.ForeignKey == foreignKey))
                {
                    continue;
                }
                relationshipsToBeRemoved.Add(
                    new RelationshipSnapshot(foreignKey,
                        foreignKey.DependentToPrincipal,
                        foreignKey.PrincipalToDependent,
                        isDependent: ignoredNavigation == foreignKey.DependentToPrincipal));
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

            var removedKeys = new List<Key>();
            foreach (var key in property.GetContainingKeys().ToList())
            {
                detachedRelationships.AddRange(key.GetReferencingForeignKeys().ToList()
                    .Select(DetachRelationship));
                var removed = RemoveKey(key, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);
                removedKeys.Add(key);
            }

            var removedIndexes = new List<Index>();
            foreach (var index in property.GetContainingIndexes().ToList())
            {
                var removed = RemoveIndex(index, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);
                removedIndexes.Add(index);
            }

            if (Metadata.GetProperties().Contains(property))
            {
                var removedProperty = Metadata.RemoveProperty(property.Name);
                Debug.Assert(removedProperty == property);
            }

            foreach (var index in removedIndexes)
            {
                Metadata.Model.ConventionDispatcher.OnIndexRemoved(this, index);
            }

            foreach (var key in removedKeys)
            {
                Metadata.Model.ConventionDispatcher.OnKeyRemoved(this, key);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
            => RemoveForeignKey(foreignKey, configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveForeignKey(
            [NotNull] ForeignKey foreignKey,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource = true,
            bool runConventions = true)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource)
                || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveShadowPropertiesIfUnused([NotNull] IReadOnlyList<Property> properties)
        {
            foreach (var property in properties.ToList())
            {
                if (property != null
                    && property.IsShadowProperty)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            List<IndexBuilderSnapshot> detachedIndexes = null;
            var existingIndex = Metadata.FindIndex(properties);
            if (existingIndex == null)
            {
                detachedIndexes = Metadata.FindDerivedIndexes(properties).ToList().Select(DetachIndex).ToList();
            }
            else if (existingIndex.DeclaringEntityType != Metadata)
            {
                return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, configurationSource);
            }

            var indexBuilder = HasIndex(existingIndex, properties, configurationSource);

            if (detachedIndexes != null)
            {
                foreach (var indexBuilderSnapshot in detachedIndexes)
                {
                    indexBuilderSnapshot.Attach();
                }
            }

            return indexBuilder;
        }

        private InternalIndexBuilder HasIndex(
            Index index, IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (index == null)
            {
                index = Metadata.AddIndex(properties, configurationSource);
            }
            else
            {
                index.UpdateConfigurationSource(configurationSource);
            }

            return index?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource, bool runConventions = true)
        {
            var currentConfigurationSource = index.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedIndex = Metadata.RemoveIndex(index.Properties, runConventions);
            Debug.Assert(removedIndex == index);

            RemoveShadowPropertiesIfUnused(index.Properties);

            return currentConfigurationSource;
        }

        private class IndexBuilderSnapshot
        {
            public IndexBuilderSnapshot(InternalIndexBuilder index, ConfigurationSource configurationSource)
            {
                Index = index;
                IndexConfigurationSource = configurationSource;
            }

            private InternalIndexBuilder Index { get; }
            private ConfigurationSource IndexConfigurationSource { get; }

            public void Attach() => Index.Attach(IndexConfigurationSource);
        }

        private static IndexBuilderSnapshot DetachIndex(Index indexToDetach)
        {
            var entityTypeBuilder = indexToDetach.DeclaringEntityType.Builder;
            var indexBuilder = indexToDetach.Builder;
            var removedConfigurationSource = entityTypeBuilder.RemoveIndex(indexToDetach, ConfigurationSource.Explicit);
            Debug.Assert(removedConfigurationSource != null);
            return new IndexBuilderSnapshot(indexBuilder, removedConfigurationSource.Value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                    GetOrCreateProperties(propertyNames, configurationSource, principalType.Metadata.FindPrimaryKey()?.Properties, useDefaultType: true),
                    null,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                    GetOrCreateProperties(propertyNames, configurationSource, principalKey.Properties, useDefaultType: true),
                    principalKey,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(principalEntityTypeBuilder,
                GetActualProperties(dependentProperties, configurationSource),
                null,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            var foreignKey = Metadata.AddForeignKey(dependentProperties, principalKey, principalType, configurationSource: null, runConventions: false);
            foreignKey.UpdateConfigurationSource(configurationSource);
            principalType.UpdateConfigurationSource(configurationSource);

            var value = foreignKey.Builder;
            if (runConventions)
            {
                value = ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(value);
            }

            return value;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                    if (navigationToTarget.Value.Name != null)
                    {
                        Metadata.Unignore(navigationToTarget.Value.Name);
                    }
                }
                if (inverseNavigation != null)
                {
                    existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityTypeBuilder.Metadata.Unignore(inverseNavigation.Value.Name);
                    }
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

            if (newRelationship.Metadata == relationship.Metadata)
            {
                return Metadata.Model.ConventionDispatcher.OnForeignKeyAdded(newRelationship);
            }

            return newRelationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, principalKey, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityTypeBuilder, null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigation(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationName,
            ConfigurationSource configurationSource)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationName),
                null,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                    && dependentProperties != null)
                {
                    if (!ForeignKey.AreCompatible(
                        principalKey.Properties,
                        dependentProperties,
                        principalType,
                        Metadata,
                        shouldThrow: false))
                    {
                        if (dependentProperties.All(p => p.GetTypeConfigurationSource() == null))
                        {
                            var detachedProperties = DetachProperties(dependentProperties);
                            GetOrCreateProperties(dependentProperties.Select(p => p.Name).ToList(), configurationSource, principalKey.Properties);
                            detachedProperties.Attach(this);
                        }
                        else
                        {
                            principalKey = null;
                        }
                    }
                    else if (Metadata.FindForeignKeysInHierarchy(dependentProperties, principalKey, principalType).Any())
                    {
                        principalKey = null;
                    }
                }
            }

            if (dependentProperties != null)
            {
                dependentProperties = GetActualProperties(dependentProperties, ConfigurationSource.Convention);
                if (principalKey == null)
                {
                    var principalKeyProperties = principalBaseEntityTypeBuilder.CreateUniqueProperties(
                        dependentProperties.Count, null, Enumerable.Repeat("", dependentProperties.Count), dependentProperties.Select(p => p.ClrType), isRequired: true, baseName: "TempId");
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
                    var principalKeyProperties = principalBaseEntityTypeBuilder.CreateUniqueProperties(
                        1, null, new[] { "TempId" }, new[] { typeof(int) }, isRequired: true, baseName: "");

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention).Metadata;
                }

                var baseName = string.IsNullOrEmpty(navigationToPrincipalName) ? principalType.DisplayName() : navigationToPrincipalName;
                dependentProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, baseName);
            }

            return CreateRelationshipBuilder(principalType, dependentProperties, principalKey, configurationSource, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> ReUniquifyTemporaryProperties(
            [NotNull] IReadOnlyList<Property> currentProperties,
            [NotNull] IReadOnlyList<Property> principalProperties,
            bool isRequired,
            [NotNull] string baseName) => CreateUniqueProperties(currentProperties, principalProperties, isRequired, baseName);

        private IReadOnlyList<Property> CreateUniqueProperties(
            IReadOnlyList<Property> currentProperties,
            IReadOnlyList<Property> principalProperties,
            bool isRequired,
            string baseName)
            => CreateUniqueProperties(
                principalProperties.Count,
                currentProperties,
                principalProperties.Select(p => p.Name),
                principalProperties.Select(p => p.ClrType),
                isRequired,
                baseName);

        private IReadOnlyList<Property> CreateUniqueProperties(
            int propertyCount,
            IReadOnlyList<Property> currentProperties,
            IEnumerable<string> principalPropertyNames,
            IEnumerable<Type> principalPropertyTypes,
            bool isRequired,
            string baseName)
        {
            var newProperties = new Property[propertyCount];
            var clrMembers = Metadata.ClrType == null
                ? null
                : new HashSet<string>(Metadata.ClrType.GetRuntimeProperties().Select(p => p.Name)
                    .Concat(Metadata.ClrType.GetRuntimeFields().Select(p => p.Name)));
            var noNewProperties = true;
            using (var principalPropertyNamesEnumerator = principalPropertyNames.GetEnumerator())
            {
                using (var principalPropertyTypesEnumerator = principalPropertyTypes.GetEnumerator())
                {
                    for (var i = 0; i < newProperties.Length
                                    && principalPropertyNamesEnumerator.MoveNext()
                                    && principalPropertyTypesEnumerator.MoveNext(); i++)
                    {
                        var keyPropertyName = principalPropertyNamesEnumerator.Current;
                        var keyPropertyType = principalPropertyTypesEnumerator.Current;
                        var keyModifiedBaseName = (keyPropertyName.StartsWith(baseName, StringComparison.OrdinalIgnoreCase) ? "" : baseName)
                                                  + keyPropertyName;
                        string propertyName;
                        var clrType = isRequired ? keyPropertyType : keyPropertyType.MakeNullable();
                        var index = -1;
                        while (true)
                        {
                            propertyName = keyModifiedBaseName + (++index > 0 ? index.ToString(CultureInfo.InvariantCulture) : "");
                            if (!Metadata.FindPropertiesInHierarchy(propertyName).Any()
                                && clrMembers?.Contains(propertyName) != true)
                            {
                                var propertyBuilder = Property(propertyName, clrType, ConfigurationSource.Convention, typeConfigurationSource: null);
                                if (propertyBuilder == null)
                                {
                                    RemoveShadowPropertiesIfUnused(newProperties);
                                    return null;
                                }

                                if (clrType.IsNullableType())
                                {
                                    propertyBuilder.IsRequired(isRequired, ConfigurationSource.Convention);
                                }
                                newProperties[i] = propertyBuilder.Metadata;
                                noNewProperties = false;
                                break;
                            }
                            if (currentProperties != null
                                && newProperties.All(p => p == null || p.Name != propertyName))
                            {
                                var currentProperty = currentProperties.SingleOrDefault(p => p.Name == propertyName);
                                if (currentProperty != null
                                    && currentProperty.ClrType == clrType
                                    && currentProperty.IsNullable == !isRequired)
                                {
                                    newProperties[i] = currentProperty;
                                    noNewProperties = noNewProperties && newProperties[i] == currentProperties[i];
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return noNewProperties ? null : newProperties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetOrCreateProperties(
            [CanBeNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource,
            [CanBeNull] IReadOnlyList<Property> referencedProperties = null,
            bool useDefaultType = false)
        {
            if (propertyNames == null)
            {
                return null;
            }

            if (referencedProperties != null
                && referencedProperties.Count != propertyNames.Count)
            {
                referencedProperties = null;
            }

            var propertyList = new List<Property>();
            for (var i = 0; i < propertyNames.Count; i++)
            {
                var propertyName = propertyNames[i];
                var property = Metadata.FindProperty(propertyName);
                if (property == null)
                {
                    var clrProperty = Metadata.ClrType?.GetMembersInHierarchy(propertyName).FirstOrDefault();
                    var type = referencedProperties == null
                        ? useDefaultType ? typeof(int) : null
                        : referencedProperties[i].ClrType;

                    InternalPropertyBuilder propertyBuilder;
                    if (clrProperty != null)
                    {
                        propertyBuilder = Property(clrProperty, configurationSource);
                    }
                    else if (type != null)
                    {
                        // TODO: Log that a shadow property is created
                        propertyBuilder = Property(propertyName, type.MakeNullable(), configurationSource, typeConfigurationSource: null);
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
                propertyList.Add(property);
            }
            return propertyList;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetOrCreateProperties([CanBeNull] IEnumerable<MemberInfo> clrProperties, ConfigurationSource configurationSource)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetActualProperties([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = new Property[properties.Count];
            for (var i = 0; i < actualProperties.Length; i++)
            {
                var property = properties[i];
                var builder = property.Builder != null && property.DeclaringEntityType.IsAssignableFrom(Metadata)
                    ? property.Builder
                    : Metadata.FindProperty(property.Name)?.Builder
                      ?? (property.IsShadowProperty
                          ? null
                          : Property(property.Name, property.ClrType, property.PropertyInfo, configurationSource, property.GetTypeConfigurationSource()));
                if (builder == null)
                {
                    return null;
                }

                actualProperties[i] = builder.Metadata;
            }

            return actualProperties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, propertyAccessMode, configurationSource);

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
