// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Model : ConventionalAnnotatable, IMutableModel
    {
        private readonly SortedDictionary<string, EntityType> _entityTypes
            = new SortedDictionary<string, EntityType>();

        private readonly IDictionary<Type, EntityType> _clrTypeMap
            = new Dictionary<Type, EntityType>();

        private readonly Dictionary<string, ConfigurationSource> _ignoredEntityTypeNames
            = new Dictionary<string, ConfigurationSource>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Model()
            : this(new ConventionSet())
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Model([NotNull] ConventionSet conventions)
        {
            ConventionDispatcher = new ConventionDispatcher(conventions);
            Builder = new InternalModelBuilder(this);
            ConventionDispatcher.OnModelInitialized(Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTrackingStrategy ChangeTrackingStrategy { get; set; } 
            = ChangeTrackingStrategy.Snapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionDispatcher ConventionDispatcher { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetEntityTypes() => _entityTypes.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, configurationSource);

            return AddEntityType(entityType, runConventions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, configurationSource);

            _clrTypeMap[type] = entityType;
            return AddEntityType(entityType, runConventions);
        }

        private EntityType AddEntityType(EntityType entityType, bool runConventions)
        {
            var previousLength = _entityTypes.Count;
            _entityTypes[entityType.Name] = entityType;
            if (previousLength == _entityTypes.Count)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.Name));
            }

            if (runConventions)
            {
                return ConventionDispatcher.OnEntityTypeAdded(entityType.Builder)?.Metadata;
            }
            return entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
            => FindEntityType(type) ?? AddEntityType(type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType GetOrAddEntityType([NotNull] string name)
            => FindEntityType(name) ?? AddEntityType(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            EntityType entityType;
            return _clrTypeMap.TryGetValue(type, out entityType)
                ? entityType
                : FindEntityType(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            EntityType entityType;
            return _entityTypes.TryGetValue(name, out entityType)
                ? entityType
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] Type type)
        {
            var entityType = FindEntityType(type);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] string name)
        {
            var entityType = FindEntityType(name);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        private EntityType RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var referencingForeignKey = entityType.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByForeignKey(
                        entityType.DisplayName(),
                        Property.Format(referencingForeignKey.Properties),
                        referencingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var derivedEntityType = entityType.GetDirectlyDerivedTypes().FirstOrDefault();
            if (derivedEntityType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByDerived(
                        entityType.DisplayName(),
                        derivedEntityType.DisplayName()));
            }

            if (entityType.ClrType != null)
            {
                _clrTypeMap.Remove(entityType.ClrType);
            }

            var removed = _entityTypes.Remove(entityType.Name);
            Debug.Assert(removed);
            entityType.Builder = null;

            return entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] Type type, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(type, nameof(type));
            Ignore(type.DisplayName(), configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore([NotNull] string name, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            ConfigurationSource existingIgnoredConfigurationSource;
            if (_ignoredEntityTypeNames.TryGetValue(name, out existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
            }

            _ignoredEntityTypeNames[name] = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredEntityTypeConfigurationSource([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindIgnoredEntityTypeConfigurationSource(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredEntityTypeConfigurationSource([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            ConfigurationSource ignoredConfigurationSource;
            if (_ignoredEntityTypeNames.TryGetValue(name, out ignoredConfigurationSource))
            {
                return ignoredConfigurationSource;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));
            Unignore(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unignore([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));
            _ignoredEntityTypeNames.Remove(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Validate() => ConventionDispatcher.OnModelBuilt(Builder);

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IMutableEntityType IMutableModel.AddEntityType(Type type) => AddEntityType(type);
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();
        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);
    }
}
