// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class UpdatesTestBase<TFixture, TTestStore> : IClassFixture<TFixture>, IDisposable
        where TFixture : UpdatesFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        [Fact]
        public virtual void Save_partial_update()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Products.Attach(
                            new Product
                            {
                                Id = productId,
                                Price = 1.99M
                            });

                        entry.Property(p => p.Price).IsModified = true;

                        Assert.False(entry.Property(p => p.DependentId).IsModified);
                        Assert.False(entry.Property(p => p.Name).IsModified);

                        context.SaveChanges();
                    },
                context =>
                    {
                        var product = context.Products.First(p => p.Id == productId);

                        Assert.Equal(1.99M, product.Price);
                        Assert.Equal(null, product.DependentId);
                        Assert.Equal("Apple Cider", product.Name);
                    });
        }

        [Fact]
        public virtual void Save_partial_update_on_missing_record_throws()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var entry = context.Products.Attach(
                        new Product
                        {
                            Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10"),
                            Name = "Apple Fritter"
                        });

                    entry.Property(c => c.Name).IsModified = true;

                    Assert.Equal(
                        UpdateConcurrencyMessage,
                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void Can_remove_partial()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Products.Remove(
                            new Product
                            {
                                Id = productId
                            });

                        context.SaveChanges();
                    },
                context =>
                    {
                        var product = context.Products.FirstOrDefault(f => f.Id == productId);

                        Assert.Equal(null, product);
                    });
        }

        [Fact]
        public virtual void Remove_partial_on_missing_record_throws()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var entry = context.Products.Remove(
                        new Product
                        {
                            Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10")
                        });

                    Assert.Equal(
                        UpdateConcurrencyMessage,
                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void SaveChanges_processes_all_tracked_entities()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
                    var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 });
                    var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
                    var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

                    entry1.SetEntityState(EntityState.Added);
                    entry2.SetEntityState(EntityState.Modified);
                    entry3.SetEntityState(EntityState.Unchanged);
                    entry4.SetEntityState(EntityState.Deleted);

                    var processedEntities = stateManager.SaveChanges(true);

                    Assert.Equal(3, processedEntities);
                    Assert.Equal(3, stateManager.Entries.Count());
                    Assert.Contains(entry1, stateManager.Entries);
                    Assert.Contains(entry2, stateManager.Entries);
                    Assert.Contains(entry3, stateManager.Entries);

                    Assert.Equal(EntityState.Unchanged, entry1.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry2.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry3.EntityState);
                });
        }

        [Fact]
        public virtual void SaveChanges_false_processes_all_tracked_entities_without_calling_AcceptAllChanges()
        {
            ExecuteWithStrategyInTransaction(context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
                    var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 });
                    var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
                    var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

                    entry1.SetEntityState(EntityState.Added);
                    entry2.SetEntityState(EntityState.Modified);
                    entry3.SetEntityState(EntityState.Unchanged);
                    entry4.SetEntityState(EntityState.Deleted);

                    var processedEntities = stateManager.SaveChanges(false);

                    Assert.Equal(3, processedEntities);
                    Assert.Equal(4, stateManager.Entries.Count());
                    Assert.Contains(entry1, stateManager.Entries);
                    Assert.Contains(entry2, stateManager.Entries);
                    Assert.Contains(entry3, stateManager.Entries);
                    Assert.Contains(entry4, stateManager.Entries);

                    Assert.Equal(EntityState.Added, entry1.EntityState);
                    Assert.Equal(EntityState.Modified, entry2.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry3.EntityState);
                    Assert.Equal(EntityState.Deleted, entry4.EntityState);
                });
        }

        [Fact]
        public Task SaveChangesAsync_processes_all_tracked_entities()
        {
            return ExecuteWithStrategyInTransactionAsync(async context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
                    var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 });
                    var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
                    var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

                    entry1.SetEntityState(EntityState.Added);
                    entry2.SetEntityState(EntityState.Modified);
                    entry3.SetEntityState(EntityState.Unchanged);
                    entry4.SetEntityState(EntityState.Deleted);

                    var processedEntities = await stateManager.SaveChangesAsync(true);

                    Assert.Equal(3, processedEntities);
                    Assert.Equal(3, stateManager.Entries.Count());
                    Assert.Contains(entry1, stateManager.Entries);
                    Assert.Contains(entry2, stateManager.Entries);
                    Assert.Contains(entry3, stateManager.Entries);

                    Assert.Equal(EntityState.Unchanged, entry1.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry2.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry3.EntityState);
                });
        }

        [Fact]
        public Task SaveChangesAsync_false_processes_all_tracked_entities_without_calling_AcceptAllChanges()
        {
            return ExecuteWithStrategyInTransactionAsync(async context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
                    var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 });
                    var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
                    var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

                    entry1.SetEntityState(EntityState.Added);
                    entry2.SetEntityState(EntityState.Modified);
                    entry3.SetEntityState(EntityState.Unchanged);
                    entry4.SetEntityState(EntityState.Deleted);

                    var processedEntities = await stateManager.SaveChangesAsync(false);

                    Assert.Equal(3, processedEntities);
                    Assert.Equal(4, stateManager.Entries.Count());
                    Assert.Contains(entry1, stateManager.Entries);
                    Assert.Contains(entry2, stateManager.Entries);
                    Assert.Contains(entry3, stateManager.Entries);
                    Assert.Contains(entry4, stateManager.Entries);

                    Assert.Equal(EntityState.Added, entry1.EntityState);
                    Assert.Equal(EntityState.Modified, entry2.EntityState);
                    Assert.Equal(EntityState.Unchanged, entry3.EntityState);
                    Assert.Equal(EntityState.Deleted, entry4.EntityState);
                });
        }

        protected abstract string UpdateConcurrencyMessage { get; }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<UpdatesContext> testOperation,
            Action<UpdatesContext> nestedTestOperation1 = null)
            => DbContextHelpers.ExecuteWithStrategyInTransaction(CreateContext, UseTransaction,
                testOperation, nestedTestOperation1);

        protected virtual Task ExecuteWithStrategyInTransactionAsync(
            Func<UpdatesContext, Task> testOperation,
            Func<UpdatesContext, Task> nestedTestOperation1 = null)
            => DbContextHelpers.ExecuteWithStrategyInTransactionAsync(CreateContext, UseTransaction,
                testOperation, nestedTestOperation1);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected UpdatesContext CreateContext() => Fixture.CreateContext(TestStore);

        protected UpdatesTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
            => TestStore.Dispose();
    }
}
