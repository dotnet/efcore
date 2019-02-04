// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalModelBuilder : InternalMetadataBuilder<Model>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalModelBuilder([NotNull] Model metadata)
            : base(metadata)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalModelBuilder ModelBuilder => this;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] string name, ConfigurationSource configurationSource, bool? owned = false, bool throwOnQuery = false)
            => Entity(new TypeIdentity(name), configurationSource, owned, throwOnQuery);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type, ConfigurationSource configurationSource, bool? owned = false, bool throwOnQuery = false)
            => Entity(new TypeIdentity(type, Metadata), configurationSource, owned, throwOnQuery);

        private InternalEntityTypeBuilder Entity(
            in TypeIdentity type, ConfigurationSource configurationSource, bool? owned, bool throwOnQuery)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type;
            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);

            using (Metadata.ConventionDispatcher.StartBatch())
            {
                if (owned == false
                    && (ShouldBeOwnedType(type)
                        || (entityType != null && entityType.IsOwned())))
                {
                    throw new InvalidOperationException(CoreStrings.ClashingOwnedEntityType(
                        clrType == null ? type.Name : clrType.ShortDisplayName()));
                }

                if (owned == true
                    && entityType != null
                    && !entityType.IsOwned()
                    && configurationSource == ConfigurationSource.Explicit
                    && entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                }

                if (entityType != null)
                {
                    if (throwOnQuery && entityType.IsQueryType)
                    {
                        if ((entityType.GetConfigurationSource() != ConfigurationSource.Explicit
                             || configurationSource != ConfigurationSource.Explicit)
                            && !RemoveEntityType(entityType, configurationSource))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        entityType.UpdateConfigurationSource(configurationSource);
                        return entityType.Builder;
                    }
                }

                Metadata.Unignore(type.Name);
                entityType = clrType == null
                    ? Metadata.AddEntityType(type.Name, configurationSource)
                    : Metadata.AddEntityType(clrType, configurationSource);
            }

            return entityType.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Query([NotNull] string name, ConfigurationSource configurationSource)
            => Query(new TypeIdentity(name), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Query([NotNull] Type clrType, ConfigurationSource configurationSource)
            => Query(new TypeIdentity(clrType, Metadata), configurationSource);

        private InternalEntityTypeBuilder Query(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type;
            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);

            using (Metadata.ConventionDispatcher.StartBatch())
            {
                if (entityType != null)
                {
                    if (!entityType.IsQueryType)
                    {
                        if ((entityType.GetConfigurationSource() != ConfigurationSource.Explicit
                             || configurationSource != ConfigurationSource.Explicit)
                            && !RemoveEntityType(entityType, configurationSource))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        entityType.UpdateConfigurationSource(configurationSource);
                        return entityType.Builder;
                    }
                }

                Metadata.Unignore(clrType);

                Metadata.Unignore(type.Name);
                entityType = clrType == null
                    ? Metadata.AddQueryType(type.Name, configurationSource)
                    : Metadata.AddQueryType(clrType, configurationSource);

                return entityType.Builder;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(name), definingNavigationName, definingEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(type, Metadata), definingNavigationName, definingEntityType, configurationSource);

        private InternalEntityTypeBuilder Entity(
            in TypeIdentity type,
            string definingNavigationName,
            EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type
                          ?? Metadata.FindClrType(type.Name);

            var weakEntityType = clrType == null
                ? Metadata.FindEntityType(type.Name, definingNavigationName, definingEntityType)
                : Metadata.FindEntityType(clrType, definingNavigationName, definingEntityType);
            if (weakEntityType == null)
            {
                var entityType = clrType == null
                    ? Metadata.FindEntityType(type.Name)
                    : Metadata.FindEntityType(clrType);

                IConventionBatch batch = null;
                EntityType.Snapshot entityTypeSnapshot = null;
                if (entityType != null)
                {
                    if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                    {
                        return null;
                    }

                    batch = ModelBuilder.Metadata.ConventionDispatcher.StartBatch();
                    entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(entityType);

                    RemoveEntityType(entityType, configurationSource);
                }

                if (clrType == null)
                {
                    Metadata.Unignore(type.Name);

                    weakEntityType = Metadata.AddEntityType(type.Name, definingNavigationName, definingEntityType, configurationSource);
                }
                else
                {
                    Metadata.Unignore(type.Name);

                    weakEntityType = Metadata.AddEntityType(clrType, definingNavigationName, definingEntityType, configurationSource);
                }

                if (batch != null)
                {
                    entityTypeSnapshot.Attach(weakEntityType.Builder);
                    batch.Dispose();
                }
            }
            else
            {
                weakEntityType.UpdateConfigurationSource(configurationSource);
            }

            return weakEntityType?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Owned(
            [NotNull] string name, ConfigurationSource configurationSource)
            => Owned(new TypeIdentity(name), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Owned(
            [NotNull] Type type, ConfigurationSource configurationSource)
            => Owned(new TypeIdentity(type, Metadata), configurationSource);

        private bool Owned(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return false;
            }

            var clrType = type.Type;
            if (clrType == null)
            {
                Metadata.Unignore(type.Name);

                Metadata.MarkAsOwnedType(type.Name);
            }
            else
            {
                Metadata.Unignore(type.Name);

                Metadata.MarkAsOwnedType(clrType);
            }

            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);

            if (entityType?.GetForeignKeys().Any(fk => fk.IsOwnership) == false)
            {
                if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                {
                    return false;
                }

                if (entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                }

                var ownershipCandidate = entityType.GetForeignKeys().FirstOrDefault(
                    fk => fk.PrincipalToDependent != null
                          && !fk.PrincipalEntityType.IsInOwnershipPath(entityType)
                          && !fk.PrincipalEntityType.IsInDefinitionPath(clrType));
                if (ownershipCandidate != null)
                {
                    if (ownershipCandidate.Builder.IsOwnership(true, configurationSource) == null)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!entityType.Builder.RemoveNonOwnershipRelationships(null, configurationSource))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ShouldBeOwnedType(in TypeIdentity type)
            => type.Type == null ? Metadata.ShouldBeOwnedType(type.Name) : Metadata.ShouldBeOwnedType(type.Type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] Type type, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(type, Metadata), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(name), configurationSource);

        private bool IsIgnored(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                return false;
            }

            var ignoredConfigurationSource = Metadata.FindIgnoredTypeConfigurationSource(type.Name);
            return ignoredConfigurationSource.HasValue
                   && ignoredConfigurationSource.Value.Overrides(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] Type type, ConfigurationSource configurationSource)
            => Ignore(new TypeIdentity(type, Metadata), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
            => Ignore(new TypeIdentity(name), configurationSource);

        private bool Ignore(in TypeIdentity type, ConfigurationSource configurationSource)
        {
            var name = type.Name;
            var ignoredConfigurationSource = Metadata.FindIgnoredTypeConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                if (configurationSource.Overrides(ignoredConfigurationSource)
                    && configurationSource != ignoredConfigurationSource)
                {
                    Metadata.Ignore(name, configurationSource);
                }

                return true;
            }

            if (ShouldBeOwnedType(type)
                && configurationSource != ConfigurationSource.Explicit)
            {
                return false;
            }

            var entityTypes = Metadata.GetEntityTypes(name).ToList();
            if (entityTypes.Count > 0)
            {
                if (entityTypes.Any(o => !configurationSource.Overrides(o.GetConfigurationSource())))
                {
                    return false;
                }

                foreach (var entityType in entityTypes)
                {
                    Ignore(entityType, configurationSource);
                }
            }

            if (type.Type == null)
            {
                Metadata.UnmarkAsOwnedType(type.Name);
            }
            else
            {
                Metadata.UnmarkAsOwnedType(type.Type);
            }

            Metadata.Ignore(name, configurationSource);
            return true;
        }

        private bool Ignore(EntityType entityType, ConfigurationSource configurationSource)
        {
            var entityTypeConfigurationSource = entityType.GetConfigurationSource();
            if (!configurationSource.Overrides(entityTypeConfigurationSource))
            {
                return false;
            }

            using (Metadata.ConventionDispatcher.StartBatch())
            {
                if (entityType.HasClrType())
                {
                    Metadata.Ignore(entityType.ClrType, configurationSource);
                }
                else
                {
                    Metadata.Ignore(entityType.Name, configurationSource);
                }

                return RemoveEntityType(entityType, configurationSource);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RemoveEntityType([NotNull] EntityType entityType, ConfigurationSource configurationSource)
        {
            var entityTypeConfigurationSource = entityType.GetConfigurationSource();
            if (!configurationSource.Overrides(entityTypeConfigurationSource))
            {
                return false;
            }

            using (Metadata.ConventionDispatcher.StartBatch())
            {
                var entityTypeBuilder = entityType.Builder;
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
                {
                    var removed = entityTypeBuilder.RemoveForeignKey(foreignKey, configurationSource);
                    Debug.Assert(removed.HasValue);
                }

                foreach (var foreignKey in entityType.GetDeclaredReferencingForeignKeys().ToList())
                {
                    var removed = foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource);
                    Debug.Assert(removed.HasValue);
                }

                foreach (var directlyDerivedType in entityType.GetDirectlyDerivedTypes().ToList())
                {
                    var derivedEntityTypeBuilder = directlyDerivedType.Builder
                        .HasBaseType(entityType.BaseType, configurationSource);
                    Debug.Assert(derivedEntityTypeBuilder != null);
                }

                foreach (var definedType in Metadata.GetEntityTypes().Where(e => e.DefiningEntityType == entityType).ToList())
                {
                    RemoveEntityType(definedType, configurationSource);
                }

                Metadata.RemoveEntityType(entityType);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveEntityTypesUnreachableByNavigations(ConfigurationSource configurationSource)
        {
            var rootEntityTypes = GetRoots(configurationSource);
            using (Metadata.ConventionDispatcher.StartBatch())
            {
                foreach (var orphan in new ModelNavigationsGraphAdapter(Metadata).GetUnreachableVertices(rootEntityTypes))
                {
                    RemoveEntityType(orphan, configurationSource);
                }
            }
        }

        private IReadOnlyList<EntityType> GetRoots(ConfigurationSource configurationSource)
        {
            var roots = new List<EntityType>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var entityType in Metadata.GetEntityTypes())
            {
                var currentConfigurationSource = entityType.GetConfigurationSource();
                if (currentConfigurationSource.Overrides(configurationSource))
                {
                    roots.Add(entityType);
                }
            }

            return roots;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<InternalEntityTypeBuilder> FindLeastDerivedEntityTypes([NotNull] Type type, [CanBeNull] Func<InternalEntityTypeBuilder, bool> condition = null)
        {
            var cache = new Dictionary<TypeInfo, int>
            {
                [type.GetTypeInfo()] = 0
            };
            var leastDerivedTypesGroups = Metadata.GetEntityTypes()
                .GroupBy(t => GetDerivedLevel(t.ClrType.GetTypeInfo(), cache), t => t.Builder)
                .Where(g => g.Key != int.MaxValue)
                .OrderBy(g => g.Key);

            foreach (var leastDerivedTypes in leastDerivedTypesGroups)
            {
                if (condition == null)
                {
                    return leastDerivedTypes.ToList();
                }

                var filteredTypes = leastDerivedTypes.Where(condition).ToList();
                if (filteredTypes.Count > 0)
                {
                    return filteredTypes;
                }
            }

            return new List<InternalEntityTypeBuilder>();
        }

        private static int GetDerivedLevel(TypeInfo derivedType, Dictionary<TypeInfo, int> cache)
        {
            if (derivedType?.BaseType == null)
            {
                return int.MaxValue;
            }

            if (cache.TryGetValue(derivedType, out var level))
            {
                return level;
            }

            var baseType = derivedType.BaseType.GetTypeInfo();
            level = GetDerivedLevel(baseType, cache);
            level += level == int.MaxValue ? 0 : 1;
            cache.Add(derivedType, level);
            return level;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, propertyAccessMode, configurationSource);
    }
}
