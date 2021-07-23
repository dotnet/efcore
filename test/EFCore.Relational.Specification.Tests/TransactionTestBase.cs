// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using IsolationLevel = System.Data.IsolationLevel;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class TransactionTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : TransactionTestBase<TFixture>.TransactionFixtureBase, new()
    {
        protected TransactionTestBase(TFixture fixture)
        {
            Fixture = fixture;
            Fixture.Reseed();

            if (TestStore.ConnectionState == ConnectionState.Closed)
            {
                TestStore.OpenConnection();
            }

            Fixture.ListLoggerFactory.Log.Clear();
        }

        protected TFixture Fixture { get; set; }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChanges_can_be_used_with_no_transaction(bool async)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = false;

                context.Add(
                    new TransactionCustomer { Id = 77, Name = "Bobble" });

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                if (async)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }

                context.Database.AutoTransactionsEnabled = true;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int>
                    {
                        1,
                        2,
                        77
                    },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChanges_implicitly_starts_transaction(bool async)
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Database.AutoTransactionsEnabled);

                context.Add(
                    new TransactionCustomer { Id = 77, Name = "Bobble" });

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                if (async)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_enlisted_transaction(bool async, bool autoTransactionsEnabled)
        {
            using (var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10)))
            {
                using (var context = CreateContext())
                {
                    context.Database.EnlistTransaction(transaction);
                    context.Database.AutoTransactionsEnabled = autoTransactionsEnabled;

                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }

                    context.Database.AutoTransactionsEnabled = true;
                }

                if (AmbientTransactionsSupported)
                {
                    Assert.Equal(
                        RelationalResources.LogExplicitTransactionEnlisted(new TestLogger<TestRelationalLoggingDefinitions>())
                            .GenerateMessage("Serializable"),
                        Fixture.ListLoggerFactory.Log.First().Message);
                }
                else
                {
                    Assert.Equal(
                        RelationalResources.LogAmbientTransaction(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage(),
                        Fixture.ListLoggerFactory.Log.First().Message);

                    if (!autoTransactionsEnabled)
                    {
                        using var context = CreateContext();
                        context.Entry(context.Set<TransactionCustomer>().Single(c => c.Id == 77)).State = EntityState.Deleted;

                        if (async)
                        {
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            context.SaveChanges();
                        }
                    }
                }
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_enlisted_transaction_after_connection_closed(bool async, bool autoTransactionsEnabled)
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (var context = CreateContext())
            {
                using (var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10)))
                {
                    context.Database.EnlistTransaction(transaction);
                    context.Database.AutoTransactionsEnabled = autoTransactionsEnabled;

                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                    context.Database.AutoTransactionsEnabled = true;
                }

                using (var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10)))
                {
                    TestStore.CloseConnection();
                    TestStore.OpenConnection();
                    context.Database.EnlistTransaction(transaction);

                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                }
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_enlisted_transaction_connectionString(bool async, bool autoTransactionsEnabled)
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10)))
            {
                using var context = CreateContextWithConnectionString();
                context.Database.OpenConnection();
                context.Database.EnlistTransaction(transaction);
                context.Database.AutoTransactionsEnabled = autoTransactionsEnabled;

                context.Add(
                    new TransactionCustomer { Id = 77, Name = "Bobble" });

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                if (async)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }

                context.Database.CloseConnection();

                context.Database.AutoTransactionsEnabled = true;
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_ambient_transaction(bool async, bool autoTransactionsEnabled)
        {
            if (TestStore.ConnectionState == ConnectionState.Closed)
            {
                TestStore.OpenConnection();
            }

            using (TestUtilities.TestStore.CreateTransactionScope())
            {
                using (var context = CreateContext())
                {
                    context.Database.AutoTransactionsEnabled = autoTransactionsEnabled;

                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }

                    context.Database.AutoTransactionsEnabled = true;
                }

                if (AmbientTransactionsSupported)
                {
                    Assert.Equal(
                        RelationalResources.LogAmbientTransactionEnlisted(new TestLogger<TestRelationalLoggingDefinitions>())
                            .GenerateMessage("Serializable"),
                        Fixture.ListLoggerFactory.Log.Skip(2).First().Message);
                }
                else
                {
                    Assert.Equal(
                        RelationalResources.LogAmbientTransaction(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage(),
                        Fixture.ListLoggerFactory.Log.Skip(2).First().Message);

                    using var context = CreateContext();
                    context.Entry(context.Set<TransactionCustomer>().Single(c => c.Id == 77)).State = EntityState.Deleted;

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_ambient_transaction_with_connectionString(bool async, bool autoTransactionsEnabled)
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            DbConnection connection;
            using (var context = CreateContextWithConnectionString())
            {
                using (TestUtilities.TestStore.CreateTransactionScope())
                {
                    context.Database.AutoTransactionsEnabled = autoTransactionsEnabled;

                    connection = context.Database.GetDbConnection();
                    Assert.Equal(ConnectionState.Closed, connection.State);

                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }

                    Assert.Equal(ConnectionState.Closed, connection.State);

                    context.Database.AutoTransactionsEnabled = true;
                }
            }

            Assert.Equal(ConnectionState.Closed, connection.State);

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void SaveChanges_throws_for_suppressed_ambient_transactions(bool connectionString)
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (var context = connectionString ? CreateContextWithConnectionString() : CreateContext())
            {
                using (TestUtilities.TestStore.CreateTransactionScope())
                {
                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;

                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        Assert.Equal(
                            RelationalStrings.PendingAmbientTransaction,
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                }
            }

            AssertStoreInitialState();
        }

        [ConditionalFact]
        public virtual void SaveChanges_allows_nested_ambient_transactions()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            if (TestStore.ConnectionState == ConnectionState.Closed)
            {
                TestStore.OpenConnection();
            }

            using (var context = CreateContext())
            {
                using (var tr = new TransactionScope())
                {
                    context.Add(new TransactionCustomer { Id = 77, Name = "Bobbie" });
                    context.SaveChanges();
                    tr.Complete();
                    TestStore.CloseConnection();
                    using (var nestedTransaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        context.Add(new TransactionOrder { Id = 300, Name = "Order3" });
                        context.SaveChanges();
                        nestedTransaction.Complete();
                        TestStore.CloseConnection();
                    }
                }

                Assert.Equal(
                    new List<int>
                    {
                        1,
                        2,
                        77
                    },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
                Assert.Equal(
                    new List<int>
                    {
                        100,
                        200,
                        300
                    },
                    context.Set<TransactionOrder>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [ConditionalFact]
        public virtual void SaveChanges_allows_independent_ambient_transaction_commits()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            if (TestStore.ConnectionState == ConnectionState.Closed)
            {
                TestStore.OpenConnection();
            }

            using (var context = CreateContext())
            {
                using (var tr = new TransactionScope())
                {
                    context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });
                    context.SaveChanges();
                    TestStore.CloseConnection();
                    using (var nestedTransaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        context.Add(new TransactionOrder { Id = 300, Name = "Order3" });
                        context.SaveChanges();
                        nestedTransaction.Complete();
                        TestStore.CloseConnection();
                    }
                }

                Assert.Equal(
                    new List<int> { 1, 2 },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
                Assert.Equal(
                    new List<int>
                    {
                        100,
                        200,
                        300
                    },
                    context.Set<TransactionOrder>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [ConditionalFact]
        public virtual void SaveChanges_uses_enlisted_transaction_after_ambient_transaction()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            if (TestStore.ConnectionState == ConnectionState.Closed)
            {
                TestStore.OpenConnection();
            }

            using (var context = CreateContext())
            {
                using (TestUtilities.TestStore.CreateTransactionScope())
                {
                    context.Add(
                        new TransactionCustomer { Id = 77, Name = "Bobble" });

                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last()).State = EntityState.Added;
                }

                using var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
                context.Database.EnlistTransaction(transaction);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChanges_does_not_close_connection_opened_by_user(bool async)
        {
            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();
                context.Database.OpenConnection();

                Assert.Equal(ConnectionState.Open, connection.State);

                context.Add(
                    new TransactionCustomer { Id = 77, Name = "Bobble" });

                if (async)
                {
                    await context.SaveChangesAsync();
                    Assert.Equal(ConnectionState.Open, connection.State);
                    await context.Database.CloseConnectionAsync();
                }
                else
                {
                    context.SaveChanges();
                    Assert.Equal(ConnectionState.Open, connection.State);
                    context.Database.CloseConnection();
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    new List<int>
                    {
                        1,
                        2,
                        77
                    },
                    context.Set<TransactionCustomer>().OrderBy(c => c.Id).Select(e => e.Id).ToList());
            }
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_explicit_transaction_without_committing(bool async, bool autoTransaction)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                var firstEntry = context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First());
                firstEntry.State = EntityState.Deleted;

                if (async)
                {
                    using (await context.Database.BeginTransactionAsync())
                    {
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    using (context.Database.BeginTransaction())
                    {
                        context.SaveChanges();
                    }
                }

                Assert.Equal(EntityState.Detached, firstEntry.State);

                context.Database.AutoTransactionsEnabled = true;
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_false_uses_explicit_transaction_without_committing_or_accepting_changes(
            bool async,
            bool autoTransaction)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                var firstEntry = context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First());
                firstEntry.State = EntityState.Deleted;

                if (async)
                {
                    using (await context.Database.BeginTransactionAsync())
                    {
                        await context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                    }
                }
                else
                {
                    using (context.Database.BeginTransaction())
                    {
                        context.SaveChanges(acceptAllChangesOnSuccess: false);
                    }
                }

                Assert.Equal(EntityState.Deleted, firstEntry.State);

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(EntityState.Detached, firstEntry.State);

                context.Database.AutoTransactionsEnabled = true;
            }

            AssertStoreInitialState();
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public virtual async Task SaveChanges_uses_explicit_transaction_with_failure_behavior(bool async, bool autoTransaction)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using var transaction = context.Database.BeginTransaction();

                var firstEntry = context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First());
                var lastEntry = context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).Last());

                if (async)
                {
                    firstEntry.State = EntityState.Deleted;
                    lastEntry.State = EntityState.Added;
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

                    lastEntry.State = EntityState.Unchanged;
                    firstEntry.Entity.Name = "John";
                    firstEntry.State = EntityState.Modified;
                    if (SavepointsSupported)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
                    }
                }
                else
                {
                    firstEntry.State = EntityState.Deleted;
                    lastEntry.State = EntityState.Added;
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                    lastEntry.State = EntityState.Unchanged;
                    firstEntry.Entity.Name = "John";
                    firstEntry.State = EntityState.Modified;
                    if (SavepointsSupported)
                    {
                        context.SaveChanges();
                    }
                    else
                    {
                        Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                    }
                }

                Assert.NotNull(transaction.GetDbTransaction().Connection);

                transaction.Commit();

                context.Database.AutoTransactionsEnabled = true;
            }

            if (SavepointsSupported)
            {
                using var context = CreateContext();
                Assert.Equal(Customers.Count, context.Set<TransactionCustomer>().Count());
                Assert.Equal("John", context.Set<TransactionCustomer>().OrderBy(c => c.Id).First().Name);
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_committed(bool autoTransaction)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }

                context.Database.AutoTransactionsEnabled = true;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_committed_from_context(bool autoTransaction)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoTransactionsEnabled = autoTransaction;

                using (await context.Database.BeginTransactionAsync())
                {
                    context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    context.Database.CommitTransaction();
                }

                context.Database.AutoTransactionsEnabled = true;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_rolled_back(bool autoTransaction)
        {
            using var context = CreateContext();
            context.Database.AutoTransactionsEnabled = autoTransaction;

            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();
                await transaction.RollbackAsync();

                AssertStoreInitialState();
            }

            context.Database.AutoTransactionsEnabled = true;
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task RelationalTransaction_can_be_rolled_back_from_context(bool autoTransaction)
        {
            using var context = CreateContext();
            context.Database.AutoTransactionsEnabled = autoTransaction;

            using (await context.Database.BeginTransactionAsync())
            {
                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();
                await context.Database.RollbackTransactionAsync();

                AssertStoreInitialState();
            }

            context.Database.AutoTransactionsEnabled = true;
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_uses_explicit_transaction(bool autoTransaction)
        {
            using var context = CreateContext();
            context.Database.AutoTransactionsEnabled = autoTransaction;

            using (var transaction = context.Database.BeginTransaction())
            {
                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                context.SaveChanges();

                using (var innerContext = CreateContextWithConnectionString())
                {
                    innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                    if (DirtyReadsOccur)
                    {
                        using (innerContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            Assert.Equal(Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                        }
                    }

                    if (SnapshotSupported)
                    {
                        using (innerContext.Database.BeginTransaction(IsolationLevel.Snapshot))
                        {
                            Assert.Equal(Customers, innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToList());
                        }
                    }

                    innerContext.Database.AutoTransactionsEnabled = true;
                }

                using (var innerContext = CreateContext())
                {
                    innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                    innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                    Assert.Equal(Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());

                    innerContext.Database.AutoTransactionsEnabled = true;
                }
            }

            context.Database.AutoTransactionsEnabled = true;
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task QueryAsync_uses_explicit_transaction(bool autoTransaction)
        {
            using var context = CreateContext();
            context.Database.AutoTransactionsEnabled = autoTransaction;

            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                using (var innerContext = CreateContextWithConnectionString())
                {
                    innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                    if (DirtyReadsOccur)
                    {
                        using (await innerContext.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted))
                        {
                            Assert.Equal(Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                        }
                    }

                    if (SnapshotSupported)
                    {
                        using (await innerContext.Database.BeginTransactionAsync(IsolationLevel.Snapshot))
                        {
                            Assert.Equal(Customers, await innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToListAsync());
                        }
                    }

                    innerContext.Database.AutoTransactionsEnabled = true;
                }

                using (var innerContext = CreateContext())
                {
                    innerContext.Database.AutoTransactionsEnabled = autoTransaction;

                    innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                    Assert.Equal(Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());

                    innerContext.Database.AutoTransactionsEnabled = true;
                }
            }

            context.Database.AutoTransactionsEnabled = true;
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Can_use_open_connection_with_started_transaction(bool autoTransaction)
        {
            using (var transaction = TestStore.BeginTransaction())
            {
                using var context = CreateContext();
                context.Database.AutoTransactionsEnabled = autoTransaction;

                context.Database.UseTransaction(transaction);

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                context.Database.AutoTransactionsEnabled = true;
            }

            AssertStoreInitialState();
        }

        [ConditionalFact]
        public virtual void UseTransaction_throws_if_mismatched_connection()
        {
            using var transaction = TestStore.BeginTransaction();
            using var context = CreateContextWithConnectionString();
            var ex = Assert.Throws<InvalidOperationException>(
                () =>
                    context.Database.UseTransaction(transaction));
            Assert.Equal(RelationalStrings.TransactionAssociatedWithDifferentConnection, ex.Message);
        }

        [ConditionalFact]
        public virtual void UseTransaction_throws_if_another_transaction_started()
        {
            using var transaction = TestStore.BeginTransaction();
            using var context = CreateContextWithConnectionString();
            using (context.Database.BeginTransaction(
                DirtyReadsOccur
                    ? IsolationLevel.ReadUncommitted
                    : IsolationLevel.Unspecified))
            {
                var ex = Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Database.UseTransaction(transaction));
                Assert.Equal(RelationalStrings.TransactionAlreadyStarted, ex.Message);
            }
        }

        [ConditionalFact]
        public virtual void UseTransaction_will_not_dispose_external_transaction()
        {
            using var transaction = TestStore.BeginTransaction();
            using var context = CreateContext();
            context.Database.UseTransaction(transaction);

            context.Database.GetService<IRelationalConnection>().Dispose();

            Assert.NotNull(transaction.Connection);
        }

        [ConditionalFact]
        public virtual void UseTransaction_throws_if_ambient_transaction_started()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (TestUtilities.TestStore.CreateTransactionScope())
            {
                using var transaction = TestStore.BeginTransaction();
                using var context = CreateContextWithConnectionString();
                var ex = Assert.Throws<InvalidOperationException>(
                    () => context.Database.UseTransaction(transaction));
                Assert.Equal(RelationalStrings.ConflictingAmbientTransaction, ex.Message);
            }
        }

        [ConditionalFact]
        public virtual void UseTransaction_throws_if_enlisted_in_transaction()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var t = new CommittableTransaction(TimeSpan.FromMinutes(10));
            using var transaction = TestStore.BeginTransaction();
            using var context = CreateContextWithConnectionString();
            context.Database.OpenConnection();

            context.Database.EnlistTransaction(t);

            var ex = Assert.Throws<InvalidOperationException>(
                () => context.Database.UseTransaction(transaction));
            Assert.Equal(RelationalStrings.ConflictingEnlistedTransaction, ex.Message);
            context.Database.CloseConnection();
        }

        [ConditionalFact]
        public virtual void BeginTransaction_throws_if_another_transaction_started()
        {
            using var context = CreateContextWithConnectionString();
            using (context.Database.BeginTransaction())
            {
                var ex = Assert.Throws<InvalidOperationException>(
                    () => context.Database.BeginTransaction());
                Assert.Equal(RelationalStrings.TransactionAlreadyStarted, ex.Message);
            }
        }

        [ConditionalFact]
        public virtual void BeginTransaction_throws_if_ambient_transaction_started()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (TestUtilities.TestStore.CreateTransactionScope())
            {
                using var context = CreateContextWithConnectionString();
                var ex = Assert.Throws<InvalidOperationException>(
                    () => context.Database.BeginTransaction());
                Assert.Equal(RelationalStrings.ConflictingAmbientTransaction, ex.Message);
            }
        }

        [ConditionalFact]
        public virtual void BeginTransaction_throws_if_enlisted_in_transaction()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
            using var context = CreateContextWithConnectionString();
            context.Database.OpenConnection();

            context.Database.EnlistTransaction(transaction);

            var ex = Assert.Throws<InvalidOperationException>(
                () => context.Database.BeginTransaction(
                    DirtyReadsOccur
                        ? IsolationLevel.ReadUncommitted
                        : IsolationLevel.Unspecified));
            Assert.Equal(RelationalStrings.ConflictingEnlistedTransaction, ex.Message);
            context.Database.CloseConnection();
        }

        [ConditionalFact]
        public virtual void BeginTransaction_can_be_used_after_ambient_transaction_ended()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var context = CreateContextWithConnectionString();
            using (TestUtilities.TestStore.CreateTransactionScope())
            {
                context.Database.OpenConnection();
            }

            using (context.Database.BeginTransaction())
            {
            }

            context.Database.CloseConnection();
        }

        [ConditionalFact]
        public virtual void BeginTransaction_can_be_used_after_enlisted_transaction_ended()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var context = CreateContextWithConnectionString();
            using (var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10)))
            {
                context.Database.OpenConnection();

                context.Database.EnlistTransaction(transaction);
            }

            using (context.Database.BeginTransaction())
            {
            }

            context.Database.CloseConnection();
        }

        [ConditionalFact]
        public virtual void BeginTransaction_can_be_used_after_another_transaction_if_connection_closed()
        {
            using var context = CreateContextWithConnectionString();
            using (context.Database.BeginTransaction())
            {
                context.Database.CloseConnection();
                using (context.Database.BeginTransaction())
                {
                }
            }
        }

        [ConditionalFact]
        public virtual void BeginTransaction_can_be_used_after_enlisted_transaction_if_connection_closed()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var context = CreateContextWithConnectionString();
            using var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
            context.Database.OpenConnection();

            context.Database.EnlistTransaction(transaction);

            context.Database.CloseConnection();

            using (context.Database.BeginTransaction())
            {
            }
        }

        [ConditionalFact]
        public virtual void EnlistTransaction_throws_if_another_transaction_started()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
            using var context = CreateContextWithConnectionString();
            using (context.Database.BeginTransaction())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Database.EnlistTransaction(transaction));
            }
        }

        [ConditionalFact]
        public virtual void EnlistTransaction_throws_if_ambient_transaction_started()
        {
            if (!AmbientTransactionsSupported)
            {
                return;
            }

            using (TestUtilities.TestStore.CreateTransactionScope())
            {
                using var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
                using var context = CreateContextWithConnectionString();
                context.Database.OpenConnection();

                Assert.Throws<InvalidOperationException>(
                    () => context.Database.EnlistTransaction(transaction));

                context.Database.CloseConnection();
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Externally_closed_connections_are_handled_correctly(bool async)
        {
            DbConnection connection;
            using (var context = CreateContextWithConnectionString())
            {
                var set = context.Set<TransactionCustomer>();

                if (async)
                {
                    await context.Database.OpenConnectionAsync();
                }
                else
                {
                    context.Database.OpenConnection();
                }

                connection = context.Database.GetDbConnection();

                connection.Close();

                var _ = async ? await set.ToListAsync() : set.ToList();

                Assert.Equal(ConnectionState.Open, connection.State);

                context.Database.CloseConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);

                _ = async ? await set.ToListAsync() : set.ToList();

                Assert.Equal(ConnectionState.Closed, connection.State);
            }

            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChanges_implicitly_creates_savepoint(bool async)
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Database.AutoSavepointsEnabled);

                using var transaction = async
                    ? await context.Database.BeginTransactionAsync()
                    : context.Database.BeginTransaction();

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                context.Add(new TransactionCustomer { Id = 78, Name = "Hobble" });
                context.Add(new TransactionCustomer { Id = 1, Name = "Gobble" }); // Cause SaveChanges failure

                if (async)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    await transaction.CommitAsync();
                }
                else
                {
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    transaction.Commit();
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(77, context.Set<TransactionCustomer>().Max(c => c.Id));
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task SaveChanges_can_be_used_with_no_savepoint(bool async)
        {
            using (var context = CreateContext())
            {
                context.Database.AutoSavepointsEnabled = false;

                using var transaction = async
                    ? await context.Database.BeginTransactionAsync()
                    : context.Database.BeginTransaction();

                context.Add(new TransactionCustomer { Id = 77, Name = "Bobble" });

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                context.Add(new TransactionCustomer { Id = 78, Name = "Hobble" });
                context.Add(new TransactionCustomer { Id = 1, Name = "Gobble" }); // Cause SaveChanges failure

                if (async)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                    await transaction.CommitAsync();
                }
                else
                {
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    transaction.Commit();
                }

                context.Database.AutoSavepointsEnabled = true;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(78, context.Set<TransactionCustomer>().Max(c => c.Id));
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Savepoint_can_be_rolled_back(bool async)
        {
            using (var context = CreateContext())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.CreateSavepointAsync("FooSavepoint");
                }
                else
                {
                    transaction.CreateSavepoint("FooSavepoint");
                }

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.RollbackToSavepointAsync("FooSavepoint");
                }
                else
                {
                    transaction.RollbackToSavepoint("FooSavepoint");
                }

                await transaction.CommitAsync();
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Savepoint_can_be_released(bool async)
        {
            using (var context = CreateContext())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.CreateSavepointAsync("FooSavepoint");
                }
                else
                {
                    transaction.CreateSavepoint("FooSavepoint");
                }

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.ReleaseSavepointAsync("FooSavepoint");
                    await Assert.ThrowsAnyAsync<DbException>(
                        async () => await transaction.ReleaseSavepointAsync("FooSavepoint"));
                }
                else
                {
                    transaction.ReleaseSavepoint("FooSavepoint");
                    Assert.ThrowsAny<DbException>(
                        () => transaction.ReleaseSavepoint("FooSavepoint"));
                }

                await transaction.CommitAsync();
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Customers.Count - 2, context.Set<TransactionCustomer>().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Savepoint_name_is_quoted(bool async)
        {
            using (var context = CreateContext())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.CreateSavepointAsync("Name with spaces");
                }
                else
                {
                    transaction.CreateSavepoint("Name with spaces");
                }

                context.Entry(context.Set<TransactionCustomer>().OrderBy(c => c.Id).First()).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                if (async)
                {
                    await transaction.RollbackToSavepointAsync("Name with spaces");
                }
                else
                {
                    transaction.RollbackToSavepoint("Name with spaces");
                }

                await transaction.CommitAsync();
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        protected virtual void AssertStoreInitialState()
        {
            using var context = CreateContext();
            Assert.Equal(Customers, context.Set<TransactionCustomer>().OrderBy(c => c.Id));
        }

        protected RelationalTestStore TestStore
            => (RelationalTestStore)Fixture.TestStore;

        protected abstract bool SnapshotSupported { get; }

        protected virtual bool AmbientTransactionsSupported
            => false;

        protected virtual bool DirtyReadsOccur
            => true;

        protected virtual bool SavepointsSupported
            => true;

        protected DbContext CreateContext()
            => Fixture.CreateContext();

        protected abstract DbContext CreateContextWithConnectionString();

        public abstract class TransactionFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "TransactionTest";

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Database.Transaction.Name;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<TransactionCustomer>(
                    ps =>
                    {
                        ps.Property(c => c.Id).ValueGeneratedNever();
                        ps.ToTable("Customers");
                    });
                modelBuilder.Entity<TransactionOrder>(
                    ps =>
                    {
                        ps.Property(c => c.Id).ValueGeneratedNever();
                        ps.ToTable("Orders");
                    });
            }

            protected override void Seed(PoolableDbContext context)
            {
                context.AddRange(Customers);
                context.AddRange(Orders);

                context.SaveChanges();
            }
        }

        protected static readonly IReadOnlyList<TransactionCustomer> Customers = new List<TransactionCustomer>
        {
            new() { Id = 1, Name = "Bob" }, new() { Id = 2, Name = "Dave" }
        };

        protected static readonly IReadOnlyList<TransactionOrder> Orders = new List<TransactionOrder>
        {
            new() { Id = 100, Name = "Order1" }, new() { Id = 200, Name = "Order2" }
        };

        protected abstract class TransactionEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                return !(obj is TransactionCustomer otherCustomer)
                    ? false
                    : Id == otherCustomer.Id
                    && Name == otherCustomer.Name;
            }

            public override string ToString()
                => "Id = " + Id + ", Name = " + Name;

            public override int GetHashCode()
                => HashCode.Combine(Id, Name);
        }

        protected class TransactionCustomer : TransactionEntity
        {
        }

        protected class TransactionOrder : TransactionEntity
        {
        }
    }
}
