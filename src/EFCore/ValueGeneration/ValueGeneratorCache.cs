// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     <para>
    ///         Keeps a cache of value generators for properties.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public class ValueGeneratorCache : IValueGeneratorCache
    {
        private static readonly bool _useOldBehavior31539 =
            AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue31539", out var enabled31539) && enabled31539;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public ValueGeneratorCache(ValueGeneratorCacheDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ValueGeneratorCacheDependencies Dependencies { get; }

        private readonly ConcurrentDictionary<CacheKey, ValueGenerator> _cache = new();

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly Guid _modelId;
            private readonly string? _property;
            private readonly string? _entityType;

            public CacheKey(IProperty property, IEntityType entityType)
            {
                if (_useOldBehavior31539)
                {
                    _modelId = default;
                    _property = null;
                    _entityType = null;
                    Property = property;
                    EntityType = entityType;
                }
                else
                {
                    _modelId = entityType.Model.ModelId;
                    _property = property.Name;
                    _entityType = entityType.Name;
                    Property = null;
                    EntityType = null;
                }
            }

            public IProperty? Property { get; }

            public IEntityType? EntityType { get; }

            public bool Equals(CacheKey other)
                => _useOldBehavior31539
                    ? Property!.Equals(other.Property) && EntityType!.Equals(other.EntityType)
                    : (_property!.Equals(other._property, StringComparison.Ordinal)
                        && _entityType!.Equals(other._entityType, StringComparison.Ordinal)
                        && _modelId.Equals(other._modelId));

            public override bool Equals(object? obj)
                => obj is CacheKey cacheKey && Equals(cacheKey);

            public override int GetHashCode()
                => _useOldBehavior31539
                    ? HashCode.Combine(Property!, EntityType!)
                    : HashCode.Combine(_property!, _entityType!, _modelId);
        }

        /// <summary>
        ///     Gets the existing value generator from the cache, or creates a new one if one is not present in
        ///     the cache.
        /// </summary>
        /// <param name="property">The property to get the value generator for.</param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
        ///     this entity type may be different from the declared entity type on <paramref name="property" />
        /// </param>
        /// <param name="factory">Factory to create a new value generator if one is not present in the cache.</param>
        /// <returns>The existing or newly created value generator.</returns>
        public virtual ValueGenerator GetOrAdd(
            IProperty property,
            IEntityType entityType,
            Func<IProperty, IEntityType, ValueGenerator> factory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(factory, nameof(factory));

            return _cache.GetOrAdd(
                new CacheKey(property, entityType), static (ck, p) => p.factory(p.property, p.entityType), (factory, entityType, property));
        }
    }
}
