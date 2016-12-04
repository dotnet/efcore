// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class RelationalLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogCommandExecuted(
            [NotNull] this ISensitiveDataLogger logger,
            [NotNull] DbCommand command,
            long startTimestamp,
            long currentTimestamp)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(command, nameof(command));

            if (logger.IsEnabled(LogLevel.Information))
            {
                var logParameterValues
                    = command.Parameters.Count > 0
                      && logger.LogSensitiveData;

#pragma warning disable 618
                var logData = new DbCommandLogData(
#pragma warning restore 618
                    command.CommandText.TrimEnd(),
                    command.CommandType,
                    command.CommandTimeout,
                    command.Parameters
                        .Cast<DbParameter>()
                        .Select(
                            p => new DbParameterLogData(
                                p.ParameterName,
                                logParameterValues ? p.Value : "?",
                                logParameterValues,
                                p.Direction,
                                p.DbType,
                                p.IsNullable,
                                p.Size,
                                p.Precision,
                                p.Scale))
                        .ToList(),
                    DeriveTimespan(startTimestamp, currentTimestamp));

                logger.Log(
                    LogLevel.Information,
                    (int)RelationalEventId.ExecutedCommand,
                    logData,
                    null,
                    (state, _) =>
                        {
                            var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                            return RelationalStrings.RelationalLoggerExecutedCommand(
                                string.Format(CultureInfo.InvariantCulture, "{0:N0}", elapsedMilliseconds),
                                state.Parameters
                                    // Interpolation okay here because value is always a string.
                                    .Select(p => $"{p.Name}={p.FormatParameter()}")
                                    .Join(),
                                state.CommandType,
                                state.CommandTimeout,
                                Environment.NewLine,
                                state.CommandText);
                        });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogDebug(
            [NotNull] this ILogger logger,
            RelationalEventId eventId,
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
            RelationalEventId eventId,
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
            RelationalEventId eventId,
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
            RelationalEventId eventId,
            [NotNull] Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log<object>(LogLevel.Information, (int)eventId, null, null, (_, __) => formatter());
            }
        }

        private static readonly double TimestampToMilliseconds = (double)TimeSpan.TicksPerSecond / (Stopwatch.Frequency * TimeSpan.TicksPerMillisecond);

        private static long DeriveTimespan(long startTimestamp, long currentTimestamp)
            => (long)((currentTimestamp - startTimestamp) * TimestampToMilliseconds);
    }
}
