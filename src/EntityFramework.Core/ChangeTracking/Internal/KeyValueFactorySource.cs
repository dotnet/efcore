// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class KeyValueFactorySource : IKeyValueFactorySource
    {
        private readonly ThreadSafeDictionaryCache<IKey, KeyValueFactory> _cache
            = new ThreadSafeDictionaryCache<IKey, KeyValueFactory>(ReferenceEqualityComparer.Instance);

        public virtual KeyValueFactory GetKeyFactory(IKey key)
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
                                return (KeyValueFactory)(Activator.CreateInstance(
                                    typeof(SimpleKeyValueFactory<>).MakeGenericType(keyType.UnwrapNullableType()), k));
                            }
                        }

                        return new CompositeKeyValueFactory(k);
                    });
    }
}
