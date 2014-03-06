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

            var stateManager = new Mock<StateManager>().Object;

            var entry = new StateEntryFactory().Create(stateManager, entityTypeMock.Object, new Random());

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
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

            var stateManager = new Mock<StateManager>().Object;
            var entity = new Random();

            var entry = new StateEntryFactory().Create(stateManager, entityTypeMock.Object, entity);

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
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

            var stateManager = new Mock<StateManager>().Object;
            var entity = new Random();

            var entry = new StateEntryFactory().Create(stateManager, entityTypeMock.Object, entity);

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityTypeMock.Object, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }
    }
}
