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
        public void Creates_CLR_only_entry_when_entity_has_no_shadow_properties()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityTypeBuilder = modelBuilder.Entity<RedHook>();
            entityTypeBuilder.Property<int>("Id");
            entityTypeBuilder.Property<int>("Long");
            entityTypeBuilder.Property<string>("Hammer");

            var model = modelBuilder.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

            Assert.IsType<InternalClrEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityTypeBuilder.Metadata, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        [ConditionalFact]
        public void Creates_mixed_entry_when_entity_CLR_entity_type_and_shadow_properties()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            var entityTypeBuilder = modelBuilder.Entity<RedHook>();
            entityTypeBuilder.Property<int>("Id");
            entityTypeBuilder.Property<int>("Long");
            entityTypeBuilder.Property<string>("Spanner");

            var model = modelBuilder.FinalizeModel();

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
            var stateManager = contextServices.GetRequiredService<IStateManager>();
            var factory = contextServices.GetRequiredService<IInternalEntityEntryFactory>();

            var entity = new RedHook();
            var entry = factory.Create(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

            Assert.IsType<InternalMixedEntityEntry>(entry);

            Assert.Same(stateManager, entry.StateManager);
            Assert.Same(entityTypeBuilder.Metadata, entry.EntityType);
            Assert.Same(entity, entry.Entity);
        }

        private class RedHook
        {
            public int Id { get; set; }
            public int Long { get; set; }
            public string Hammer { get; set; }
        }
    }
}
