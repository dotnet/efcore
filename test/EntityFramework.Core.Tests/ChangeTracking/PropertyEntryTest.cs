// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class PropertyEntryTest
    {
        #region NonGeneric PropertyEntry Tests

        [Fact]
        public void Can_get_name()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());

            Assert.Equal("Monkey", new PropertyEntry(internalEntryMock.Object, "Monkey").Metadata.Name);
        }

        [Fact]
        public void Can_get_current_value()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());
            internalEntryMock.Setup(m => m[It.IsAny<IProperty>()]).Returns("Chimp");

            Assert.Equal("Chimp", new PropertyEntry(internalEntryMock.Object, "Monkey").CurrentValue);
        }

        [Fact]
        public void Can_set_current_value()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            new PropertyEntry(internalEntryMock.Object, "Monkey").CurrentValue = "Chimp";

            internalEntryMock.VerifySet(m => m[property.Object] = "Chimp");
        }

        [Fact]
        public void Can_set_current_value_to_null()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            new PropertyEntry(internalEntryMock.Object, "Monkey").CurrentValue = null;

            internalEntryMock.VerifySet(m => m[property.Object] = null);
        }

        [Fact]
        public void Can_get_original_value()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());
            internalEntryMock.Setup(m => m.OriginalValues[It.IsAny<IProperty>()]).Returns("Chimp");

            Assert.Equal("Chimp", new PropertyEntry(internalEntryMock.Object, "Monkey").OriginalValue);
        }

        [Fact]
        public void Can_set_original_value()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            var sideCarMock = new Mock<Sidecar>();
            internalEntryMock.Setup(m => m.OriginalValues).Returns(sideCarMock.Object);

            new PropertyEntry(internalEntryMock.Object, "Monkey").OriginalValue = "Chimp";

            sideCarMock.VerifySet(m => m[property.Object] = "Chimp");
        }

        [Fact]
        public void Can_set_original_value_to_null()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            var sideCarMock = new Mock<Sidecar>();
            internalEntryMock.Setup(m => m.OriginalValues).Returns(sideCarMock.Object);

            new PropertyEntry(internalEntryMock.Object, "Monkey").OriginalValue = null;

            sideCarMock.VerifySet(m => m[property.Object] = null);
        }

        [Fact]
        public void IsModified_delegates_to_state_object()
        {
            var propertyMock = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(propertyMock);
            internalEntryMock.Setup(m => m.IsPropertyModified(propertyMock.Object)).Returns(true);

            var propertyEntry = new PropertyEntry(internalEntryMock.Object, "Monkey");

            Assert.True(propertyEntry.IsModified);
            internalEntryMock.Verify(m => m.IsPropertyModified(propertyMock.Object));

            propertyEntry.IsModified = true;

            internalEntryMock.Verify(m => m.SetPropertyModified(propertyMock.Object, true));
        }

        #endregion

        #region Generic PropertyEntry Tests

        [Fact]
        public void Can_get_name_generic()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());

            Assert.Equal("Monkey", new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").Metadata.Name);
        }

        [Fact]
        public void Can_get_current_value_generic()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());
            internalEntryMock.Setup(m => m[It.IsAny<IProperty>()]).Returns("Chimp");

            Assert.Equal("Chimp", new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").CurrentValue);
        }

        [Fact]
        public void Can_set_current_value_generic()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").CurrentValue = "Chimp";

            internalEntryMock.VerifySet(m => m[property.Object] = "Chimp");
        }

        [Fact]
        public void Can_set_current_value_to_null_generic()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").CurrentValue = null;

            internalEntryMock.VerifySet(m => m[property.Object] = null);
        }

        [Fact]
        public void Can_get_original_value_generic()
        {
            var internalEntryMock = CreateInternalEntryMock(new Mock<IProperty>());
            internalEntryMock.Setup(m => m.OriginalValues[It.IsAny<IProperty>()]).Returns("Chimp");

            Assert.Equal("Chimp", new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").OriginalValue);
        }

        [Fact]
        public void Can_set_original_value_generic()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            var sideCarMock = new Mock<Sidecar>();
            internalEntryMock.Setup(m => m.OriginalValues).Returns(sideCarMock.Object);

            new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").OriginalValue = "Chimp";

            sideCarMock.VerifySet(m => m[property.Object] = "Chimp");
        }

        [Fact]
        public void Can_set_original_value_to_null_generic()
        {
            var property = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(property);

            var sideCarMock = new Mock<Sidecar>();
            internalEntryMock.Setup(m => m.OriginalValues).Returns(sideCarMock.Object);

            new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey").OriginalValue = null;

            sideCarMock.VerifySet(m => m[property.Object] = null);
        }

        [Fact]
        public void IsModified_delegates_to_state_object_generic()
        {
            var propertyMock = new Mock<IProperty>();
            var internalEntryMock = CreateInternalEntryMock(propertyMock);
            internalEntryMock.Setup(m => m.IsPropertyModified(propertyMock.Object)).Returns(true);

            var propertyEntry = new PropertyEntry<Random, string>(internalEntryMock.Object, "Monkey");

            Assert.True(propertyEntry.IsModified);
            internalEntryMock.Verify(m => m.IsPropertyModified(propertyMock.Object));

            propertyEntry.IsModified = true;

            internalEntryMock.Verify(m => m.SetPropertyModified(propertyMock.Object, true));
        }

        #endregion

        #region Helper Functions

        private static Mock<InternalEntityEntry> CreateInternalEntryMock(Mock<IProperty> propertyMock)
        {
            propertyMock.Setup(m => m.Name).Returns("Monkey");

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.GetProperty("Monkey")).Returns(propertyMock.Object);

            var internalEntryMock = new Mock<InternalEntityEntry>();
            internalEntryMock.Setup(m => m.EntityType).Returns(entityTypeMock.Object);
            return internalEntryMock;
        }

        #endregion
    }
}
