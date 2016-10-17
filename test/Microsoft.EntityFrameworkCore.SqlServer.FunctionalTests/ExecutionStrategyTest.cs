// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        // #6720
        //[Fact]
        public void Retries_on_true_commit_failure()
        {
            Test_commit_failure(true);
        }

        private void Test_commit_failure(bool realFailure)
        {
            using (var context = CreateContext())
            {
                var contextServices = context.GetService<IServiceProvider>();
                var connection = (TestSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

                connection.CommitFailures.Enqueue(new bool?[] { realFailure });

                context.Products.Add(new Product());
                new TestSqlServerRetryingExecutionStrategy(context).ExecuteInTransaction(
                    c => c.SaveChanges(acceptAllChangesOnSuccess: false),
                    c =>
                        {
                            var succeeded = c.Products.AsNoTracking().Any();
                            if (!succeeded)
                            {
                                // TODO: call DiscardStoreGeneratedValues()
                                // #6719
                            }
                            return succeeded;
                        },
                    context);
                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(realFailure ? 3 : 2, connection.OpenCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(1, context.Products.Count());
            }
        }

        // #6720
        //[Fact]
        public void Does_not_throw_or_retry_on_false_execution_failure()
        {
            Test_execution_failure(false);
        }

        // #6720
        //[Fact]
        public void Retries_on_true_execution_failure()
        {
            Test_execution_failure(true);
        }

        private void Test_execution_failure(bool realFailure)
        {
            using (var context = CreateContext())
            {
                var contextServices = context.GetService<IServiceProvider>();
                var connection = (TestSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

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
                Assert.Equal(2, connection.ExecutionCount);
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.Products.Count());
            }
        }

        // #6720
        //[Fact]
        public void Verification_is_retried_using_same_retry_limit()
        {
            using (var context = CreateContext())
            {
                var contextServices = context.GetService<IServiceProvider>();
                var connection = (TestSqlServerConnection)contextServices.GetRequiredService<ISqlServerConnection>();

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

        protected virtual ExecutionStrategyContext CreateContext() => (ExecutionStrategyContext)Fixture.CreateContext();

        public class ExecutionStrategyFixture
        {
            private readonly DbContextOptions _options;

            public ExecutionStrategyFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton<ILoggerFactory, TestSqlLoggerFactory>()
                    .AddScoped<ISqlServerConnection, TestSqlServerConnection>()
                    .AddScoped<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>()
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b =>
                        {
                            b.ApplyConfiguration();
                            b.MaxBatchSize(1);
                        })
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging()
                    .Options;
            }

            public virtual DbContext CreateContext()
                => new ExecutionStrategyContext(_options);
        }
    }
}
