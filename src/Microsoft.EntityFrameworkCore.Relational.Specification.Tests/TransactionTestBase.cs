// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class TransactionTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : RelationalTestStore
        where TFixture : TransactionFixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void SaveChanges_can_be_used_with_no_transaction()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = false;

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                context.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int> { 1, 2, 77 },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [Fact]
        public virtual async Task SaveChangesAsync_can_be_used_with_no_transaction()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = false;

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                context.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int> { 1, 2, 77 },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [Fact]
        public virtual void SaveChanges_implicitly_starts_transaction()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                context.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }

            AssertStoreInitialState();
        }

        [Fact]
        public virtual async Task SaveChangesAsync_implicitly_starts_transaction()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
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
        public virtual void SaveChanges_does_not_close_connection_opened_by_user()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();
                context.Database.OpenConnection();

                Assert.Equal(ConnectionState.Open, connection.State);

                context.Database.AutoTransactionsEnabled = true;

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                context.SaveChanges();

                Assert.Equal(ConnectionState.Open, connection.State);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int> { 1, 2, 77 },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [Fact]
        public virtual async Task SaveChangesAsync_does_not_close_connection_opened_by_user()
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();
                context.Database.OpenConnection();

                Assert.Equal(ConnectionState.Open, connection.State);

                context.Database.AutoTransactionsEnabled = true;

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                await context.SaveChangesAsync();

                Assert.Equal(ConnectionState.Open, connection.State);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int> { 1, 2, 77 },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void SaveChanges_uses_explicit_transaction_without_committing(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void SaveChanges_false_uses_explicit_transaction_without_committing_or_accepting_changes(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChangesAsync_uses_explicit_transaction_without_committing(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChangesAsync_false_uses_explicit_transaction_without_committing_or_accepting_changes(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void SaveChanges_uses_explicit_transaction_and_does_not_rollback_on_failure(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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
                    Assert.NotNull(transaction.GetDbTransaction().Connection);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChangesAsync_uses_explicit_transaction_and_does_not_rollback_on_failure(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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
                    Assert.NotNull(transaction.GetDbTransaction().Connection);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_commited(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_commited_from_context(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_rolled_back(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Rollback();

                    AssertStoreInitialState();
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_rolled_back_from_context(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    context.Database.RollbackTransaction();

                    AssertStoreInitialState();
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_uses_explicit_transaction(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (var transaction = context.Database.BeginTransaction())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.SaveChanges();

                    using (var innerContext = CreateContext())
                    {
                        innerContext.Database.AutoTransactionsEnabled = autoTransaction;

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
                        innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                        innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                        Assert.Equal(Fixture.Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                    }
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task QueryAsync_uses_explicit_transaction(bool autoTransaction)
        {
            EnsureInitialState();

            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();

                    using (var innerContext = CreateContext())
                    {
                        innerContext.Database.AutoTransactionsEnabled = autoTransaction;

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
                        innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                        innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                        Assert.Equal(Fixture.Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                    }
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Can_use_open_connection_with_started_transaction(bool autoTransaction)
        {
            EnsureInitialState();

            try
            {
                TestDatabase.OpenConnection();

                using (var transaction = TestDatabase.Connection.BeginTransaction())
                {
                    using (var context = CreateContext(TestDatabase.Connection))
                    {
                        context.Database.AutoTransactionsEnabled = autoTransaction;

                        context.Database.UseTransaction(transaction);

                        context.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                    }
                }
            }
            finally
            {
                TestDatabase.Connection.Close();
            }

            AssertStoreInitialState();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void UseTransaction_throws_if_mismatched_connection(bool autoTransaction)
        {
            EnsureInitialState();

            try
            {
                TestDatabase.OpenConnection();

                using (var transaction = TestDatabase.Connection.BeginTransaction())
                {
                    using (var context = CreateContext())
                    {
                        context.Database.AutoTransactionsEnabled = autoTransaction;

                        var ex = Assert.Throws<InvalidOperationException>(() =>
                                context.Database.UseTransaction(transaction));
                        Assert.Equal(RelationalStrings.TransactionAssociatedWithDifferentConnection, ex.Message);
                    }
                }
            }
            finally
            {
                TestDatabase.Connection.Close();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void UseTransaction_throws_if_another_transaction_started(bool autoTransaction)
        {
            EnsureInitialState();

            try
            {
                TestDatabase.OpenConnection();

                using (var transaction = TestDatabase.Connection.BeginTransaction())
                {
                    using (var context = CreateContext())
                    {
                        context.Database.AutoTransactionsEnabled = autoTransaction;

                        using (context.Database.BeginTransaction())
                        {
                            var ex = Assert.Throws<InvalidOperationException>(() =>
                                    context.Database.UseTransaction(transaction));
                            Assert.Equal(RelationalStrings.TransactionAlreadyStarted, ex.Message);
                        }
                    }
                }
            }
            finally
            {
                TestDatabase.Connection.Close();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void UseTransaction_will_not_dispose_external_transaction(bool autoTransaction)
        {
            EnsureInitialState();

            try
            {
                TestDatabase.OpenConnection();

                using (var transaction = TestDatabase.Connection.BeginTransaction())
                {
                    using (var context = CreateContext(TestDatabase.Connection))
                    {
                        context.Database.AutoTransactionsEnabled = autoTransaction;

                        context.Database.UseTransaction(transaction);

                        context.Database.GetService<IRelationalConnection>().Dispose();

                        Assert.NotNull(transaction.Connection);
                    }
                }
            }
            finally
            {
                TestDatabase.Connection.Close();
            }
        }

        protected virtual void EnsureInitialState()
        {
            using (var context = CreateContext())
            {
                context.RemoveRange(context.Set<TransactionCustomer>().ToList());
                context.SaveChanges();

                context.AddRange(Fixture.Customers);
                context.SaveChanges();
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

        public virtual void Dispose() => TestDatabase.Dispose();

        protected abstract bool SnapshotSupported { get; }

        protected virtual bool DirtyReadsOccur => true;

        protected DbContext CreateContext() => Fixture.CreateContext(TestDatabase);

        protected DbContext CreateContext(DbConnection connection) => Fixture.CreateContext(connection);

        #endregion
    }
}
