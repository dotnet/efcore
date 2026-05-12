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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    private class SomeDbContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Foo> Foos { get; set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>().Property(e => e.Token).IsConcurrencyToken();

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(EntityFrameworkMetricsTest));
    }

    private class Foo
    {
        public Guid Id { get; set; }
        public int Token { get; set; }
    }
}

[CollectionDefinition(nameof(MetricsDataCollection), DisableParallelization = true)]
public class MetricsDataCollection;
