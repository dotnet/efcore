// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalEntityEntryFactoryTest
    {
        [ConditionalFact]
        public void Creates_shadow_state_only_entry_when_entity_is_fully_shadow_state()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType("RedHook");
            entityType.AddProperty("Long", typeof(int));
            entityType.AddProperty("Hammer", typeof(string));
            model.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entry = factory.Create(stateManager, entityType, new Random());

            Assert.IsType<InternalShadowEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Null(entry.Entity);
        }

        [ConditionalFact]
        public void Creates_CLR_only_entry_when_entity_has_no_shadow_properties()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(RedHook));
            entityType.AddProperty("Long", typeof(int));
            entityType.AddProperty("Hammer", typeof(string));
            model.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<InternalClrEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [ConditionalFact]
        public void Creates_mixed_entry_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(RedHook));
            entityType.AddProperty("Long", typeof(int));
            entityType.AddProperty("Spanner", typeof(string));
            model.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, entityType, entity);

            Assert.IsType<InternalMixedEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityType, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        private static IMutableModel CreateModel() => new Model();

        private class RedHook
        {
            public int Long { get; set; }
            public string Hammer { get; set; }
        }
    }
}
