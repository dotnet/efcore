// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     An <see cref="EventSource" /> emitting Entity Framework performance counter data.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
/// </remarks>
public sealed class EntityFrameworkEventSource : EventSource
{
    private long _activeDbContexts, _totalQueries, _totalSaveChanges;
    private long _totalExecutionStrategyOperationFailures, _totalOptimisticConcurrencyFailures;
    private CacheInfo _compiledQueryCacheInfo;

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
        => Interlocked.Increment(ref _activeDbContexts);

    /// <summary>
    ///     Indicates that a <see cref="DbContext" /> instance is being disposed.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void DbContextDisposing()
        => Interlocked.Decrement(ref _activeDbContexts);

    /// <summary>
    ///     Indicates that a query is about to begin execution.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void QueryExecuting()
        => Interlocked.Increment(ref _totalQueries);

    /// <summary>
    ///     Indicates that changes are about to be saved.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void SavingChanges()
        => Interlocked.Increment(ref _totalSaveChanges);

    /// <summary>
    ///     Indicates a hit in the compiled query cache, signifying that query compilation will not need to occur.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void CompiledQueryCacheHit()
        => Interlocked.Increment(ref _compiledQueryCacheInfo.Hits);

    /// <summary>
    ///     Indicates a miss in the compiled query cache, signifying that query compilation will need to occur.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void CompiledQueryCacheMiss()
        => Interlocked.Increment(ref _compiledQueryCacheInfo.Misses);

    /// <summary>
    ///     Indicates that an operation executed by an <see cref="IExecutionStrategy" /> failed (and may be retried).
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void ExecutionStrategyOperationFailure()
        => Interlocked.Increment(ref _totalExecutionStrategyOperationFailures);

    /// <summary>
    ///     Indicates that an optimistic concurrency failure has occurred.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-event-counters">EF Core event counters</see> for more information and examples.
    /// </remarks>
    public void OptimisticConcurrencyFailure()
        => Interlocked.Increment(ref _totalOptimisticConcurrencyFailures);

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

            _activeDbContextsCounter ??= new PollingCounter("active-db-contexts", this, () => Interlocked.Read(ref _activeDbContexts))
            {
                DisplayName = "Active DbContexts"
            };

            _totalQueriesCounter ??= new PollingCounter("total-queries", this, () => Interlocked.Read(ref _totalQueries))
            {
                DisplayName = "Queries (Total)"
            };

            _queriesPerSecondCounter ??= new IncrementingPollingCounter(
                "queries-per-second",
                this,
                () => Interlocked.Read(ref _totalQueries)) { DisplayName = "Queries", DisplayRateTimeScale = TimeSpan.FromSeconds(1) };

            _totalSaveChangesCounter ??= new PollingCounter("total-save-changes", this, () => Interlocked.Read(ref _totalSaveChanges))
            {
                DisplayName = "SaveChanges (Total)"
            };

            _saveChangesPerSecondCounter ??= new IncrementingPollingCounter(
                "save-changes-per-second",
                this,
                () => Interlocked.Read(ref _totalSaveChanges))
            {
                DisplayName = "SaveChanges", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _compiledQueryCacheHitRateCounter ??= new PollingCounter(
                "compiled-query-cache-hit-rate",
                this,
                () => _compiledQueryCacheInfo.CalculateAndReset()) { DisplayName = "Query Cache Hit Rate", DisplayUnits = "%" };

            _totalExecutionStrategyOperationFailuresCounter ??= new PollingCounter(
                "total-execution-strategy-operation-failures",
                this,
                () => Interlocked.Read(ref _totalExecutionStrategyOperationFailures))
            {
                DisplayName = "Execution Strategy Operation Failures (Total)"
            };

            _executionStrategyOperationFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                "execution-strategy-operation-failures-per-second",
                this,
                () => Interlocked.Read(ref _totalExecutionStrategyOperationFailures))
            {
                DisplayName = "Execution Strategy Operation Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };

            _totalOptimisticConcurrencyFailuresCounter ??= new PollingCounter(
                "total-optimistic-concurrency-failures",
                this,
                () => Interlocked.Read(ref _totalOptimisticConcurrencyFailures))
            {
                DisplayName = "Optimistic Concurrency Failures (Total)"
            };

            _optimisticConcurrencyFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                "optimistic-concurrency-failures-per-second",
                this,
                () => Interlocked.Read(ref _totalOptimisticConcurrencyFailures))
            {
                DisplayName = "Optimistic Concurrency Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
            };
        }
    }

    [UsedImplicitly]
    private void ResetCacheInfo()
        => _compiledQueryCacheInfo = new CacheInfo();

    [StructLayout(LayoutKind.Explicit)]
    private struct CacheInfo
    {
        [FieldOffset(0)]
        internal int Hits;

        [FieldOffset(4)]
        internal int Misses;

        [FieldOffset(0)]
        private long _all;

        /// <summary>
        ///     Returns the atomically-calculated hit rate and atomically resets <see cref="Hits" /> and <see cref="Misses" /> to 0.
        /// </summary>
        internal double CalculateAndReset()
        {
            var clone = new CacheInfo { _all = Interlocked.Exchange(ref _all, 0) };

            var hitsAndMisses = clone.Hits + clone.Misses;

            // Report -1 for no data to avoid returning NaN, which can trigger issues in downstream consumers
            return hitsAndMisses == 0
                ? -1
                : ((double)clone.Hits / hitsAndMisses) * 100;
        }
    }
}
