// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class SingleValueGeneratorPoolTest
    {
        [Fact]
        public void Returns_the_same_instance_every_time()
        {
            var pool = new SingleValueGeneratorPool(new SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator>(), Mock.Of<IProperty>());

            var generator = pool.GetGenerator();
            Assert.NotNull(generator);
            Assert.Same(generator, pool.GetGenerator());
        }
    }
}
