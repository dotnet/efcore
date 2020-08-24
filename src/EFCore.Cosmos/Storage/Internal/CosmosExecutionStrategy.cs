// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosExecutionStrategy : ExecutionStrategy
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy([NotNull] DbContext context, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(context, maxRetryCount, maxRetryDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool ShouldRetryOn(Exception exception)
        {
            if (exception is CosmosException cosmosException)
            {
                return IsTransient(cosmosException.StatusCode);
            }

            if (exception is HttpException httpException)
            {
                return IsTransient(httpException.Response.StatusCode);
            }

            if (exception is WebException webException)
            {
                return IsTransient(((HttpWebResponse)webException.Response).StatusCode);
            }

            return false;

            static bool IsTransient(HttpStatusCode statusCode)
                => statusCode == HttpStatusCode.ServiceUnavailable
                    || statusCode == HttpStatusCode.TooManyRequests;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            return baseDelay == null
                ? null
                : CallOnWrappedException(lastException, GetDelayFromException)
                    ?? baseDelay;
        }

        private static TimeSpan? GetDelayFromException(Exception exception)
        {
            if (exception is CosmosException cosmosException)
            {
                return cosmosException.RetryAfter;
            }

            if (exception is HttpException httpException)
            {
                if (httpException.Response.Headers.TryGetValues("x-ms-retry-after-ms", out var values)
                    && TryParseMsRetryAfter(values.FirstOrDefault(), out var delay))
                {
                    return delay;
                }

                if (httpException.Response.Headers.TryGetValues("Retry-After", out values)
                    && TryParseRetryAfter(values.FirstOrDefault(), out delay))
                {
                    return delay;
                }
            }

            if (exception is WebException webException)
            {
                var response = (HttpWebResponse)webException.Response;

                var delayString = response.Headers.GetValues("x-ms-retry-after-ms")?.FirstOrDefault();
                if (TryParseMsRetryAfter(delayString, out var delay))
                {
                    return delay;
                }

                delayString = response.Headers.GetValues("Retry-After")?.FirstOrDefault();
                if (TryParseRetryAfter(delayString, out delay))
                {
                    return delay;
                }
            }

            return null;
            static bool TryParseMsRetryAfter(string delayString, out TimeSpan delay)
            {
                delay = default;
                if (delayString == null)
                {
                    return false;
                }

                if (int.TryParse(delayString, out var intDelay))
                {
                    delay = TimeSpan.FromMilliseconds(intDelay);
                    return true;
                }

                return false;
            }

            static bool TryParseRetryAfter(string delayString, out TimeSpan delay)
            {
                delay = default;
                if (delayString == null)
                {
                    return false;
                }

                if (int.TryParse(delayString, out var intDelay))
                {
                    delay = TimeSpan.FromSeconds(intDelay);
                    return true;
                }

                if (DateTimeOffset.TryParse(delayString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retryDate))
                {
                    delay = retryDate.Subtract(DateTimeOffset.Now);
                    delay = delay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : delay;
                    return true;
                }

                return false;
            }
        }
    }
}
