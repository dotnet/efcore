// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public static class SqlServerLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogDebug(
            [NotNull] this ILogger logger,
            SqlServerEventId eventId,
            [NotNull] Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogDebug<TState>(
            [NotNull] this ILogger logger,
            SqlServerEventId eventId,
            [CanBeNull] TState state,
            [NotNull] Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, state, null, (s, __) => formatter(s));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogWarning(
            [NotNull] this ILogger logger,
            SqlServerEventId eventId,
            [NotNull] Func<string> formatter)
        {
            // Always call Log for Warnings because Warnings as Errors should work even
            // if LogLevel.Warning is not enabled.
            logger.Log<object>(LogLevel.Warning, (int)eventId, eventId, null, (_, __) => formatter());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogInformation(
            [NotNull] this ILogger logger,
            SqlServerEventId eventId,
            [NotNull] Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log<object>(LogLevel.Information, (int)eventId, null, null, (_, __) => formatter());
            }
        }
    }
}
