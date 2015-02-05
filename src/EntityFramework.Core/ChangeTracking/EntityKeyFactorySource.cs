// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyFactorySource
    {
        private readonly ThreadSafeDictionaryCache<IProperty, EntityKeyFactory> _cache
            = new ThreadSafeDictionaryCache<IProperty, EntityKeyFactory>();

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties)
        {
            Check.NotNull(keyProperties, nameof(keyProperties));

            if (keyProperties.Count == 1)
            {
                var keyProperty = keyProperties[0];
                var keyType = keyProperty.PropertyType;

                // Use composite key for anything with structural (e.g. byte[]) properties even if they are
                // not composite because it is setup to do structural comparisons and the generic typing
                // advantages of the simple key don't really apply anyway.
                if (!typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(keyType.GetTypeInfo()))
                {
                    return _cache.GetOrAdd(
                        keyProperty,
                        t =>
                            {
                                var sentinel = keyProperty.SentinelValue;

                                return (EntityKeyFactory)(keyType.IsNullableType()
                                    ? Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(keyType.UnwrapNullableType()), sentinel)
                                    : Activator.CreateInstance(typeof(SimpleEntityKeyFactory<>).MakeGenericType(keyType), sentinel));
                            });
                }
            }

            // Consider caching these factories for perf
            return new CompositeEntityKeyFactory(keyProperties.Select(k => k.SentinelValue).ToList());
        }
    }
}
