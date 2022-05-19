// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Extension methods for <see cref="IExecutionStrategy" /> that can only be used with a
///     relational database provider.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///     for more information and examples.
/// </remarks>
public static class RelationalExecutionStrategyExtensions
{
    /// <summary>
    ///     Executes the specified operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
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
        this IExecutionStrategy strategy,
        Action operation,
        Func<bool> verifySucceeded,
        IsolationLevel isolationLevel)
        => strategy.ExecuteInTransaction<object?>(null, _ => operation(), _ => verifySucceeded(), isolationLevel);

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
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
        this IExecutionStrategy strategy,
        Func<Task> operation,
        Func<Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel)
        => strategy.ExecuteInTransactionAsync<object?>(null, (_, _) => operation(), (_, _) => verifySucceeded(), isolationLevel);

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
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
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task ExecuteInTransactionAsync(
        this IExecutionStrategy strategy,
        Func<CancellationToken, Task> operation,
        Func<CancellationToken, Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync<object?>(
            null, (_, ct) => operation(ct), (_, ct) => verifySucceeded(ct), isolationLevel, cancellationToken);

    /// <summary>
    ///     Executes the specified operation in a transaction and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="verifySucceeded">
    ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
    ///     transaction was being committed.
    /// </param>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static TResult ExecuteInTransaction<TResult>(
        this IExecutionStrategy strategy,
        Func<TResult> operation,
        Func<bool> verifySucceeded,
        IsolationLevel isolationLevel)
        => strategy.ExecuteInTransaction<object?, TResult>(null, _ => operation(), _ => verifySucceeded(), isolationLevel);

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
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
    /// <typeparam name="TResult">The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.</typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<TResult> ExecuteInTransactionAsync<TResult>(
        this IExecutionStrategy strategy,
        Func<CancellationToken, Task<TResult>> operation,
        Func<CancellationToken, Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync<object?, TResult>(
            null, (_, ct) => operation(ct), (_, ct) => verifySucceeded(ct), isolationLevel, cancellationToken);

    /// <summary>
    ///     Executes the specified operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation.
    /// </param>
    /// <param name="verifySucceeded">
    ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
    ///     transaction was being committed.
    /// </param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static void ExecuteInTransaction<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Action<TState> operation,
        Func<TState, bool> verifySucceeded,
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">
    ///     A function that returns a started task.
    /// </param>
    /// <param name="verifySucceeded">
    ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
    ///     transaction was being committed.
    /// </param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task ExecuteInTransactionAsync<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task> operation,
        Func<TState, CancellationToken, Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync(
            state,
            async (s, ct) =>
            {
                await operation(s, ct).ConfigureAwait(false);
                return true;
            }, verifySucceeded, isolationLevel, cancellationToken);

    /// <summary>
    ///     Executes the specified operation in a transaction and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="verifySucceeded">
    ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
    ///     transaction was being committed.
    /// </param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static TResult ExecuteInTransaction<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, TResult> operation,
        Func<TState, bool> verifySucceeded,
        IsolationLevel isolationLevel)
        => ExecutionStrategyExtensions.ExecuteInTransaction(
            strategy, state, operation, verifySucceeded, c => c.Database.BeginTransaction(isolationLevel));

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">
    ///     A function that returns a started task of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="verifySucceeded">
    ///     A delegate that tests whether the operation succeeded even though an exception was thrown when the
    ///     transaction was being committed.
    /// </param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The result type of the <see cref="Task{TResult}" /> returned by <paramref name="operation" />.</typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<TResult> ExecuteInTransactionAsync<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        Func<TState, CancellationToken, Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => ExecutionStrategyExtensions.ExecuteInTransactionAsync(
            strategy, state, operation, verifySucceeded, (c, ct) => c.Database.BeginTransactionAsync(isolationLevel, ct),
            cancellationToken);
}
