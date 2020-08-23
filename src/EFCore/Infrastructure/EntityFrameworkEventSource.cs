// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     An <see cref="EventSource" /> emitting Entity Framework performance counter data.
    /// </summary>
    public sealed class EntityFrameworkEventSource : EventSource
    {
        private long _activeDbContexts, _totalQueries, _totalSaveChanges;
        private long _totalExecutionStrategyOperationFailures, _totalOptimisticConcurrencyFailures;
        private CacheInfo _compiledQueryCacheInfo;

        // ReSharper disable NotAccessedField.Local
        private PollingCounter _activeDbContextsCounter;
        private PollingCounter _totalQueriesCounter;
        private IncrementingPollingCounter _queriesPerSecondCounter;
        private PollingCounter _totalSaveChangesCounter;
        private IncrementingPollingCounter _saveChangesPerSecondCounter;
        private PollingCounter _compiledQueryCacheHitRateCounter;
        private PollingCounter _totalExecutionStrategyOperationFailuresCounter;
        private IncrementingPollingCounter _executionStrategyOperationFailuresPerSecondCounter;
        private PollingCounter _totalOptimisticConcurrencyFailuresCounter;

        private IncrementingPollingCounter _optimisticConcurrencyFailuresPerSecondCounter;
        // ReSharper restore NotAccessedField.Local

        /// <summary>
        ///     The singleton instance of <see cref="EntityFrameworkEventSource" />.
        /// </summary>
        public static readonly EntityFrameworkEventSource Log = new EntityFrameworkEventSource();

        private EntityFrameworkEventSource()
            : base("Microsoft.EntityFrameworkCore")
        {
        }

        /// <summary>
        ///     Indicates that a new <see cref="DbContext" /> instance is being initialized.
        /// </summary>
        public void DbContextInitializing()
            => Interlocked.Increment(ref _activeDbContexts);

        /// <summary>
        ///     Indicates that a <see cref="DbContext" /> instance is being disposed.
        /// </summary>
        public void DbContextDisposing()
            => Interlocked.Decrement(ref _activeDbContexts);

        /// <summary>
        ///     Indicates that a query is about to begin execution.
        /// </summary>
        public void QueryExecuting()
            => Interlocked.Increment(ref _totalQueries);

        /// <summary>
        ///     Indicates that changes are about to be saved.
        /// </summary>
        public void SavingChanges()
            => Interlocked.Increment(ref _totalSaveChanges);

        /// <summary>
        ///     Indicates a hit in the compiled query cache, signifying that query compilation will not need to occur.
        /// </summary>
        public void CompiledQueryCacheHit()
            => Interlocked.Increment(ref _compiledQueryCacheInfo.Hits);

        /// <summary>
        ///     Indicates a miss in the compiled query cache, signifying that query compilation will need to occur.
        /// </summary>
        public void CompiledQueryCacheMiss()
            => Interlocked.Increment(ref _compiledQueryCacheInfo.Misses);

        /// <summary>
        ///     Indicates that an operation executed by an <see cref="IExecutionStrategy" /> failed (and may be retried).
        /// </summary>
        public void ExecutionStrategyOperationFailure()
            => Interlocked.Increment(ref _totalExecutionStrategyOperationFailures);

        /// <summary>
        ///     Indicates that an optimistic concurrency failure has occurred.
        /// </summary>
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

                _activeDbContextsCounter ??= new PollingCounter("active-db-contexts", this, () => _activeDbContexts)
                {
                    DisplayName = "Active DbContexts"
                };

                _totalQueriesCounter ??= new PollingCounter("total-queries", this, () => _totalQueries) { DisplayName = "Queries (Total)" };

                _queriesPerSecondCounter ??= new IncrementingPollingCounter(
                    "queries-per-second",
                    this,
                    () => _totalQueries) { DisplayName = "Queries", DisplayRateTimeScale = TimeSpan.FromSeconds(1) };

                _totalSaveChangesCounter ??= new PollingCounter("total-save-changes", this, () => _totalSaveChanges)
                {
                    DisplayName = "SaveChanges (Total)"
                };

                _saveChangesPerSecondCounter ??= new IncrementingPollingCounter(
                    "save-changes-per-second",
                    this,
                    () => _totalSaveChanges) { DisplayName = "SaveChanges", DisplayRateTimeScale = TimeSpan.FromSeconds(1) };

                _compiledQueryCacheHitRateCounter ??= new PollingCounter(
                    "compiled-query-cache-hit-rate",
                    this,
                    () => _compiledQueryCacheInfo.CalculateAndReset()) { DisplayName = "Query Cache Hit Rate", DisplayUnits = "%" };

                _totalExecutionStrategyOperationFailuresCounter ??= new PollingCounter(
                    "total-execution-strategy-operation-failures",
                    this,
                    () => _totalExecutionStrategyOperationFailures) { DisplayName = "Execution Strategy Operation Failures (Total)" };

                _executionStrategyOperationFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                    "execution-strategy-operation-failures-per-second",
                    this,
                    () => _totalExecutionStrategyOperationFailures)
                {
                    DisplayName = "Execution Strategy Operation Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };

                _totalOptimisticConcurrencyFailuresCounter ??= new PollingCounter(
                    "total-optimistic-concurrency-failures",
                    this,
                    () => _totalOptimisticConcurrencyFailures) { DisplayName = "Optimistic Concurrency Failures (Total)" };

                _optimisticConcurrencyFailuresPerSecondCounter ??= new IncrementingPollingCounter(
                    "optimistic-concurrency-failures-per-second",
                    this,
                    () => _totalOptimisticConcurrencyFailures)
                {
                    DisplayName = "Optimistic Concurrency Failures", DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
            }
        }

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
                var clone = new CacheInfo { _all = Volatile.Read(ref _all) };
                Volatile.Write(ref _all, 0);
                return ((double)clone.Hits / (clone.Hits + clone.Misses)) * 100;
            }
        }
    }
}
