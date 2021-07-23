// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     The base class for <see cref="IExecutionStrategy" /> implementations.
    /// </summary>
    public abstract class ExecutionStrategy : IExecutionStrategy
    {
        /// <summary>
        ///     The default number of retry attempts.
        /// </summary>
        protected static readonly int DefaultMaxRetryCount = 6;

        /// <summary>
        ///     The default maximum time delay between retries, must be nonnegative.
        /// </summary>
        protected static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The default maximum random factor, must not be lesser than 1.
        /// </summary>
        private const double DefaultRandomFactor = 1.1;

        /// <summary>
        ///     The default base for the exponential function used to compute the delay between retries, must be positive.
        /// </summary>
        private const double DefaultExponentialBase = 2;

        /// <summary>
        ///     The default coefficient for the exponential function used to compute the delay between retries, must be nonnegative.
        /// </summary>
        private static readonly TimeSpan _defaultCoefficient = TimeSpan.FromSeconds(1);

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        protected ExecutionStrategy(
            DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay)
            : this(
                context.GetService<ExecutionStrategyDependencies>(),
                maxRetryCount,
                maxRetryDelay)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        protected ExecutionStrategy(
            ExecutionStrategyDependencies dependencies,
            int maxRetryCount,
            TimeSpan maxRetryDelay)
        {
            if (maxRetryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount));
            }

            if (maxRetryDelay.TotalMilliseconds < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryDelay));
            }

            Dependencies = dependencies;
            MaxRetryCount = maxRetryCount;
            MaxRetryDelay = maxRetryDelay;
        }

        /// <summary>
        ///     The list of exceptions that caused the operation to be retried so far.
        /// </summary>
        protected virtual List<Exception> ExceptionsEncountered { get; } = new();

        /// <summary>
        ///     A pseudo-random number generator that can be used to vary the delay between retries.
        /// </summary>
        protected virtual Random Random { get; } = new();

        /// <summary>
        ///     The maximum number of retry attempts.
        /// </summary>
        protected virtual int MaxRetryCount { get; }

        /// <summary>
        ///     The maximum delay between retries.
        /// </summary>
        protected virtual TimeSpan MaxRetryDelay { get; }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ExecutionStrategyDependencies Dependencies { get; }

        private static readonly AsyncLocal<bool?> _suspended = new();

        /// <summary>
        ///     Indicates whether the strategy is suspended. The strategy is typically suspending while executing to avoid
        ///     recursive execution from nested operations.
        /// </summary>
        protected static bool Suspended
        {
            get => _suspended.Value ?? false;
            set => _suspended.Value = value;
        }

        /// <summary>
        ///     Indicates whether this <see cref="IExecutionStrategy" /> might retry the execution after a failure.
        /// </summary>
        public virtual bool RetriesOnFailure
            => !Suspended && MaxRetryCount > 0;

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
        public virtual TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        {
            Check.NotNull(operation, nameof(operation));

            if (Suspended)
            {
                return operation(Dependencies.CurrentContext.Context, state);
            }

            OnFirstExecution();

            // In order to avoid infinite recursive generics, wrap operation with ExecutionResult
            return ExecuteImplementation(
                (context, state) => new ExecutionResult<TResult>(true, operation(context, state)),
                verifySucceeded,
                state).Result;
        }

        private ExecutionResult<TResult> ExecuteImplementation<TState, TResult>(
            Func<DbContext, TState, ExecutionResult<TResult>> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded,
            TState state)
        {
            while (true)
            {
                try
                {
                    Suspended = true;
                    var result = operation(Dependencies.CurrentContext.Context, state);
                    Suspended = false;
                    return result;
                }
                catch (Exception ex)
                {
                    Suspended = false;

                    EntityFrameworkEventSource.Log.ExecutionStrategyOperationFailure();

                    if (verifySucceeded != null
                        && CallOnWrappedException(ex, ShouldVerifySuccessOn))
                    {
                        var result = ExecuteImplementation(verifySucceeded, null, state);
                        if (result.IsSuccessful)
                        {
                            return result;
                        }
                    }

                    if (!CallOnWrappedException(ex, ShouldRetryOn))
                    {
                        throw;
                    }

                    ExceptionsEncountered.Add(ex);

                    var delay = GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(CoreStrings.RetryLimitExceeded(MaxRetryCount, GetType().Name), ex);
                    }

                    Dependencies.Logger.ExecutionStrategyRetrying(ExceptionsEncountered, delay.Value, async: true);

                    OnRetry();

                    using var waitEvent = new ManualResetEventSlim(false);
                    waitEvent.WaitHandle.WaitOne(delay.Value);
                }
            }
        }

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
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(operation, nameof(operation));

            if (Suspended)
            {
                return await operation(Dependencies.CurrentContext.Context, state, cancellationToken).ConfigureAwait(false);
            }

            OnFirstExecution();

            // In order to avoid infinite recursive generics, wrap operation with ExecutionResult
            var result = await ExecuteImplementationAsync(
                async (context, state, cancellationToken) => new ExecutionResult<TResult>(true, await operation(context, state, cancellationToken).ConfigureAwait(false)),
                verifySucceeded,
                state,
                cancellationToken).ConfigureAwait(false);
            return result.Result;
        }

        private async Task<ExecutionResult<TResult>> ExecuteImplementationAsync<TState, TResult>(
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            TState state,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    Suspended = true;
                    var result = await operation(Dependencies.CurrentContext.Context, state, cancellationToken)
                        .ConfigureAwait(false);
                    Suspended = false;
                    return result;
                }
                catch (Exception ex)
                {
                    Suspended = false;

                    EntityFrameworkEventSource.Log.ExecutionStrategyOperationFailure();

                    if (verifySucceeded != null
                        && CallOnWrappedException(ex, ShouldVerifySuccessOn))
                    {
                        var result = await ExecuteImplementationAsync(verifySucceeded, null, state, cancellationToken)
                            .ConfigureAwait(false);
                        if (result.IsSuccessful)
                        {
                            return result;
                        }
                    }

                    if (!CallOnWrappedException(ex, ShouldRetryOn))
                    {
                        throw;
                    }

                    ExceptionsEncountered.Add(ex);

                    var delay = GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(CoreStrings.RetryLimitExceeded(MaxRetryCount, GetType().Name), ex);
                    }

                    Dependencies.Logger.ExecutionStrategyRetrying(ExceptionsEncountered, delay.Value, async: true);

                    OnRetry();

                    await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        ///     Method called before the first operation execution
        /// </summary>
        protected virtual void OnFirstExecution()
        {
            if (RetriesOnFailure &&
                (Dependencies.CurrentContext.Context.Database.CurrentTransaction is not null
                 || Dependencies.CurrentContext.Context.Database.GetEnlistedTransaction() is not null
                 || (((IDatabaseFacadeDependenciesAccessor)Dependencies.CurrentContext.Context.Database).Dependencies.TransactionManager as
                     ITransactionEnlistmentManager)?.CurrentAmbientTransaction is not null))
            {
                throw new InvalidOperationException(
                    CoreStrings.ExecutionStrategyExistingTransaction(
                        GetType().Name,
                        nameof(DbContext)
                        + "."
                        + nameof(DbContext.Database)
                        + "."
                        + nameof(DatabaseFacade.CreateExecutionStrategy)
                        + "()"));
            }

            ExceptionsEncountered.Clear();
        }

        /// <summary>
        ///     Method called before retrying the operation execution
        /// </summary>
        protected virtual void OnRetry()
        {
        }

        /// <summary>
        ///     Determines whether the operation should be retried and the delay before the next attempt.
        /// </summary>
        /// <param name="lastException"> The exception thrown during the last execution attempt. </param>
        /// <returns>
        ///     Returns the delay indicating how long to wait for before the next execution attempt if the operation should be retried;
        ///     <see langword="null" /> otherwise
        /// </returns>
        protected virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            var currentRetryCount = ExceptionsEncountered.Count - 1;
            if (currentRetryCount < MaxRetryCount)
            {
                var delta = (Math.Pow(DefaultExponentialBase, currentRetryCount) - 1.0)
                    * (1.0 + Random.NextDouble() * (DefaultRandomFactor - 1.0));

                var delay = Math.Min(
                    _defaultCoefficient.TotalMilliseconds * delta,
                    MaxRetryDelay.TotalMilliseconds);

                return TimeSpan.FromMilliseconds(delay);
            }

            return null;
        }

        /// <summary>
        ///     Determines whether the specified exception could be thrown after a successful execution.
        /// </summary>
        /// <param name="exception"> The exception object to be verified. </param>
        /// <returns>
        ///     <see langword="true" /> if the specified exception could be thrown after a successful execution, otherwise <see langword="false" />.
        /// </returns>
        protected internal virtual bool ShouldVerifySuccessOn(Exception? exception)
            => ShouldRetryOn(exception);

        /// <summary>
        ///     Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="exception"> The exception object to be verified. </param>
        /// <returns>
        ///     <see langword="true" /> if the specified exception is considered as transient, otherwise <see langword="false" />.
        /// </returns>
        protected internal abstract bool ShouldRetryOn(Exception? exception);

        /// <summary>
        ///     Recursively gets InnerException from <paramref name="exception" /> as long as it is an
        ///     exception created by Entity Framework and calls <paramref name="exceptionHandler" /> on the innermost one.
        /// </summary>
        /// <param name="exception"> The exception to be unwrapped. </param>
        /// <param name="exceptionHandler"> A delegate that will be called with the unwrapped exception. </param>
        /// <typeparam name="TResult"> The return type of <paramref name="exceptionHandler" />. </typeparam>
        /// <returns>
        ///     The result from <paramref name="exceptionHandler" />.
        /// </returns>
        public static TResult CallOnWrappedException<TResult>(
            Exception? exception,
            Func<Exception?, TResult> exceptionHandler)
            => exception is DbUpdateException dbUpdateException
                ? CallOnWrappedException(dbUpdateException.InnerException, exceptionHandler)
                : exceptionHandler(exception);
    }
}
