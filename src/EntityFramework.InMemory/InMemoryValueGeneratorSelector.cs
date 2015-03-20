// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector, IInMemoryValueGeneratorSelector
    {
        private readonly IInMemoryValueGeneratorCache _cache;
        private readonly InMemoryIntegerValueGeneratorFactory _inMemoryFactory = new InMemoryIntegerValueGeneratorFactory();

        public InMemoryValueGeneratorSelector([NotNull] IInMemoryValueGeneratorCache cache)
        {
            Check.NotNull(cache, nameof(cache));

            _cache = cache;
        }

        public override ValueGenerator Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return _cache.GetOrAdd(property, Create);
        }

        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.ClrType.IsInteger()
                ? _inMemoryFactory.Create(property)
                : base.Create(property);
        }
    }
}
