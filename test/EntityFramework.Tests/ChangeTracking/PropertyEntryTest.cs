// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry(new Mock<StateEntry>().Object, "")).Message);
            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry(null, "Kake")).ParamName);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry<Random, int>(new Mock<StateEntry>().Object, "")).Message);
            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry<Random, int>(null, "Kake")).ParamName);
        }

        [Fact]
        public void Can_get_name()
        {
            var stateEntryMock = CreateStateEntryMock(new Mock<IProperty>());

            Assert.Equal("Monkey", new PropertyEntry(stateEntryMock.Object, "Monkey").Name);
            Assert.Equal("Monkey", new PropertyEntry<Random, string>(stateEntryMock.Object, "Monkey").Name);
        }

        [Fact]
        public void Can_get_current_value()
        {
            var stateEntryMock = CreateStateEntryMock(new Mock<IProperty>());
            stateEntryMock.Setup(m => m[It.IsAny<IProperty>()]).Returns("Chimp");

            Assert.Equal("Chimp", new PropertyEntry(stateEntryMock.Object, "Monkey").CurrentValue);
        }

        [Fact]
        public void IsModified_delegates_to_state_object()
        {
            var propertyMock = new Mock<IProperty>();
            var stateEntryMock = CreateStateEntryMock(propertyMock);
            stateEntryMock.Setup(m => m.IsPropertyModified(propertyMock.Object)).Returns(true);

            var propertyEntry = new PropertyEntry<Random, string>(stateEntryMock.Object, "Monkey");

            Assert.True(propertyEntry.IsModified);
            stateEntryMock.Verify(m => m.IsPropertyModified(propertyMock.Object));

            propertyEntry.IsModified = true;

            stateEntryMock.Verify(m => m.SetPropertyModified(propertyMock.Object, true));
        }

        private static Mock<StateEntry> CreateStateEntryMock(Mock<IProperty> propertyMock)
        {
            propertyMock.Setup(m => m.Name).Returns("Monkey");

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.GetProperty("Monkey")).Returns(propertyMock.Object);

            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.EntityType).Returns(entityTypeMock.Object);
            return stateEntryMock;
        }
    }
}
