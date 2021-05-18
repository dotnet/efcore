// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
    public class Model : ConventionAnnotatable, IMutableModel, IConventionModel, IRuntimeModel
    {
        /// <summary>
        ///     The CLR type that is used for property bag entity types when no other type is specified.
        /// </summary>
        public static readonly Type DefaultPropertyBagType = typeof(Dictionary<string, object>);

        private readonly SortedDictionary<string, EntityType> _entityTypes = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Type, PropertyInfo?> _indexerPropertyInfoMap = new();
        private readonly ConcurrentDictionary<Type, string> _clrTypeNameMap = new();
        private readonly Dictionary<string, ConfigurationSource> _ignoredTypeNames = new(StringComparer.Ordinal);
        private readonly Dictionary<Type, (ConfigurationSource ConfigurationSource, SortedSet<EntityType> Types)> _sharedTypes =
            new() { { DefaultPropertyBagType, (ConfigurationSource.Convention, new SortedSet<EntityType>(EntityTypeFullNameComparer.Instance)) } };

        private ConventionDispatcher? _conventionDispatcher;
        private IList<IModelFinalizedConvention>? _modelFinalizedConventions;
        private bool? _skipDetectChanges;
        private ChangeTrackingStrategy? _changeTrackingStrategy;

        private ConfigurationSource? _changeTrackingStrategyConfigurationSource;
        private ModelDependencies? _scopedModelDependencies;

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
        public Model(ConventionSet conventions, ModelDependencies? modelDependencies = null)
        {
            if (modelDependencies != null)
            {
                ScopedModelDependencies = modelDependencies;
            }
            var dispatcher = new ConventionDispatcher(conventions);
            var builder = new InternalModelBuilder(this);
            _conventionDispatcher = dispatcher;
            _modelFinalizedConventions = conventions.ModelFinalizedConventions;
            Builder = builder;
            dispatcher.OnModelInitialized(builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConventionDispatcher ConventionDispatcher
        {
            [DebuggerStepThrough]
            get => _conventionDispatcher ?? throw new InvalidOperationException(CoreStrings.ModelReadOnly);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DisallowNull]
        public virtual ModelDependencies? ScopedModelDependencies
        {
            get => _scopedModelDependencies;
            set => _scopedModelDependencies = value;
        }

        /// <summary>
        ///     Indicates whether the model is read-only.
        /// </summary>
        public override bool IsReadOnly => _conventionDispatcher == null;

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
            => _entityTypes.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? AddEntityType(
            string name,
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
        public virtual EntityType? AddEntityType(
            Type type,
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
        public virtual EntityType? AddEntityType(
            string name,
            Type type,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            if (GetDisplayName(type) == name)
            {
                throw new InvalidOperationException(CoreStrings.AmbiguousSharedTypeEntityTypeName(name));
            }

            var entityType = new EntityType(name, type, this, configurationSource);

            return AddEntityType(entityType);
        }

        private EntityType? AddEntityType(EntityType entityType)
        {
            EnsureMutable();

            var entityTypeName = entityType.Name;
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

                if (_sharedTypes.TryGetValue(entityType.ClrType, out var existingTypes))
                {
                    var newConfigurationSource = entityType.GetConfigurationSource().Max(existingTypes.ConfigurationSource);
                    existingTypes.Types.Add(entityType);
                    _sharedTypes[entityType.ClrType] = (newConfigurationSource, existingTypes.Types);
                }
                else
                {
                    var types = new SortedSet<EntityType>(EntityTypeFullNameComparer.Instance) { entityType };
                    _sharedTypes.Add(entityType.ClrType, (entityType.GetConfigurationSource(), types));
                }
            }
            else if (_sharedTypes.ContainsKey(entityType.ClrType))
            {
                throw new InvalidOperationException(CoreStrings.ClashingSharedType(entityType.DisplayName()));
            }

            _entityTypes.Add(entityTypeName, entityType);

            return (EntityType?)ConventionDispatcher.OnEntityTypeAdded(entityType.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? FindEntityType(Type type)
            => FindEntityType(GetDisplayName(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? FindEntityType(string name)
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
        public virtual EntityType? RemoveEntityType(Type type)
            => RemoveEntityType(FindEntityType(type));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? RemoveEntityType(string name)
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
        public virtual EntityType? RemoveEntityType(EntityType? entityType)
        {
            if (entityType is null || !entityType.IsInModel)
            {
                return null;
            }

            EnsureMutable();
            AssertCanRemove(entityType);

            if (_sharedTypes.TryGetValue(entityType.ClrType, out var existingTypes))
            {
                existingTypes.Types.Remove(entityType);
            }

            var removed = _entityTypes.Remove(entityType.Name);
            Check.DebugAssert(removed, "removed is false");

            entityType.OnTypeRemoved();

            return entityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? AddEntityType(
            string name,
            string definingNavigationName,
            EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            name = definingEntityType.GetOwnedName(name, definingNavigationName);
            var entityType = new EntityType(name, DefaultPropertyBagType, this, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? AddEntityType(
            Type type,
            string definingNavigationName,
            EntityType definingEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(type, nameof(type));

            var name = definingEntityType.GetOwnedName(type.ShortDisplayName(), definingNavigationName);
            var entityType = new EntityType(name, type, this, configurationSource);

            return AddEntityType(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual string GetDisplayName(Type type)
            => _clrTypeNameMap.GetOrAdd(type, t => t.DisplayName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? FindEntityType(
            Type type,
            string definingNavigationName,
            EntityType definingEntityType)
            => FindEntityType(type.ShortDisplayName(), definingNavigationName, definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? FindEntityType(
            string name,
            string definingNavigationName,
            EntityType definingEntityType)
            => FindEntityType(definingEntityType.GetOwnedName(name, definingNavigationName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? FindActualEntityType(EntityType entityType)
            => entityType.IsInModel
                ? entityType
                : FindEntityType(entityType.Name)
                    ?? (entityType.HasSharedClrType
                        ? entityType.FindOwnership() is ForeignKey ownership
                            ? FindActualEntityType(ownership.PrincipalEntityType)
                                ?.FindNavigation(ownership.PrincipalToDependent!.Name)?.TargetEntityType
                            : null
                        : null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type? FindClrType(string name)
            => _entityTypes.TryGetValue(name, out var entityType)
                ? entityType.HasSharedClrType
                    ? null
                    : entityType.ClrType
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<EntityType> FindEntityTypes(Type type)
        {
            var result = GetEntityTypes(GetDisplayName(type));
            return _sharedTypes.TryGetValue(type, out var existingTypes)
                ? result.Concat(existingTypes.Types)
                : result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyCollection<EntityType> GetEntityTypes(string name)
        {
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
        public virtual EntityType? RemoveEntityType(
            Type type,
            string definingNavigationName,
            EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(type, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? RemoveEntityType(
            string name,
            string definingNavigationName,
            EntityType definingEntityType)
            => RemoveEntityType(FindEntityType(name, definingNavigationName, definingEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsShared(Type type)
            => _sharedTypes.ContainsKey(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AddIgnored(
            Type type,
            ConfigurationSource configurationSource)
            => AddIgnored(GetDisplayName(Check.NotNull(type, nameof(type))), type, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AddIgnored(
            string name,
            ConfigurationSource configurationSource)
            => AddIgnored(Check.NotNull(name, nameof(name)), null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? AddIgnored(
            string name,
            Type? type,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();

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
        public virtual ConfigurationSource? FindIgnoredConfigurationSource(Type type)
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
        public virtual bool IsIgnored(Type type)
            => FindIgnoredConfigurationSource(GetDisplayName(type)) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? RemoveIgnored(Type type)
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
        public virtual string? RemoveIgnored(string name)
        {
            Check.NotNull(name, nameof(name));
            EnsureMutable();

            return _ignoredTypeNames.Remove(name) ? name : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOwned(Type type)
            => FindIsOwnedConfigurationSource(type) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? FindIsOwnedConfigurationSource(Type type)
        {
            if (this[CoreAnnotationNames.OwnedTypes] is not Dictionary<string, ConfigurationSource> ownedTypes)
            {
                return null;
            }

            var currentType = type;

            while (currentType != null)
            {
                if (ownedTypes.TryGetValue(GetDisplayName(currentType), out var configurationSource))
                {
                    return configurationSource;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddOwned(Type type, ConfigurationSource configurationSource)
        {
            EnsureMutable();
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
        public virtual string? RemoveOwned(Type type)
        {
            EnsureMutable();

            if (this[CoreAnnotationNames.OwnedTypes] is not Dictionary<string, ConfigurationSource> ownedTypes)
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
        public virtual void AddShared(Type type, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            if (_entityTypes.Any(et => !et.Value.HasSharedClrType && et.Value.ClrType == type))
            {
                throw new InvalidOperationException(CoreStrings.CannotMarkShared(type.ShortDisplayName()));
            }

            if (_sharedTypes.TryGetValue(type, out var existingTypes))
            {
                _sharedTypes[type] = (configurationSource.Max(existingTypes.ConfigurationSource), existingTypes.Types);
            }
            else
            {
                _sharedTypes.Add(type, (configurationSource, new SortedSet<EntityType>(EntityTypeFullNameComparer.Instance)));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessMode GetPropertyAccessMode()
            => (PropertyAccessMode?)this[CoreAnnotationNames.PropertyAccessMode]
                ?? DefaultPropertyAccessMode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const PropertyAccessMode DefaultPropertyAccessMode = PropertyAccessMode.PreferField;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyAccessMode? SetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
            => (PropertyAccessMode?)SetOrRemoveAnnotation(
                CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource)?.Value;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetPropertyAccessModeConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();

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
            EnsureMutable();

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
        protected override IConventionAnnotation? OnAnnotationSet(
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
            => ConventionDispatcher.OnModelAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionBatch DelayConventions()
        {
            EnsureMutable();
            return ConventionDispatcher.DelayConventions();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual T Track<T>(Func<T> func, [DisallowNull] ref IConventionForeignKey? foreignKey)
        {
            EnsureMutable();
            return ConventionDispatcher.Track(func, ref foreignKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel FinalizeModel()
        {
            EnsureMutable();
            ConventionDispatcher.AssertNoScope();

            var finalizedModel = (IModel)ConventionDispatcher.OnModelFinalizing(Builder).Metadata;

            if (finalizedModel is Model model)
            {
                finalizedModel = model.MakeReadonly();
            }

            return finalizedModel;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel OnModelFinalized()
        {
            IModel model = this;
            foreach (var modelConvention in _modelFinalizedConventions!)
            {
                model = modelConvention.ProcessModelFinalized(model);
            }

            _modelFinalizedConventions = null;

            return model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Model MakeReadonly()
        {
            // ConventionDispatcher should never be accessed once the model is made read-only.
            _conventionDispatcher = null;
            _scopedModelDependencies = null;
            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyInfo? FindIndexerPropertyInfo(Type type)
            => _indexerPropertyInfoMap.GetOrAdd(type, type.FindIndexerProperty());

        /// <summary>
        ///     Gets a value indicating whether the given MethodInfo reprensents an indexer access.
        /// </summary>
        /// <param name="methodInfo"> The MethodInfo to check for. </param>
        public virtual bool IsIndexerMethod(MethodInfo methodInfo)
            => !methodInfo.IsStatic
                && methodInfo.IsSpecialName
                && methodInfo.DeclaringType != null
                && FindIndexerPropertyInfo(methodInfo.DeclaringType) is PropertyInfo indexerProperty
                && (methodInfo == indexerProperty.GetMethod || methodInfo == indexerProperty.SetMethod);

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
            EnsureMutable();

            _skipDetectChanges = skipDetectChanges;

            return skipDetectChanges;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object? RelationalModel
            => ((IAnnotatable)this).FindRuntimeAnnotationValue("Relational:RelationalModel");

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new(
                () => ((IReadOnlyModel)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IReadOnlyModel)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

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
        [DebuggerStepThrough]
        void IMutableModel.SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode)
            => SetPropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        PropertyAccessMode? IConventionModel.SetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => SetPropertyAccessMode(
                propertyAccessMode,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableModel.SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy)
            => SetChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        ChangeTrackingStrategy? IConventionModel.SetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation)
            => SetChangeTrackingStrategy(
                changeTrackingStrategy,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(Type type)
            => FindEntityType(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(Type type)
            => FindEntityType(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name, string definingNavigationName, IReadOnlyEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.FindEntityType(
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
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.FindEntityType(
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
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(
            string name,
            string definingNavigationName,
            IEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(
            Type type,
            string definingNavigationName,
            IReadOnlyEntityType definingEntityType)
            => FindEntityType(type, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionEntityType> IConventionModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> IModel.GetEntityTypes()
            => GetEntityTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyModel.FindEntityTypes(Type type)
            => FindEntityTypes(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> IModel.FindEntityTypes(Type type)
            => FindEntityTypes(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType IMutableModel.AddEntityType(string name)
            => AddEntityType(name, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.AddEntityType(string name, bool fromDataAnnotation)
            => AddEntityType(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType IMutableModel.AddEntityType(Type type)
            => AddEntityType(type, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.AddEntityType(Type type, bool fromDataAnnotation)
            => AddEntityType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType IMutableModel.AddEntityType(string name, Type type)
            => AddEntityType(name, type, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.AddEntityType(string name, Type type, bool fromDataAnnotation)
            => AddEntityType(name, type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType IMutableModel.AddEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(name, definingNavigationName, (EntityType)definingEntityType, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Obsolete]
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.AddEntityType(
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
        [DebuggerStepThrough]
        IMutableEntityType IMutableModel.AddEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => AddEntityType(type, definingNavigationName, (EntityType)definingEntityType, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Obsolete]
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.AddEntityType(
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
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.RemoveEntityType(IMutableEntityType entityType)
            => RemoveEntityType((EntityType)entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.RemoveEntityType(IConventionEntityType entityType)
            => RemoveEntityType((EntityType)entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.RemoveEntityType(Type type)
            => RemoveEntityType(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.RemoveEntityType(Type type)
            => RemoveEntityType(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.RemoveEntityType(
            Type type, string definingNavigationName, IMutableEntityType definingEntityType)
            => RemoveEntityType(type, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.RemoveEntityType(
            Type type, string definingNavigationName, IConventionEntityType definingEntityType)
            => RemoveEntityType(type, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.RemoveEntityType(string name)
            => RemoveEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.RemoveEntityType(string name)
            => RemoveEntityType(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableEntityType? IMutableModel.RemoveEntityType(
            string name, string definingNavigationName, IMutableEntityType definingEntityType)
            => RemoveEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityType? IConventionModel.RemoveEntityType(
            string name, string definingNavigationName, IConventionEntityType definingEntityType)
            => RemoveEntityType(name, definingNavigationName, (EntityType)definingEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableModel.AddShared(Type type)
            => AddShared(type, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IConventionModel.AddShared(Type type, bool fromDataAnnotation)
            => AddShared(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableModel.AddOwned(Type type)
            => AddOwned(type, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IConventionModel.AddOwned(Type type, bool fromDataAnnotation)
            => AddOwned(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        string IMutableModel.AddIgnored(string name)
            => AddIgnored(name, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        string? IConventionModel.AddIgnored(string name, bool fromDataAnnotation)
            => AddIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        string IMutableModel.AddIgnored(Type type)
            => AddIgnored(type, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        string? IConventionModel.AddIgnored(Type type, bool fromDataAnnotation)
            => AddIgnored(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
