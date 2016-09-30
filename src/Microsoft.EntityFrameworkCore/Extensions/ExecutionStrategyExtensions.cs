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
        public static void Execute(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Action operation
        ) => strategy.Execute(operationScoped =>
            {
                operationScoped();
                return true;
            }, operation);

        /// <summary>
        ///     Executes the specified operation and returns the result.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">
        ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
        /// </param>
        /// <typeparam name="TResult">The return type of <paramref name="operation" />.</typeparam>
        /// <returns>The result from the operation.</returns>
        public static TResult Execute<TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<TResult> operation
        ) => strategy.Execute(operationScoped => operationScoped(), operation);

        /// <summary>
        ///     Executes the specified operation.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">A delegate representing an executable operation that doesn't return any results.</param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        public static void Execute<TState>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Action<TState> operation,
            [CanBeNull] TState state
        ) => strategy.Execute(s =>
            {
                s.operation(s.state);
                return true;
            }, new { operation, state });

        /// <summary>
        ///     Executes the specified asynchronous operation.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">A function that returns a started task.</param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        public static Task ExecuteAsync(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<Task> operation
        ) => strategy.ExecuteAsync(async (operationScoped, ct) =>
            {
                await operationScoped();
                return true;
            }, operation, default(CancellationToken));

        /// <summary>
        ///     Executes the specified asynchronous operation.
        /// </summary>
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
        public static Task ExecuteAsync(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken
        ) => strategy.ExecuteAsync(async (operationScoped, ct) =>
            {
                await operationScoped(ct);
                return true;
            }, operation, cancellationToken);

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
        /// </typeparam>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        public static Task<TResult> ExecuteAsync<TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<Task<TResult>> operation
        ) => strategy.ExecuteAsync((operationScoped, ct) => operationScoped(), operation, default(CancellationToken));

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
        /// </typeparam>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
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
        public static Task<TResult> ExecuteAsync<TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken
        ) => strategy.ExecuteAsync((operationScoped, ct) => operationScoped(ct), operation, cancellationToken);

        /// <summary>
        ///     Executes the specified asynchronous operation.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <param name="operation">A function that returns a started task.</param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        public static Task ExecuteAsync<TState>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<TState, Task> operation,
            [CanBeNull] TState state
        ) => strategy.ExecuteAsync(async (t, ct) =>
            {
                await t.operation(t.state);
                return true;
            }, new { operation, state }, default(CancellationToken));

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
            [NotNull] Func<TState, CancellationToken, Task> operation,
            [CanBeNull] TState state,
            CancellationToken cancellationToken
        ) => strategy.ExecuteAsync(async (t, ct) =>
            {
                await t.operation(t.state, ct);
                return true;
            }, new { operation, state }, cancellationToken);

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result.
        /// </summary>
        /// <param name="strategy">The strategy that will be used for the execution.</param>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
        /// </typeparam>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <returns>
        ///     A task that will run to completion if the original task completes successfully (either the
        ///     first time or after retrying transient failures). If the task fails with a non-transient error or
        ///     the retry limit is reached, the returned task will become faulted and the exception must be observed.
        /// </returns>
        public static Task<TResult> ExecuteAsync<TState, TResult>(
            [NotNull] this IExecutionStrategy strategy,
            [NotNull] Func<TState, Task<TResult>> operation,
            [CanBeNull] TState state
        ) => strategy.ExecuteAsync((t, ct) => t.operation(t.state), new { operation, state }, default(CancellationToken));
    }
}
