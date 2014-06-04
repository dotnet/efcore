// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class CompositeEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var keyPart1 = new Mock<IProperty>().Object;
            var keyPart2 = new Mock<IProperty>().Object;
            var keyPart3 = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPart1, keyPart2, keyPart3 });

            var random = new Random();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyPart1]).Returns(7);
            entryMock.Setup(m => m[keyPart2]).Returns("Ate");
            entryMock.Setup(m => m[keyPart3]).Returns(random);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory().Create(
                typeMock.Object, typeMock.Object.GetKey().Properties, entryMock.Object);

            Assert.Equal(new object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var keyProp = new Mock<IProperty>().Object;
            var nonKeyPart1 = new Mock<IProperty>().Object;
            var nonKeyPart2 = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyProp });

            var random = new Random();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyProp]).Returns(7);
            entryMock.Setup(m => m[nonKeyPart1]).Returns("Ate");
            entryMock.Setup(m => m[nonKeyPart2]).Returns(random);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory().Create(
                typeMock.Object, new[] { nonKeyPart2, nonKeyPart1 }, entryMock.Object);

            Assert.Equal(new object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_entry_properties_is_null()
        {
            var keyProp = new Mock<IProperty>().Object;
            var nonKeyPart1 = new Mock<IProperty>().Object;
            var nonKeyPart2 = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyProp });

            var random = new Random();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyProp]).Returns(7);
            entryMock.Setup(m => m[nonKeyPart1]).Returns("Ate");
            entryMock.Setup(m => m[nonKeyPart2]).Returns(null);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            Assert.Null(new CompositeEntityKeyFactory().Create(
                typeMock.Object, new[] { nonKeyPart2, nonKeyPart1 }, entryMock.Object));
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var keyPart1Mock = new Mock<IProperty>();
            keyPart1Mock.Setup(m => m.Index).Returns(0);
            var keyPart2Mock = new Mock<IProperty>();
            keyPart2Mock.Setup(m => m.Index).Returns(1);
            var keyPart3Mock = new Mock<IProperty>();
            keyPart3Mock.Setup(m => m.Index).Returns(2);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPart1Mock.Object, keyPart2Mock.Object, keyPart3Mock.Object });

            var random = new Random();

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory().Create(
                typeMock.Object, typeMock.Object.GetKey().Properties, new ObjectArrayValueReader(new object[] { 7, "Ate", random }));

            Assert.Equal(new Object[] { 7, "Ate", random }, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var keyPropMock = new Mock<IProperty>();
            keyPropMock.Setup(m => m.Index).Returns(0);
            var nonKeyPart1Mock = new Mock<IProperty>();
            nonKeyPart1Mock.Setup(m => m.Index).Returns(1);
            var nonKeyPart2Mock = new Mock<IProperty>();
            nonKeyPart2Mock.Setup(m => m.Index).Returns(2);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPropMock.Object });

            var random = new Random();

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory().Create(
                typeMock.Object, new[] { nonKeyPart2Mock.Object, nonKeyPart1Mock.Object }, new ObjectArrayValueReader(new object[] { 7, "Ate", random }));

            Assert.Equal(new Object[] { random, "Ate" }, key.Value);
        }

        [Fact]
        public void Returns_null_if_any_value_in_the_given_buffer_is_null()
        {
            var keyPropMock = new Mock<IProperty>();
            keyPropMock.Setup(m => m.Index).Returns(0);
            var nonKeyPart1Mock = new Mock<IProperty>();
            nonKeyPart1Mock.Setup(m => m.Index).Returns(1);
            var nonKeyPart2Mock = new Mock<IProperty>();
            nonKeyPart2Mock.Setup(m => m.Index).Returns(2);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPropMock.Object });

            var random = new Random();

            var key = new CompositeEntityKeyFactory().Create(
                typeMock.Object,
                new[]
                    {
                        nonKeyPart2Mock.Object,
                        nonKeyPart1Mock.Object
                    },
                new ObjectArrayValueReader(new object[] { 7, null, random }));

            Assert.Null(key);
        }
    }
}
