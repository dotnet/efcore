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
            var entity = new Random();
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, entity);

            Assert.Equal(null, entry.GetPropertyValue(propertyMock.Object));

            propertyMock.Verify(m => m.GetValue(entity));

            entry.SetPropertyValue(propertyMock.Object, "Magic Tree House");

            propertyMock.Verify(m => m.SetValue(entity, "Magic Tree House"));
        }

        [Fact]
        public void Can_get_value_buffer_from_CLR_properties()
        {
            var propertyMock1 = new Mock<IProperty>();
            var propertyMock2 = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock1, propertyMock2);
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            propertyMock1.Setup(m => m.GetValue(It.IsAny<object>())).Returns("Magic");
            propertyMock2.Setup(m => m.GetValue(It.IsAny<object>())).Returns("Tree House");

            Assert.Equal(new object[] { "Magic", "Tree House" }, entry.GetValueBuffer());
        }

        protected override StateEntry CreateStateEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            return new ClrStateEntry(stateManager, entityType, entity);
        }
    }
}
