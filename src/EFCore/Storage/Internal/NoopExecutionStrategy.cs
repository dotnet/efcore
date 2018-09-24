// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NoopExecutionStrategy : IExecutionStrategy
    {
        private ExecutionStrategyDependencies Dependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RetriesOnFailure => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NoopExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies)
            => Dependencies = dependencies;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TResult Execute<TState, TResult>(
            TState state, Func<DbContext, TState, TResult> operation, Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
            => operation(Dependencies.CurrentDbContext.Context, state);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<TResult> ExecuteAsync<TState, TResult>(
            TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState,
                CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded, CancellationToken cancellationToken = default)
            => operation(Dependencies.CurrentDbContext.Context, state, cancellationToken);
    }
}
