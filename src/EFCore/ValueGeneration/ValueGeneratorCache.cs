// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
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
    public class ValueGeneratorCache : IValueGeneratorCache
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        private readonly ConcurrentDictionary<CacheKey, ValueGenerator> _cache
            = new ConcurrentDictionary<CacheKey, ValueGenerator>();

        private readonly struct CacheKey
        {
            public CacheKey(IProperty property, IEntityType entityType, Func<IProperty, IEntityType, ValueGenerator> factory)
            {
                Property = property;
                EntityType = entityType;
                Factory = factory;
            }

            public IProperty Property { get; }

            public IEntityType EntityType { get; }

            public Func<IProperty, IEntityType, ValueGenerator> Factory { get; }

            private bool Equals(CacheKey other)
                => Property.Equals(other.Property) && EntityType.Equals(other.EntityType);

            public override bool Equals(object obj)
            {
                return obj is null ? false : obj is CacheKey cacheKey && Equals(cacheKey);
            }

            public override int GetHashCode() => HashCode.Combine(Property, EntityType);
        }

        /// <summary>
        ///     Gets the existing value generator from the cache, or creates a new one if one is not present in
        ///     the cache.
        /// </summary>
        /// <param name="property"> The property to get the value generator for. </param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
        ///     this entity type may be different from the declared entity type on <paramref name="property" />
        /// </param>
        /// <param name="factory"> Factory to create a new value generator if one is not present in the cache. </param>
        /// <returns> The existing or newly created value generator. </returns>
        public virtual ValueGenerator GetOrAdd(
            IProperty property, IEntityType entityType, Func<IProperty, IEntityType, ValueGenerator> factory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(factory, nameof(factory));

            return _cache.GetOrAdd(new CacheKey(property, entityType, factory), ck => ck.Factory(ck.Property, ck.EntityType));
        }
    }
}
