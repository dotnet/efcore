// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateEntryFactoryTest
    {
        [Fact]
        public void Creates_shadow_state_only_entry_when_entity_is_fully_shadow_state()
        {
            var model = new Model();
            var entityType = model.AddEntityType("RedHook");
            entityType.GetOrAddProperty("Long", typeof(int), shadowProperty: true);
            entityType.GetOrAddProperty("Hammer", typeof(string), shadowProperty: true);

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();

            var entry = factory.Create(stateManager, entityType, new Random());

            Assert.IsType<ShadowStateEntry>(entry);

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
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<ClrStateEntry>(entry);

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
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Creates_shadow_state_only_entry_from_value_buffer_when_entity_is_fully_shadow_state()
        {
            var model = new Model();
            var entityType = model.AddEntityType("RedHook");
            var property1 = entityType.GetOrAddProperty("Long", typeof(int), shadowProperty: true);
            var property2 = entityType.GetOrAddProperty("Hammer", typeof(string), shadowProperty: true);

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();
            var entry = factory.Create(stateManager, entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<ShadowStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);
            Assert.Null(entry.Entity);
        }

        [Fact]
        public void Creates_CLR_only_entry_from_value_buffer_when_entity_has_no_shadow_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(RedHook));
            var property1 = entityType.GetOrAddProperty("Long", typeof(int));
            var property2 = entityType.GetOrAddProperty("Hammer", typeof(string));

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();

            var entry = factory.Create(stateManager, entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<ClrStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);

            var entity = (RedHook)entry.Entity;
            Assert.Equal(77, entity.Long);
            Assert.Equal("Green", entity.Hammer);
        }

        [Fact]
        public void Creates_mixed_entry_from_value_buffer_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(RedHook));
            var property1 = entityType.GetOrAddProperty("Long", typeof(int));
            var property2 = entityType.GetOrAddProperty("Hammer", typeof(string), shadowProperty: true);

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<StateManager>();
            var factory = contextServices.GetRequiredService<StateEntryFactory>();

            var entry = factory.Create(stateManager, entityType, new ObjectArrayValueReader(new object[] { "Green", 77 }));

            Assert.IsType<MixedStateEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Equal(77, entry[property1]);
            Assert.Equal("Green", entry[property2]);

            var entity = (RedHook)entry.Entity;
            Assert.Equal(77, entity.Long);
            Assert.Null(entity.Hammer);
        }

        private class RedHook
        {
            public int Long { get; set; }
            public string Hammer { get; set; }
        }
    }
}
