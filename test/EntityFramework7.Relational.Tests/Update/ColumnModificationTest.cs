// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
{
    public class ColumnModificationTest
    {
        [Fact]
        public void Parameters_return_set_values()
        {
            var columnModification = new ColumnModification(
                CreateInternalEntryMock(Mock.Of<IProperty>()).Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator(),
                isRead: true,
                isWrite: true,
                isKey: true,
                isCondition: true);

            Assert.Null(columnModification.ColumnName);
            Assert.True(columnModification.IsRead);
            Assert.True(columnModification.IsWrite);
            Assert.True(columnModification.IsKey);
            Assert.True(columnModification.IsCondition);
            Assert.Equal("@p0", columnModification.ParameterName);
            Assert.Equal("@p1", columnModification.OriginalParameterName);
            Assert.Equal("@p2", columnModification.OutputParameterName);
        }

        [Fact]
        public void Get_OriginalValue_delegates_to_OriginalValues_if_possible()
        {
            var internalEntryMock = CreateInternalEntryMock(Mock.Of<IProperty>());
            var originalValuesMock = new Mock<Sidecar>(internalEntryMock.Object);
            originalValuesMock.Setup(m => m.CanStoreValue(It.IsAny<IPropertyBase>())).Returns(true);
            internalEntryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator(),
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.OriginalValue;

            originalValuesMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
            internalEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Never);
        }

        [Fact]
        public void Get_OriginalValue_delegates_to_Entry_if_OriginalValues_if_unavailable()
        {
            var internalEntryMock = CreateInternalEntryMock(Mock.Of<IProperty>());
            var originalValuesMock = new Mock<Sidecar>(internalEntryMock.Object);
            originalValuesMock.Setup(m => m.CanStoreValue(It.IsAny<IPropertyBase>())).Returns(false);
            internalEntryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator(),
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.OriginalValue;

            internalEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
            originalValuesMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Never);
        }

        [Fact]
        public void Get_Value_delegates_to_Entry()
        {
            var internalEntryMock = CreateInternalEntryMock(Mock.Of<IProperty>());
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator(),
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.Value;

            internalEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
        }

        [Fact]
        public void Set_Value_delegates_to_Entry()
        {
            var property = new Mock<IProperty>().Object;
            var internalEntryMock = CreateInternalEntryMock(property);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                property,
                property.Relational(),
                new ParameterNameGenerator(),
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);
            var value = new object();

            columnModification.Value = value;

            internalEntryMock.VerifySet(m => m[property] = It.IsAny<object>(), Times.Once);
        }

        private static Mock<InternalEntityEntry> CreateInternalEntryMock(IProperty property)
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(e => e.GetProperties()).Returns(new[] { property });

            var internalEntryMock = new Mock<InternalEntityEntry>(
                Mock.Of<IStateManager>(), entityTypeMock.Object, Mock.Of<IEntityEntryMetadataServices>());
            return internalEntryMock;
        }
    }
}
