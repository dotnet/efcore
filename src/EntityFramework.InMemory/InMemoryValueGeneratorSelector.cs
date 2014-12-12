// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<InMemoryValueGenerator> _inMemoryFactory;

        public InMemoryValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<InMemoryValueGenerator> inMemoryFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator> integerFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory)
            : base(guidFactory, integerFactory, stringFactory, binaryFactory)
        {
            Check.NotNull(inMemoryFactory, "inMemoryFactory");

            _inMemoryFactory = inMemoryFactory;
        }

        public override IValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.PropertyType.IsInteger() 
                ? _inMemoryFactory 
                : base.Select(property);
        }
    }
}
