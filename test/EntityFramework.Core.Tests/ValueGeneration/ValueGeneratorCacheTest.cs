// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class ValueGeneratorCacheTest
    {
        [Fact]
        public void Uses_single_generator_per_property()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var cache = TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IInMemoryValueGeneratorCache>();

            var generator1 = cache.GetOrAdd(property1, p => new GuidValueGenerator());
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetOrAdd(property1, p => new GuidValueGenerator()));

            var generator2 = cache.GetOrAdd(property2, p => new GuidValueGenerator());
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetOrAdd(property2, p => new GuidValueGenerator()));
            Assert.NotSame(generator1, generator2);
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
