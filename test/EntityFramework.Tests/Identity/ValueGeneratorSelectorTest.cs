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
        public void Returns_in_memory_GUID_generator_for_GUID_types_setup_for_value_generation()
        {
            var guidFactory = new SimpleValueGeneratorFactory<GuidValueGenerator>();

            var selector = new ValueGeneratorSelector(guidFactory);

            Assert.Same(guidFactory, selector.Select(CreateProperty(typeof(Guid))));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_not_required()
        {
            var selector = new ValueGeneratorSelector(new SimpleValueGeneratorFactory<GuidValueGenerator>());

            Assert.Null(selector.Select(CreateProperty(typeof(int), generateValues: false)));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new ValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>());

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            var property = CreateProperty(typeof(Random));

            Assert.Equal(
                Strings.FormatNoValueGenerator("MyProperty", "MyType", "Random"),
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
