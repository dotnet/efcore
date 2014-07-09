// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ColumnModificationTest
    {
        [Fact]
        public void Get_OriginalValue_delegates_to_OriginalValues_if_possible()
        {
            var originalValuesMock = new Mock<Sidecar>();
            originalValuesMock.Setup(m => m.CanStoreValue(It.IsAny<IPropertyBase>())).Returns(true);
            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);
            var columnModification = new ColumnModification(
                stateEntryMock.Object,
                new Mock<IProperty>().Object,
                parameterName: null,
                originalParameterName: null,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.OriginalValue;

            originalValuesMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
            stateEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Never);
        }

        [Fact]
        public void Get_OriginalValue_delegates_to_StateEntry_if_OriginalValues_if_unavailable()
        {
            var originalValuesMock = new Mock<Sidecar>();
            originalValuesMock.Setup(m => m.CanStoreValue(It.IsAny<IPropertyBase>())).Returns(false);
            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);
            var columnModification = new ColumnModification(
                stateEntryMock.Object,
                new Mock<IProperty>().Object,
                parameterName: null,
                originalParameterName: null,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.OriginalValue;

            stateEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
            originalValuesMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Never);
        }

        [Fact]
        public void Get_Value_delegates_to_StateEntry()
        {
            var stateEntryMock = new Mock<StateEntry>();
            var columnModification = new ColumnModification(
                stateEntryMock.Object,
                new Mock<IProperty>().Object,
                parameterName: null,
                originalParameterName: null,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);

            var value = columnModification.Value;

            stateEntryMock.Verify(m => m[It.IsAny<IPropertyBase>()], Times.Once);
        }

        [Fact]
        public void Set_Value_delegates_to_StateEntry()
        {
            var property = new Mock<IProperty>().Object;
            var stateEntryMock = new Mock<StateEntry>();
            var columnModification = new ColumnModification(
                stateEntryMock.Object,
                property,
                parameterName: null,
                originalParameterName: null,
                isRead: false,
                isWrite: false,
                isKey: false,
                isCondition: false);
            var value = new object();

            columnModification.Value = value;

            stateEntryMock.VerifySet(m => m[property] = It.IsAny<object>(), Times.Once);
        }
    }
}
