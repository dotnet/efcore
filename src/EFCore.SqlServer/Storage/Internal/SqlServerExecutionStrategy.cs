// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqlServerExecutionStrategy : IExecutionStrategy
    {
        private ExecutionStrategyDependencies Dependencies { get; }

        public SqlServerExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        public virtual bool RetriesOnFailure => false;

        public virtual TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
        {
            try
            {
                return operation(Dependencies.CurrentDbContext.Context, state);
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.CallOnWrappedException(ex, SqlServerTransientExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException(SqlServerStrings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

        public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken)
        {
            try
            {
                return await operation(Dependencies.CurrentDbContext.Context, state, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.CallOnWrappedException(ex, SqlServerTransientExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException(SqlServerStrings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }
    }
}
