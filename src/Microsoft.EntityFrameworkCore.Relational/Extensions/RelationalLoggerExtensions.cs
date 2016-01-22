// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.Storage
{
    internal static class RelationalLoggerExtensions
    {
        public static void LogCommandExecuted(
            [NotNull] this ISensitiveDataLogger logger, [NotNull] DbCommand command, long? elapsedMilliseconds)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(command, nameof(command));

            logger.LogInformation(
                RelationalLoggingEventId.ExecutedCommand,
                () =>
                    {
                        var logParameterValues
                            = command.Parameters.Count > 0
                              && logger.LogSensitiveData;

                        return new DbCommandLogData(
                            command.CommandText.TrimEnd(),
                            command.CommandType,
                            command.CommandTimeout,
                            command.Parameters
                                .Cast<DbParameter>()
                                .ToDictionary(p => p.ParameterName, p => logParameterValues ? p.Value : "?"),
                            elapsedMilliseconds);
                    },
                state =>
                    RelationalStrings.RelationalLoggerExecutedCommand(
                        string.Format($"{elapsedMilliseconds:N0}"),
                        state.Parameters
                            .Select(kv => $"{kv.Key}='{Convert.ToString(kv.Value, CultureInfo.InvariantCulture)}'")
                            .Join(),
                        state.CommandType,
                        state.CommandTimeout,
                        Environment.NewLine,
                        state.CommandText));
        }

        private static void LogInformation<TState>(
            this ILogger logger, RelationalLoggingEventId eventId, Func<TState> state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log(LogLevel.Information, (int)eventId, state(), null, (s, _) => formatter((TState)s));
            }
        }

        public static void LogDebug(
            this ILogger logger, RelationalLoggingEventId eventId, Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
            }
        }

        public static void LogDebug<TState>(
            this ILogger logger, RelationalLoggingEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, state, null, (s, __) => formatter((TState)s));
            }
        }
    }
}
