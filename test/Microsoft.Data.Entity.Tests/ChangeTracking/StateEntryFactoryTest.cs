// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateEntryFactoryTest
    {
        [Fact]
        public void Creates_shadow_state_only_entry_when_entity_is_fully_shadow_state()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(false);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { new Mock<IProperty>().Object });

            var configuration = Mock.Of<ContextConfiguration>();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, new Random());

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_when_entity_has_no_shadow_properties()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(true);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { new Mock<IProperty>().Object });
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(0);

            var configuration = Mock.Of<ContextConfiguration>();
            var entity = new Random();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, entity);

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_mixed_entry_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(true);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { new Mock<IProperty>().Object });
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(1);

            var configuration = Mock.Of<ContextConfiguration>();
            var entity = new Random();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, entity);

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_shadow_state_only_entry_from_value_buffer_when_entity_is_fully_shadow_state()
        {
            var property = new Mock<IProperty>().Object;
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(false);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { property });

            var configuration = Mock.Of<ContextConfiguration>();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, new object[] { 77 });

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Equal(77, entry.GetPropertyValue(property));
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_from_value_buffer_when_entity_has_no_shadow_properties()
        {
            var property = new Mock<IProperty>().Object;
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(true);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { property });
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(0);

            var configuration = Mock.Of<ContextConfiguration>();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, new object[] { 77 });

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Equal(77, entry.GetPropertyValue(property));
        }

        [Fact]
        public void Creates_mixed_entry_from_value_buffer_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var property = new Mock<IProperty>().Object;
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.HasClrType).Returns(true);
            entityTypeMock.Setup(m => m.Properties).Returns(new[] { property });
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(1);

            var configuration = Mock.Of<ContextConfiguration>();

            var entry = new StateEntryFactory(configuration).Create(entityTypeMock.Object, new object[] { 77 });

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(configuration, entry.Configuration);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Equal(77, entry.GetPropertyValue(property));
        }
    }
}
