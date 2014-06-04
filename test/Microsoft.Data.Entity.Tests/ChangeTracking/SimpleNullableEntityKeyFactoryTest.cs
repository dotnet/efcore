// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class SimpleNullableEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var nonKeyProp = new Mock<IProperty>().Object;
            var typeMock = new Mock<IEntityType>();

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[nonKeyProp]).Returns(7);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(
                typeMock.Object, new[] { nonKeyProp }, entryMock.Object);

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Returns_null_if_key_value_is_null()
        {
            var keyProp = new Mock<IProperty>().Object;
            var nonKeyProp = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyProp });

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyProp]).Returns(7);
            entryMock.Setup(m => m[nonKeyProp]).Returns(null);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            Assert.Null(new SimpleNullableEntityKeyFactory<int, int?>().Create(
                typeMock.Object, new[] { nonKeyProp }, entryMock.Object));
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var nonKeyPropMock = new Mock<IProperty>();
            nonKeyPropMock.Setup(m => m.Index).Returns(0);
            var typeMock = new Mock<IEntityType>();

            var key = (SimpleEntityKey<int>)new SimpleNullableEntityKeyFactory<int, int?>().Create(
                typeMock.Object, new[] { nonKeyPropMock.Object }, new ObjectArrayValueReader(new object[] { 7 }));

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Returns_null_if_value_in_buffer_is_null()
        {
            var nonKeyPropMock = new Mock<IProperty>();
            nonKeyPropMock.Setup(m => m.Index).Returns(0);
            var typeMock = new Mock<IEntityType>();

            Assert.Null(new SimpleNullableEntityKeyFactory<int, int?>().Create(
                typeMock.Object, new[] { nonKeyPropMock.Object }, new ObjectArrayValueReader(new object[] { null })));
        }
    }
}
