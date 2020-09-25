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
    /// <inheritdoc />
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

        private readonly struct CacheKey : IEquatable<CacheKey>
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

            public bool Equals(CacheKey other)
                => Property.Equals(other.Property) && EntityType.Equals(other.EntityType);

            public override bool Equals(object obj)
                => obj is CacheKey cacheKey && Equals(cacheKey);

            public override int GetHashCode()
                => HashCode.Combine(Property, EntityType);
        }

        /// <inheritdoc />
        public virtual ValueGenerator GetOrAdd(
            IProperty property,
            IEntityType entityType,
            Func<IProperty, IEntityType, ValueGenerator> factory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(factory, nameof(factory));

            return _cache.GetOrAdd(new CacheKey(property, entityType, factory), ck => ck.Factory(ck.Property, ck.EntityType));
        }
    }
}
