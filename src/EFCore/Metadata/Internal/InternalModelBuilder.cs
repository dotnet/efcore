// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

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
            [NotNull] string name, ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(name), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Entity(
            [NotNull] Type type, ConfigurationSource configurationSource)
            => Entity(new TypeIdentity(type), configurationSource);

        private InternalEntityTypeBuilder Entity(
            TypeIdentity type, ConfigurationSource configurationSource)
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
                    Metadata.Unignore(type.Name);

                    entityType = Metadata.AddEntityType(type.Name, configurationSource);
                }
                else
                {
                    Metadata.Unignore(clrType);

                    entityType = Metadata.AddEntityType(clrType, configurationSource);
                }
            }
            else
            {
                entityType.UpdateConfigurationSource(configurationSource);
            }

            return entityType?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder AddDelegatedIdentityEntity(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => AddDelegatedIdentityEntity(new TypeIdentity(name), definingNavigationName, definingEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder AddDelegatedIdentityEntity(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            => AddDelegatedIdentityEntity(new TypeIdentity(type), definingNavigationName, definingEntityType, configurationSource);

        private InternalEntityTypeBuilder AddDelegatedIdentityEntity(
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
            var entityType = clrType == null
                ? Metadata.FindEntityType(type.Name)
                : Metadata.FindEntityType(clrType);
            if (entityType != null)
            {
                if (!configurationSource.Overrides(entityType.GetConfigurationSource()))
                {
                    return null;
                }

                Ignore(entityType, configurationSource);
            }

            if (clrType == null)
            {
                Metadata.Unignore(type.Name);

                entityType = Metadata.AddDelegatedIdentityEntityType(type.Name, definingNavigationName, definingEntityType, configurationSource);
            }
            else
            {
                Metadata.Unignore(clrType);

                entityType = Metadata.AddDelegatedIdentityEntityType(clrType, definingNavigationName, definingEntityType, configurationSource);
            }

            return entityType?.Builder;
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
            => Ignore(type.DisplayName(), type, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
            => Ignore(name, null, configurationSource);

        private bool Ignore([NotNull] string name, [CanBeNull] Type type, ConfigurationSource configurationSource)
        {
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
            if (entityType == null)
            {
                if (type != null)
                {
                    Metadata.Ignore(type, configurationSource);
                }
                else
                {
                    Metadata.Ignore(name, configurationSource);
                }
                return true;
            }

            return Ignore(entityType, configurationSource);
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

        private bool RemoveEntityType(EntityType entityType, ConfigurationSource configurationSource)
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

            foreach (var definedDelegatedTypes in Metadata.GetEntityTypes().Where(e => e.DefiningEntityType == entityType).ToList())
            {
                RemoveDelegatedIdentityEntityType(definedDelegatedTypes, configurationSource);
            }

            if (entityType.HasDelegatedIdentity())
            {
                Metadata.RemoveDelegatedIdentityEntityType(entityType);
            }
            else
            {
                Metadata.RemoveEntityType(entityType.Name);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RemoveDelegatedIdentityEntityType([NotNull] EntityType entityType, ConfigurationSource configurationSource)
        {
            if (!entityType.HasDelegatedIdentity())
            {
                return false;
            }

            using (Metadata.ConventionDispatcher.StartBatch())
            {
                RemoveEntityType(entityType, configurationSource);

                return true;
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
            var cache = new Dictionary<TypeInfo, int> { [type.GetTypeInfo()] = 0 };
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

        private int GetDerivedLevel(TypeInfo derivedType, Dictionary<TypeInfo, int> cache)
        {
            if (derivedType?.BaseType == null)
            {
                return int.MaxValue;
            }

            int level;
            if (cache.TryGetValue(derivedType, out level))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Validate() => Metadata.Validate();
    }
}
