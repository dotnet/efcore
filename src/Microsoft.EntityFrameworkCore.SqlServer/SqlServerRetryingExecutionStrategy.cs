// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerRetryingExecutionStrategy : ExecutionStrategy
    {
        private readonly ICollection<int> _additionalErrorNumbers;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <remarks>
        ///     The default retry limit is 5, which means that the total amount of time spent before failing is 26 seconds plus the random factor.
        /// </remarks>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The required dependencies. </param>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The required dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay in milliseconds between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : this(new ExecutionStrategyContext(
                context, context.GetService<IDbContextServices>().LoggerFactory.CreateLogger<IExecutionStrategy>()),
                maxRetryCount, maxRetryDelay, errorNumbersToAdd)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The required dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay in milliseconds between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public SqlServerRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(context, maxRetryCount, maxRetryDelay)
        {
            _additionalErrorNumbers = errorNumbersToAdd;
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (_additionalErrorNumbers != null)
            {
                var sqlException = exception as SqlException;
                if (sqlException != null)
                {
                    foreach (SqlError err in sqlException.Errors)
                    {
                        if (_additionalErrorNumbers.Contains(err.Number))
                        {
                            return true;
                        }
                    }
                }
            }

            return SqlServerTransientExceptionDetector.ShouldRetryOn(exception);
        }

        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            if (baseDelay == null)
            {
                return null;
            }

            if (CallOnWrappedException(lastException, IsMemoryOptimizedError))
            {
                return TimeSpan.FromMilliseconds(baseDelay.Value.TotalSeconds);
            }

            return baseDelay;
        }

        private bool IsMemoryOptimizedError(Exception exception)
        {
            var sqlException = exception as SqlException;
            if (sqlException != null)
            {
                foreach (SqlError err in sqlException.Errors)
                {
                    switch (err.Number)
                    {
                        case 41301:
                        case 41302:
                        case 41305:
                        case 41325:
                        case 41839:
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
