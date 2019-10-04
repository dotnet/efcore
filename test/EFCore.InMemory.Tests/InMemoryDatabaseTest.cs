// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryDatabaseTest
    {
        [Fact]
        public void Uses_persistent_database_by_default()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            var store1 = InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, CreateModel()).GetRequiredService<IInMemoryDatabase>();
            var store2 = InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, CreateModel()).GetRequiredService<IInMemoryDatabase>();

            Assert.Same(store1.Store, store2.Store);
        }

        [Fact]
        public void Uses_persistent_database_if_configured_as_persistent()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            Assert.Same(
                CreateStore(serviceProvider).Store,
                CreateStore(serviceProvider).Store);
        }

        [Fact]
        public void EnsureDatabaseCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var store = CreateStore(serviceProvider);
            var stateManager = CreateContextServices(serviceProvider).GetRequiredService<StateManagerDependencies>().With(model);

            Assert.True(store.EnsureDatabaseCreated(stateManager));
            Assert.False(store.EnsureDatabaseCreated(stateManager));
            Assert.False(store.EnsureDatabaseCreated(stateManager));

            store = CreateStore(serviceProvider);

            Assert.False(store.EnsureDatabaseCreated(stateManager));
        }

        private static IInMemoryDatabase CreateStore(IServiceProvider serviceProvider)
            => CreateContextServices(serviceProvider).GetRequiredService<IInMemoryDatabase>();

        private static IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(nameof(InMemoryDatabaseCreatorTest));

            return InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, optionsBuilder.Options);
        }

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());
            var customer = new Customer
            {
                Id = 42,
                Name = "Unikorn"
            };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(1, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows).Count());
            Assert.Equal(new object[] { 42, "Unikorn" }, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).Single().Rows.Single());
        }

        [Fact]
        public async Task Save_changes_updates_changed_objects_in_store()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());

            var customer = new Customer
            {
                Id = 42,
                Name = "Unikorn"
            };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            customer.Name = "Unikorn, The Return";
            entityEntry.SetEntityState(EntityState.Modified);

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(1, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows).Count());
            Assert.Equal(new object[] { 42, "Unikorn, The Return" }, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).Single().Rows.Single());
        }

        [Fact]
        public async Task Save_changes_removes_deleted_objects_from_store()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());

            var customer = new Customer
            {
                Id = 42,
                Name = "Unikorn"
            };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var inMemoryDatabase = serviceProvider.GetRequiredService<IInMemoryDatabase>();

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            // Because the database is being used directly the entity state must be manually changed after saving.
            entityEntry.SetEntityState(EntityState.Unchanged);

            customer.Name = "Unikorn, The Return";
            entityEntry.SetEntityState(EntityState.Deleted);

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(0, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows).Count());
        }

        [Fact]
        public async Task Should_log_writes()
        {
            var loggerFactory = new ListLoggerFactory();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            var scopedServices = InMemoryTestHelpers.Instance.CreateContextServices(serviceCollection, CreateModel());

            var customer = new Customer
            {
                Id = 42,
                Name = "Unikorn"
            };
            var entityEntry = scopedServices.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var inMemoryDatabase = scopedServices.GetRequiredService<IInMemoryDatabase>();

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            var (Level, _, Message, _, _) = loggerFactory.Log.Single(t => t.Id.Id == InMemoryEventId.ChangesSaved.Id);

            Assert.Equal(LogLevel.Information, Level);
            Assert.Equal(InMemoryStrings.LogSavedChanges.GenerateMessage(1), Message);
        }

        private static IModel CreateModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name);
                });

            return modelBuilder.Model;
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
