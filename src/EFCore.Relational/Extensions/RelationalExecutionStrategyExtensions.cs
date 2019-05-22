// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for <see cref="IExecutionStrategy" /> that can only be used with a
    ///     relational database provider.
    /// </summary>
    public static class RelationalExecutionStrategyExtensions
    {
        /// <summary>
        ///     Executes the specified operation in a transaction. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="operation">
        ///     A delegate representing an executable operation.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static void ExecuteInTransaction(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Action operation,
            [NotNull] Func<bool> verifySucceeded,
            IsolationLevel isolationLevel)
            => strategy.ExecuteInTransaction<object>(null, s => operation(), s => verifySucceeded(), isolationLevel);

        /// <summary>
        ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="operation">
        ///     A function that returns a started task.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static Task ExecuteInTransactionAsync(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<Task> operation,
            [NotNull] Func<Task<bool>> verifySucceeded,
            IsolationLevel isolationLevel)
            => strategy.ExecuteInTransactionAsync<object>(null, (s, ct) => operation(), (s, ct) => verifySucceeded(), isolationLevel);

        /// <summary>
        ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="operation">
        ///     A function that returns a started task.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static Task ExecuteInTransactionAsync(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<CancellationToken, Task> operation,
            [NotNull] Func<CancellationToken, Task<bool>> verifySucceeded,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => strategy.ExecuteInTransactionAsync<object>(
                null, (s, ct) => operation(ct), (s, ct) => verifySucceeded(ct), isolationLevel, cancellationToken);

        /// <summary>
        ///     Executes the specified operation in a transaction and returns the result. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="operation">
        ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <typeparam name="TResult"> The return type of <paramref name="operation" />. </typeparam>
        /// <returns> The result from the operation. </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static TResult ExecuteInTransaction<TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<TResult> operation,
            [NotNull] Func<bool> verifySucceeded,
            IsolationLevel isolationLevel)
            => strategy.ExecuteInTransaction<object, TResult>(null, s => operation(), s => verifySucceeded(), isolationLevel);

        /// <summary>
        ///     Executes the specified asynchronous operation in a transaction and returns the result. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <typeparam name="TResult"> The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />. </typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static Task<TResult> ExecuteInTransactionAsync<TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<CancellationToken, Task<TResult>> operation,
            [NotNull] Func<CancellationToken, Task<bool>> verifySucceeded,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => strategy.ExecuteInTransactionAsync<object, TResult>(
                null, (s, ct) => operation(ct), (s, ct) => verifySucceeded(ct), isolationLevel, cancellationToken);

        /// <summary>
        ///     Executes the specified operation in a transaction. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A delegate representing an executable operation.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static void ExecuteInTransaction<TState>(
            [NotNull] this IExecutionStrategy strategy,
            [CanBeNull] TState state,
            [NotNull] Action<TState> operation,
            [NotNull] Func<TState, bool> verifySucceeded,
            IsolationLevel isolationLevel)
            => strategy.ExecuteInTransaction(
                state,
                s =>
                {
                    operation(s);
                    return true;
                }, verifySucceeded, isolationLevel);

        /// <summary>
        ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A function that returns a started task.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static Task ExecuteInTransactionAsync<TState>(
            [NotNull] this IExecutionStrategy strategy,
            [CanBeNull] TState state,
            [NotNull] Func<TState, CancellationToken, Task> operation,
            [NotNull] Func<TState, CancellationToken, Task<bool>> verifySucceeded,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => strategy.ExecuteInTransactionAsync(
                state,
                async (s, ct) =>
                {
                    await operation(s, ct);
                    return true;
                }, verifySucceeded, isolationLevel, cancellationToken);

        /// <summary>
        ///     Executes the specified operation in a transaction and returns the result. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <typeparam name="TResult"> The return type of <paramref name="operation" />. </typeparam>
        /// <returns> The result from the operation. </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static TResult ExecuteInTransaction<TState, TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [CanBeNull] TState state,
            [NotNull] Func<TState, TResult> operation,
            [NotNull] Func<TState, bool> verifySucceeded,
            IsolationLevel isolationLevel)
            => ExecutionStrategyExtensions.ExecuteInTransaction(
                strategy, state, operation, verifySucceeded, c => c.Database.BeginTransaction(isolationLevel));

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result. Allows to check whether
        ///     the transaction has been rolled back if an error occurs during commit.
        /// </summary>
        /// <param name="strategy"> The strategy that will be used for the execution. </param>
        /// <param name="state"> The state that will be passed to the operation. </param>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="verifySucceeded">
        ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
        ///     transaction was being committed.
        /// </param>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="cancellationToken">
        ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
        ///     or that already completed successfully.
        /// </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <typeparam name="TResult"> The result type of the <see cref="Task{TResult}" /> returned by <paramref name="operation" />. </typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        /// <exception cref="RetryLimitExceededException">
        ///     The operation has not succeeded after the configured number of retries.
        /// </exception>
        public static Task<TResult> ExecuteInTransactionAsync<TState, TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [CanBeNull] TState state,
            [NotNull] Func<TState, CancellationToken, Task<TResult>> operation,
            [NotNull] Func<TState, CancellationToken, Task<bool>> verifySucceeded,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => ExecutionStrategyExtensions.ExecuteInTransactionAsync(
                strategy, state, operation, verifySucceeded, (c, ct) => c.Database.BeginTransactionAsync(isolationLevel, ct),
                cancellationToken);
    }
}
