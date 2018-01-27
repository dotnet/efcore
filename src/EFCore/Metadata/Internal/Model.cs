// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        private readonly SortedDictionary<string, SortedSet<EntityType>> _entityTypesWithDefiningNavigation
            = new SortedDictionary<string, SortedSet<EntityType>>();

        private readonly Dictionary<string, ConfigurationSource> _ignoredTypeNames
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
            var dispatcher = new ConventionDispatcher(conventions);
            var builder = new InternalModelBuilder(this);
            ConventionDispatcher = dispatcher;
            Builder = builder;
            dispatcher.OnModelInitialized(builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTrackingStrategy ChangeTrackingStrategy { [DebuggerStepThrough] get; set; }
            = ChangeTrackingStrategy.Snapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionDispatcher ConventionDispatcher { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Builder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetEntityTypes()
            => _entityTypes.Values.Concat(_entityTypesWithDefiningNavigation.Values.SelectMany(e => e));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, configurationSource);

            _clrTypeMap[type] = entityType;

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddQueryType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            var queryType = new EntityType(type, this, ConfigurationSource.Explicit)
            {
                IsQueryType = true
            };

            _clrTypeMap[type] = queryType;

            return AddEntityType(queryType);
        }

        private EntityType AddEntityType(EntityType entityType)
        {
            var entityTypeName = entityType.Name;
            if (entityType.HasDefiningNavigation())
            {
                if (_entityTypes.ContainsKey(entityTypeName))
                {
                    throw new InvalidOperationException(CoreStrings.ClashingNonWeakEntityType(entityType.DisplayName()));
                }

                if (!_entityTypesWithDefiningNavigation.TryGetValue(entityTypeName, out var entityTypesWithSameType))
                {
                    entityTypesWithSameType = new SortedSet<EntityType>(EntityTypePathComparer.Instance);
                    _entityTypesWithDefiningNavigation[entityTypeName] = entityTypesWithSameType;
                }

                var added = entityTypesWithSameType.Add(entityType);
                Debug.Assert(added);
            }
            else
            {
                if (_entityTypesWithDefiningNavigation.ContainsKey(entityTypeName))
                {
                    throw new InvalidOperationException(CoreStrings.ClashingWeakEntityType(entityType.DisplayName()));
                }

                var previousLength = _entityTypes.Count;
                _entityTypes[entityTypeName] = entityType;
                if (previousLength == _entityTypes.Count)
                {
                    throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.DisplayName()));
                }
            }

            return ConventionDispatcher.OnEntityTypeAdded(entityType.Builder)?.Metadata;
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
            => _clrTypeMap.TryGetValue(Check.NotNull(type, nameof(type)), out var entityType)
                ? entityType
                : FindEntityType(type.DisplayName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] string name)
            => _entityTypes.TryGetValue(Check.NotEmpty(name, nameof(name)), out var entityType)
                ? entityType
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] Type type)
            => RemoveEntityType(FindEntityType(type));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] string name)
            => RemoveEntityType(FindEntityType(name));

        private static void AssertCanRemove(EntityType entityType)
        {
            var foreignKey = entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.PrincipalEntityType != entityType);
            if (foreignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByForeignKey(
                        entityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        Property.Format(foreignKey.Properties)));
            }

            var referencingForeignKey = entityType.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByReferencingForeignKey(
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
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType([CanBeNull] EntityType entityType)
        {
            if (entityType?.Builder == null)
            {
                return null;
            }

            AssertCanRemove(entityType);

            var entityTypeName = entityType.Name;
            if (entityType.HasDefiningNavigation())
            {
                if (!_entityTypesWithDefiningNavigation.TryGetValue(entityTypeName, out var entityTypesWithSameType))
                {
                    return null;
                }

                var removed = entityTypesWithSameType.Remove(entityType);
                Debug.Assert(removed);

                if (entityTypesWithSameType.Count == 0)
                {
                    _entityTypesWithDefiningNavigation.Remove(entityTypeName);
                }
            }
            else
            {
                var removed = _entityTypes.Remove(entityTypeName);
                Debug.Assert(removed);

                if (entityType.ClrType != null)
                {
                    removed = _clrTypeMap.Remove(entityType.ClrType);
                    Debug.Assert(removed);
                }
            }

            entityType.Builder = null;
            return entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, definingNavigationName, definingEntityType, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, definingNavigationName, definingEntityType, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasEntityTypeWithDefiningNavigation([NotNull] Type clrType)
            => _entityTypesWithDefiningNavigation.ContainsKey(clrType.DisplayName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasEntityTypeWithDefiningNavigation([NotNull] string name)
            => _entityTypesWithDefiningNavigation.ContainsKey(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => FindEntityType(type.DisplayName(), definingNavigationName, definingEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] string definingEntityTypeName)
        {
            if (!_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType))
            {
                return null;
            }

            return entityTypesWithSameType
                .FirstOrDefault(e => e.DefiningNavigationName == definingNavigationName
                                     && e.DefiningEntityType.Name == definingEntityTypeName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
        {
            if (!_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType))
            {
                return null;
            }

            return entityTypesWithSameType
                .FirstOrDefault(e => e.DefiningNavigationName == definingNavigationName && e.DefiningEntityType == definingEntityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyCollection<EntityType> GetEntityTypes([NotNull] Type type)
            => GetEntityTypes(type.DisplayName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyCollection<EntityType> GetEntityTypes([NotNull] string name)
        {
            if (_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType))
            {
                return entityTypesWithSameType;
            }

            var entityType = FindEntityType(name);
            return entityType == null ? new EntityType[0] : new[] { entityType };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(type, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType RemoveEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(name, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore(
            [NotNull] Type type,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => Ignore(Check.NotNull(type, nameof(type)).DisplayName(), type, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Ignore(
            [NotNull] string name,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => Ignore(Check.NotNull(name, nameof(name)), null, configurationSource);

        private void Ignore(
            [NotNull] string name,
            [CanBeNull] Type type,
            ConfigurationSource configurationSource)
        {
            if (_ignoredTypeNames.TryGetValue(name, out var existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
                _ignoredTypeNames[name] = configurationSource;
                return;
            }

            _ignoredTypeNames[name] = configurationSource;

            ConventionDispatcher.OnEntityTypeIgnored(Builder, name, type);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredTypeConfigurationSource([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindIgnoredTypeConfigurationSource(type.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredTypeConfigurationSource([NotNull] string name)
            => _ignoredTypeNames.TryGetValue(Check.NotEmpty(name, nameof(name)), out var ignoredConfigurationSource)
                ? (ConfigurationSource?)ignoredConfigurationSource
                : null;

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
            _ignoredTypeNames.Remove(name);
        }

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
            => ConventionDispatcher.OnModelAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Validate() => ConventionDispatcher.OnModelBuilt(Builder);

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IMutableEntityType IMutableModel.AddEntityType(Type type) => AddEntityType(type);
        IMutableEntityType IMutableModel.AddQueryType(Type type) => AddQueryType(type);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);

        IEntityType IModel.FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        IMutableEntityType IMutableModel.FindEntityType(
            string name, string definingNavigationName, IMutableEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        IMutableEntityType IMutableModel.AddEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        IMutableEntityType IMutableModel.AddEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(type, definingNavigationName, (EntityType)definingEntityType);

        IMutableEntityType IMutableModel.RemoveEntityType(
            string name, string definingNavigationName, IMutableEntityType definingEntityType)
            => RemoveEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Model> DebugView
            => new DebugView<Model>(this, m => m.ToDebugString());
    }
}
