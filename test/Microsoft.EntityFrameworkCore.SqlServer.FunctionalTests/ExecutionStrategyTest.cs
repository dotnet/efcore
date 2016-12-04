// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class ExecutionStrategyTest : IClassFixture<ExecutionStrategyTest.ExecutionStrategyFixture>, IDisposable
    {
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
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.CommitFailures.Enqueue(new bool?[] { realFailure });

                context.Products.Add(new Product());
                new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c => c.Products.AsNoTracking().Any(),
                    context);
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, context.Products.Count());
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

                using (var context2 = CreateContext(connection.DbConnection))
                {
                    connection.CommitFailures.Enqueue(new bool?[] { realFailure });

                    context1.Products.Add(new Product());
                    context2.Products.Add(new Product());

                    new TestSqlServerRetryingExecutionStrategy(context1).ExecuteInTransaction(
                        _ =>
                            {
                                context2.Database.UseTransaction(null);
                                context2.Database.UseTransaction(context1.Database.CurrentTransaction.GetDbTransaction());

                                context1.SaveChanges(acceptAllChangesOnSuccess: false);

                                return context2.SaveChanges(acceptAllChangesOnSuccess: false);
                            },
                        c => c.Products.AsNoTracking().Any(),
                        context1);

                    context1.ChangeTracker.AcceptAllChanges();
                    context2.ChangeTracker.AcceptAllChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(2, context.Products.Count());
                }
            }
        }

        [Fact]
        public void Does_not_throw_or_retry_on_false_execution_failure()
        {
            Test_execution_failure(false);
        }

        [Fact]
        public void Retries_on_true_execution_failure()
        {
            Test_execution_failure(true);
        }

        private void Test_execution_failure(bool realFailure)
        {
            CleanContext();

            using (var context = CreateContext())
            {
                var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();

                connection.ExecutionFailures.Enqueue(new bool?[] { null, realFailure });

                context.Products.Add(new Product());
                context.Products.Add(new Product());
                new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c =>
                        {
                            // This shouldn't be called if SaveChanges failed
                            Assert.True(false);
                            return false;
                        },
                    context);
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(2, connection.OpenCount);
                Assert.Equal(4, connection.ExecutionCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.Products.Count());
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
                connection.CommitFailures.Enqueue(new bool?[] { true, true, true });

                context.Products.Add(new Product());
                Assert.Throws<RetryLimitExceededException>(() =>
                    new TestSqlServerRetryingExecutionStrategy(context, TimeSpan.FromMilliseconds(100))
                        .ExecuteInTransaction(
                            c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                            c => false,
                            context));
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(6, connection.OpenCount);
                Assert.Equal(6, connection.ExecutionCount);
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

        private const string DatabaseName = nameof(ExecutionStrategyTest);

        public ExecutionStrategyTest(ExecutionStrategyFixture fixture)
        {
            Fixture = fixture;
            TestStore = SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureCreated();
                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        protected ExecutionStrategyFixture Fixture { get; }
        protected SqlServerTestStore TestStore { get; }

        public virtual void Dispose() => TestStore.Dispose();

        protected virtual ExecutionStrategyContext CreateContext()
            => (ExecutionStrategyContext)Fixture.CreateContext();

        protected virtual ExecutionStrategyContext CreateContext(DbConnection connection)
            => (ExecutionStrategyContext)Fixture.CreateContext(connection);

        public class ExecutionStrategyFixture
        {
            private readonly DbContextOptions _baseOptions;
            private readonly DbContextOptions _options;

            public ExecutionStrategyFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton<ILoggerFactory, TestSqlLoggerFactory>()
                    .AddScoped<ISqlServerConnection, TestSqlServerConnection>()
                    .AddScoped<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>()
                    .BuildServiceProvider();

                _baseOptions = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging()
                    .Options;

                _options = new DbContextOptionsBuilder(_baseOptions)
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), ApplySqlServerOptions)
                    .Options;
            }

            private static void ApplySqlServerOptions(SqlServerDbContextOptionsBuilder b)
            {
                b.ApplyConfiguration();
                b.MaxBatchSize(1);
            }

            public virtual DbContext CreateContext()
                => new ExecutionStrategyContext(_options);

            public virtual DbContext CreateContext(DbConnection connection)
                => new ExecutionStrategyContext(
                    new DbContextOptionsBuilder(_baseOptions)
                        .UseSqlServer(connection, ApplySqlServerOptions)
                        .Options);
        }

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
    }
}
