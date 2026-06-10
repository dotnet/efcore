// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Implementation of performance metrics.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[Experimental(EFDiagnostics.MetricsExperimental)]
public static class EntityFrameworkMetricsData
{
#pragma warning disable CS0618 // Type or member is obsolete
    private static readonly EntityFrameworkEventSource EventSourceInstance = EntityFrameworkEventSource.Log;
#pragma warning restore CS0618 // Type or member is obsolete
    private static readonly EntityFrameworkMetrics MetricsInstance = new();

    private static int _activeDbContexts;
    private static long _totalQueriesExecuted;
    private static long _totalSaveChanges;
    private static long _totalExecutionStrategyOperationFailures;
    private static long _totalOptimisticConcurrencyFailures;
    private static CacheInfo _compiledQueryCacheInfo;
    private static CacheInfo _compiledQueryCacheInfoEventSource;

    /// <summary>
    ///     Indicates that a new <see cref="DbContext" /> instance is being initialized.
    /// </summary>
    public static void ReportDbContextInitializing()
        => Interlocked.Increment(ref _activeDbContexts);

    /// <summary>
    ///     Indicates that a <see cref="DbContext" /> instance is being disposed.
    /// </summary>
    public static void ReportDbContextDisposing()
        => Interlocked.Decrement(ref _activeDbContexts);

    /// <summary>
    ///     Number of currently active <see cref="DbContext" /> instances.
    /// </summary>
    internal static int GetActiveDbContexts()
        => Volatile.Read(ref _activeDbContexts);

    /// <summary>
    ///     Indicates that a query is about to begin execution.
    /// </summary>
    public static void ReportQueryExecuting()
        => Interlocked.Increment(ref _totalQueriesExecuted);

    /// <summary>
    ///     Cumulative count of queries executed.
    /// </summary>
    internal static long GetTotalQueriesExecuted()
        => Interlocked.Read(ref _totalQueriesExecuted);

    /// <summary>
    ///     Indicates that changes are about to be saved.
    /// </summary>
    public static void ReportSavingChanges()
        => Interlocked.Increment(ref _totalSaveChanges);

    /// <summary>
    ///     Cumulative count of changes saved.
    /// </summary>
    internal static long GetTotalSaveChanges()
        => Interlocked.Read(ref _totalSaveChanges);

    /// <summary>
    ///     Indicates a hit in the compiled query cache, signifying that query compilation will not need to occur.
    /// </summary>
    public static void ReportCompiledQueryCacheHit()
    {
        Interlocked.Increment(ref _compiledQueryCacheInfo.Hits);
        Interlocked.Increment(ref _compiledQueryCacheInfoEventSource.Hits);
    }

    /// <summary>
    ///     Indicates a miss in the compiled query cache, signifying that query compilation will need to occur.
    /// </summary>
    public static void ReportCompiledQueryCacheMiss()
    {
        Interlocked.Increment(ref _compiledQueryCacheInfo.Misses);
        Interlocked.Increment(ref _compiledQueryCacheInfoEventSource.Misses);
    }

    /// <summary>
    ///     Gets number of hits and misses and also the computed hit rate for the compiled query cache.
    /// </summary>
    internal static (int hits, int misses, double hitRate) GetCompiledQueryCacheHitRate()
        => _compiledQueryCacheInfo.CalculateHitsMissesHitRate(false);

    /// <summary>
    ///     Gets number of hits and misses and also the computed hit rate for the compiled query cache.
    /// </summary>
    internal static (int hits, int misses, double hitRate) GetCompiledQueryCacheHitRateEventSource()
        => _compiledQueryCacheInfoEventSource.CalculateHitsMissesHitRate(true);

    /// <summary>
    ///     Indicates that an operation executed by an <see cref="IExecutionStrategy" /> failed (and may be retried).
    /// </summary>
    public static void ReportExecutionStrategyOperationFailure()
        => Interlocked.Increment(ref _totalExecutionStrategyOperationFailures);

    /// <summary>
    ///     Cumulative number of failed operation executed by an <see cref="IExecutionStrategy" />.
    /// </summary>
    internal static long GetTotalExecutionStrategyOperationFailures()
        => Interlocked.Read(ref _totalExecutionStrategyOperationFailures);

    /// <summary>
    ///     Indicates that an optimistic concurrency failure has occurred.
    /// </summary>
    public static void ReportOptimisticConcurrencyFailure()
        => Interlocked.Increment(ref _totalOptimisticConcurrencyFailures);

    /// <summary>
    ///     Cumulative number of optimistic concurrency failures.
    /// </summary>
    internal static long GetTotalOptimisticConcurrencyFailures()
        => Interlocked.Read(ref _totalOptimisticConcurrencyFailures);

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
        ///     Returns the atomically-calculated hits, misses and hit rate and atomically resets <see cref="Hits" /> and <see cref="Misses" /> to 0.
        /// </summary>
        internal (int hits, int misses, double hitRate) CalculateHitsMissesHitRate(bool reset)
        {
            var clone = reset
                ? new CacheInfo { _all = Interlocked.Exchange(ref _all, 0) }
                : new CacheInfo { _all = Interlocked.Read(ref _all) };
            return CalculateHitsMissesHitRateImpl(clone);

            static (int hits, int misses, double hitRate) CalculateHitsMissesHitRateImpl(CacheInfo cacheInfo)
            {
                var hitsAndMisses = cacheInfo.Hits + cacheInfo.Misses;
                // Report -1 for no data to avoid returning NaN, which can trigger issues in downstream consumers
                var hitRate = hitsAndMisses == 0
                    ? -1
                    : ((double)cacheInfo.Hits / hitsAndMisses) * 100;
                return (cacheInfo.Hits, cacheInfo.Misses, hitRate);
            }
        }
    }
}
