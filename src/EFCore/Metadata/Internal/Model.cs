// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class Model : ConventionAnnotatable, IMutableModel, IConventionModel
    {
        /// <summary>
        ///     The CLR type that is used for property bag entity types when no other type is specified.
        /// </summary>
        public static readonly Type DefaultPropertyBagType = typeof(Dictionary<string, object>);

        private readonly SortedDictionary<string, EntityType> _entityTypes
            = new SortedDictionary<string, EntityType>(StringComparer.Ordinal);

        private readonly ConcurrentDictionary<Type, PropertyInfo> _indexerPropertyInfoMap
            = new ConcurrentDictionary<Type, PropertyInfo>();

        private readonly ConcurrentDictionary<Type, string> _clrTypeNameMap
            = new ConcurrentDictionary<Type, string>();

        private readonly SortedDictionary<string, SortedSet<EntityType>> _entityTypesWithDefiningNavigation
            = new SortedDictionary<string, SortedSet<EntityType>>(StringComparer.Ordinal);

        private readonly SortedDictionary<string, List<(string, string)>> _detachedEntityTypesWithDefiningNavigation
            = new SortedDictionary<string, List<(string, string)>>(StringComparer.Ordinal);

        private readonly Dictionary<string, ConfigurationSource> _ignoredTypeNames
            = new Dictionary<string, ConfigurationSource>(StringComparer.Ordinal);

        private readonly Dictionary<Type, ConfigurationSource> _sharedTypes =
            new Dictionary<Type, ConfigurationSource> { { DefaultPropertyBagType, ConfigurationSource.Convention } };

        private bool? _skipDetectChanges;
        private ChangeTrackingStrategy? _changeTrackingStrategy;

        private ConfigurationSource? _changeTrackingStrategyConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Model()
            : this(new ConventionSet())
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Model([NotNull] ConventionSet conventions, [CanBeNull] ModelDependencies modelDependencies = null)
        {
            ModelDependencies = modelDependencies;
            var dispatcher = new ConventionDispatcher(conventions);
            var builder = new InternalModelBuilder(this);
            ConventionDispatcher = dispatcher;
            Builder = builder;
            dispatcher.OnModelInitialized(builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConventionDispatcher ConventionDispatcher { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ModelDependencies ModelDependencies { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsReadonly
            => ConventionDispatcher == null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsValidated { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Builder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<EntityType> GetEntityTypes()
            => _entityTypes.Values.Concat(_entityTypesWithDefiningNavigation.Values.SelectMany(e => e));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            [NotNull] Type type,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(name, type, this, configurationSource);

            return AddEntityType(entityType);
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

                if (_detachedEntityTypesWithDefiningNavigation.TryGetValue(entityTypeName, out var detachedEntityTypes))
                {
                    for (var i = 0; i < detachedEntityTypes.Count; i++)
                    {
                        var (definingNavigation, definingEntityType) = detachedEntityTypes[i];
                        if (definingNavigation == entityType.DefiningNavigationName
                            && definingEntityType == entityType.DefiningEntityType.Name)
                        {
                            detachedEntityTypes.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (!_entityTypesWithDefiningNavigation.TryGetValue(entityTypeName, out var entityTypesWithSameType))
                {
                    entityTypesWithSameType = new SortedSet<EntityType>(EntityTypeFullNameComparer.Instance);
                    _entityTypesWithDefiningNavigation[entityTypeName] = entityTypesWithSameType;
                }

                var added = entityTypesWithSameType.Add(entityType);
                Check.DebugAssert(added, "added is false");
            }
            else
            {
                if (_entityTypesWithDefiningNavigation.ContainsKey(entityTypeName))
                {
                    throw new InvalidOperationException(CoreStrings.ClashingWeakEntityType(entityType.DisplayName()));
                }

                _detachedEntityTypesWithDefiningNavigation.Remove(entityTypeName);

                if (_entityTypes.ContainsKey(entityTypeName))
                {
                    throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.DisplayName()));
                }

                if (entityType.HasSharedClrType)
                {
                    if (_entityTypes.Any(et => !et.Value.HasSharedClrType && et.Value.ClrType == entityType.ClrType))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClashingNonSharedType(entityType.Name, entityType.ClrType.DisplayName()));
                    }

                    if (_sharedTypes.TryGetValue(entityType.ClrType, out var existingConfigurationSource))
                    {
                        _sharedTypes[entityType.ClrType] = entityType.GetConfigurationSource().Max(existingConfigurationSource);
                    }
                    else
                    {
                        _sharedTypes.Add(entityType.ClrType, entityType.GetConfigurationSource());
                    }
                }
                else if (entityType.ClrType != null
                    && _sharedTypes.ContainsKey(entityType.ClrType))
                {
                    throw new InvalidOperationException(CoreStrings.ClashingSharedType(entityType.DisplayName()));
                }

                _entityTypes.Add(entityTypeName, entityType);
            }

            return (EntityType)ConventionDispatcher.OnEntityTypeAdded(entityType.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] Type type)
            => FindEntityType(GetDisplayName(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.DebugAssert(!string.IsNullOrEmpty(name), "name is null or empty");
            return _entityTypes.TryGetValue(name, out var entityType)
                ? entityType
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] Type type)
            => RemoveEntityType(FindEntityType(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType RemoveEntityType([NotNull] string name)
            => RemoveEntityType(FindEntityType(name));

        private static void AssertCanRemove(EntityType entityType)
        {
            var referencingForeignKey = entityType.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByReferencingForeignKey(
                        entityType.DisplayName(),
                        referencingForeignKey.Properties.Format(),
                        referencingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var referencingSkipNavigation = entityType.GetDeclaredReferencingSkipNavigations().FirstOrDefault();
            if (referencingSkipNavigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByReferencingSkipNavigation(
                        entityType.DisplayName(),
                        referencingSkipNavigation.Name,
                        referencingSkipNavigation.DeclaringEntityType.DisplayName()));
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                Check.DebugAssert(removed, "removed is false");

                if (entityTypesWithSameType.Count == 0)
                {
                    _entityTypesWithDefiningNavigation.Remove(entityTypeName);
                }
            }
            else
            {
                var removed = _entityTypes.Remove(entityTypeName);
                Check.DebugAssert(removed, "removed is false");
            }

            entityType.OnTypeRemoved();

            return entityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this, definingNavigationName, definingEntityType, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType AddEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(type, nameof(type));

            var entityType = new EntityType(type, this, definingNavigationName, definingEntityType, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddDetachedEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] string definingEntityTypeName)
        {
            if (!_detachedEntityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType))
            {
                entityTypesWithSameType = new List<(string, string)>();
                _detachedEntityTypesWithDefiningNavigation[name] = entityTypesWithSameType;
            }

            entityTypesWithSameType.Add((definingNavigationName, definingEntityTypeName));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual string GetDisplayName([NotNull] Type type)
            => _clrTypeNameMap.GetOrAdd(type, t => t.DisplayName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasEntityTypeWithDefiningNavigation([NotNull] Type type)
            => HasEntityTypeWithDefiningNavigation(GetDisplayName(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasEntityTypeWithDefiningNavigation([NotNull] string name)
            => _entityTypesWithDefiningNavigation.ContainsKey(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasOtherEntityTypesWithDefiningNavigation([NotNull] EntityType entityType)
        {
            if (!entityType.HasDefiningNavigation())
            {
                return false;
            }

            if (_entityTypesWithDefiningNavigation.TryGetValue(entityType.Name, out var entityTypesWithSameType))
            {
                if (entityTypesWithSameType.Any(e => EntityTypeFullNameComparer.Instance.Compare(e, entityType) != 0))
                {
                    return true;
                }
            }

            if (_detachedEntityTypesWithDefiningNavigation.TryGetValue(entityType.Name, out var detachedEntityTypesWithSameType))
            {
                if (detachedEntityTypesWithSameType.Any(
                    e => e.Item1 != entityType.DefiningNavigationName || e.Item2 != entityType.DefiningEntityType.Name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EntityTypeShouldHaveDefiningNavigation([NotNull] Type type)
            => EntityTypeShouldHaveDefiningNavigation(new TypeIdentity(type, this));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EntityTypeShouldHaveDefiningNavigation([NotNull] string name)
            => EntityTypeShouldHaveDefiningNavigation(new TypeIdentity(name));

        private bool EntityTypeShouldHaveDefiningNavigation(in TypeIdentity type)
            => _entityTypesWithDefiningNavigation.ContainsKey(type.Name)
                || (_detachedEntityTypesWithDefiningNavigation.TryGetValue(type.Name, out var detachedTypes)
                    && detachedTypes.Count > 1);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType FindEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => FindEntityType(GetDisplayName(type), definingNavigationName, definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => !_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType)
                ? null
                : entityTypesWithSameType
                    .FirstOrDefault(e => e.DefiningNavigationName == definingNavigationName && e.DefiningEntityType == definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType FindActualEntityType([NotNull] EntityType entityType)
            => entityType.Builder != null
                ? entityType
                : (entityType.HasDefiningNavigation()
                    ? FindActualEntityType(entityType.DefiningEntityType)
                        ?.FindNavigation(entityType.DefiningNavigationName)?.TargetEntityType
                    : FindEntityType(entityType.Name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type FindClrType([NotNull] string name)
            => _entityTypes.TryGetValue(name, out var entityType)
                ? entityType.HasSharedClrType
                    ? null
                    : entityType.ClrType
                : (_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType)
                    ? entityTypesWithSameType.FirstOrDefault()?.ClrType
                    : null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyCollection<EntityType> GetEntityTypes([NotNull] Type type)
            => GetEntityTypes(GetDisplayName(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyCollection<EntityType> GetEntityTypes([NotNull] string name)
        {
            if (_entityTypesWithDefiningNavigation.TryGetValue(name, out var entityTypesWithSameType))
            {
                return entityTypesWithSameType;
            }

            var entityType = FindEntityType(name);
            return entityType == null
                ? Array.Empty<EntityType>()
                : new[] { entityType };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<EntityType> FindLeastDerivedEntityTypes(
            [NotNull] Type type,
            [CanBeNull] Func<EntityType, bool> condition = null)
        {
            var derivedLevels = new Dictionary<Type, int> { [type] = 0 };

            var leastDerivedTypesGroups = GetEntityTypes()
                .GroupBy(t => GetDerivedLevel(t.ClrType, derivedLevels))
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

            return new List<EntityType>();
        }

        private static int GetDerivedLevel(Type derivedType, Dictionary<Type, int> derivedLevels)
        {
            if (derivedType?.BaseType == null)
            {
                return int.MaxValue;
            }

            if (derivedLevels.TryGetValue(derivedType, out var level))
            {
                return level;
            }

            var baseType = derivedType.BaseType;
            level = GetDerivedLevel(baseType, derivedLevels);
            level += level == int.MaxValue ? 0 : 1;
            derivedLevels.Add(derivedType, level);
            return level;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType RemoveEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(type, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType RemoveEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(name, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string AddIgnored(
            [NotNull] Type type,
            ConfigurationSource configurationSource)
            => AddIgnored(GetDisplayName(Check.NotNull(type, nameof(type))), type, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string AddIgnored(
            [NotNull] string name,
            ConfigurationSource configurationSource)
            => AddIgnored(Check.NotNull(name, nameof(name)), null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string AddIgnored(
            [NotNull] string name,
            [CanBeNull] Type type,
            ConfigurationSource configurationSource)
        {
            if (_ignoredTypeNames.TryGetValue(name, out var existingIgnoredConfigurationSource))
            {
                configurationSource = configurationSource.Max(existingIgnoredConfigurationSource);
                _ignoredTypeNames[name] = configurationSource;
                return name;
            }

            _ignoredTypeNames[name] = configurationSource;

            if (type == null)
            {
                // This is to populate Type for convention when removing shared type entity type
                type = _entityTypes.TryGetValue(name, out var existingEntityType)
                    && existingEntityType.HasSharedClrType
                        ? existingEntityType.ClrType
                        : null;
            }

            return ConventionDispatcher.OnEntityTypeIgnored(Builder, name, type);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredConfigurationSource([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindIgnoredConfigurationSource(GetDisplayName(type));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindIgnoredConfigurationSource(string name)
            => _ignoredTypeNames.TryGetValue(Check.NotEmpty(name, nameof(name)), out var ignoredConfigurationSource)
                ? (ConfigurationSource?)ignoredConfigurationSource
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored(string name)
            => FindIgnoredConfigurationSource(name) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored([NotNull] Type type)
            => FindIgnoredConfigurationSource(GetDisplayName(type)) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsShared([NotNull] Type type)
            => _sharedTypes.ContainsKey(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string RemoveIgnored([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));
            return RemoveIgnored(GetDisplayName(type));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string RemoveIgnored(string name)
        {
            Check.NotNull(name, nameof(name));

            return _ignoredTypeNames.Remove(name) ? name : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOwned([NotNull] Type type)
            => FindIsOwnedConfigurationSource(type) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindIsOwnedConfigurationSource([NotNull] Type type)
        {
            if (!(this[CoreAnnotationNames.OwnedTypes] is Dictionary<string, ConfigurationSource> ownedTypes))
            {
                return null;
            }

            while (type != null)
            {
                if (ownedTypes.TryGetValue(GetDisplayName(type), out var configurationSource))
                {
                    return configurationSource;
                }

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddOwned([NotNull] Type type, ConfigurationSource configurationSource)
        {
            var name = GetDisplayName(type);
            if (!(this[CoreAnnotationNames.OwnedTypes] is Dictionary<string, ConfigurationSource> ownedTypes))
            {
                ownedTypes = new Dictionary<string, ConfigurationSource>(StringComparer.Ordinal);
                this[CoreAnnotationNames.OwnedTypes] = ownedTypes;
            }

            if (ownedTypes.TryGetValue(name, out var oldConfigurationSource))
            {
                ownedTypes[name] = configurationSource.Max(oldConfigurationSource);
                return;
            }

            ownedTypes.Add(name, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string RemoveOwned([NotNull] Type type)
        {
            if (!(this[CoreAnnotationNames.OwnedTypes] is Dictionary<string, ConfigurationSource> ownedTypes))
            {
                return null;
            }

            var name = GetDisplayName(type);
            return ownedTypes.Remove(name) ? name : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddShared([NotNull] Type type, ConfigurationSource configurationSource)
        {
            if (_entityTypes.Any(et => !et.Value.HasSharedClrType && et.Value.ClrType == type))
            {
                throw new InvalidOperationException(CoreStrings.CannotMarkShared(type.ShortDisplayName()));
            }

            if (_sharedTypes.TryGetValue(type, out var existingConfigurationSource))
            {
                _sharedTypes[type] = configurationSource.Max(existingConfigurationSource);
            }
            else
            {
                _sharedTypes.Add(type, configurationSource);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessMode? SetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

            return propertyAccessMode;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ChangeTrackingStrategy GetChangeTrackingStrategy()
            => _changeTrackingStrategy ?? ChangeTrackingStrategy.Snapshot;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ChangeTrackingStrategy? SetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
        {
            _changeTrackingStrategy = changeTrackingStrategy;

            _changeTrackingStrategyConfigurationSource = _changeTrackingStrategy == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_changeTrackingStrategyConfigurationSource);

            return changeTrackingStrategy;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetChangeTrackingStrategyConfigurationSource()
            => _changeTrackingStrategyConfigurationSource;

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation)
            => ConventionDispatcher.OnModelAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel FinalizeModel()
        {
            ConventionDispatcher.AssertNoScope();
            IModel finalizedModel = ConventionDispatcher.OnModelFinalizing(Builder)?.Metadata;
            if (finalizedModel != null)
            {
                finalizedModel = ConventionDispatcher.OnModelFinalized(finalizedModel);
            }

            return (finalizedModel as Model)?.MakeReadonly() ?? finalizedModel;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private IModel MakeReadonly()
        {
            ConventionDispatcher = null;
            ModelDependencies = null;
            IsValidated = true;
            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyInfo FindIndexerPropertyInfo([NotNull] Type type)
            => _indexerPropertyInfoMap.GetOrAdd(type, type.FindIndexerProperty());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool SkipDetectChanges
        {
            get => _skipDetectChanges ?? false;
            set => SetSkipDetectChanges(value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetSkipDetectChanges(bool? skipDetectChanges)
        {
            _skipDetectChanges = skipDetectChanges;

            return skipDetectChanges;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object RelationalModel
            => this["Relational:RelationalModel"];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IEntityType> IModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.AddEntityType(string name)
            => AddEntityType(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.AddEntityType(Type type)
            => AddEntityType(type, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.AddEntityType(string name, Type type)
            => AddEntityType(name, type, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.RemoveEntityType(IMutableEntityType entityType)
            => RemoveEntityType((EntityType)entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IModel.FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.FindEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.AddEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(name, definingNavigationName, (EntityType)definingEntityType, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableModel.AddEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(type, definingNavigationName, (EntityType)definingEntityType, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        string IMutableModel.AddIgnored(string name)
            => AddIgnored(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModelBuilder IConventionModel.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionAnnotatableBuilder IConventionAnnotatable.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.FindEntityType(
            string name,
            string definingNavigationName,
            IConventionEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.AddEntityType(string name, bool fromDataAnnotation)
            => AddEntityType(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.AddEntityType(Type type, bool fromDataAnnotation)
            => AddEntityType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.AddEntityType(string name, Type type, bool fromDataAnnotation)
            => AddEntityType(name, type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.AddEntityType(
            string name,
            string definingNavigationName,
            IConventionEntityType definingEntityType,
            bool fromDataAnnotation)
            => AddEntityType(
                name, definingNavigationName, (EntityType)definingEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.AddEntityType(
            Type type,
            string definingNavigationName,
            IConventionEntityType definingEntityType,
            bool fromDataAnnotation)
            => AddEntityType(
                type, definingNavigationName, (EntityType)definingEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionModel.RemoveEntityType(IConventionEntityType entityType)
            => RemoveEntityType((EntityType)entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionEntityType> IConventionModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        string IConventionModel.AddIgnored(string name, bool fromDataAnnotation)
            => AddIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionModel.IsShared(Type type)
            => IsShared(type);
    }
}
