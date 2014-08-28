// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class ValueGeneratorCacheTest
    {
        [Fact]
        public void Returns_null_if_selector_returns_null()
        {
            var property = CreateProperty(ValueGenerationOnAdd.None);
            var selector = new ValueGeneratorSelector(new SimpleValueGeneratorFactory<GuidValueGenerator>());
            var cache = new ValueGeneratorCache(selector, Mock.Of<ForeignKeyValueGenerator>());

            Assert.Null(cache.GetGenerator(property));
        }

        [Fact]
        public void Uses_single_generator_per_cache_key_when_pool_size_is_one()
        {
            var property = CreateProperty(ValueGenerationOnAdd.Client);

            var factoryMock = new Mock<SimpleValueGeneratorFactory<GuidValueGenerator>>();
            factoryMock.Setup(m => m.Create(property)).Returns(CreateValueGeneratorCallback);
            factoryMock.Setup(m => m.GetPoolSize(property)).Returns(1);
            factoryMock.Setup(m => m.GetCacheKey(property)).Returns("TheKeyMaster");

            var selector = new ValueGeneratorSelector(factoryMock.Object);
            var cache = new ValueGeneratorCache(selector, Mock.Of<ForeignKeyValueGenerator>());

            var generator1 = cache.GetGenerator(property);
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetGenerator(property));

            factoryMock.Setup(m => m.GetCacheKey(property)).Returns("TheGatekeeper");

            var generator2 = cache.GetGenerator(property);
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetGenerator(property));
            Assert.NotSame(generator1, generator2);
        }

        [Fact]
        public void Uses_pool_per_cache_key_when_pool_size_is_greater_than_one()
        {
            var property = CreateProperty(ValueGenerationOnAdd.Client);

            var factoryMock = new Mock<SimpleValueGeneratorFactory<GuidValueGenerator>>();
            factoryMock.Setup(m => m.Create(property)).Returns(CreateValueGeneratorCallback);
            factoryMock.Setup(m => m.GetPoolSize(property)).Returns(2);
            factoryMock.Setup(m => m.GetCacheKey(property)).Returns("TheKeyMaster");

            var selector = new ValueGeneratorSelector(factoryMock.Object);
            var cache = new ValueGeneratorCache(selector, Mock.Of<ForeignKeyValueGenerator>());

            var generator1a = cache.GetGenerator(property);
            var generator1b = cache.GetGenerator(property);
            Assert.NotSame(generator1a, generator1b);

            Assert.Same(generator1a, cache.GetGenerator(property));
            Assert.Same(generator1b, cache.GetGenerator(property));
            Assert.Same(generator1a, cache.GetGenerator(property));
            Assert.Same(generator1b, cache.GetGenerator(property));

            factoryMock.Setup(m => m.GetCacheKey(property)).Returns("TheGatekeeper");

            var generator2a = cache.GetGenerator(property);
            var generator2b = cache.GetGenerator(property);
            Assert.NotSame(generator2a, generator2b);
            Assert.NotSame(generator1a, generator2a);
            Assert.NotSame(generator1b, generator2a);

            Assert.Same(generator2a, cache.GetGenerator(property));
            Assert.Same(generator2b, cache.GetGenerator(property));
            Assert.Same(generator2a, cache.GetGenerator(property));
            Assert.Same(generator2b, cache.GetGenerator(property));
        }

        private static TemporaryValueGenerator CreateValueGeneratorCallback()
        {
            return new TemporaryValueGenerator();
        }

        private static Property CreateProperty(ValueGenerationOnAdd valueGeneration)
        {
            var entityType = new EntityType("Led");
            var property = entityType.GetOrAddProperty("Zeppelin", typeof(Guid), shadowProperty: true);
            property.ValueGenerationOnAdd = valueGeneration;
            return property;
        }
    }
}
