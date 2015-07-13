// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class TransactionTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : RelationalTestStore
        where TFixture : TransactionFixtureBase<TTestStore>, new()
    {
        [Fact]
        public void SaveChanges_implicitly_starts_transaction()
        {
            using (var context = CreateContext())
            {
                context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                context.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }

            AssertStoreInitialState();
        }

        [Fact]
        public async Task SaveChangesAsync_implicitly_starts_transaction()
        {
            using (var context = CreateContext())
            {
                context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                context.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void SaveChanges_uses_explicit_transaction_without_committing()
        {
            using (var context = CreateContext())
            {
                var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                firstEntry.State = EntityState.Deleted;

                using (context.Database.BeginTransaction())
                {
                    context.SaveChanges();
                }

                Assert.Equal(EntityState.Detached, firstEntry.State);
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void SaveChanges_false_uses_explicit_transaction_without_committing_or_accepting_changes()
        {
            using (var context = CreateContext())
            {
                var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                firstEntry.State = EntityState.Deleted;

                using (context.Database.BeginTransaction())
                {
                    context.SaveChanges(acceptAllChangesOnSuccess: false);
                }

                Assert.Equal(EntityState.Deleted, firstEntry.State);

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(EntityState.Detached, firstEntry.State);
            }

            AssertStoreInitialState();
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_without_committing()
        {
            using (var context = CreateContext())
            {
                var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                firstEntry.State = EntityState.Deleted;

                using (await context.Database.BeginTransactionAsync())
                {
                    await context.SaveChangesAsync();
                }

                Assert.Equal(EntityState.Detached, firstEntry.State);
            }

            AssertStoreInitialState();
        }

        [Fact]
        public async Task SaveChangesAsync_false_uses_explicit_transaction_without_committing_or_accepting_changes()
        {
            using (var context = CreateContext())
            {
                var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                firstEntry.State = EntityState.Deleted;

                using (await context.Database.BeginTransactionAsync())
                {
                    await context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                }

                Assert.Equal(EntityState.Deleted, firstEntry.State);

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(EntityState.Detached, firstEntry.State);
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void SaveChanges_uses_explicit_transaction_and_does_not_rollback_on_failure()
        {
            using (var context = CreateContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                    firstEntry.State = EntityState.Deleted;
                    var lastEntry = context.Entry(context.Set<TransactionCustomer>().Last());
                    lastEntry.State = EntityState.Added;

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                    }

                    Assert.Equal(EntityState.Deleted, firstEntry.State);
                    Assert.Equal(EntityState.Added, lastEntry.State);
                    Assert.NotNull(transaction.DbTransaction.Connection);
                }
            }
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_and_does_not_rollback_on_failure()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var firstEntry = context.Entry(context.Set<TransactionCustomer>().First());
                    firstEntry.State = EntityState.Deleted;
                    var lastEntry = context.Entry(context.Set<TransactionCustomer>().Last());
                    lastEntry.State = EntityState.Added;

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                    }

                    Assert.Equal(EntityState.Deleted, firstEntry.State);
                    Assert.Equal(EntityState.Added, lastEntry.State);
                    Assert.NotNull(transaction.DbTransaction.Connection);
                }
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_commited()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Fixture.Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_commited_from_context()
        {
            using (var context = CreateContext())
            {
                using (await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    context.Database.CommitTransaction();
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Fixture.Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_rolled_back()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Rollback();

                    AssertStoreInitialState();
                }
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_rolled_back_from_context()
        {
            using (var context = CreateContext())
            {
                using (await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    context.Database.RollbackTransaction();

                    AssertStoreInitialState();
                }
            }
        }

        [Fact]
        public virtual void Query_uses_explicit_transaction()
        {
            using (var context = CreateContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.SaveChanges();

                    using (var innerContext = CreateContext())
                    {
                        if (DirtyReadsOccur)
                        {
                            using (innerContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                            {
                                Assert.Equal(Fixture.Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                            }
                        }

                        if (SnapshotSupported)
                        {
                            using (innerContext.Database.BeginTransaction(IsolationLevel.Snapshot))
                            {
                                Assert.Equal(Fixture.Customers, innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToList());
                            }
                        }
                    }

                    using (var innerContext = CreateContext(context.Database.GetDbConnection()))
                    {
                        innerContext.Database.UseTransaction(transaction.DbTransaction);
                        Assert.Equal(Fixture.Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                    }
                }
            }
        }

        [Fact]
        public virtual async Task QueryAsync_uses_explicit_transaction()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();

                    using (var innerContext = CreateContext())
                    {
                        if (DirtyReadsOccur)
                        {
                            using (await innerContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted))
                            {
                                Assert.Equal(Fixture.Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                            }
                        }

                        if (SnapshotSupported)
                        {
                            using (await innerContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot))
                            {
                                Assert.Equal(Fixture.Customers, await innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToListAsync());
                            }
                        }
                    }

                    using (var innerContext = CreateContext(context.Database.GetDbConnection()))
                    {
                        innerContext.Database.UseTransaction(transaction.DbTransaction);
                        Assert.Equal(Fixture.Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task Can_use_open_connection_with_started_transaction()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext(TestDatabase.Connection))
                {
                    context.Database.UseTransaction(transaction);

                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void UseTransaction_throws_if_mismatched_connection()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() =>
                        context.Database.UseTransaction(transaction));
                    Assert.Equal(Strings.TransactionAssociatedWithDifferentConnection, ex.Message);
                }
            }
        }

        [Fact]
        public void UseTransaction_throws_if_another_transaction_started()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext())
                {
                    using (context.Database.BeginTransaction())
                    {
                        var ex = Assert.Throws<InvalidOperationException>(() =>
                            context.Database.UseTransaction(transaction));
                        Assert.Equal(Strings.TransactionAlreadyStarted, ex.Message);
                    }
                }
            }
        }

        [Fact]
        public void UseTransaction_will_not_dispose_external_transaction()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext(TestDatabase.Connection))
                {
                    context.Database.UseTransaction(transaction);

                    context.Database.GetService<IRelationalConnection>().Dispose();

                    Assert.NotNull(transaction.Connection);
                }
            }
        }

        protected virtual void AssertStoreInitialState()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(Fixture.Customers, context.Set<TransactionCustomer>().OrderBy(c => c.Id));
            }
        }

        #region Helpers

        protected TransactionTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestDatabase = Fixture.CreateTestStore();
        }

        protected TTestStore TestDatabase { get; set; }
        protected TFixture Fixture { get; set; }

        public void Dispose()
        {
            TestDatabase.Dispose();
        }

        protected abstract bool SnapshotSupported { get; }

        protected virtual bool DirtyReadsOccur => true;

        protected DbContext CreateContext()
        {
            return Fixture.CreateContext(TestDatabase);
        }

        protected DbContext CreateContext(DbConnection connection)
        {
            return Fixture.CreateContext(connection);
        }

        #endregion
    }
}
