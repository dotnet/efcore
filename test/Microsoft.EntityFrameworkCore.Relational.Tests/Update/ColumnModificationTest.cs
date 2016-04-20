// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class ColumnModificationTest
    {
        [Fact]
        public void Parameters_return_set_values()
        {
            var property = new Model().AddEntityType(typeof(object)).AddProperty("Kake", typeof(string));

            var columnModification = new ColumnModification(
                CreateInternalEntryMock(property).Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator().GenerateNext,
                isRead: true,
                isWrite: true,
                isKey: true,
                isCondition: true,
                isConcurrencyToken: false);

            Assert.Null(columnModification.ColumnName);
            Assert.True(columnModification.IsRead);
            Assert.True(columnModification.IsWrite);
            Assert.True(columnModification.IsKey);
            Assert.True(columnModification.IsCondition);
            Assert.Equal("p0", columnModification.ParameterName);
            Assert.Equal("p1", columnModification.OriginalParameterName);
        }

        [Fact]
        public void Get_Value_delegates_to_Entry()
        {
            var property = new Model().AddEntityType(typeof(object)).AddProperty("Kake", typeof(string));

            var internalEntryMock = CreateInternalEntryMock(property);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                new Mock<IProperty>().Object,
                new Mock<IRelationalPropertyAnnotations>().Object,
                new ParameterNameGenerator().GenerateNext,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false,
                isConcurrencyToken: false);

            var value = columnModification.Value;

            internalEntryMock.Verify(m => m.GetCurrentValue(It.IsAny<IPropertyBase>()), Times.Once);
        }

        [Fact]
        public void Set_Value_delegates_to_Entry()
        {
            var property = new Model().AddEntityType(typeof(object)).AddProperty("Kake", typeof(string));

            var internalEntryMock = CreateInternalEntryMock(property);
            var columnModification = new ColumnModification(
                internalEntryMock.Object,
                property,
                property.TestProvider(),
                new ParameterNameGenerator().GenerateNext,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false,
                isConcurrencyToken: false);
            var value = new object();

            columnModification.Value = value;

            internalEntryMock.Verify(m => m.SetCurrentValue(property, It.IsAny<object>()), Times.Once);
        }

        private static Mock<InternalEntityEntry> CreateInternalEntryMock(Property property)
        {
            var entityTypeMock = new Mock<EntityType>("Entity", new Model(), ConfigurationSource.Explicit);
            entityTypeMock.Setup(e => e.GetProperties()).Returns(new[] { property });

            entityTypeMock.Setup(e => e.Counts).Returns(new PropertyCounts(0, 0, 0, 0, 0, 0));

            var internalEntryMock = new Mock<InternalEntityEntry>(
                Mock.Of<IStateManager>(), entityTypeMock.Object);
            return internalEntryMock;
        }
    }
}
