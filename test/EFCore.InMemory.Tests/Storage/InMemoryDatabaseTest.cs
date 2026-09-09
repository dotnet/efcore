// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Storage;

public class InMemoryDatabaseTest
{
    [ConditionalFact]
    public void Uses_persistent_database_by_default()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

        var store1 = InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, CreateModel())
            .GetRequiredService<IInMemoryDatabase>();
        var store2 = InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, CreateModel())
            .GetRequiredService<IInMemoryDatabase>();

        Assert.Same(store1.Store, store2.Store);
    }

    [ConditionalFact]
    public void Uses_persistent_database_if_configured_as_persistent()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

        Assert.Same(
            CreateStore(serviceProvider).Store,
            CreateStore(serviceProvider).Store);
    }

    [ConditionalFact]
    public void EnsureDatabaseCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
        var store = CreateStore(serviceProvider);

        Assert.True(store.EnsureDatabaseCreated());
        Assert.False(store.EnsureDatabaseCreated());
        Assert.False(store.EnsureDatabaseCreated());

        store = CreateStore(serviceProvider);

        Assert.False(store.EnsureDatabaseCreated());
    }

    private static IInMemoryDatabase CreateStore(IServiceProvider serviceProvider)
        => CreateContextServices(serviceProvider).GetRequiredService<IInMemoryDatabase>();

    private static IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseInMemoryDatabase(nameof(InMemoryDatabaseCreatorTest));

        return InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, optionsBuilder.Options);
    }

    [ConditionalFact]
    public async Task Save_changes_adds_new_objects_to_store()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());
        var customer = new Customer { Id = 42, Name = "Unikorn" };
        var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
        entityEntry.SetEntityState(EntityState.Added);

        var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        Assert.Single(inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows));
        Assert.Equal([42, "Unikorn"], inMemoryDatabase.Store.GetTables(entityEntry.EntityType).Single().Rows.Single());
    }

    [ConditionalFact]
    public async Task Save_changes_updates_changed_objects_in_store()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());

        var customer = new Customer { Id = 42, Name = "Unikorn" };
        var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
        entityEntry.SetEntityState(EntityState.Added);

        var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        customer.Name = "Unikorn, The Return";
        entityEntry.SetEntityState(EntityState.Modified);

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        Assert.Single(inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows));
        Assert.Equal(
            [42, "Unikorn, The Return"],
            inMemoryDatabase.Store.GetTables(entityEntry.EntityType).Single().Rows.Single());
    }

    [ConditionalFact]
    public async Task Save_changes_removes_deleted_objects_from_store()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());

        var customer = new Customer { Id = 42, Name = "Unikorn" };
        var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
        entityEntry.SetEntityState(EntityState.Added);

        var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        // Because the database is being used directly the entity state must be manually changed after saving.
        entityEntry.SetEntityState(EntityState.Unchanged);

        customer.Name = "Unikorn, The Return";
        entityEntry.SetEntityState(EntityState.Deleted);

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        Assert.Empty(inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows));
    }

    [ConditionalFact]
    public async Task Should_log_writes()
    {
        var loggerFactory = new ListLoggerFactory();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

        var scopedServices = InMemoryTestHelpers.Instance.CreateContextServices(serviceCollection, CreateModel());

        var customer = new Customer { Id = 42, Name = "Unikorn" };
        var entityEntry = scopedServices.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
        entityEntry.SetEntityState(EntityState.Added);

        var inMemoryDatabase = scopedServices.GetRequiredService<IInMemoryDatabase>();

        await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

        var (Level, _, Message, _, _) = loggerFactory.Log.Single(t => t.Id.Id == InMemoryEventId.ChangesSaved.Id);

        Assert.Equal(LogLevel.Information, Level);
        Assert.Equal(InMemoryResources.LogSavedChanges(new TestLogger<InMemoryLoggingDefinitions>()).GenerateMessage(1), Message);
    }

    private static IModel CreateModel()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Customer>(
            b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Name);
            });

        return modelBuilder.Model.FinalizeModel();
    }

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
