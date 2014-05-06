// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreTest
    {
        [Fact]
        public void Uses_persistent_database_by_default()
        {
            var configuration = CreateConfiguration();
            var persistentDatabase = new InMemoryDatabase(new NullLoggerFactory());

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.Same(persistentDatabase, inMemoryDataStore.Database);
        }

        [Fact]
        public void Uses_persistent_database_if_configured_as_persistent()
        {
            var configuration = CreateConfiguration(new DbContextOptions()
                .UseInMemoryStore(persist: true)
                .BuildConfiguration());

            var persistentDatabase = new InMemoryDatabase(new NullLoggerFactory());

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.Same(persistentDatabase, inMemoryDataStore.Database);
        }

        [Fact]
        public void Uses_transient_database_if_not_configured_as_persistent()
        {
            var configuration = CreateConfiguration(new DbContextOptions()
                .UseInMemoryStore(persist: false)
                .BuildConfiguration());

            var persistentDatabase = new InMemoryDatabase(new NullLoggerFactory());

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.NotNull(inMemoryDataStore.Database);
            Assert.NotSame(persistentDatabase, inMemoryDataStore.Database);
            Assert.Same(inMemoryDataStore.Database, inMemoryDataStore.Database);
        }

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new NullLoggerFactory()));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

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

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new NullLoggerFactory()));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

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

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new NullLoggerFactory()));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Deleted, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

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

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(mockFactory.Object));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

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

        private static DbContextConfiguration CreateConfiguration()
        {
            return CreateConfiguration(new DbContextOptions().BuildConfiguration());
        }

        private static DbContextConfiguration CreateConfiguration(ImmutableDbContextOptions configuration)
        {
            return new DbContext(configuration).Configuration;
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
