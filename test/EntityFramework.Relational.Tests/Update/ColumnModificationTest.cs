// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
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
            Assert.Equal("p0", columnModification.ParameterName);
            Assert.Equal("p1", columnModification.OriginalParameterName);
            Assert.Equal("p2", columnModification.OutputParameterName);
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

            internalEntryMock.Verify(m => m.GetValue(It.IsAny<IPropertyBase>(), ValueSource.Current), Times.Once);
        }

        [Fact]
        public void Set_Value_delegates_to_Entry()
        {
            var property = new Mock<IProperty>().Object;
            var internalEntryMock = CreateInternalEntryMock(property);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                property,
                property.TestProvider(),
                new ParameterNameGenerator(),
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);
            var value = new object();

            columnModification.Value = value;

            internalEntryMock.Verify(m => m.SetValue(property, It.IsAny<object>(), ValueSource.Current), Times.Once);
        }

        private static Mock<InternalEntityEntry> CreateInternalEntryMock(IProperty property)
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(e => e.GetProperties()).Returns(new[] { property });

            entityTypeMock.As<IPropertyCountsAccessor>().Setup(e => e.Counts).Returns(new PropertyCounts(0, 0, 0, 0, 0, 0));

            var internalEntryMock = new Mock<InternalEntityEntry>(
                Mock.Of<IStateManager>(), entityTypeMock.Object);
            return internalEntryMock;
        }
    }
}
