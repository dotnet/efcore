// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

// The tests interact with global state and should never be run in parallel
[Collection(nameof(MetricsDataCollection))]
public class EntityFrameworkMetricsTest
{
    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_active_dbcontexts_created_disposed(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetActiveDbContexts();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
            var contexts = new List<DbContext>
            {
                new SomeDbContext(),
                new SomeDbContext(),
                new SomeDbContext()
            };

            metricsProvider.ForceFlush();
            var value3 = GetMetricPoints(exportedItems, EntityFrameworkMetrics.ActiveDbContextsInstrumentName)
                .Single()
                .GetSumLong();
            Assert.Equal(3 + initial, value3);

            for (var i = 2; i >= 0; i--)
            {
                if (async)
                {
                    await contexts[i].DisposeAsync();
                }
                else
                {
                    contexts[i].Dispose();
                }

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.ActiveDbContextsInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Active_dbcontexts_is_decremented_when_pooled_context_is_torn_down(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetActiveDbContexts();

        // A pool size of 1 means the second returned context overflows the pool and is fully torn down.
        using var serviceProvider = new ServiceCollection()
            .AddPooledDbContextFactory<SomeDbContext>(ob => ob.UseInMemoryDatabase(nameof(EntityFrameworkMetricsTest)), poolSize: 1)
            .BuildServiceProvider(validateScopes: true);

        var factory = serviceProvider.GetRequiredService<IDbContextFactory<SomeDbContext>>();

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Equal(initial + 2, EntityFrameworkMetricsData.GetActiveDbContexts());

        // Returning the first context fills the single-slot pool; it stays counted while idle in the pool.
        await Dispose(context1, async);
        Assert.Equal(initial + 2, EntityFrameworkMetricsData.GetActiveDbContexts());

        // Returning the second context overflows the pool, so it is fully torn down and the counter is decremented.
        await Dispose(context2, async);
        Assert.Equal(initial + 1, EntityFrameworkMetricsData.GetActiveDbContexts());
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Active_dbcontexts_is_decremented_when_scoped_pooled_context_is_torn_down(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetActiveDbContexts();

        // A pool size of 1 means the second released context overflows the pool and is fully torn down.
        using var serviceProvider = new ServiceCollection()
            .AddDbContextPool<SomeDbContext>(ob => ob.UseInMemoryDatabase(nameof(EntityFrameworkMetricsTest)), poolSize: 1)
            .BuildServiceProvider(validateScopes: true);

        var scope1 = serviceProvider.CreateScope();
        _ = scope1.ServiceProvider.GetRequiredService<SomeDbContext>();

        Assert.Equal(initial + 1, EntityFrameworkMetricsData.GetActiveDbContexts());

        using (var scope2 = serviceProvider.CreateScope())
        {
            _ = scope2.ServiceProvider.GetRequiredService<SomeDbContext>();            
        }

        // Disposing the second scope releases its context, filling the single-slot pool; it stays counted while idle.
        Assert.Equal(initial + 2, EntityFrameworkMetricsData.GetActiveDbContexts());

        // Disposing the first scope releases a context that overflows the pool, so it is fully torn down and the
        // counter is decremented.
        await Dispose(scope1, async);
        Assert.Equal(initial + 1, EntityFrameworkMetricsData.GetActiveDbContexts());
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_query_executed(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetTotalQueriesExecuted();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
            for (var i = 1; i <= 3; i++)
            {
                using var context = new SomeDbContext();

                _ = async ? await context.Foos.ToListAsync() : context.Foos.ToList();

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.QueriesInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_savechanges(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetTotalSaveChanges();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
            for (var i = 1; i <= 3; i++)
            {
                using var context = new SomeDbContext();

                await context.AddAsync(new Foo());

                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.SaveChangesInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_query_cache_hits(bool async)
    {
        var (initial, _, _) = EntityFrameworkMetricsData.GetCompiledQueryCacheHitRate();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
            for (var i = 0; i < 3; i++)
            {
                using var context = new SomeDbContext();

                var query = context.Foos.Where(e => e.Id == new Guid("BB833808-1ADC-4FC2-ACB2-AA6EA31A7DBF"));

                _ = async ? await query.ToListAsync() : query.ToList();

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.CompiledQueryCacheHitsInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_query_cache_misses(bool async)
    {
        var (_, initial, _) = EntityFrameworkMetricsData.GetCompiledQueryCacheHitRate();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
            for (var i = 0; i < 3; i++)
            {
                using var context = new SomeDbContext();

                var query = i switch
                {
                    0 => context.Foos.Where(e => e.Id == new Guid("BB833808-1ADC-4FC2-ACB2-AA6EA31A7DBE")),
                    1 => context.Foos.Where(e => e.Id == new Guid("BB833808-1ADC-4FC2-ACB2-AA6EA31A7DBD")),
                    2 => context.Foos.Where(e => e.Id == new Guid("BB833808-1ADC-4FC2-ACB2-AA6EA31A7DBC")),
                    _ => throw new UnreachableException(),
                };

                _ = async ? await query.ToListAsync() : query.ToList();

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.CompiledQueryCacheMissesInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + 1 + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Validate_optimistic_concurrency(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetTotalOptimisticConcurrencyFailures();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
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

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.OptimisticConcurrencyFailuresInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(i + initial, value);
            }
        }
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task Counts_when_execution_strategy_retries(bool async)
    {
        var initial = EntityFrameworkMetricsData.GetTotalExecutionStrategyOperationFailures();
        var (metricsProvider, exportedItems) = Setup();
        using (metricsProvider)
        {
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
                        (await Assert.ThrowsAsync<RetryLimitExceededException>(() =>
                            executionStrategyMock.ExecuteAsync(() =>
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
                        Assert.Throws<RetryLimitExceededException>(() =>
                            executionStrategyMock.Execute(() =>
                            {
                                if (executionCount++ < 3)
                                {
                                    throw new ArgumentOutOfRangeException();
                                }

                                Assert.True(false);
                                return 0;
                            })).InnerException);
                }

                metricsProvider.ForceFlush();
                var value = GetMetricPoints(exportedItems, EntityFrameworkMetrics.ExecutionStrategyFailuresInstrumentName)
                    .Single()
                    .GetSumLong();
                Assert.Equal(3, executionCount);
                Assert.Equal(i * 3 + initial, value);
            }
        }
    }

    private static (MeterProvider, List<Metric>) Setup()
    {
        var exportedItems = new List<Metric>();
        var meterProvider = Sdk
            .CreateMeterProviderBuilder()
            .AddMeter(EntityFrameworkMetrics.MeterName)
            .AddInMemoryExporter(exportedItems)
            .Build();
        return (meterProvider, exportedItems);
    }

    private static IEnumerable<MetricPoint> GetMetricPoints(List<Metric> exportedItems, string name)
    {
        var metric = exportedItems
            .First(x => name.Equals(x.Name, StringComparison.Ordinal));
        foreach (var mp in metric.GetMetricPoints())
        {
            yield return mp;
        }
    }

    private static async Task Dispose(IDisposable disposable, bool async)
    {
        if (async)
        {
            await ((IAsyncDisposable)disposable).DisposeAsync();
        }
        else
        {
            disposable.Dispose();
        }
    }

    private class SomeDbContext : DbContext
    {
        public SomeDbContext()
        {
        }

        public SomeDbContext(DbContextOptions<SomeDbContext> options)
            : base(options)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Foo> Foos { get; set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>().Property(e => e.Token).IsConcurrencyToken();

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(nameof(EntityFrameworkMetricsTest));
            }
        }
    }

    private class Foo
    {
        public Guid Id { get; set; }
        public int Token { get; set; }
    }
}

[CollectionDefinition(nameof(MetricsDataCollection), DisableParallelization = true)]
public class MetricsDataCollection;
