// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class ValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_in_memory_GUID_generator_for_GUID_types_setup_for_client_values()
        {
            var guidFactory = new SimpleValueGeneratorFactory<GuidValueGenerator>();

            var selector = new ValueGeneratorSelector(guidFactory);

            Assert.Same(guidFactory, selector.Select(CreateProperty(typeof(Guid), ValueGenerationOnAdd.Client)));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_configured()
        {
            var selector = new ValueGeneratorSelector(new SimpleValueGeneratorFactory<GuidValueGenerator>());

            Assert.Null(selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.None)));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new ValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>());

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            var property = CreateProperty(typeof(Guid), ValueGenerationOnAdd.Server);

            Assert.Equal(
                Strings.FormatNoValueGenerator("server", "MyType", "MyProperty", "Guid"),
                Assert.Throws<NotSupportedException>(() => selector.Select(property)).Message);
        }

        private static Property CreateProperty(Type propertyType, ValueGenerationOnAdd valueGeneration)
        {
            var entityType = new EntityType("MyType");
            var property = entityType.AddProperty("MyProperty", propertyType);
            property.ValueGenerationOnAdd = valueGeneration;
            entityType.StorageName = "MyTable";

            new Model().AddEntityType(entityType);

            return property;
        }
    }
}
