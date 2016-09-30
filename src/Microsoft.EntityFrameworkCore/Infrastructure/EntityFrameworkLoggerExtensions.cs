// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Extension method for using Entity Framework Core features of <see cref="ILogger" />.
    /// </summary>
    public static class EntityFrameworkLoggerExtensions
    {
        /// <summary>
        /// Writes a log entry that will also be reported in tools via <see cref="IReportedLogData"/> .
        /// </summary>
        /// <typeparam name="TState"> The type of the entry. </typeparam>
        /// <param name="logger"> The <see cref="ILogger"/> to write to. </param>
        /// <param name="logLevel"> Entry will be written on this level. </param>
        /// <param name="eventId"> Id of the event. </param>
        /// <param name="state"> The entry to be written. Can be also an object. </param>
        /// <param name="exception"> The exception related to this entry. </param>
        /// <param name="formatter"> Function to create a string message of the state and exception. </param>
        public static void LogReported<TState>(
            [NotNull] this ILogger logger,
            LogLevel logLevel,
            EventId eventId,
            [CanBeNull] TState state,
            [CanBeNull] Exception exception,
            [CanBeNull] Func<TState, Exception, string> formatter)
        {
            Check.NotNull(logger, nameof(logger));

            var reportedState = new ReportedLogData<TState>(state, exception, formatter);

            logger.Log(
                logLevel,
                eventId,
                reportedState,
                exception,
                (_, __) => reportedState.ToString());
        }
    }
}
