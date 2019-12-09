// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     An implementation of <see cref="ISimpleLogger" /> that will log filtered events to a given sink
    ///     with some control over formatting.
    /// </summary>
    public class SimpleLogger : ISimpleLogger
    {
        /// <summary>
        ///     Creates a new <see cref="SimpleLogger" /> instance.
        /// </summary>
        /// <param name="sink"> The sink to which messages will be logged. </param>
        /// <param name="filter"> A delegate that returns true to log the message; false to filter it out. </param>
        /// <param name="formatOptions"> Formatting options for log messages. </param>
        public SimpleLogger(
            [NotNull] Action<string> sink,
            [NotNull] Func<EventId, LogLevel, bool> filter,
            SimpleLoggerFormatOptions formatOptions)
        {
            FormatOptions = formatOptions;
            Sink = sink;
            Filter = filter;
        }

        /// <summary>
        ///     The <see cref="FormatOptions" /> to used when formatting messages to log.
        /// </summary>
        public SimpleLoggerFormatOptions FormatOptions { get; } // Intentionally not virtual for perf

        /// <summary>
        ///     The sink to which messages are being logged.
        /// </summary>
        public Action<string> Sink { get; } // Intentionally not virtual for perf

        /// <summary>
        ///     A delegate that returns true to log the message; false to filter it out.
        /// </summary>
        public Func<EventId, LogLevel, bool> Filter { get; } // Intentionally not virtual for perf

        /// <inheritdoc />
        public virtual void Log(EventData eventData)
        {
            Check.NotNull(eventData, nameof(eventData));

            var message = eventData.ToString();
            var logLevel = eventData.LogLevel;
            var eventId = eventData.EventId;

            if (FormatOptions != SimpleLoggerFormatOptions.None)
            {
                var messageBuilder = new StringBuilder();

                if ((FormatOptions & SimpleLoggerFormatOptions.Level) != 0)
                {
                    messageBuilder.Append(GetLogLevelString(logLevel));
                }

                if ((FormatOptions & SimpleLoggerFormatOptions.LocalTime) != 0)
                {
                    messageBuilder.Append(DateTime.Now.ToShortDateString()).Append(DateTime.Now.ToString(" HH:mm:ss.fff "));
                }

                if ((FormatOptions & SimpleLoggerFormatOptions.UtcTime) != 0)
                {
                    messageBuilder.Append(DateTime.UtcNow.ToString("o")).Append(' ');
                }

                if ((FormatOptions & SimpleLoggerFormatOptions.Id) != 0)
                {
                    messageBuilder.Append(eventData.EventIdCode).Append('[').Append(eventId.Id).Append("] ");
                }

                if ((FormatOptions & SimpleLoggerFormatOptions.Category) != 0)
                {
                    var lastDot = eventId.Name.LastIndexOf('.');
                    if (lastDot > 0)
                    {
                        messageBuilder.Append('(').Append(eventId.Name.Substring(0, lastDot)).Append(") ");
                    }
                }

                const string padding = "      ";
                var preambleLength = messageBuilder.Length;

                if (FormatOptions == SimpleLoggerFormatOptions.SingleLine) // Single line ONLY
                {
                    message = messageBuilder
                        .Append(message)
                        .Replace(Environment.NewLine, "")
                        .ToString();
                }
                else
                {
                    message = (FormatOptions & SimpleLoggerFormatOptions.SingleLine) != 0
                        ? messageBuilder
                            .Append("-> ")
                            .Append(message)
                            .Replace(Environment.NewLine, "", preambleLength, messageBuilder.Length - preambleLength)
                            .ToString()
                        : messageBuilder
                            .AppendLine()
                            .Append(message)
                            .Replace(Environment.NewLine, Environment.NewLine + padding, preambleLength, messageBuilder.Length - preambleLength)
                            .ToString();
                }
            }

            Sink(message);
        }

        /// <inheritdoc />
        public virtual bool ShouldLog(EventId eventId, LogLevel logLevel)
            => Filter(eventId, logLevel);

        private static string GetLogLevelString(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => "trce: ",
                LogLevel.Debug => "dbug: ",
                LogLevel.Information => "info: ",
                LogLevel.Warning => "warn: ",
                LogLevel.Error => "fail: ",
                LogLevel.Critical => "crit: ",
                _ => "none",
            };
    }
}
