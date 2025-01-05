// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Net;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

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
        DbContext context)
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
        ExecutionStrategyDependencies dependencies)
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
        DbContext context,
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
        ExecutionStrategyDependencies dependencies,
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
    public CosmosExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay)
        : base(context, maxRetryCount, maxRetryDelay)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosExecutionStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay)
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
        return exception switch
        {
            CosmosException cosmosException => IsTransient(cosmosException.StatusCode),
            HttpException httpException => IsTransient(httpException.Response.StatusCode),
            WebException webException => IsTransient(((HttpWebResponse)webException.Response!).StatusCode),
            _ => false
        };

        static bool IsTransient(HttpStatusCode statusCode)
            => statusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout
                or HttpStatusCode.Gone;
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
            : (CallOnWrappedException(lastException, GetDelayFromException)
                ?? baseDelay);
    }

    private static TimeSpan? GetDelayFromException(Exception exception)
    {
        switch (exception)
        {
            case CosmosException cosmosException:
                return cosmosException.RetryAfter;

            case HttpException httpException:
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

                return null;
            }

            case WebException webException:
            {
                var response = (HttpWebResponse)webException.Response!;

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

                return null;
            }

            default:
                return null;
        }

        static bool TryParseMsRetryAfter(string? delayString, out TimeSpan delay)
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

        static bool TryParseRetryAfter(string? delayString, out TimeSpan delay)
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
