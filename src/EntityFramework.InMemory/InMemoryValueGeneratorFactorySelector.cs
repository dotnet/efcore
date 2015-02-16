// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorFactorySelector : ValueGeneratorFactorySelector
    {
        private readonly InMemoryIntegerValueGeneratorFactory _inMemoryFactory;

        public InMemoryValueGeneratorFactorySelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] InMemoryIntegerValueGeneratorFactory inMemoryFactory,
            [NotNull] TemporaryIntegerValueGeneratorFactory integerFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(inMemoryFactory, nameof(inMemoryFactory));

            _inMemoryFactory = inMemoryFactory;
        }

        public override ValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.PropertyType.IsInteger()
                ? _inMemoryFactory
                : base.Select(property);
        }
    }
}