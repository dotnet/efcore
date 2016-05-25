// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.FunctionalTests;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.Tests
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
                CreateStore(serviceProvider, persist: true).Store,
                CreateStore(serviceProvider, persist: true).Store);
        }

        [Fact]
        public void EnsureDatabaseCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var store = CreateStore(serviceProvider, persist: true);

            Assert.True(store.EnsureDatabaseCreated(model));
            Assert.False(store.EnsureDatabaseCreated(model));
            Assert.False(store.EnsureDatabaseCreated(model));

            store = CreateStore(serviceProvider, persist: true);

            Assert.False(store.EnsureDatabaseCreated(model));
        }

        private static IInMemoryDatabase CreateStore(IServiceProvider serviceProvider, bool persist)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            return InMemoryTestHelpers.Instance.CreateContextServices(serviceProvider, optionsBuilder.Options).GetRequiredService<IInMemoryDatabase>();
        }

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(CreateModel());
            var customer = new Customer { Id = 42, Name = "Unikorn" };
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

            var customer = new Customer { Id = 42, Name = "Unikorn" };
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

            Assert.Equal(0, inMemoryDatabase.Store.GetTables(entityEntry.EntityType).SelectMany(t => t.Rows).Count());
        }

        [Fact]
        public async Task Should_log_writes()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockFactory = new Mock<ILoggerFactory>();
            mockFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockFactory.Object);

            var scopedServices = InMemoryTestHelpers.Instance.CreateContextServices(serviceCollection, CreateModel());

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = scopedServices.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var inMemoryDatabase = scopedServices.GetRequiredService<IInMemoryDatabase>();

            await inMemoryDatabase.SaveChangesAsync(new[] { entityEntry });

            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    (int)InMemoryEventId.SavedChanges,
                    1,
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        private static IModel CreateModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<Customer>(b =>
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
