// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ShadowStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Constructors_check_arguments()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var stateManager = CreateManagerMock(entityTypeMock).Object;

            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ShadowStateEntry(null, entityTypeMock.Object)).ParamName);
            Assert.Equal(
                "entityType",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ShadowStateEntry(stateManager, null)).ParamName);
        }

        [Fact]
        public void Entity_is_null()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var entry = new ShadowStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object);

            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_shadow_state()
        {
            var propertyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock);
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            Assert.Equal(null, entry.GetPropertyValue(propertyMock.Object));

            entry.SetPropertyValue(propertyMock.Object, "Magic Tree House");

            Assert.Equal("Magic Tree House", entry.GetPropertyValue(propertyMock.Object));
        }

        [Fact]
        public void Can_get_value_buffer_from_only_shadow_properties()
        {
            var propertyMock1 = new Mock<IProperty>();
            var propertyMock2 = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock1, propertyMock2);
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            entry.SetPropertyValue(propertyMock1.Object, "Magic");
            entry.SetPropertyValue(propertyMock2.Object, "Tree House");

            Assert.Equal(new object[] { "Magic", "Tree House" }, entry.GetValueBuffer());
        }

        protected override StateEntry CreateStateEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            return new ShadowStateEntry(stateManager, entityType);
        }

        protected override Mock<IEntityType> CreateEntityTypeMock(Mock<IProperty> key = null, Mock<IProperty> nonKey = null)
        {
            key = key ?? new Mock<IProperty>();
            key.Setup(m => m.Index).Returns(0);
            key.Setup(m => m.ShadowIndex).Returns(0);
            key.Setup(m => m.HasClrProperty).Returns(false);
            var keys = new[] { key.Object };
            nonKey = nonKey ?? new Mock<IProperty>();
            nonKey.Setup(m => m.Index).Returns(1);
            nonKey.Setup(m => m.ShadowIndex).Returns(1);
            nonKey.Setup(m => m.HasClrProperty).Returns(false);

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.GetKey().Properties).Returns(keys);
            entityTypeMock.Setup(m => m.Properties).Returns(keys.Concat(new[] { nonKey.Object }).ToArray());
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(2);

            return entityTypeMock;
        }
    }
}
