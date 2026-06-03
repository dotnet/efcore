// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Metrics;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class EntityFrameworkMetrics
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string MeterName = "Microsoft.EntityFrameworkCore";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string ActiveDbContextsInstrumentName = $"{InstrumentPrefix}.active_dbcontexts";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string QueriesInstrumentName = $"{InstrumentPrefix}.queries";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string SaveChangesInstrumentName = $"{InstrumentPrefix}.savechanges";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string CompiledQueryCacheHitsInstrumentName = $"{InstrumentPrefix}.compiled_query_cache_hits";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string CompiledQueryCacheMissesInstrumentName = $"{InstrumentPrefix}.compiled_query_cache_misses";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string ExecutionStrategyFailuresInstrumentName = $"{InstrumentPrefix}.execution_strategy_operation_failures";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string OptimisticConcurrencyFailuresInstrumentName = $"{InstrumentPrefix}.optimistic_concurrency_failures";

    private const string InstrumentPrefix = "microsoft.entityframeworkcore";

    private readonly ObservableUpDownCounter<int> _activeDbContextsCounter;
    private readonly ObservableCounter<long> _queriesCounter;
    private readonly ObservableCounter<long> _saveChangesCounter;
    private readonly ObservableCounter<long> _compiledQueryCacheHitsCounter;
    private readonly ObservableCounter<long> _compiledQueryCacheMissesCounter;
    private readonly ObservableCounter<long> _executionStrategyOperationFailuresCounter;
    private readonly ObservableCounter<long> _optimisticConcurrencyFailuresCounter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityFrameworkMetrics()
    {
        var meter = new Meter(MeterName);

        _activeDbContextsCounter = meter.CreateObservableUpDownCounter(
            ActiveDbContextsInstrumentName,
            EntityFrameworkMetricsData.GetActiveDbContexts,
            unit: "{dbcontext}");
        _queriesCounter = meter.CreateObservableCounter(
            QueriesInstrumentName,
            EntityFrameworkMetricsData.GetTotalQueriesExecuted,
            unit: "{query}");
        _saveChangesCounter = meter.CreateObservableCounter(
            SaveChangesInstrumentName,
            EntityFrameworkMetricsData.GetTotalSaveChanges,
            unit: "{savechanges}");
        _compiledQueryCacheHitsCounter = meter.CreateObservableCounter(
            CompiledQueryCacheHitsInstrumentName,
            () => (long)EntityFrameworkMetricsData.GetCompiledQueryCacheHitRate().hits,
            unit: "{hits}");
        _compiledQueryCacheMissesCounter = meter.CreateObservableCounter(
            CompiledQueryCacheMissesInstrumentName,
            () => (long)EntityFrameworkMetricsData.GetCompiledQueryCacheHitRate().misses,
            unit: "{misses}");
        _executionStrategyOperationFailuresCounter = meter.CreateObservableCounter(
            ExecutionStrategyFailuresInstrumentName,
            EntityFrameworkMetricsData.GetTotalExecutionStrategyOperationFailures,
            unit: "{failure}");
        _optimisticConcurrencyFailuresCounter = meter.CreateObservableCounter(
            OptimisticConcurrencyFailuresInstrumentName,
            EntityFrameworkMetricsData.GetTotalOptimisticConcurrencyFailures,
            unit: "{failure}");
    }
}
