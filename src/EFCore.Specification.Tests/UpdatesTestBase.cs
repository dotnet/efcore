// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : UpdatesFixtureBase
    {
        protected UpdatesTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Mutation_of_tracked_values_does_not_mutate_values_in_store()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var bytes = new byte[] { 1, 2, 3, 4 };

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.AFewBytes.AddRange(
                        new AFewBytes()
                        {
                            Id = id1,
                            Bytes = bytes,
                        },
                        new AFewBytes()
                        {
                            Id = id2,
                            Bytes = bytes,
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    bytes[1] = 22;

                    var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                    var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                    Assert.Equal(2, fromStore1.Bytes[1]);
                    Assert.Equal(2, fromStore2.Bytes[1]);

                    fromStore1.Bytes[1] = 222;
                    fromStore2.Bytes[1] = 222;

                    context.Entry(fromStore1).State = EntityState.Modified;

                    context.SaveChanges();
                },
                context =>
                {
                    var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                    var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                    Assert.Equal(222, fromStore1.Bytes[1]);
                    Assert.Equal(2, fromStore2.Bytes[1]);
                });
        }

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
                            Price = 1.49M
                        });

                    entry.Property(c => c.Price).CurrentValue = 1.99M;
                    entry.Property(p => p.Price).IsModified = true;

                    Assert.False(entry.Property(p => p.DependentId).IsModified);
                    Assert.False(entry.Property(p => p.Name).IsModified);

                    context.SaveChanges();
                },
                context =>
                {
                    var product = context.Products.First(p => p.Id == productId);

                    Assert.Equal(1.99M, product.Price);
                    Assert.Equal("Apple Cider", product.Name);
                });
        }

        [Fact]
        public virtual void Save_partial_update_on_missing_record_throws()
        {
            ExecuteWithStrategyInTransaction(
                context =>
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
        public virtual void Save_partial_update_on_concurrency_token_original_value_mismatch_throws()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Products.Attach(
                        new Product
                        {
                            Id = productId,
                            Name = "Apple Fritter",
                            Price = 3.49M // Not the same as the value stored in the database
                        });

                    entry.Property(c => c.Name).IsModified = true;

                    Assert.Equal(
                        UpdateConcurrencyTokenMessage,
                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void Update_on_bytes_concurrency_token_original_value_mismatch_throws()
        {
            var productId = Guid.NewGuid();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var entry = context.ProductWithBytes.Attach(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }
                        });

                    entry.Entity.Name = "GigaChips";

                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges());
                },
                context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId).Name));
        }

        [Fact]
        public virtual void Update_on_bytes_concurrency_token_original_value_matches_does_not_throw()
        {
            var productId = Guid.NewGuid();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var entry = context.ProductWithBytes.Attach(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    entry.Entity.Name = "GigaChips";

                    Assert.Equal(1, context.SaveChanges());
                },
                context => Assert.Equal("GigaChips", context.ProductWithBytes.Find(productId).Name));
        }

        [Fact]
        public virtual void Remove_on_bytes_concurrency_token_original_value_mismatch_throws()
        {
            var productId = Guid.NewGuid();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var entry = context.ProductWithBytes.Attach(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }
                        });

                    entry.State = EntityState.Deleted;

                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges());
                },
                context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId).Name));
        }

        [Fact]
        public virtual void Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw()
        {
            var productId = Guid.NewGuid();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var entry = context.ProductWithBytes.Attach(
                        new ProductWithBytes
                        {
                            Id = productId,
                            Name = "MegaChips",
                            Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                        });

                    entry.State = EntityState.Deleted;

                    Assert.Equal(1, context.SaveChanges());
                },
                context => Assert.Null(context.ProductWithBytes.Find(productId)));
        }

        [Fact]
        public virtual void Can_remove_partial()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Products.Remove(
                        new Product
                        {
                            Id = productId,
                            Price = 1.49M
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var product = context.Products.FirstOrDefault(f => f.Id == productId);

                    Assert.Null(product);
                });
        }

        [Fact]
        public virtual void Remove_partial_on_missing_record_throws()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Products.Remove(
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
        public virtual void Remove_partial_on_concurrency_token_original_value_mismatch_throws()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Products.Remove(
                        new Product
                        {
                            Id = productId,
                            Price = 3.49M // Not the same as the value stored in the database
                        });

                    Assert.Equal(
                        UpdateConcurrencyTokenMessage,
                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges()).Message);
                });
        }

        [Fact]
        public virtual void Save_replaced_principal()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var category = context.Categories.Single();
                    var products = context.Products.Where(p => p.DependentId == category.PrincipalId).ToList();

                    Assert.Equal(2, products.Count);

                    var newCategory = new Category
                    {
                        Id = category.Id,
                        PrincipalId = category.PrincipalId,
                        Name = "New Category"
                    };
                    context.Remove(category);
                    context.Add(newCategory);

                    context.SaveChanges();
                },
                context =>
                {
                    var category = context.Categories.Single();
                    var products = context.Products.Where(p => p.DependentId == category.PrincipalId).ToList();

                    Assert.Equal("New Category", category.Name);
                    Assert.Equal(2, products.Count);
                });
        }

        [Fact]
        public virtual void SaveChanges_processes_all_tracked_entities()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 77,
                            PrincipalId = 777
                        });
                    var entry2 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 78,
                            PrincipalId = 778
                        });
                    var entry3 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId1
                        });
                    var entry4 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId2,
                            Price = 2.49M
                        });

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
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 77,
                            PrincipalId = 777
                        });
                    var entry2 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 78,
                            PrincipalId = 778
                        });
                    var entry3 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId1
                        });
                    var entry4 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId2,
                            Price = 2.49M
                        });

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
            return ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 77,
                            PrincipalId = 777
                        });
                    var entry2 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 78,
                            PrincipalId = 778
                        });
                    var entry3 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId1
                        });
                    var entry4 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId2,
                            Price = 2.49M
                        });

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
            return ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var stateManager = context.ChangeTracker.GetInfrastructure();

                    var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                    var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                    var entry1 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 77,
                            PrincipalId = 777
                        });
                    var entry2 = stateManager.GetOrCreateEntry(
                        new Category
                        {
                            Id = 78,
                            PrincipalId = 778
                        });
                    var entry3 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId1
                        });
                    var entry4 = stateManager.GetOrCreateEntry(
                        new Product
                        {
                            Id = productId2,
                            Price = 2.49M
                        });

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

        protected abstract string UpdateConcurrencyTokenMessage { get; }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<UpdatesContext> testOperation,
            Action<UpdatesContext> nestedTestOperation1 = null,
            Action<UpdatesContext> nestedTestOperation2 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2);

        protected virtual Task ExecuteWithStrategyInTransactionAsync(
            Func<UpdatesContext, Task> testOperation,
            Func<UpdatesContext, Task> nestedTestOperation1 = null,
            Func<UpdatesContext, Task> nestedTestOperation2 = null)
            => TestHelpers.ExecuteWithStrategyInTransactionAsync(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected UpdatesContext CreateContext() => Fixture.CreateContext();
    }
}
