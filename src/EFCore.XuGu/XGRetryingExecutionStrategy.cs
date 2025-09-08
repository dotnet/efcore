// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;
using System.Text.RegularExpressions;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class XGRetryingExecutionStrategy : ExecutionStrategy
    {
        private readonly ICollection<int> _additionalErrorNumbers;

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <remarks>
        ///     The default retry limit is 6, which means that the total amount of time spent before failing is about a minute.
        /// </remarks>
        public XGRetryingExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        public XGRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public XGRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: new List<int>())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public XGRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: new List<int>())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public XGRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(context,
                maxRetryCount,
                maxRetryDelay)
            => _additionalErrorNumbers = errorNumbersToAdd;

        /// <summary>
        ///     Creates a new instance of <see cref="XGRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public XGRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(dependencies, maxRetryCount, maxRetryDelay)
            => _additionalErrorNumbers = errorNumbersToAdd;

        protected override bool ShouldRetryOn(Exception exception)
        {
            Match match = Regex.Match(exception.Message, @"\[E(\d+)\]");
            return match.Success && _additionalErrorNumbers?.Contains(Convert.ToInt32(match.Groups[1].Value)) == true
               || XGTransientExceptionDetector.ShouldRetryOn(exception);
        }
    }
}
