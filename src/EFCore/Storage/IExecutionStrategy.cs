// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     A strategy that is used to execute a command or query against the database, possibly with logic to retry when a failure occurs.
    /// </summary>
    public interface IExecutionStrategy
    {
        /// <summary>
        ///     Indicates whether this <see cref="IExecutionStrategy" /> might retry the execution after a failure.
        /// </summary>
        bool RetriesOnFailure { get; }

        /// <summary>
        ///     Executes the specified operation and returns the result.
        /// </summary>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded"> A delegate that tests whether the operation succeeded even though an exception was thrown. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <typeparam name="TResult"> The return type of <paramref name="operation" />. </typeparam>
        /// <returns> The result from the operation. </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        TResult Execute<TState, TResult>(
            [CanBeNull] TState state,
            [NotNull] Func<DbContext, TState, TResult> operation,
            [CanBeNull] Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded);

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result.
        /// </summary>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded"> A delegate that tests whether the operation succeeded even though an exception was thrown. </param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <typeparam name="TResult"> The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />. </typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        Task<TResult> ExecuteAsync<TState, TResult>(
            [CanBeNull] TState state,
            [NotNull] Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            [CanBeNull] Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken = default);
    }
}
