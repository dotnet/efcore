// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IExecutionStrategy" />
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///     for more information and examples.
/// </remarks>
public static class ExecutionStrategyExtensions
{
    /// <summary>
    ///     Executes the specified operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">A delegate representing an executable operation that doesn't return any results.</param>
    public static void Execute(
        this IExecutionStrategy strategy,
        Action operation)
    {
        Check.NotNull(operation, nameof(operation));

        strategy.Execute(
            operation, operationScoped =>
            {
                operationScoped();
                return true;
            });
    }

    /// <summary>
    ///     Executes the specified operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
    /// </param>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    public static TResult Execute<TResult>(
        this IExecutionStrategy strategy,
        Func<TResult> operation)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.Execute(operation, operationScoped => operationScoped());
    }

    /// <summary>
    ///     Executes the specified operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">A delegate representing an executable operation that doesn't return any results.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    public static void Execute<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Action<TState> operation)
    {
        Check.NotNull(operation, nameof(operation));

        strategy.Execute(
            new { operation, state }, s =>
            {
                s.operation(s.state);
                return true;
            });
    }

    /// <summary>
    ///     Executes the specified asynchronous operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">A function that returns a started task.</param>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    public static Task ExecuteAsync(
        this IExecutionStrategy strategy,
        Func<Task> operation)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(
            operation, async (operationScoped, _) =>
            {
                await operationScoped().ConfigureAwait(false);
                return true;
            }, default);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">A function that returns a started task.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task ExecuteAsync(
        this IExecutionStrategy strategy,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(
            operation, async (operationScoped, ct) =>
            {
                await operationScoped(ct).ConfigureAwait(false);
                return true;
            }, cancellationToken);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">
    ///     A function that returns a started task of type <typeparamref name="TResult" />.
    /// </param>
    /// <typeparam name="TResult">
    ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
    /// </typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    public static Task<TResult> ExecuteAsync<TResult>(
        this IExecutionStrategy strategy,
        Func<Task<TResult>> operation)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(operation, (operationScoped, _) => operationScoped(), default);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">
    ///     A function that returns a started task of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TResult">
    ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
    /// </typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<TResult> ExecuteAsync<TResult>(
        this IExecutionStrategy strategy,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(operation, (operationScoped, ct) => operationScoped(ct), cancellationToken);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">A function that returns a started task.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    public static Task ExecuteAsync<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, Task> operation)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(
            new { operation, state }, async (t, _) =>
            {
                await t.operation(t.state).ConfigureAwait(false);
                return true;
            }, default);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">A function that returns a started task.</param>
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
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task ExecuteAsync<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(
            new { operation, state }, async (t, ct) =>
            {
                await t.operation(t.state, ct).ConfigureAwait(false);
                return true;
            }, cancellationToken);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
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
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">
    ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
    /// </typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    public static Task<TResult> ExecuteAsync<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, Task<TResult>> operation)
    {
        Check.NotNull(operation, nameof(operation));

        return strategy.ExecuteAsync(
            new { operation, state }, (t, _) => t.operation(t.state), default);
    }

    /// <summary>
    ///     Executes the specified operation and returns the result.
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
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    public static TResult Execute<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, TResult> operation)
        => strategy.Execute(state, operation, verifySucceeded: null);

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
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
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">
    ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
    /// </typeparam>
    /// <returns>
    ///     A task that will run to completion if the original task completes successfully (either the
    ///     first time or after retrying transient failures). If the task fails with a non-transient error or
    ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<TResult> ExecuteAsync<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
        => strategy.ExecuteAsync(state, operation, verifySucceeded: null, cancellationToken);

    /// <summary>
    ///     Executes the specified operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="verifySucceeded">A delegate that tests whether the operation succeeded even though an exception was thrown.</param>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static TResult Execute<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, TResult> operation,
        Func<TState, ExecutionResult<TResult>>? verifySucceeded)
        => strategy.Execute(
            state,
            (_, s) => operation(s),
            verifySucceeded == null ? null : (_, s) => verifySucceeded(s));

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
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
    /// <param name="verifySucceeded">A delegate that tests whether the operation succeeded even though an exception was thrown.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
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
    public static Task<TResult> ExecuteAsync<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        Func<TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
        => strategy.ExecuteAsync(
            state,
            (_, s, ct) => operation(s, ct),
            verifySucceeded == null
                ? null
                : (_, s, ct) => verifySucceeded(s, ct), cancellationToken);

    /// <summary>
    ///     Executes the specified operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
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
        Func<bool> verifySucceeded)
        => strategy.ExecuteInTransaction<object?>(null, _ => operation(), _ => verifySucceeded());

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
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
        Func<Task<bool>> verifySucceeded)
        => strategy.ExecuteInTransactionAsync<object?>(null, (_, _) => operation(), (_, _) => verifySucceeded());

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
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
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync<object?>(
            null, (_, ct) => operation(ct), (_, ct) => verifySucceeded(ct), cancellationToken);

    /// <summary>
    ///     Executes the specified operation in a transaction and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
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
        Func<bool> verifySucceeded)
        => strategy.ExecuteInTransaction<object?, TResult>(null, _ => operation(), _ => verifySucceeded());

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction and returns the result. Allows to check whether
    ///     the transaction has been rolled back if an error occurs during commit.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="strategy">The strategy that will be used for the execution.</param>
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
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync<object?, TResult>(
            null, (_, ct) => operation(ct), (_, ct) => verifySucceeded(ct), cancellationToken);

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
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static void ExecuteInTransaction<TState>(
        this IExecutionStrategy strategy,
        TState state,
        Action<TState> operation,
        Func<TState, bool> verifySucceeded)
        => strategy.ExecuteInTransaction(
            state, s =>
            {
                operation(s);
                return true;
            }, verifySucceeded);

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
        CancellationToken cancellationToken = default)
        => strategy.ExecuteInTransactionAsync(
            state, async (s, ct) =>
            {
                await operation(s, ct).ConfigureAwait(false);
                return true;
            }, verifySucceeded, cancellationToken);

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
        Func<TState, bool> verifySucceeded)
        => ExecuteInTransaction(
            strategy,
            state,
            operation, verifySucceeded, c => c.Database.BeginTransaction());

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction and returns the result. Allows to check whether
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
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
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
    public static Task<TResult> ExecuteInTransactionAsync<TState, TResult>(
        this IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        Func<TState, CancellationToken, Task<bool>> verifySucceeded,
        CancellationToken cancellationToken = default)
        => ExecuteInTransactionAsync(
            strategy,
            state,
            operation,
            verifySucceeded, (c, ct) => c.Database.BeginTransactionAsync(ct), cancellationToken);

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
    /// <param name="beginTransaction">A delegate that begins a transaction using the given context.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public static TResult ExecuteInTransaction<TState, TResult>(
        IExecutionStrategy strategy,
        TState state,
        Func<TState, TResult> operation,
        Func<TState, bool> verifySucceeded,
        Func<DbContext, IDbContextTransaction> beginTransaction)
        => strategy.Execute(
            new ExecutionState<TState, TResult>(
                Check.NotNull(operation, nameof(operation)), Check.NotNull(verifySucceeded, nameof(verifySucceeded)), state),
            (c, s) =>
            {
                Check.NotNull(beginTransaction, nameof(beginTransaction));
                using (var transaction = beginTransaction(c))
                {
                    s.CommitFailed = false;
                    s.Result = s.Operation(s.State);
                    s.CommitFailed = true;
                    transaction.Commit();
                }

                return s.Result;
            }, (_, s) => new ExecutionResult<TResult>(s.CommitFailed && s.VerifySucceeded(s.State), s.Result));

    /// <summary>
    ///     Executes the specified asynchronous operation in a transaction and returns the result. Allows to check whether
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
    /// <param name="beginTransaction">A delegate that begins a transaction using the given context.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token used to cancel the retry operation, but not operations that are already in flight
    ///     or that already completed successfully.
    /// </param>
    /// <typeparam name="TState">The type of the state.</typeparam>
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
    public static Task<TResult> ExecuteInTransactionAsync<TState, TResult>(
        IExecutionStrategy strategy,
        TState state,
        Func<TState, CancellationToken, Task<TResult>> operation,
        Func<TState, CancellationToken, Task<bool>> verifySucceeded,
        Func<DbContext, CancellationToken, Task<IDbContextTransaction>> beginTransaction,
        CancellationToken cancellationToken = default)
        => strategy.ExecuteAsync(
            new ExecutionStateAsync<TState, TResult>(
                Check.NotNull(operation, nameof(operation)), Check.NotNull(verifySucceeded, nameof(verifySucceeded)), state),
            async (c, s, ct) =>
            {
                Check.NotNull(beginTransaction, nameof(beginTransaction));
                var transaction = await beginTransaction(c, cancellationToken).ConfigureAwait(false);
                await using var _ = transaction.ConfigureAwait(false);

                s.CommitFailed = false;
                s.Result = await s.Operation(s.State, ct).ConfigureAwait(false);
                s.CommitFailed = true;
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                return s.Result;
            }, async (_, s, ct) => new ExecutionResult<TResult>(
                s.CommitFailed && await s.VerifySucceeded(s.State, ct).ConfigureAwait(false),
                s.Result), cancellationToken);

    private sealed class ExecutionState<TState, TResult>
    {
        public ExecutionState(
            Func<TState, TResult> operation,
            Func<TState, bool> verifySucceeded,
            TState state)
        {
            Operation = operation;
            VerifySucceeded = verifySucceeded;
            State = state;
            Result = default!;
        }

        public Func<TState, TResult> Operation { get; }
        public Func<TState, bool> VerifySucceeded { get; }
        public TState State { get; }
        public TResult Result { get; set; }
        public bool CommitFailed { get; set; }
    }

    private sealed class ExecutionStateAsync<TState, TResult>
    {
        public ExecutionStateAsync(
            Func<TState, CancellationToken, Task<TResult>> operation,
            Func<TState, CancellationToken, Task<bool>> verifySucceeded,
            TState state)
        {
            Operation = operation;
            VerifySucceeded = verifySucceeded;
            State = state;
            Result = default!;
        }

        public Func<TState, CancellationToken, Task<TResult>> Operation { get; }
        public Func<TState, CancellationToken, Task<bool>> VerifySucceeded { get; }
        public TState State { get; }
        public TResult Result { get; set; }
        public bool CommitFailed { get; set; }
    }
}
