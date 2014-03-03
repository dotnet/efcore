// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStoreTest
    {
        #region Fixture

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var model = CreateModel();
            var changeTracker = new StateManager(
                model, new ActiveIdentityGenerators(new InMemoryIdentityGeneratorFactory()), Enumerable.Empty<IEntityStateListener>());
            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new StateEntry(changeTracker, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);
            var inMemoryDataStore = new InMemoryDataStore();

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(1, inMemoryDataStore.Objects.Count);
            Assert.Equal(new object[] { 42, "Unikorn" }, inMemoryDataStore.Objects[customer]);
        }

        [Fact]
        public async Task Save_changes_updates_changed_objects_in_store()
        {
            var model = CreateModel();
            var changeTracker = new StateManager(
                model, new ActiveIdentityGenerators(new InMemoryIdentityGeneratorFactory()), Enumerable.Empty<IEntityStateListener>());

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new StateEntry(changeTracker, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);
            var inMemoryDataStore = new InMemoryDataStore();
            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            customer.Name = "Unikorn, The Return";
            entityEntry = new StateEntry(changeTracker, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(1, inMemoryDataStore.Objects.Count);
            Assert.Equal(new object[] { 42, "Unikorn, The Return" }, inMemoryDataStore.Objects[customer]);
        }

        [Fact]
        public async Task Save_changes_removes_deleted_objects_from_store()
        {
            var model = CreateModel();
            var changeTracker = new StateManager(
                model, new ActiveIdentityGenerators(new InMemoryIdentityGeneratorFactory()), Enumerable.Empty<IEntityStateListener>());

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new StateEntry(changeTracker, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);
            var inMemoryDataStore = new InMemoryDataStore();
            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            customer.Name = "Unikorn, The Return";
            entityEntry = new StateEntry(changeTracker, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Deleted, CancellationToken.None);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry }, model);

            Assert.Equal(0, inMemoryDataStore.Objects.Count);
        }

        private static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Name));

            return model;
        }
    }
}
