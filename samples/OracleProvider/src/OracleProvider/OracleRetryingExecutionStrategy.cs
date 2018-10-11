// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleRetryingExecutionStrategy : ExecutionStrategy
    {
        private readonly ICollection<int> _additionalErrorNumbers;

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <remarks>
        ///     The default retry limit is 6, which means that the total amount of time spent before failing is about a minute.
        /// </remarks>
        public OracleRetryingExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        public OracleRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public OracleRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public OracleRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public OracleRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(
                context,
                maxRetryCount,
                maxRetryDelay)
        {
            _additionalErrorNumbers = errorNumbersToAdd;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="OracleRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public OracleRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
            _additionalErrorNumbers = errorNumbersToAdd;
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (_additionalErrorNumbers != null)
            {
                if (exception is OracleException sqlException)
                {
                    foreach (OracleError err in sqlException.Errors)
                    {
                        if (_additionalErrorNumbers.Contains(err.Number))
                        {
                            return true;
                        }
                    }
                }
            }

            return OracleTransientExceptionDetector.ShouldRetryOn(exception);
        }
    }
}
