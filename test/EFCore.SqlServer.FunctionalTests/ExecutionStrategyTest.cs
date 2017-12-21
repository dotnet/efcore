// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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

        [Fact]
        public void Does_not_throw_or_retry_on_false_commit_failure()
        {
            Test_commit_failure(false);
        }

        [Fact]
        public void Retries_on_true_commit_failure()
        {
            Test_commit_failure(true);
        }

        private void Test_commit_failure(bool realFailure)
        {
            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c => c.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c => c.Products.AsNoTracking().Any()));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    () => db.SaveChanges(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c => c.Products.AsNoTracking().Any(),
                    IsolationLevel.Serializable));

            Test_commit_failure(
                realFailure, (e, db) => e.ExecuteInTransaction(
                    db,
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
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

                context.Products.Add(new Product());
                execute(new TestSqlServerRetryingExecutionStrategy(context), context);
                context.ChangeTracker.AcceptAllChanges();

                var retryMessage =
                    "A transient exception has been encountered during execution and the operation will be retried after 0ms." + Environment.NewLine +
                    "System.Data.SqlClient.SqlException (0x80131904): Bang!";
                if (realFailure)
                {
                    Assert.Contains(retryMessage, Fixture.TestSqlLoggerFactory.Log);
                }
                else
                {
                    Assert.DoesNotContain(retryMessage, Fixture.TestSqlLoggerFactory.Log);
                }

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, context.Products.Count());
            }
        }

        [Fact]
        public Task Does_not_throw_or_retry_on_false_commit_failure_async()
        {
            return Test_commit_failure_async(false);
        }

        [Fact]
        public Task Retries_on_true_commit_failure_async()
        {
            return Test_commit_failure_async(true);
        }

        private async Task Test_commit_failure_async(bool realFailure)
        {
            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    () => db.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().AnyAsync()));

            var cancellationToken = CancellationToken.None;
            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    async ct => { await db.SaveChangesAsync(acceptAllChangesOnSuccess: false); },
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    ct => db.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    async (c, ct) => { await c.SaveChangesAsync(acceptAllChangesOnSuccess: false); },
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    (c, ct) => c.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    () => db.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    () => db.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    async ct => { await db.SaveChangesAsync(acceptAllChangesOnSuccess: false); },
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable,
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    ct => db.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    ct => db.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable,
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    async (c, ct) => { await c.SaveChangesAsync(acceptAllChangesOnSuccess: false); },
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable,
                    cancellationToken));

            await Test_commit_failure_async(
                realFailure, (e, db) => e.ExecuteInTransactionAsync(
                    db,
                    (c, ct) => c.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                    (c, ct) => c.Products.AsNoTracking().AnyAsync(),
                    IsolationLevel.Serializable,
                    cancellationToken));
        }

        private async Task Test_commit_failure_async(bool realFailure, Func<TestSqlServerRetryingExecutionStrategy, ExecutionStrategyContext, Task> execute)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.CommitFailures.Enqueue(new bool?[] { realFailure });

                context.Products.Add(new Product());
                await execute(new TestSqlServerRetryingExecutionStrategy(context), context);
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, await context.Products.CountAsync());
            }
        }

        [Fact]
        public void Does_not_throw_or_retry_on_false_commit_failure_multiple_SaveChanges()
        {
            Test_commit_failure_multiple_SaveChanges(false);
        }

        [Fact]
        public void Retries_on_true_commit_failure_multiple_SaveChanges()
        {
            Test_commit_failure_multiple_SaveChanges(true);
        }

        private void Test_commit_failure_multiple_SaveChanges(bool realFailure)
        {
            CleanContext();

            using (var context1 = CreateContext())
            {
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

                                c1.SaveChanges(acceptAllChangesOnSuccess: false);

                                return context2.SaveChanges(acceptAllChangesOnSuccess: false);
                            },
                        c => c.Products.AsNoTracking().Any());

                    context1.ChangeTracker.AcceptAllChanges();
                    context2.ChangeTracker.AcceptAllChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(2, context.Products.Count());
                }
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public async Task Retries_only_on_true_execution_failure(bool realFailure, bool openConnection, bool async)
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
                    await new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransactionAsync(
                        context,
                        (c, _) => c.SaveChangesAsync(acceptAllChangesOnSuccess: false),
                        (c, _) =>
                            {
                                // This shouldn't be called if SaveChanges failed
                                Assert.True(false);
                                return Task.FromResult(false);
                            });
                }
                else
                {
                    new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                        context,
                        c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                        c =>
                            {
                                // This shouldn't be called if SaveChanges failed
                                Assert.True(false);
                                return false;
                            });
                }
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(2, connection.OpenCount);
                Assert.Equal(4, connection.ExecutionCount);

                Assert.Equal(
                    openConnection
                        ? ConnectionState.Open
                        : ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (openConnection)
                {
                    context.Database.CloseConnection();
                }

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.Products.Count());
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Retries_query_on_execution_failure(bool async)
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

                if (async)
                {
                    var list = await new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransactionAsync(
                        context,
                        (c, _) => context.Products.ToListAsync(),
                        (c, _) =>
                            {
                                // This shouldn't be called if query failed
                                Assert.True(false);
                                return Task.FromResult(false);
                            });

                    Assert.Equal(2, list.Count);
                }
                else
                {
                    var list = new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                        context,
                        c => context.Products.ToList(),
                        c =>
                            {
                                // This shouldn't be called if query failed
                                Assert.True(false);
                                return false;
                            });

                    Assert.Equal(2, list.Count);
                }
                Assert.Equal(2, connection.OpenCount);
                Assert.Equal(2, connection.ExecutionCount);

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Retries_OpenConnection_on_execution_failure(bool async)
        {
            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.OpenFailures.Enqueue(new bool?[] { true });

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);

                if (async)
                {
                    await new TestSqlServerRetryingExecutionStrategy(context).ExecuteAsync(
                        context,
                        c => context.Database.OpenConnectionAsync());
                }
                else
                {
                    new TestSqlServerRetryingExecutionStrategy(context).Execute(
                        context,
                        c => context.Database.OpenConnection());
                }

                Assert.Equal(2, connection.OpenCount);

                context.Database.CloseConnection();

                Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Retries_BeginTransaction_on_execution_failure(bool async)
        {
            using (var context = CreateContext())
            {
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
        }

        [Fact]
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
                                c => c.SaveChanges(acceptAllChangesOnSuccess: false),
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
            using (var context = CreateContext())
            {
                foreach (var product in context.Products.ToList())
                {
                    context.Remove(product);
                    context.SaveChanges();
                }
            }
        }

        public class ExecutionStrategyFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = nameof(ExecutionStrategyTest);
            protected override bool UsePooling => false;
            public new RelationalTestStore TestStore => (RelationalTestStore)base.TestStore;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            protected override Type ContextType { get; } = typeof(ExecutionStrategyContext);

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            {
                return base.AddServices(serviceCollection)
                    .AddSingleton<IRelationalTransactionFactory, TestRelationalTransactionFactory>()
                    .AddScoped<ISqlServerConnection, TestSqlServerConnection>()
                    .AddScoped<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder);
                new SqlServerDbContextOptionsBuilder(options).MaxBatchSize(1);
                return options;
            }
        }
    }
}
