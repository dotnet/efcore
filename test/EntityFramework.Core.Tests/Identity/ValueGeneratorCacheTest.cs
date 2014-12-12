// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class ValueGeneratorCacheTest
    {
        [Fact]
        public void Uses_single_generator_per_cache_key_when_pool_size_is_one()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var cache = TestHelpers.CreateContextServices(model).GetRequiredService<InMemoryValueGeneratorCache>();

            var generator1 = cache.GetGenerator(property1);
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetGenerator(property1));

            var generator2 = cache.GetGenerator(property2);
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetGenerator(property2));
            Assert.NotSame(generator1, generator2);
        }

        [Fact]
        public void Uses_pool_per_cache_key_when_pool_size_is_greater_than_one()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);

            var customServices = TestHelpers.CreateContextServices(
                new ServiceCollection()
                    .AddInstance<SimpleValueGeneratorFactory<GuidValueGenerator>>(new FakeGuidValueGeneratorFactory()),
                model);

            var cache = customServices.GetRequiredService<InMemoryValueGeneratorCache>();

            var generator1a = cache.GetGenerator(property1);
            var generator1b = cache.GetGenerator(property1);
            Assert.NotSame(generator1a, generator1b);

            Assert.Same(generator1a, cache.GetGenerator(property1));
            Assert.Same(generator1b, cache.GetGenerator(property1));
            Assert.Same(generator1a, cache.GetGenerator(property1));
            Assert.Same(generator1b, cache.GetGenerator(property1));

            var generator2a = cache.GetGenerator(property2);
            var generator2b = cache.GetGenerator(property2);
            Assert.NotSame(generator2a, generator2b);
            Assert.NotSame(generator1a, generator2a);
            Assert.NotSame(generator1b, generator2a);

            Assert.Same(generator2a, cache.GetGenerator(property2));
            Assert.Same(generator2b, cache.GetGenerator(property2));
            Assert.Same(generator2a, cache.GetGenerator(property2));
            Assert.Same(generator2b, cache.GetGenerator(property2));
        }

        private class FakeGuidValueGeneratorFactory : SimpleValueGeneratorFactory<GuidValueGenerator>
        {
            public override int GetPoolSize(IProperty property)
            {
                return 2;
            }
        }

        private static Property GetProperty1(Model model)
        {
            return model.GetEntityType("Led").GetProperty("Zeppelin");
        }

        private static Property GetProperty2(Model model)
        {
            return model.GetEntityType("Led").GetProperty("Stairway");
        }

        private static Model CreateModel(bool generateValues = true)
        {
            var model = new Model();

            var entityType = model.AddEntityType("Led");
            var property1 = entityType.GetOrAddProperty("Zeppelin", typeof(Guid), shadowProperty: true);
            property1.GenerateValueOnAdd = generateValues;
            var property2 = entityType.GetOrAddProperty("Stairway", typeof(Guid), shadowProperty: true);
            property2.GenerateValueOnAdd = generateValues;

            return model;
        }
    }
}
