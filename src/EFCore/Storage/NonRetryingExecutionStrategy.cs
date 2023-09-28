// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     An implementation of <see cref="IExecutionStrategy" /> that does no retries.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///         for more information and examples.
///     </para>
/// </remarks>
public sealed class NonRetryingExecutionStrategy : IExecutionStrategy
{
    /// <summary>
    ///     Constructs a new <see cref="NonRetryingExecutionStrategy" /> with the given service dependencies.
    /// </summary>
    /// <param name="dependencies">Dependencies for this execution strategy.</param>
    public NonRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Constructs a new <see cref="NonRetryingExecutionStrategy" /> with the given context.
    /// </summary>
    /// <param name="context">The context on which the operations will be invoked.</param>
    public NonRetryingExecutionStrategy(DbContext context)
    {
        Dependencies = context.GetService<ExecutionStrategyDependencies>();
    }

    private ExecutionStrategyDependencies Dependencies { get; }

    /// <summary>
    ///     Always returns false, since the <see cref="NonRetryingExecutionStrategy" /> does not perform retries.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    public bool RetriesOnFailure
        => false;

    /// <summary>
    ///     Executes the specified operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="state">The state that will be passed to the operation.</param>
    /// <param name="operation">
    ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
    /// </param>
    /// <param name="verifySucceeded">A delegate that tests whether the operation succeeded even though an exception was thrown.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
    /// <returns>The result from the operation.</returns>
    /// <exception cref="RetryLimitExceededException">
    ///     The operation has not succeeded after the configured number of retries.
    /// </exception>
    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        => operation(Dependencies.CurrentContext.Context, state);

    /// <summary>
    ///     Executes the specified asynchronous operation and returns the result.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
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
    public Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState,
            CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
        => operation(Dependencies.CurrentContext.Context, state, cancellationToken);
}
