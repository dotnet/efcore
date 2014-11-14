// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_in_memory_integer_generator_for_all_integer_types_setup_for_generation_on_Add()
        {
            var inMemoryFactory = new SimpleValueGeneratorFactory<InMemoryValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                inMemoryFactory);

            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(long))));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(int))));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(short))));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(byte))));
        }

        [Fact]
        public void Returns_in_memory_GUID_generator_for_GUID_types_setup_for_generation_on_Add()
        {
            var guidFactory = new SimpleValueGeneratorFactory<GuidValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                guidFactory,
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            Assert.Same(guidFactory, selector.Select(CreateProperty(typeof(Guid))));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_configured()
        {
            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            Assert.Null(selector.Select(CreateProperty(typeof(int), generateValues: false)));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            var property = CreateProperty(typeof(double));

            Assert.Equal(
                CoreStrings.NoValueGenerator("MyProperty", "MyType", "Double"),
                Assert.Throws<NotSupportedException>(() => selector.Select(property)).Message);
        }

        private static Property CreateProperty(Type propertyType, bool generateValues = true)
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
            property.GenerateValueOnAdd = generateValues;

            return property;
        }
    }
}
