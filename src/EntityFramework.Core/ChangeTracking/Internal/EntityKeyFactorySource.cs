// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityKeyFactorySource : IEntityKeyFactorySource
    {
        private readonly ThreadSafeDictionaryCache<IKey, EntityKeyFactory> _cache
            = new ThreadSafeDictionaryCache<IKey, EntityKeyFactory>(ReferenceEqualityComparer.Instance);

        public virtual EntityKeyFactory GetKeyFactory(IKey key)
            => _cache.GetOrAdd(
                key,
                k =>
                    {
                        if (k.Properties.Count == 1)
                        {
                            var keyProperty = k.Properties[0];
                            var keyType = keyProperty.ClrType;

                            // Use composite key for anything with structural (e.g. byte[]) properties even if they are
                            // not composite because it is setup to do structural comparisons and the generic typing
                            // advantages of the simple key don't really apply anyway.
                            if (!typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(keyType.GetTypeInfo()))
                            {
                                var sentinel = keyProperty.SentinelValue;

                                return (EntityKeyFactory)(sentinel == null
                                    ? Activator.CreateInstance(
                                        typeof(SimpleNullSentinelEntityKeyFactory<>).MakeGenericType(keyType.UnwrapNullableType()), k)
                                    : Activator.CreateInstance(
                                        typeof(SimpleEntityKeyFactory<>).MakeGenericType(keyType.UnwrapNullableType()), k, sentinel));
                            }
                        }

                        return new CompositeEntityKeyFactory(k);
                    });
    }
}
