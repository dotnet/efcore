// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_in_memory_integer_generator_for_all_integer_types_setup_for_client_values()
        {
            var inMemoryFactory = new SimpleValueGeneratorFactory<InMemoryValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                inMemoryFactory);

            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(long), ValueGenerationOnAdd.Client)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.Client)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(short), ValueGenerationOnAdd.Client)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(byte), ValueGenerationOnAdd.Client)));
        }

        [Fact]
        public void Returns_in_memory_integer_generator_for_all_integer_types_setup_for_server_values()
        {
            var inMemoryFactory = new SimpleValueGeneratorFactory<InMemoryValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                inMemoryFactory);

            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(long), ValueGenerationOnAdd.Server)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.Server)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(short), ValueGenerationOnAdd.Server)));
            Assert.Same(inMemoryFactory, selector.Select(CreateProperty(typeof(byte), ValueGenerationOnAdd.Server)));
        }

        [Fact]
        public void Returns_in_memory_GUID_generator_for_GUID_types_setup_for_client_values()
        {
            var guidFactory = new SimpleValueGeneratorFactory<GuidValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                guidFactory,
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            Assert.Same(guidFactory, selector.Select(CreateProperty(typeof(Guid), ValueGenerationOnAdd.Client)));
        }

        [Fact]
        public void Returns_in_memory_GUID_generator_for_GUID_types_setup_for_server_values()
        {
            var guidFactory = new SimpleValueGeneratorFactory<GuidValueGenerator>();

            var selector = new InMemoryValueGeneratorSelector(
                guidFactory,
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            Assert.Same(guidFactory, selector.Select(CreateProperty(typeof(Guid), ValueGenerationOnAdd.Server)));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_configured()
        {
            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            Assert.Null(selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.None)));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new InMemoryValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<InMemoryValueGenerator>());

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            var property = CreateProperty(typeof(double), ValueGenerationOnAdd.Client);

            Assert.Equal(
                GetString("FormatNoValueGenerator", "client", "MyType", "MyProperty", "Double"),
                Assert.Throws<NotSupportedException>(() => selector.Select(property)).Message);
        }

        private static Property CreateProperty(Type propertyType, ValueGenerationOnAdd valueGeneration)
        {
            var entityType = new EntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
            property.ValueGenerationOnAdd = valueGeneration;

            new Model().AddEntityType(entityType);

            return property;
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
