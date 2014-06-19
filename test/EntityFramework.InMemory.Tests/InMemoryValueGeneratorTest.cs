// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorTest
    {
        [Fact]
        public async Task Creates_values()
        {
            var generator = new InMemoryValueGenerator();

            var propertyMock = new Mock<IProperty>();
            propertyMock.Setup(m => m.PropertyType).Returns(typeof(int));

            Assert.Equal(1, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
            Assert.Equal(2, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
            Assert.Equal(3, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), propertyMock.Object));

            Assert.Equal(4, generator.Next(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
            Assert.Equal(5, generator.Next(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
            Assert.Equal(6, generator.Next(Mock.Of<DbContextConfiguration>(), propertyMock.Object));

            generator = new InMemoryValueGenerator();

            Assert.Equal(1, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
            Assert.Equal(2, generator.Next(Mock.Of<DbContextConfiguration>(), propertyMock.Object));
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types()
        {
            var generator = new InMemoryValueGenerator();

            Assert.Equal(1L, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(long))));
            Assert.Equal(2, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(int))));
            Assert.Equal((short)3, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(short))));
            Assert.Equal((byte)4, await generator.NextAsync(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(byte))));

            Assert.Equal(5L, generator.Next(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(long))));
            Assert.Equal(6, generator.Next(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(int))));
            Assert.Equal((short)7, generator.Next(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(short))));
            Assert.Equal((byte)8, generator.Next(Mock.Of<DbContextConfiguration>(), CreateProperty(typeof(byte))));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryValueGenerator();
            var property = CreateProperty(typeof(byte));

            for (var i = 1; i < 256; i++)
            {
                generator.Next(Mock.Of<DbContextConfiguration>(), property);
            }

            Assert.Throws<OverflowException>(() => generator.Next(Mock.Of<DbContextConfiguration>(), property));
        }

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new EntityType("MyType");
            return entityType.AddProperty("MyProperty", propertyType);
        }
    }
}
