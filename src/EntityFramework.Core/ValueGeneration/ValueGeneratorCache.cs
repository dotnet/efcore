// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGeneratorCache : IValueGeneratorCache
    {
        private readonly ThreadSafeDictionaryCache<CacheKey, ValueGenerator> _cache
            = new ThreadSafeDictionaryCache<CacheKey, ValueGenerator>();

        private struct CacheKey
        {
            public CacheKey(IProperty property, IEntityType entityType)
            {
                Property = property;
                EntityType = entityType;
            }

            public IProperty Property { get; }

            public IEntityType EntityType { get; }

            private bool Equals(CacheKey other)
            {
                return Property.Equals(other.Property) && EntityType.Equals(other.EntityType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CacheKey && Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Property.GetHashCode() * 397) ^ EntityType.GetHashCode();
                }
            }
        }

        public virtual ValueGenerator GetOrAdd(
            IProperty property, IEntityType entityType, Func<IProperty, IEntityType, ValueGenerator> factory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(factory, nameof(factory));

            return _cache.GetOrAdd(new CacheKey(property, entityType), ck => factory(ck.Property, ck.EntityType));
        }
    }
}
