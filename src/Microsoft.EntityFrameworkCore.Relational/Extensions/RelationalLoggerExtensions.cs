// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage
{
    internal static class RelationalLoggerExtensions
    {
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

                var logData = new DbCommandLogData(
                    command.CommandText.TrimEnd(),
                    command.CommandType,
                    command.CommandTimeout,
                    command.Parameters
                        .Cast<DbParameter>()
                        .ToDictionary(p => p.ParameterName, p => logParameterValues ? p.Value : "?"),
                    DeriveTimespan(startTimestamp, currentTimestamp));

                logger.Log(
                    LogLevel.Information,
                    (int)RelationalLoggingEventId.ExecutedCommand,
                    logData,
                    null,
                    (state, _) =>
                        {
                            var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                            return RelationalStrings.RelationalLoggerExecutedCommand(
                                string.Format($"{elapsedMilliseconds:N0}"),
                                state.Parameters
                                    .Select(kv => $"{kv.Key}='{FormatParameterValue(kv.Value)}'")
                                    .Join(),
                                state.CommandType,
                                state.CommandTimeout,
                                Environment.NewLine,
                                state.CommandText);
                        });
            }
        }

        public static object FormatParameterValue(object parameterValue)
        {
            if (parameterValue.GetType() != typeof(byte[]))
            {
                return Convert.ToString(parameterValue, CultureInfo.InvariantCulture);
            }
            var stringValueBuilder = new StringBuilder();
            var buffer = (byte[])parameterValue;
            stringValueBuilder.Append("0x");

            for (var i = 0; i < buffer.Length; i++)
            {
                if (i > 31)
                {
                    stringValueBuilder.Append("...");
                    break;
                }
                stringValueBuilder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringValueBuilder.ToString();
        }

        public static void LogInformation<TState>(
            this ILogger logger, RelationalLoggingEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log(LogLevel.Information, (int)eventId, state, null, (s, _) => formatter(s));
            }
        }

        public static void LogDebug(
            this ILogger logger, RelationalLoggingEventId eventId, Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
            }
        }

        public static void LogDebug<TState>(
            this ILogger logger, RelationalLoggingEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, state, null, (s, __) => formatter(s));
            }
        }

        private static long DeriveTimespan(long startTimestamp, long currentTimestamp)
            => (currentTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    }
}
