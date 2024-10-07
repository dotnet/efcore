// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Tracing;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     An <see cref="EventSource" /> emitting Entity Framework performance counter data.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
/// </remarks>
[Obsolete("Use OpenTelemetry metrics via EntityFrameworkMetricsData instead.")]
public sealed class EntityFrameworkEventSource : EventSource
{
    // ReSharper disable NotAccessedField.Local
    private PollingCounter? _activeDbContextsCounter;
    private PollingCounter? _totalQueriesCounter;
    private IncrementingPollingCounter? _queriesPerSecondCounter;
    private PollingCounter? _totalSaveChangesCounter;
    private IncrementingPollingCounter? _saveChangesPerSecondCounter;
    private PollingCounter? _compiledQueryCacheHitRateCounter;
    private PollingCounter? _totalExecutionStrategyOperationFailuresCounter;
    private IncrementingPollingCounter? _executionStrategyOperationFailuresPerSecondCounter;
    private PollingCounter? _totalOptimisticConcurrencyFailuresCounter;
    private IncrementingPollingCounter? _optimisticConcurrencyFailuresPerSecondCounter;
    // ReSharper restore NotAccessedField.Local

    /// <summary>
    ///     The singleton instance of <see cref="EntityFrameworkEventSource" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public static readonly EntityFrameworkEventSource Log = new();

    private EntityFrameworkEventSource()
        : base("Microsoft.EntityFrameworkCore")
    {
    }

    /// <summary>
    ///     Indicates that a new <see cref="DbContext" /> instance is being initialized.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void DbContextInitializing()
        => EntityFrameworkMetricsData.ReportDbContextInitializing();

    /// <summary>
    ///     Indicates that a <see cref="DbContext" /> instance is being disposed.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void DbContextDisposing()
        => EntityFrameworkMetricsData.ReportDbContextDisposing();

    /// <summary>
    ///     Indicates that a query is about to begin execution.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void QueryExecuting()
        => EntityFrameworkMetricsData.ReportQueryExecuting();

    /// <summary>
    ///     Indicates that changes are about to be saved.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void SavingChanges()
        => EntityFrameworkMetricsData.ReportSavingChanges();

    /// <summary>
    ///     Indicates a hit in the compiled query cache, signifying that query compilation will not need to occur.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void CompiledQueryCacheHit()
        => EntityFrameworkMetricsData.ReportCompiledQueryCacheHit();

    /// <summary>
    ///     Indicates a miss in the compiled query cache, signifying that query compilation will need to occur.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void CompiledQueryCacheMiss()
        => EntityFrameworkMetricsData.ReportCompiledQueryCacheMiss();

    /// <summary>
    ///     Indicates that an operation executed by an <see cref="IExecutionStrategy" /> failed (and may be retried).
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void ExecutionStrategyOperationFailure()
        => EntityFrameworkMetricsData.ReportExecutionStrategyOperationFailure();

    /// <summary>
    ///     Indicates that an optimistic concurrency failure has occurred.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void OptimisticConcurrencyFailure()
        => EntityFrameworkMetricsData.ReportOptimisticConcurrencyFailure();

    /// <inheritdoc />
    protected override void OnEventCommand(EventCommandEventArgs command)
    {
        if (command.Command == EventCommand.Enable)
        {
            // Comment taken from RuntimeEventSource in CoreCLR
            // NOTE: These counters will NOT be disposed on disable command because we may be introducing
            // a race condition by doing that. We still want to create these lazily so that we aren't adding
            // overhead by at all times even when counters aren't enabled.
            // On disable, PollingCounters will stop polling for values so it should be fine to leave them around.

            _activeDbContextsCounter ??= new PollingCounter(
                "active-db-contexts",
                this,
                () => EntityFrameworkMetricsData.GetActiveDbContexts()) { DisplayName = "Active DbContexts" };

            _totalQueriesCounter ??= new PollingCounter(
                "total-queries",
                this,
                () => EntityFrameworkMetricsData.GetTotalQueriesExecuted()) { DisplayName = "Queries (Total)" };

            _queriesPerSecondCounter ??= new IncrementingPollingCounter(
                "queries-per-second",
                this,
                () => EntityFrameworkMetricsData.GetTotalQueriesExecuted())
            {
                DisplayName = "Queries", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _totalSaveChangesCounter ??= new PollingCounter(
                "total-save-changes",
                this,
                () => EntityFrameworkMetricsData.GetTotalSaveChanges()) { DisplayName = "SaveChanges (Total)" };

            _saveChangesPerSecondCounter ??= new IncrementingPollingCounter(
                "save-changes-per-second",
                this,
                () => EntityFrameworkMetricsData.GetTotalSaveChanges())
            {
                DisplayName = "SaveChanges", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _compiledQueryCacheHitRateCounter ??= new PollingCounter(
                "compiled-query-cache-hit-rate",
                this,
                () => EntityFrameworkMetricsData.GetCompiledQueryCacheHitRateEventSource().hitRate)
            {
                DisplayName = "Query Cache Hit Rate", DisplayUnits = "%"
            };

            _totalExecutionStrategyOperationFailuresCounter ??= new PollingCounter(
                "total-execution-strategy-operation-failures",
                this,
                () => EntityFrameworkMetricsData.GetTotalExecutionStrategyOperationFailures())
            {
                DisplayName = "Execution Strategy Operation Failures (Total)"
            };

            _executionStrategyOperationFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                "execution-strategy-operation-failures-per-second",
                this,
                () => EntityFrameworkMetricsData.GetTotalExecutionStrategyOperationFailures())
            {
                DisplayName = "Execution Strategy Operation Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _totalOptimisticConcurrencyFailuresCounter ??= new PollingCounter(
                "total-optimistic-concurrency-failures",
                this,
                () => EntityFrameworkMetricsData.GetTotalOptimisticConcurrencyFailures())
            {
                DisplayName = "Optimistic Concurrency Failures (Total)"
            };

            _optimisticConcurrencyFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                "optimistic-concurrency-failures-per-second",
                this,
                () => EntityFrameworkMetricsData.GetTotalOptimisticConcurrencyFailures())
            {
                DisplayName = "Optimistic Concurrency Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };
        }
    }
}
