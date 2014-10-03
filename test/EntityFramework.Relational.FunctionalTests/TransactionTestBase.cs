// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class TransactionTestBase<TTestStore>
        where TTestStore : RelationalTestStore
    {
        [Fact]
        public async Task SaveChanges_implicitly_starts_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                    context.ChangeTracker.Entry(context.Set<Customer>().Last()).State = EntityState.Added;

                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }

                await AssertStoreInitialStateAsync(testDatabase);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_implicitly_starts_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                    context.ChangeTracker.Entry(context.Set<Customer>().Last()).State = EntityState.Added;

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                    }
                }

                await AssertStoreInitialStateAsync(testDatabase);
            }
        }

        [Fact]
        public async Task SaveChanges_uses_explicit_transaction_without_committing()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (context.Database.AsRelational().Connection.BeginTransaction())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        context.SaveChanges();
                    }
                }

                await AssertStoreInitialStateAsync(testDatabase);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_without_committing()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (await context.Database.AsRelational().Connection.BeginTransactionAsync())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                    }
                }

                await AssertStoreInitialStateAsync(testDatabase);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_and_rollsback_on_failure()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        context.ChangeTracker.Entry(context.Set<Customer>().Last()).State = EntityState.Added;

                        try
                        {
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateException)
                        {
                        }

                        Assert.Null(transaction.DbTransaction.Connection);
                    }
                }
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_commited()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                        transaction.Commit();
                    }
                }

                using (var context = await CreateContextAsync(testDatabase))
                {
                    Assert.Equal(Customers.Count - 1, context.Set<Customer>().Count());
                }
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_rolled_back()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                        transaction.Rollback();

                        await AssertStoreInitialStateAsync(testDatabase);
                    }
                }
            }
        }

        [Fact]
        public async Task Query_uses_explicit_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (var transaction = context.Database.AsRelational().Connection.BeginTransaction())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();

                        using (var innerContext = await CreateContextAsync(testDatabase))
                        {
                            using (innerContext.Database.AsRelational().Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                            {
                                Assert.Equal(Customers.Count - 1, innerContext.Set<Customer>().Count());
                            }

                            if (SnapshotSupported)
                            {
                                using (innerContext.Database.AsRelational().Connection.BeginTransaction(IsolationLevel.Snapshot))
                                {
                                    Assert.Equal(Customers, innerContext.Set<Customer>().OrderBy(c => c.Id).ToList());
                                }
                            }
                        }

                        using (var innerContext = await CreateContextAsync(context.Database.AsRelational().Connection.DbConnection))
                        {
                            innerContext.Database.AsRelational().Connection.UseTransaction(transaction.DbTransaction);
                            Assert.Equal(Customers.Count - 1, innerContext.Set<Customer>().Count());
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task QueryAsync_uses_explicit_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var context = await CreateContextAsync(testDatabase))
                {
                    using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                    {
                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();

                        using (var innerContext = await CreateContextAsync(testDatabase))
                        {
                            using (await innerContext.Database.AsRelational().Connection.BeginTransactionAsync(IsolationLevel.ReadUncommitted))
                            {
                                Assert.Equal(Customers.Count - 1, await innerContext.Set<Customer>().CountAsync());
                            }

                            if (SnapshotSupported)
                            {
                                using (await innerContext.Database.AsRelational().Connection.BeginTransactionAsync(IsolationLevel.Snapshot))
                                {
                                    Assert.Equal(Customers, await innerContext.Set<Customer>().OrderBy(c => c.Id).ToListAsync());
                                }
                            }
                        }

                        using (var innerContext = await CreateContextAsync(context.Database.AsRelational().Connection.DbConnection))
                        {
                            innerContext.Database.AsRelational().Connection.UseTransaction(transaction.DbTransaction);
                            Assert.Equal(Customers.Count - 1, await innerContext.Set<Customer>().CountAsync());
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task Can_use_open_connection_with_started_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var transaction = testDatabase.Connection.BeginTransaction())
                {
                    using (var context = await CreateContextAsync(testDatabase.Connection))
                    {
                        context.Database.AsRelational().Connection.UseTransaction(transaction);

                        context.ChangeTracker.Entry(context.Set<Customer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                    }
                }

                await AssertStoreInitialStateAsync(testDatabase);
            }
        }

        [Fact]
        public async Task UseTransaction_throws_if_mismatched_connection()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var transaction = testDatabase.Connection.BeginTransaction())
                {
                    using (var context = await CreateContextAsync(testDatabase))
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                            context.Database.AsRelational().Connection.UseTransaction(transaction))
                            .ValidateMessage(typeof(RelationalConnection), "FormatTransactionAssociatedWithDifferentConnection");
                    }
                }
            }
        }

        [Fact]
        public async Task UseTransaction_throws_if_another_transaction_started()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var transaction = testDatabase.Connection.BeginTransaction())
                {
                    using (var context = await CreateContextAsync(testDatabase))
                    {
                        using (context.Database.AsRelational().Connection.BeginTransaction())
                        {
                            Assert.Throws<InvalidOperationException>(() =>
                                context.Database.AsRelational().Connection.UseTransaction(transaction))
                                .ValidateMessage(typeof(RelationalConnection), "FormatTransactionAlreadyStarted");
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task UseTransaction_will_not_dispose_external_transaction()
        {
            using (var testDatabase = await CreateTestDatabaseAsync())
            {
                using (var transaction = testDatabase.Connection.BeginTransaction())
                {
                    using (var context = await CreateContextAsync(testDatabase.Connection))
                    {
                        context.Database.AsRelational().Connection.UseTransaction(transaction);

                        context.Database.AsRelational().Connection.Dispose();

                        Assert.NotNull(transaction.Connection);
                    }
                }
            }
        }

        protected virtual async Task AssertStoreInitialStateAsync(TTestStore testDatabase)
        {
            using (var context = await CreateContextAsync(testDatabase))
            {
                Assert.Equal(Customers, context.Set<Customer>().OrderBy(c => c.Id));
            }
        }

        #region Helpers

        protected Entity.Metadata.Model CreateModel()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new BasicModelBuilder(model);

            // TODO: Uncomment when complex types are supported
            //builder.ComplexType<Location>();
            modelBuilder.Entity<Customer>(ps =>
                {
                    ps.Property(c => c.Name);
                    ps.Key(c => c.Id);
                    ps.ToTable("Customers");
                });

            return model;
        }

        protected abstract bool SnapshotSupported { get; }

        protected abstract Task<TTestStore> CreateTestDatabaseAsync();

        protected abstract Task<DbContext> CreateContextAsync(TTestStore testDatabase);

        protected abstract Task<DbContext> CreateContextAsync(DbConnection connection);

        protected async Task SeedAsync(DbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            foreach (var customer in Customers)
            {
                context.Add(customer);
            }

            await context.SaveChangesAsync();
        }

        protected static readonly IReadOnlyList<Customer> Customers = new List<Customer>
            {
                new Customer
                    {
                        Id = 1,
                        Name = "Bob"
                    },
                new Customer
                    {
                        Id = 2,
                        Name = "Dave"
                    }
            };

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var otherCustomer = obj as Customer;
                if (otherCustomer == null)
                {
                    return false;
                }

                return Id == otherCustomer.Id
                       && Name == otherCustomer.Name;
            }

            public override string ToString()
            {
                return "Id = " + Id + ", Name = " + Name;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        #endregion
    }
}
