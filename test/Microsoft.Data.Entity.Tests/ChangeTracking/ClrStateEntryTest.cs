// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ClrStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Constructors_check_arguments()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var stateManager = CreateManagerMock(entityTypeMock).Object;

            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ClrStateEntry(null, entityTypeMock.Object, new Random())).ParamName);
            Assert.Equal(
                "entityType",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new ClrStateEntry(stateManager, null, new Random())).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new ClrStateEntry(stateManager, entityTypeMock.Object, null)).ParamName);
        }

        [Fact]
        public void Can_get_entity()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var entity = new Random();
            var entry = new ClrStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var propertyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), propertyMock);
            var managerMock = CreateManagerMock(entityTypeMock);

            var getterMock = new Mock<IClrPropertyGetter>();
            managerMock.Setup(m => m.GetClrPropertyGetter(propertyMock.Object)).Returns(getterMock.Object);

            var setterMock = new Mock<IClrPropertySetter>();
            managerMock.Setup(m => m.GetClrPropertySetter(propertyMock.Object)).Returns(setterMock.Object);

            var entity = new Random();
            var entry = new ClrStateEntry(managerMock.Object, entityTypeMock.Object, entity);

            Assert.Equal(null, entry.GetPropertyValue(propertyMock.Object));

            getterMock.Verify(m => m.GetClrValue(entity));

            entry.SetPropertyValue(propertyMock.Object, "Magic Tree House");

            setterMock.Verify(m => m.SetClrValue(entity, "Magic Tree House"));
        }

        [Fact]
        public void Can_get_value_buffer_from_CLR_properties()
        {
            var propertyMock1 = new Mock<IProperty>();
            var propertyMock2 = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock1, propertyMock2);
            var managerMock = CreateManagerMock(entityTypeMock);

            var getterMock1 = new Mock<IClrPropertyGetter>();
            getterMock1.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Magic");

            var getterMock2 = new Mock<IClrPropertyGetter>();
            getterMock2.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Tree House");

            managerMock.Setup(m => m.GetClrPropertyGetter(propertyMock1.Object)).Returns(getterMock1.Object);
            managerMock.Setup(m => m.GetClrPropertyGetter(propertyMock2.Object)).Returns(getterMock2.Object);

            var entry = new ClrStateEntry(managerMock.Object, entityTypeMock.Object, new Random());

            Assert.Equal(new object[] { "Magic", "Tree House" }, entry.GetValueBuffer());
        }

        protected override StateEntry CreateStateEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            return new ClrStateEntry(stateManager, entityType, entity);
        }
    }
}
