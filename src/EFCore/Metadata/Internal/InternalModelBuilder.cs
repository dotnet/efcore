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
            [NotNull] string name, ConfigurationSource configurationSource, bool throwOnQuery = false)
            => Entity(new TypeIdentity(name), configurationSource, throwOnQuery);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type, ConfigurationSource configurationSource, bool throwOnQuery = false)
            => Entity(new TypeIdentity(type), configurationSource, throwOnQuery);

        private InternalEntityTypeBuilder Entity(
            TypeIdentity type, ConfigurationSource configurationSource, bool throwOnQuery)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type;
            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);

            if (entityType == null)
            {
                if (clrType == null)
                {
                    if (Metadata.ShouldBeOwnedType(type.Name)
                        && Metadata.HasEntityTypeWithDefiningNavigation(type.Name))
                    {
                        Debug.Assert(configurationSource == ConfigurationSource.Explicit,
                            "If a type is marked as an owned entity it can only be configured as a non-owned entity type explicitly");

                        Metadata.UnmarkAsOwnedType(type.Name);

                        using (Metadata.ConventionDispatcher.StartBatch())
                        {
                            foreach (var entityTypeWithDefiningNavigation in Metadata.GetEntityTypes(type.Name).ToList())
                            {
                                if (entityTypeWithDefiningNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    RemoveEntityType(entityTypeWithDefiningNavigation, configurationSource);
                                }
                            }

                            return Entity(type, configurationSource, throwOnQuery);
                        }
                    }

                    Metadata.Unignore(type.Name);

                    entityType = Metadata.AddEntityType(type.Name, configurationSource);
                }
                else
                {
                    if (Metadata.ShouldBeOwnedType(clrType)
                        && Metadata.HasEntityTypeWithDefiningNavigation(clrType))
                    {
                        Debug.Assert(configurationSource == ConfigurationSource.Explicit,
                            "If a type is marked as an owned entity it can only be configured as a non-owned entity type explicitly");

                        Metadata.UnmarkAsOwnedType(clrType);

                        using (Metadata.ConventionDispatcher.StartBatch())
                        {
                            foreach (var entityTypeWithDefiningNavigation in Metadata.GetEntityTypes(clrType).ToList())
                            {
                                if (entityTypeWithDefiningNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    RemoveEntityType(entityTypeWithDefiningNavigation, configurationSource);
                                }
                            }

                            return Entity(type, configurationSource, throwOnQuery);
                        }
                    }

                    Metadata.Unignore(clrType);

                    entityType = Metadata.AddEntityType(clrType, configurationSource);
                }
            }
            else
            {
                if (throwOnQuery && entityType.IsQueryType)
                {
                    throw new InvalidOperationException(
                        CoreStrings.CannotAccessQueryAsEntity(entityType.DisplayName()));
                }

                entityType.UpdateConfigurationSource(configurationSource);
            }

            return entityType?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Query([NotNull] Type clrType)
        {
            if (IsIgnored(clrType, ConfigurationSource.Explicit))
            {
                return null;
            }

            var entityType = Metadata.FindEntityType(clrType);

            if (entityType == null)
            {
                Metadata.Unignore(clrType);

                entityType = Metadata.AddQueryType(clrType);
            }
            else
            {
                if (!entityType.IsQueryType)
                {
                    throw new InvalidOperationException(
                        CoreStrings.CannotAccessEntityAsQuery(entityType.DisplayName()));
                }
            }

            return entityType.Builder;
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
            => Entity(new TypeIdentity(type), definingNavigationName, definingEntityType, configurationSource);

        private InternalEntityTypeBuilder Entity(
            TypeIdentity type,
            string definingNavigationName,
            EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return null;
            }

            var clrType = type.Type;
            var weakEntityType = clrType == null
                ? Metadata.FindEntityType(type.Name, definingNavigationName, definingEntityType)
                : Metadata.FindEntityType(clrType, definingNavigationName, definingEntityType);
            if (weakEntityType == null)
            {
                var entityType = clrType == null
                    ? Metadata.FindEntityType(type.Name)
                    : Metadata.FindEntityType(clrType);

                IConventionBatch batch = null;
                EntityTypeSnapshot entityTypeSnapshot = null;
                if (entityType != null)
                {
                    if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                    {
                        return null;
                    }

                    batch = ModelBuilder.Metadata.ConventionDispatcher.StartBatch();
                    entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(entityType);

                    Ignore(entityType, configurationSource);
                }

                if (clrType == null)
                {
                    Metadata.Unignore(type.Name);

                    weakEntityType = Metadata.AddEntityType(type.Name, definingNavigationName, definingEntityType, configurationSource);
                }
                else
                {
                    Metadata.Unignore(clrType);

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
            => Owned(new TypeIdentity(type), configurationSource);

        private bool Owned(
            TypeIdentity type, ConfigurationSource configurationSource)
        {
            if (IsIgnored(type, configurationSource))
            {
                return false;
            }

            var clrType = type.Type;
            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);
            if (entityType != null
                && !entityType.GetForeignKeys().Any(fk => fk.IsOwnership))
            {
                if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                {
                    return false;
                }

                if (entityType.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonOwnedEntityType(entityType.DisplayName()));
                }

                var potentialOwnerships = entityType.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).ToList();
                foreach (var foreignKey in potentialOwnerships)
                {
                    foreignKey.PrincipalEntityType.FindNavigation(foreignKey.PrincipalToDependent.Name).ForeignKey.Builder
                        .IsOwnership(true, configurationSource);
                }
            }

            if (clrType == null)
            {
                Metadata.Unignore(type.Name);

                Metadata.MarkAsOwnedType(type.Name);
            }
            else
            {
                Metadata.Unignore(clrType);

                Metadata.MarkAsOwnedType(clrType);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] Type type, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(type), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource configurationSource)
            => IsIgnored(new TypeIdentity(name), configurationSource);

        private bool IsIgnored(TypeIdentity type, ConfigurationSource configurationSource)
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
            => Ignore(new TypeIdentity(type), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
            => Ignore(new TypeIdentity(name), configurationSource);

        private bool Ignore(TypeIdentity type, ConfigurationSource configurationSource)
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

            var entityType = Metadata.FindEntityType(name);
            if (entityType != null)
            {
                return Ignore(entityType, configurationSource);
            }

            var ownedTypes = Metadata.GetEntityTypes(name).ToList();
            if (ownedTypes.Count > 0)
            {
                if (ownedTypes.Any(o => !configurationSource.Overrides(o.GetConfigurationSource())))
                {
                    return false;
                }

                foreach (var ownedType in ownedTypes)
                {
                    Ignore(ownedType, configurationSource);
                }

                return true;
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

            // Set base type as null to remove the entityType from directly derived types of the base type
            var baseType = entityType.BaseType;
            entityType.Builder.HasBaseType((EntityType)null, configurationSource);

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
                    .HasBaseType(baseType, configurationSource);
                Debug.Assert(derivedEntityTypeBuilder != null);
            }

            using (Metadata.ConventionDispatcher.StartBatch())
            {
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
            foreach (var orphan in new ModelNavigationsGraphAdapter(Metadata).GetUnreachableVertices(rootEntityTypes))
            {
                // Ignoring the type prevents it from being rediscovered by conventions that run as part of the removal
                Ignore(orphan, configurationSource);
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
