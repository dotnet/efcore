// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosExecutionStrategy : ExecutionStrategy
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <remarks>
        ///     The default retry limit is 6, which means that the total amount of time spent before failing is about a minute.
        /// </remarks>
        public CosmosExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        public CosmosExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public CosmosExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public CosmosExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        public CosmosExecutionStrategy([NotNull] DbContext context, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(context, maxRetryCount, maxRetryDelay)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="CosmosExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        public CosmosExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (exception is HttpException httpException)
            {
                var statusCode = (int)httpException.Response.StatusCode;
                return statusCode == 429
                       || statusCode == 503;
            }

            if (exception is WebException webException)
            {
                var statusCode = (int)((HttpWebResponse)webException.Response).StatusCode;
                return statusCode == 429
                       || statusCode == 503;
            }

            return false;
        }

        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            if (baseDelay == null)
            {
                return null;
            }

            return CallOnWrappedException(lastException, GetDelayFromException)
                   ?? baseDelay;
        }

        private static TimeSpan? GetDelayFromException(Exception exception)
        {
            if (exception is HttpException httpException)
            {
                if (httpException.Response.Headers.TryGetValues("x-ms-retry-after-ms", out var values))
                {
                    var delayString = values.Single();
                    return TimeSpan.FromMilliseconds(int.Parse(delayString));
                }

                var retryDate = httpException.Response.Headers.RetryAfter.Date;
                if (retryDate != null)
                {
                    var delay = retryDate.Value.Subtract(DateTime.Now);
                    return delay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : delay;
                }

                return httpException.Response.Headers.RetryAfter.Delta;
            }

            if (exception is WebException webException)
            {
                var response = (HttpWebResponse)webException.Response;

                var delayString = response.Headers.GetValues("x-ms-retry-after-ms")?.Single();
                if (delayString != null)
                {
                    return TimeSpan.FromMilliseconds(int.Parse(delayString));
                }

                delayString = response.Headers.GetValues("Retry-After")?.Single();
                if (delayString != null)
                {
                    if (int.TryParse(delayString, out var intDelay))
                    {
                        return TimeSpan.FromSeconds(intDelay);
                    }

                    if (DateTime.TryParse(delayString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retryDate))
                    {
                        var delay = retryDate.Subtract(DateTime.Now);
                        return delay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : delay;
                    }
                }
            }

            return null;
        }
    }
}
