// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration.Internal
{
    public class SingleValueGeneratorPoolTest
    {
        [Fact]
        public void Returns_the_same_instance_every_time()
        {
            var pool = new SingleValueGeneratorPool(new ValueGeneratorFactory<TemporaryIntegerValueGenerator>(), Mock.Of<IProperty>());

            var generator = pool.GetGenerator();
            Assert.NotNull(generator);
            Assert.Same(generator, pool.GetGenerator());
        }
    }
}
