// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ValueGeneratorCache
    {
        private readonly ValueGeneratorSelector _selector;

        private readonly ThreadSafeDictionaryCache<string, IValueGeneratorPool> _cache
            = new ThreadSafeDictionaryCache<string, IValueGeneratorPool>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ValueGeneratorCache()
        {
        }

        public ValueGeneratorCache([NotNull] ValueGeneratorSelector selector)
        {
            Check.NotNull(selector, "selector");

            _selector = selector;
        }

        public virtual IValueGenerator GetGenerator([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var factory = _selector.Select(property);
            var pool = _cache.GetOrAdd(factory.GetCacheKey(property), k => CreatePool(property, factory));

            return pool.GetGenerator();
        }

        private static IValueGeneratorPool CreatePool(IProperty property, IValueGeneratorFactory factory)
        {
            var poolSize = factory.GetPoolSize(property);
            return poolSize == 1
                ? (IValueGeneratorPool)new SingleValueGeneratorPool(factory, property)
                : new ValueGeneratorPool(factory, property, poolSize);
        }
    }
}
