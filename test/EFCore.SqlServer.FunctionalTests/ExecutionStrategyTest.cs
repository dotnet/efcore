// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ExecutionStrategyTest : IClassFixture<ExecutionStrategyTest.ExecutionStrategyFixture>
    {
        public ExecutionStrategyTest(ExecutionStrategyFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestStore.CloseConnection();
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected ExecutionStrategyFixture Fixture { get; }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
        public void Handles_commit_failure(bool realFailure)
        {
            // Use all overloads of ExecuteInTransaction
            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => { db.SaveChanges(false); },
                    () => db.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(false),
                    () => db.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => { c.SaveChanges(false); },
                    c => c.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(false),
                    c => c.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => { db.SaveChanges(false); },
                    () => db.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(false),
                    () => db.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => { c.SaveChanges(false); },
                    c => c.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(false),
                    c => c.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));
        }

        private void Test_commit_failure(bool realFailure, Action<TestSqlServerRetryingExecutionStrategy, ExecutionStrategyContext> execute)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.CommitFailures.Enqueue(new bool?[] { realFailure });
                Fixture.TestSqlLoggerFactory.Clear();

                context.Products.Add(new Product());
                execute(new TestSqlServerRetryingExecutionStrategy(context), context);
                context.ChangeTracker.AcceptAllChanges();

                var retryMessage =
                    "A transient exception occurred during execution. The operation will be retried after 0ms."
                    + Environment.NewLine
                    + "Microsoft.Data.SqlClient.SqlException (0x80131904): Bang!";
                if (realFailure)
                {
                    var logEntry = Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == CoreEventId.ExecutionStrategyRetrying);
                    Assert.Contains(retryMessage, logEntry.Message);
                    Assert.Equal(LogLevel.Information, logEntry.Level);
                }
                else
                {
                    Assert.Empty(Fixture.TestSqlLoggerFactory.Log.Where(l => l.Id == CoreEventId.ExecutionStrategyRetrying));
                }

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, context.Products.Count());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
        public async Task Handles_commit_failure_async(bool realFailure)
        {
            // Use all overloads of ExecuteInTransactionAsync
            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    () => db.SaveChangesAsync(false),
                    () => db.Products.AsNoTracking().AnyAsync()));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    async ct => { await db.SaveChangesAsync(false); },
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    ct => db.SaveChangesAsync(false, ct),
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    async (c, ct) => { await c.SaveChangesAsync(false, ct); },
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    (c, ct) => c.SaveChangesAsync(false, ct),
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    () => db.SaveChangesAsync(false),
                    () => db.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    async ct => { await db.SaveChangesAsync(false, ct); },
                    ct => db.Products.AsNoTracking().AnyAsync(ct),
                    IsolationLevel.Serializable,
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    ct => db.SaveChangesAsync(false, ct),
                    ct => db.Products.AsNoTracking().AnyAsync(ct),
                    IsolationLevel.Serializable,
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    async (c, ct) => { await c.SaveChangesAsync(false, ct); },
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(ct),
                    IsolationLevel.Serializable,
                    CancellationToken.None));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    (c, ct) => c.SaveChangesAsync(false, ct),
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(ct),
                    IsolationLevel.Serializable,
                    CancellationToken.None));
        }

        private async Task Test_commit_failure_async(
            bool realFailure,
            Func<TestSqlServerRetryingExecutionStrategy, ExecutionStrategyContext, Task> execute)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.CommitFailures.Enqueue(new bool?[] { realFailure });
                Fixture.TestSqlLoggerFactory.Clear();

                context.Products.Add(new Product());
                await execute(new TestSqlServerRetryingExecutionStrategy(context), context);
                context.ChangeTracker.AcceptAllChanges();

                var retryMessage =
                    "A transient exception occurred during execution. The operation will be retried after 0ms."
                    + Environment.NewLine
                    + "Microsoft.Data.SqlClient.SqlException (0x80131904): Bang!";
                if (realFailure)
                {
                    var logEntry = Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == CoreEventId.ExecutionStrategyRetrying);
                    Assert.Contains(retryMessage, logEntry.Message);
                    Assert.Equal(LogLevel.Information, logEntry.Level);
                }
                else
                {
                    Assert.Empty(Fixture.TestSqlLoggerFactory.Log.Where(l => l.Id == CoreEventId.ExecutionStrategyRetrying));
                }

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, await context.Products.CountAsync());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 1, MemberType = typeof(DataGenerator))]
        public void Handles_commit_failure_multiple_SaveChanges(bool realFailure)
        {
            CleanContext();

            using var context1 = CreateContext();
            var connection = (TestSqlServerConnection)context1.GetService<ISqlServerConnection>();

            using (var context2 = CreateContext())
            {
                connection.CommitFailures.Enqueue(new bool?[] { realFailure });

                context1.Products.Add(new Product());
                context2.Products.Add(new Product());

                new TestSqlServerRetryingExecutionStrategy(context1).ExecuteInTransaction(
                    context1,
                    c1 =>
                    {
                        context2.Database.UseTransaction(null);
                        context2.Database.UseTransaction(context1.Database.CurrentTransaction.GetDbTransaction());

                        c1.SaveChanges(false);

                        return context2.SaveChanges(false);
                    },
                    c => c.Products.AsNoTracking().Any());

                context1.ChangeTracker.AcceptAllChanges();
                context2.ChangeTracker.AcceptAllChanges();
            }

            using var context = CreateContext();
            Assert.Equal(2, context.Products.Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 4, MemberType = typeof(DataGenerator))]
        public async Task Retries_SaveChanges_on_execution_failure(
            bool realFailure,
            bool externalStrategy,
            bool openConnection,
            bool async)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.ExecutionFailures.Enqueue(new bool?[] { null, realFailure });

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (openConnection)
                {
                    if (async)
                    {
                        await context.Database.OpenConnectionAsync();
                    }
                    else
                    {
                        context.Database.OpenConnection();
                    }

                    Assert.Equal(ConnectionState.Open, context.Database.GetDbConnection().State);
                }

                context.Products.Add(new Product());
                context.Products.Add(new Product());

                if (async)
                {
                    if (externalStrategy)
                    {
                        await new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransactionAsync(
                            context,
                            (c, ct) => c.SaveChangesAsync(false, ct),
                            (c, _) =>
                            {
                                Assert.True(false);
                                return Task.FromResult(false);
                            });

                        context.ChangeTracker.AcceptAllChanges();
                    }
                    else
                    {
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (externalStrategy)
                    {
                        new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                            context,
                            c => c.SaveChanges(false),
                            c =>
                            {
                                Assert.True(false);
                                return false;
                            });

                        context.ChangeTracker.AcceptAllChanges();
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }

                Assert.Equal(2, connection.OpenCount);
                Assert.Equal(4, connection.ExecutionCount);

                Assert.Equal(
                    openConnection
                        ? ConnectionState.Open
                        : ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (openConnection)
                {
                    if (async)
                    {
                        await context.Database.CloseConnectionAsync();
                    }
                    else
                    {
                        context.Database.CloseConnection();
                    }
                }

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.Products.Count());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
        public async Task Retries_query_on_execution_failure(bool externalStrategy, bool async)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                context.Products.Add(new Product());
                context.Products.Add(new Product());

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.ExecutionFailures.Enqueue(new bool?[] { true });

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                List<Product> list;
                if (async)
                {
                    if (externalStrategy)
                    {
                        list = await new TestSqlServerRetryingExecutionStrategy(context)
                            .ExecuteAsync(context, (c, ct) => c.Products.ToListAsync(ct), null);
                    }
                    else
                    {
                        list = await context.Products.ToListAsync();
                    }
                }
                else
                {
                    if (externalStrategy)
                    {
                        list = new TestSqlServerRetryingExecutionStrategy(context)
                            .Execute(context, c => c.Products.ToList(), null);
                    }
                    else
                    {
                        list = context.Products.ToList();
                    }
                }

                Assert.Equal(2, list.Count);
                Assert.Equal(1, connection.OpenCount);
                Assert.Equal(2, connection.ExecutionCount);

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
        public async Task Retries_FromSqlRaw_on_execution_failure(bool externalStrategy, bool async)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                context.Products.Add(new Product());
                context.Products.Add(new Product());

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.ExecutionFailures.Enqueue(new bool?[] { true });

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                List<Product> list;
                if (async)
                {
                    if (externalStrategy)
                    {
                        list = await new TestSqlServerRetryingExecutionStrategy(context)
                            .ExecuteAsync(
                                context, (c, ct) => c.Set<Product>().FromSqlRaw(
                                    @"SELECT [ID], [name]
                              FROM [Products]").ToListAsync(ct), null);
                    }
                    else
                    {
                        list = await context.Set<Product>().FromSqlRaw(
                            @"SELECT [ID], [name]
                              FROM [Products]").ToListAsync();
                    }
                }
                else
                {
                    if (externalStrategy)
                    {
                        list = new TestSqlServerRetryingExecutionStrategy(context)
                            .Execute(
                                context, c => c.Set<Product>().FromSqlRaw(
                                    @"SELECT [ID], [name]
                              FROM [Products]").ToList(), null);
                    }
                    else
                    {
                        list = context.Set<Product>().FromSqlRaw(
                            @"SELECT [ID], [name]
                              FROM [Products]").ToList();
                    }
                }

                Assert.Equal(2, list.Count);
                Assert.Equal(1, connection.OpenCount);
                Assert.Equal(2, connection.ExecutionCount);

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(DataGenerator.GetBoolCombinations), 2, MemberType = typeof(DataGenerator))]
        public async Task Retries_OpenConnection_on_execution_failure(bool externalStrategy, bool async)
        {
            using var context = CreateContext();
            var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

            connection.OpenFailures.Enqueue(new bool?[] { true });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            if (async)
            {
                if (externalStrategy)
                {
                    await new TestSqlServerRetryingExecutionStrategy(context).ExecuteAsync(
                        context,
                        c => c.Database.OpenConnectionAsync());
                }
                else
                {
                    await context.Database.OpenConnectionAsync();
                }
            }
            else
            {
                if (externalStrategy)
                {
                    new TestSqlServerRetryingExecutionStrategy(context).Execute(
                        context,
                        c => c.Database.OpenConnection());
                }
                else
                {
                    context.Database.OpenConnection();
                }
            }

            Assert.Equal(2, connection.OpenCount);

            if (async)
            {
                context.Database.CloseConnection();
            }
            else
            {
                await context.Database.CloseConnectionAsync();
            }

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Retries_BeginTransaction_on_execution_failure(bool async)
        {
            using var context = CreateContext();
            var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

            connection.OpenFailures.Enqueue(new bool?[] { true });

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

            if (async)
            {
                var transaction = await new TestSqlServerRetryingExecutionStrategy(context).ExecuteAsync(
                    context,
                    c => context.Database.BeginTransactionAsync());

                transaction.Dispose();
            }
            else
            {
                var transaction = new TestSqlServerRetryingExecutionStrategy(context).Execute(
                    context,
                    c => context.Database.BeginTransaction());

                transaction.Dispose();
            }

            Assert.Equal(2, connection.OpenCount);

            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        [ConditionalFact]
        public void Verification_is_retried_using_same_retry_limit()
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.ExecutionFailures.Enqueue(new bool?[] { true, null, true, true });
                connection.CommitFailures.Enqueue(new bool?[] { true, true, true, true });

                context.Products.Add(new Product());
                Assert.Throws<RetryLimitExceededException>(
                    () =>
                        new TestSqlServerRetryingExecutionStrategy(context, TimeSpan.FromMilliseconds(100))
                            .ExecuteInTransaction(
                                context,
                                c => c.SaveChanges(false),
                                c => false));
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(7, connection.OpenCount);
                Assert.Equal(7, connection.ExecutionCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(0, context.Products.Count());
            }
        }

        protected class ExecutionStrategyContext : DbContext
        {
            public ExecutionStrategyContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        protected class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected virtual ExecutionStrategyContext CreateContext()
            => (ExecutionStrategyContext)Fixture.CreateContext();

        private void CleanContext()
        {
            using var context = CreateContext();
            foreach (var product in context.Products.ToList())
            {
                context.Remove(product);
                context.SaveChanges();
            }
        }

        public class ExecutionStrategyFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override bool UsePooling
                => false;

            protected override string StoreName { get; } = nameof(ExecutionStrategyTest);

            public new RelationalTestStore TestStore
                => (RelationalTestStore)base.TestStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override Type ContextType { get; } = typeof(ExecutionStrategyContext);

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddSingleton<IRelationalTransactionFactory, TestRelationalTransactionFactory>()
                    .AddScoped<ISqlServerConnection, TestSqlServerConnection>()
                    .AddSingleton<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder);
                new SqlServerDbContextOptionsBuilder(options).MaxBatchSize(1);
                return options;
            }

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Infrastructure.Name;
        }
    }
}
