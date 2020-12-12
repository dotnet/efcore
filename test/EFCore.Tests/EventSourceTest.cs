// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class EventSourceTest : IDisposable, IClassFixture<EventSourceTest.EventSourceFixture>
    {
        private static readonly AsyncLocal<bool> _isTestThread = new();
        private readonly EventSourceFixture _fixture;

        public EventSourceTest(EventSourceFixture fixture)
        {
            _fixture = fixture;
            _isTestThread.Value = true;
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_when_query_is_executed(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                using (var context = new SomeDbContext())
                {
                    var _ = async
                        ? await context.Foos.ToListAsync()
                        : context.Foos.ToList();

                    Assert.Equal(i, EventSourceFixture.TestEventSource.QueryExecutingCount);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_when_SaveChanges_is_called(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                using (var context = new SomeDbContext())
                {
                    context.Add(new Foo());

                    var _ = async
                        ? await context.SaveChangesAsync()
                        : context.SaveChanges();

                    Assert.Equal(i, EventSourceFixture.TestEventSource.SavingChangesCount);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_when_context_is_constructed_and_disposed(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                if (async)
                {
                    await using (var context = new SomeDbContext())
                    {
                        var _ = context.Model;
                        Assert.Equal(i, EventSourceFixture.TestEventSource.DbContextInitializingCount);
                    }

                    Assert.Equal(i, EventSourceFixture.TestEventSource.DbContextDisposingCount);
                }
                else
                {
                    using (var context = new SomeDbContext())
                    {
                        var _ = context.Model;
                        Assert.Equal(i, EventSourceFixture.TestEventSource.DbContextInitializingCount);
                    }

                    Assert.Equal(i, EventSourceFixture.TestEventSource.DbContextDisposingCount);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_query_cache_hits_and_misses(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                using (var context = new SomeDbContext())
                {
                    var query = context.Foos.Where(e => e.Id == new Guid("6898CFFC-3DCC-45A6-A472-A23057462EE6"));

                    var _ = async
                        ? await query.ToListAsync()
                        : query.ToList();

                    Assert.Equal(1, EventSourceFixture.TestEventSource.CompiledQueryCacheMissCount);
                    Assert.Equal(i - 1, EventSourceFixture.TestEventSource.CompiledQueryCacheHitCount);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_when_DbUpdateConcurrencyException_is_thrown(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                using (var context = new SomeDbContext())
                {
                    var entity = new Foo();
                    context.Add(entity);
                    context.SaveChanges();

                    using (var innerContext = new SomeDbContext())
                    {
                        innerContext.Foos.Find(entity.Id).Token = 1;
                        innerContext.SaveChanges();
                    }

                    entity.Token = 2;

                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
                    }

                    Assert.Equal(i, EventSourceFixture.TestEventSource.OptimisticConcurrencyFailureCount);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Counts_when_execution_strategy_retries(bool async)
        {
            EventSourceFixture.TestEventSource.Clear();

            for (var i = 1; i <= 3; i++)
            {
                using (var context = new SomeDbContext())
                {
                    var executionCount = 0;
                    var executionStrategyMock = new ExecutionStrategyTest.TestExecutionStrategy(
                        context,
                        retryCount: 2,
                        shouldRetryOn: e => e is ArgumentOutOfRangeException,
                        getNextDelay: e => TimeSpan.FromTicks(0));

                    if (async)
                    {
                        Assert.IsType<ArgumentOutOfRangeException>(
                            (await Assert.ThrowsAsync<RetryLimitExceededException>(
                                () =>
                                    executionStrategyMock.ExecuteAsync(
                                        () =>
                                        {
                                            if (executionCount++ < 3)
                                            {
                                                throw new ArgumentOutOfRangeException();
                                            }

                                            Assert.True(false);
                                            return Task.FromResult(1);
                                        }))).InnerException);
                    }
                    else
                    {
                        Assert.IsType<ArgumentOutOfRangeException>(
                            Assert.Throws<RetryLimitExceededException>(
                                () =>
                                    executionStrategyMock.Execute(
                                        () =>
                                        {
                                            if (executionCount++ < 3)
                                            {
                                                throw new ArgumentOutOfRangeException();
                                            }

                                            Assert.True(false);
                                            return 0;
                                        })).InnerException);
                    }

                    Assert.Equal(3, executionCount);
                    Assert.Equal(i * 3, EventSourceFixture.TestEventSource.ExecutionStrategyOperationFailureCount);
                }
            }
        }

        public void Dispose()
            => _isTestThread.Value = false;

        private class SomeDbContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Foo>().Property(e => e.Token).IsConcurrencyToken();

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(nameof(EventSourceTest));
        }

        private class Foo
        {
            public Guid Id { get; set; }
            public int Token { get; set; }
        }

        public class EventSourceFixture
        {
            public static TestEntityFrameworkEventSource TestEventSource = new();

            public EventSourceFixture()
                => typeof(EntityFrameworkEventSource).GetField(nameof(EntityFrameworkEventSource.Log))!.SetValue(null, TestEventSource);
        }

        public class TestEntityFrameworkEventSource : EntityFrameworkEventSource
        {
            public int DbContextInitializingCount { get; private set; }
            public override void DbContextInitializing()
            {
                if (_isTestThread.Value)
                {
                    DbContextInitializingCount++;
                }

                base.DbContextInitializing();
            }

            public int DbContextDisposingCount { get; private set; }
            public override void DbContextDisposing()
            {
                if (_isTestThread.Value)
                {
                    DbContextDisposingCount++;
                }

                base.DbContextDisposing();
            }

            public int QueryExecutingCount { get; private set; }
            public override void QueryExecuting()
            {
                if (_isTestThread.Value)
                {
                    QueryExecutingCount++;
                }

                base.QueryExecuting();
            }

            public int SavingChangesCount { get; private set; }
            public override void SavingChanges()
            {
                if (_isTestThread.Value)
                {
                    SavingChangesCount++;
                }

                base.SavingChanges();
            }

            public int CompiledQueryCacheHitCount { get; private set; }
            public override void CompiledQueryCacheHit()
            {
                if (_isTestThread.Value)
                {
                    CompiledQueryCacheHitCount++;
                }

                base.CompiledQueryCacheHit();
            }

            public int CompiledQueryCacheMissCount { get; private set; }
            public override void CompiledQueryCacheMiss()
            {
                if (_isTestThread.Value)
                {
                    CompiledQueryCacheMissCount++;
                }

                base.CompiledQueryCacheMiss();
            }

            public int ExecutionStrategyOperationFailureCount { get; private set; }
            public override void ExecutionStrategyOperationFailure()
            {
                if (_isTestThread.Value)
                {
                    ExecutionStrategyOperationFailureCount++;
                }

                base.ExecutionStrategyOperationFailure();
            }

            public int OptimisticConcurrencyFailureCount { get; private set; }
            public override void OptimisticConcurrencyFailure()
            {
                if (_isTestThread.Value)
                {
                    OptimisticConcurrencyFailureCount++;
                }

                base.OptimisticConcurrencyFailure();
            }

            public void Clear()
            {
                QueryExecutingCount = 0;
                SavingChangesCount = 0;
                DbContextDisposingCount = 0;
                DbContextInitializingCount = 0;
                OptimisticConcurrencyFailureCount = 0;
                CompiledQueryCacheHitCount = 0;
                CompiledQueryCacheMissCount = 0;
                ExecutionStrategyOperationFailureCount = 0;
            }
        }
    }
}
