// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Identity
{
    public class SingleValueGeneratorPoolTest
    {
        [Fact]
        public void Returns_the_same_instance_every_time()
        {
            var pool = new SingleValueGeneratorPool(new SimpleValueGeneratorFactory<TemporaryValueGenerator>(), Mock.Of<IProperty>());

            var generator = pool.GetGenerator();
            Assert.NotNull(generator);
            Assert.Same(generator, pool.GetGenerator());
        }
    }
}
