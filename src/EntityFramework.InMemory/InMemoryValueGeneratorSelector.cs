// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector, IInMemoryValueGeneratorSelector
    {
        private readonly IInMemoryValueGeneratorCache _cache;
        private readonly InMemoryIntegerValueGeneratorFactory _inMemoryFactory;

        public InMemoryValueGeneratorSelector(
            [NotNull] IInMemoryValueGeneratorCache cache,
            [NotNull] ValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] InMemoryIntegerValueGeneratorFactory inMemoryFactory,
            [NotNull] TemporaryIntegerValueGeneratorFactory integerFactory,
            [NotNull] ValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] ValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(cache, nameof(cache));
            Check.NotNull(inMemoryFactory, nameof(inMemoryFactory));

            _cache = cache;
            _inMemoryFactory = inMemoryFactory;
        }

        public override ValueGenerator Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return _cache.GetOrAdd(property, Create);
        }

        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.PropertyType.IsInteger()
                ? _inMemoryFactory.Create(property)
                : base.Create(property);
        }
    }
}