// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public abstract class ValueGeneratorCache : IValueGeneratorCache
    {
        private readonly ThreadSafeDictionaryCache<IProperty, ValueGenerator> _cache
            = new ThreadSafeDictionaryCache<IProperty, ValueGenerator>();

        public virtual ValueGenerator GetOrAdd(IProperty property, Func<IProperty, ValueGenerator> factory)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(factory, nameof(factory));

            return _cache.GetOrAdd(property, factory);
        }
    }
}
