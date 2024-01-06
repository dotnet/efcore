// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

// The tests interact with global state and should never be run in parallel
[Collection("EventSourceTest")]
public class EventSourceTest
{
    private static readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_when_query_is_executed(bool async)
    {
        TotalQueries = 0;

        for (var i = 1; i <= 3; i++)
        {
            using var context = new SomeDbContext();

            _ = async ? await context.Foos.ToListAsync() : context.Foos.ToList();

            Assert.Equal(i, TotalQueries);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_when_SaveChanges_is_called(bool async)
    {
        TotalSaveChanges = 0;

        for (var i = 1; i <= 3; i++)
        {
            using var context = new SomeDbContext();

            await context.AddAsync(new Foo());

            _ = async ? await context.SaveChangesAsync() : context.SaveChanges();

            Assert.Equal(i, TotalSaveChanges);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_when_context_is_constructed_and_disposed(bool async)
    {
        ActiveDbContexts = 0;
        var contexts = new List<DbContext>();

        for (var i = 1; i <= 3; i++)
        {
            contexts.Add(new SomeDbContext());

            Assert.Equal(i, ActiveDbContexts);
        }

        for (var i = 2; i >= 0; i--)
        {
            if (async)
            {
                await ((IAsyncDisposable)contexts[i]).DisposeAsync();
            }
            else
            {
                contexts[i].Dispose();
            }

            Assert.Equal(i, ActiveDbContexts);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_query_cache_hits_and_misses(bool async)
    {
        ResetCacheInfo();

        Assert.Equal(-1, CompiledQueryCacheInfoCalculateAndReset());

        for (var i = 1; i <= 3; i++)
        {
            using var context = new SomeDbContext();

            var query = context.Foos.Where(e => e.Id == new Guid("6898CFFC-3DCC-45A6-A472-A23057462EE6"));

            _ = async ? await query.ToListAsync() : query.ToList();

            Assert.Equal(1, CompiledQueryCacheInfoMisses);
            Assert.Equal(i - 1, CompiledQueryCacheInfoHits);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_when_DbUpdateConcurrencyException_is_thrown(bool async)
    {
        TotalOptimisticConcurrencyFailures = 0;

        for (var i = 1; i <= 3; i++)
        {
            using var context = new SomeDbContext();

            var entity = new Foo();
            await context.AddAsync(entity);
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

            Assert.Equal(i, TotalOptimisticConcurrencyFailures);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Counts_when_execution_strategy_retries(bool async)
    {
        TotalExecutionStrategyOperationFailures = 0;

        for (var i = 1; i <= 3; i++)
        {
            using var context = new SomeDbContext();

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
            Assert.Equal(i * 3, TotalExecutionStrategyOperationFailures);
        }
    }

    private static readonly FieldInfo _activeDbContexts
        = typeof(EntityFrameworkEventSource).GetField("_activeDbContexts", _bindingFlags);

    private static long ActiveDbContexts
    {
        get => (long)_activeDbContexts.GetValue(EntityFrameworkEventSource.Log);
        set => _activeDbContexts.SetValue(EntityFrameworkEventSource.Log, value);
    }

    private static readonly FieldInfo _totalQueries
        = typeof(EntityFrameworkEventSource).GetField(nameof(_totalQueries), _bindingFlags);

    private static long TotalQueries
    {
        get => (long)_totalQueries.GetValue(EntityFrameworkEventSource.Log);
        set => _totalQueries.SetValue(EntityFrameworkEventSource.Log, value);
    }

    private static readonly FieldInfo _totalSaveChanges
        = typeof(EntityFrameworkEventSource).GetField(nameof(_totalSaveChanges), _bindingFlags);

    private static long TotalSaveChanges
    {
        get => (long)_totalSaveChanges.GetValue(EntityFrameworkEventSource.Log);
        set => _totalSaveChanges.SetValue(EntityFrameworkEventSource.Log, value);
    }

    private static readonly FieldInfo _totalExecutionStrategyOperationFailures
        = typeof(EntityFrameworkEventSource).GetField(nameof(_totalExecutionStrategyOperationFailures), _bindingFlags);

    private static long TotalExecutionStrategyOperationFailures
    {
        get => (long)_totalExecutionStrategyOperationFailures.GetValue(EntityFrameworkEventSource.Log);
        set => _totalExecutionStrategyOperationFailures.SetValue(EntityFrameworkEventSource.Log, value);
    }

    private static readonly FieldInfo _totalOptimisticConcurrencyFailures
        = typeof(EntityFrameworkEventSource).GetField(nameof(_totalOptimisticConcurrencyFailures), _bindingFlags);

    private static long TotalOptimisticConcurrencyFailures
    {
        get => (long)_totalOptimisticConcurrencyFailures.GetValue(EntityFrameworkEventSource.Log);
        set => _totalOptimisticConcurrencyFailures.SetValue(EntityFrameworkEventSource.Log, value);
    }

    private static readonly FieldInfo _compiledQueryCacheInfo
        = typeof(EntityFrameworkEventSource).GetField(nameof(_compiledQueryCacheInfo), _bindingFlags);

    private static readonly MethodInfo _resetCacheInfo
        = typeof(EntityFrameworkEventSource).GetMethod("ResetCacheInfo", _bindingFlags);

    private static readonly FieldInfo _compiledQueryCacheInfoHits
        = _compiledQueryCacheInfo.FieldType.GetField("Hits", _bindingFlags);

    private static int CompiledQueryCacheInfoHits
        => (int)_compiledQueryCacheInfoHits.GetValue(_compiledQueryCacheInfo.GetValue(EntityFrameworkEventSource.Log));

    private static readonly FieldInfo _compiledQueryCacheInfoMisses
        = _compiledQueryCacheInfo.FieldType.GetField("Misses", _bindingFlags);

    private static int CompiledQueryCacheInfoMisses
        => (int)_compiledQueryCacheInfoMisses.GetValue(_compiledQueryCacheInfo.GetValue(EntityFrameworkEventSource.Log));

    private static readonly MethodInfo _compiledQueryCacheInfoCalculateAndReset
        = _compiledQueryCacheInfo.FieldType.GetMethod("CalculateAndReset", _bindingFlags);

    private static double CompiledQueryCacheInfoCalculateAndReset()
        => (double)_compiledQueryCacheInfoCalculateAndReset.Invoke(
            _compiledQueryCacheInfo.GetValue(EntityFrameworkEventSource.Log), []);

    private static void ResetCacheInfo()
        => _resetCacheInfo.Invoke(EntityFrameworkEventSource.Log, null);

    private class SomeDbContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
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
}

[CollectionDefinition("EventSourceTest", DisableParallelization = true)]
public class EventSourceTestCollection;
