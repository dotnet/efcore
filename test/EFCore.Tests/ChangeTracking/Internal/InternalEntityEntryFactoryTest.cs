// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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

        var entity = new RedHook();
        var entry = new InternalEntityEntry(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

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

        var entity = new RedHook();
        var entry = new InternalEntityEntry(stateManager, (IEntityType)entityTypeBuilder.Metadata, entity);

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
