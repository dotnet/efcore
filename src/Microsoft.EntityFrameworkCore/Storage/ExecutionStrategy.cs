// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
#if NET451
using System.Runtime.Remoting.Messaging;

#endif
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
        protected static readonly int DefaultMaxRetryCount = 5;

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
        /// <param name="context"> The required dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay in milliseconds between retries. </param>
        protected ExecutionStrategy(
            [CanBeNull] ExecutionStrategyContext context,
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

            Context = context?.Context;
            Logger = context?.Logger;
            MaxRetryCount = maxRetryCount;
            MaxRetryDelay = maxRetryDelay;
        }

        /// <summary>
        ///     The list of exceptions that caused the operation to be retried so far.
        /// </summary>
        protected virtual List<Exception> ExceptionsEncountered { get; } = new List<Exception>();

        /// <summary>
        ///     A pseudo-random number generater that can be used to vary the delay between retries.
        /// </summary>
        protected virtual Random Random { get; } = new Random();

        /// <summary>
        ///     The maximum number of retry attempts.
        /// </summary>
        protected virtual int MaxRetryCount { get; }

        /// <summary>
        ///     The maximum delay in milliseconds between retries.
        /// </summary>
        protected virtual TimeSpan MaxRetryDelay { get; }

        /// <summary>
        ///     The context on which the operations will be invoked.
        /// </summary>
        protected virtual DbContext Context { get; }

        /// <summary>
        ///     The logger for this <see cref="ExecutionStrategy" />.
        /// </summary>
        protected virtual ILogger<IExecutionStrategy> Logger { get; }

#if NET451
        private const string ContextName = "ExecutionStrategySuspended";

        /// <summary>
        ///     Indicates whether the strategy is suspended. The strategy is typically suspending while executing to avoid
        ///     recursive execution from nested operations.
        /// </summary>
        protected static bool Suspended
        {
            get { return (bool?)CallContext.LogicalGetData(ContextName) ?? false; }
            set { CallContext.LogicalSetData(ContextName, value); }
        }
#else
        private readonly static AsyncLocal<bool?> _suspended = new AsyncLocal<bool?>();

        /// <summary>
        ///     Indicates whether the strategy is suspended. The strategy is typically suspending while executing to avoid
        ///     recursive execution from nested operations.
        /// </summary>
        protected static bool Suspended
        {
            get { return _suspended.Value ?? false; }
            set { _suspended.Value = value; }
        }
#endif

        /// <summary>
        ///     Indicates whether this <see cref="IExecutionStrategy" /> might retry the execution after a failure.
        /// </summary>
        public virtual bool RetriesOnFailure => !Suspended;

        /// <summary>
        ///     Executes the specified operation and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The return type of <paramref name="operation" />.
        /// </typeparam>
        /// <param name="operation">
        ///     A delegate representing an executable operation that returns the result of type <typeparamref name="TResult" />.
        /// </param>
        /// <param name="state">The state that will be passed to the operation.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <returns>The result from the operation.</returns>
        public virtual TResult Execute<TState, TResult>(Func<TState, TResult> operation, TState state)
        {
            if (Suspended)
            {
                return operation(state);
            }

            OnFirstExecution();

            while (true)
            {
                TimeSpan? delay;
                try
                {
                    Suspended = true;
                    return operation(state);
                }
                catch (Exception ex)
                {
                    if (!UnwrapAndHandleException(ex, ShouldRetryOn))
                    {
                        throw;
                    }

                    ExceptionsEncountered.Add(ex);

                    delay = GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(CoreStrings.RetryLimitExceeded(MaxRetryCount, GetType().Name), ex);
                    }

                    OnRetry();
                }
                finally
                {
                    Suspended = false;
                }

                using (var waitEvent = new ManualResetEventSlim(false))
                {
                    waitEvent.WaitHandle.WaitOne(delay.Value);
                }
            }
        }

        /// <summary>
        ///     Executes the specified asynchronous operation and returns the result.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type of the <see cref="Task{T}" /> returned by <paramref name="operation" />.
        /// </typeparam>
        /// <param name="operation">
        ///     A function that returns a started task of type <typeparamref name="TResult" />.
        /// </param>
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
        public virtual Task<TResult> ExecuteAsync<TState, TResult>(
            Func<TState, CancellationToken, Task<TResult>> operation,
            TState state,
            CancellationToken cancellationToken = default(CancellationToken)
        ) => Suspended ? operation(state, cancellationToken) : ExecuteAsyncImplementation(operation, state, cancellationToken);

        private async Task<TResult> ExecuteAsyncImplementation<TState, TResult>(
            Func<TState, CancellationToken, Task<TResult>> operation,
            TState state,
            CancellationToken cancellationToken)
        {
            OnFirstExecution();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TimeSpan? delay;
                try
                {
                    Suspended = true;
                    return await operation(state, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!UnwrapAndHandleException(ex, ShouldRetryOn))
                    {
                        throw;
                    }

                    ExceptionsEncountered.Add(ex);

                    delay = GetNextDelay(ex);
                    if (delay == null)
                    {
                        throw new RetryLimitExceededException(CoreStrings.RetryLimitExceeded(MaxRetryCount, GetType().Name), ex);
                    }

                    OnRetry();
                }
                finally
                {
                    Suspended = false;
                }

                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        /// <summary>
        ///     Method called before the first operation execution
        /// </summary>
        protected virtual void OnFirstExecution()
        {
            if (Context?.Database.CurrentTransaction != null)
            {
                throw new InvalidOperationException(CoreStrings.ExecutionStrategyExistingTransaction(
                    GetType().Name,
                    nameof(DbContext) + "." + nameof(DbContext.Database) + "." + nameof(DatabaseFacade.CreateExecutionStrategy) + "()"));
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
        /// <param name="lastException">The exception thrown during the last execution attempt.</param>
        /// <returns>
        ///     Returns the delay indicating how long to wait for before the next execution attempt if the operation should be retried;
        ///     <c>null</c> otherwise
        /// </returns>
        protected internal virtual TimeSpan? GetNextDelay([NotNull] Exception lastException)
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
        ///     Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="exception">The exception object to be verified.</param>
        /// <returns>
        ///     <c>true</c> if the specified exception is considered as transient, otherwise <c>false</c>.
        /// </returns>
        protected internal abstract bool ShouldRetryOn([NotNull] Exception exception);

        /// <summary>
        ///     Recursively gets InnerException from <paramref name="exception" /> as long as it's an
        ///     exception created by Entity Framework and passes it to <paramref name="exceptionHandler" />
        /// </summary>
        /// <typeparam name="T">The type of the unwrapped exception.</typeparam>
        /// <param name="exception"> The exception to be unwrapped. </param>
        /// <param name="exceptionHandler"> A delegate that will be called with the unwrapped exception. </param>
        /// <returns>
        ///     The result from <paramref name="exceptionHandler" />.
        /// </returns>
        public static T UnwrapAndHandleException<T>([NotNull] Exception exception, [NotNull] Func<Exception, T> exceptionHandler)
        {
            var dbUpdateException = exception as DbUpdateException;
            return dbUpdateException != null
                ? UnwrapAndHandleException(dbUpdateException.InnerException, exceptionHandler)
                : exceptionHandler(exception);
        }
    }
}
