// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class InteranlEntryEntryFactoryTest
    {
        [Fact]
        public void Creates_shadow_state_only_entry_when_entity_is_fully_shadow_state()
        {
            var model = new Model();
            var entityType = model.AddEntityType("RedHook");
            entityType.GetOrAddProperty("Long", typeof(int), shadowProperty: true);
            entityType.GetOrAddProperty("Hammer", typeof(string), shadowProperty: true);

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entry = factory.Create(stateManager, entityType, new Random());

            Assert.IsType<InternalShadowEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_when_entity_has_no_shadow_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(RedHook));
            entityType.GetOrAddProperty("Long", typeof(int));
            entityType.GetOrAddProperty("Hammer", typeof(string));

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<InternalClrEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_mixed_entry_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(RedHook));
            entityType.GetOrAddProperty("Long", typeof(int));
            entityType.GetOrAddProperty("Hammer", typeof(string), shadowProperty: true);

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<InternalMixedEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        private class RedHook
        {
            public int Long { get; set; }
            public string Hammer { get; set; }
        }
    }
}
