// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class TemporaryValueGeneratorTest
    {
        [Fact]
        public async Task Creates_negative_values()
        {
            var generator = new TemporaryValueGenerator();

            var property = CreateProperty(typeof(int));

            Assert.Equal(-1, await generator.NextAsync(Mock.Of<StateEntry>(), property));
            Assert.Equal(-2, await generator.NextAsync(Mock.Of<StateEntry>(), property));
            Assert.Equal(-3, await generator.NextAsync(Mock.Of<StateEntry>(), property));

            Assert.Equal(-4, generator.Next(Mock.Of<StateEntry>(), property));
            Assert.Equal(-5, generator.Next(Mock.Of<StateEntry>(), property));
            Assert.Equal(-6, generator.Next(Mock.Of<StateEntry>(), property));
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types_except_byte()
        {
            var generator = new TemporaryValueGenerator();

            Assert.Equal(-1L, await generator.NextAsync(Mock.Of<StateEntry>(), CreateProperty(typeof(long))));
            Assert.Equal(-2, await generator.NextAsync(Mock.Of<StateEntry>(), CreateProperty(typeof(int))));
            Assert.Equal((short)-3, await generator.NextAsync(Mock.Of<StateEntry>(), CreateProperty(typeof(short))));

            Assert.Equal(-4L, generator.Next(Mock.Of<StateEntry>(), CreateProperty(typeof(long))));
            Assert.Equal(-5, generator.Next(Mock.Of<StateEntry>(), CreateProperty(typeof(int))));
            Assert.Equal((short)-6, generator.Next(Mock.Of<StateEntry>(), CreateProperty(typeof(short))));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new TemporaryValueGenerator();

            Assert.Throws<OverflowException>(() => generator.Next(Mock.Of<StateEntry>(), CreateProperty(typeof(byte))));
        }

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new EntityType("MyType");
            return entityType.AddProperty("MyProperty", propertyType);
        }
    }
}
