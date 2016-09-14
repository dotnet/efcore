// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqlServerExecutionStrategy : IExecutionStrategy
    {
        private SqlServerExecutionStrategy()
        {
        }

        public static SqlServerExecutionStrategy Instance => new SqlServerExecutionStrategy();

        public virtual bool RetriesOnFailure => false;

        public virtual TResult Execute<TState, TResult>(Func<TState, TResult> operation, TState state)
        {
            try
            {
                return operation(state);
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException(SqlServerStrings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }

        public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
            Func<TState, CancellationToken, Task<TResult>> operation,
            TState state,
            CancellationToken cancellationToken)
        {
            try
            {
                return await operation(state, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.UnwrapAndHandleException(ex, SqlAzureRetriableExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException(SqlServerStrings.TransientExceptionDetected, ex);
                }

                throw;
            }
        }
    }
}
