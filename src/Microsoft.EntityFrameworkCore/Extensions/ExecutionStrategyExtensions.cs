// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IExecutionStrategy" />
    /// </summary>
    public static class ExecutionStrategyExtensions
    {
        /// <summary>
        ///     Executes the specified operation.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">A delegate representing an executable operation that doesn't return any results.</param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        public static void Execute<TState>([NotNull] this IExecutionStrategy strategy, Action<TState> operation, TState state)
            => strategy.Execute(t =>
                {
                    t.Item1(t.Item2);
                    return true;
                },
                Tuple.Create(operation, state));

        /// <summary>
        ///     Executes the specified asynchronous operation.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">A function that returns a started task.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        public static Task ExecuteAsync<TState>(
            [NotNull] this IExecutionStrategy strategy,
            Func<TState, CancellationToken, Task> operation,
            TState state,
            CancellationToken cancellationToken = new CancellationToken())
            => strategy.ExecuteAsync(async (t, ct) =>
                {
                    await t.Item1(t.Item2, ct);
                    return true;
                }, Tuple.Create(operation, state), cancellationToken);
    }
}
