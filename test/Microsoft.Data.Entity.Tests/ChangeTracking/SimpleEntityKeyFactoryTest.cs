// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class SimpleEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_entry()
        {
            var keyProp = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyProp });

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyProp]).Returns(7);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>().Create(
                typeMock.Object, typeMock.Object.GetKey().Properties, entryMock.Object);

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_entry()
        {
            var keyProp = new Mock<IProperty>().Object;
            var nonKeyProp = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyProp });

            var random = new Random();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m[keyProp]).Returns(7);
            entryMock.Setup(m => m[nonKeyProp]).Returns("Ate");
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (SimpleEntityKey<string>)new SimpleEntityKeyFactory<string>().Create(
                typeMock.Object, new[] { nonKeyProp }, entryMock.Object);

            Assert.Equal("Ate", key.Value);
        }

        [Fact]
        public void Creates_a_new_primary_key_for_key_values_in_the_given_value_buffer()
        {
            var keyPropMock = new Mock<IProperty>();
            keyPropMock.Setup(m => m.Index).Returns(0);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPropMock.Object });

            var key = (SimpleEntityKey<int>)new SimpleEntityKeyFactory<int>().Create(
                typeMock.Object, typeMock.Object.GetKey().Properties, new ObjectArrayValueReader(new object[] { 7 }));

            Assert.Equal(7, key.Value);
        }

        [Fact]
        public void Creates_a_new_key_for_non_primary_key_values_in_the_given_value_buffer()
        {
            var keyPropMock = new Mock<IProperty>();
            keyPropMock.Setup(m => m.Index).Returns(0);
            var nonKeyPropMock = new Mock<IProperty>();
            nonKeyPropMock.Setup(m => m.Index).Returns(1);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.GetKey().Properties).Returns(new[] { keyPropMock.Object });

            var key = (SimpleEntityKey<string>)new SimpleEntityKeyFactory<string>().Create(
                typeMock.Object, new[] { nonKeyPropMock.Object }, new ObjectArrayValueReader(new object[] { 7, "Ate" }));

            Assert.Equal("Ate", key.Value);
        }
    }
}
