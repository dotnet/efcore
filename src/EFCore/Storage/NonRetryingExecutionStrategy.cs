// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     An implementation of <see cref="IExecutionStrategy" /> that does no retries.
    /// </summary>
    public sealed class NonRetryingExecutionStrategy : IExecutionStrategy
    {
        private ExecutionStrategyDependencies Dependencies { get; }

        /// <summary>
        ///     Always returns false, since the <see cref="NonRetryingExecutionStrategy" /> does not perform retries.
        /// </summary>
        public bool RetriesOnFailure
            => false;

        /// <summary>
        ///     Constructs a new <see cref="NonRetryingExecutionStrategy" /> with the given service dependencies.
        /// </summary>
        /// <param name="dependencies"> Dependencies for this execution strategy. </param>
        public NonRetryingExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies)
            => Dependencies = dependencies;

        /// <inheritdoc />
        public TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
            => operation(Dependencies.CurrentContext.Context, state);

        /// <inheritdoc />
        public Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState,
                CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken = default)
            => operation(Dependencies.CurrentContext.Context, state, cancellationToken);
    }
}
