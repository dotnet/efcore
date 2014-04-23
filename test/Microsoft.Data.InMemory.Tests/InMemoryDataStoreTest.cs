// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Moq;
using Xunit;

namespace Microsoft.Data.InMemory.Tests
{
    public class InMemoryDataStoreTest
    {
        #region Fixture

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);

            var loggerFactory = new NullLoggerFactory();
            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(loggerFactory), loggerFactory);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(1, inMemoryDataStore.Database.SelectMany(t => t).Count());
            Assert.Equal(new object[] { 42, "Unikorn" }, inMemoryDataStore.Database.Single().Single());
        }

        [Fact]
        public async Task Save_changes_updates_changed_objects_in_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);
            
            var loggerFactory = new NullLoggerFactory();
            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(loggerFactory), loggerFactory);
            
            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(1, inMemoryDataStore.Database.SelectMany(t => t).Count());
            Assert.Equal(new object[] { 42, "Unikorn, The Return" }, inMemoryDataStore.Database.Single().Single());
        }

        [Fact]
        public async Task Save_changes_removes_deleted_objects_from_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);
            
            var loggerFactory = new NullLoggerFactory();
            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(loggerFactory), loggerFactory);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Deleted, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(0, inMemoryDataStore.Database.SelectMany(t => t).Count());
        }

        [Fact]
        public async Task Should_log_writes()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);


            var mockLogger = new Mock<ILogger>();
            var mockFactory = new Mock<ILoggerFactory>();
            mockFactory.Setup(m => m.Create(It.IsAny<string>())).Returns(mockLogger.Object);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(mockFactory.Object), mockFactory.Object);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            mockLogger.Verify(
                l => l.WriteCore(
                    TraceType.Information,
                    0,
                    It.IsAny<string>(),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Name));

            return model;
        }

        private static ContextConfiguration CreateConfiguration()
        {
            return new EntityContext(new EntityConfigurationBuilder().BuildConfiguration()).Configuration;
        }
    }
}
