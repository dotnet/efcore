// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A specialed logger that intercepts messages such that warnings
    ///     can be either logged or thrown, and such that a decision as to whether to log
    ///     sensitive data or not can be made.
    /// </summary>
    /// <typeparam name="TLoggerCategory"> The category of this logger. </typeparam>
    public interface IInterceptingLogger<TLoggerCategory>
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        /// <summary>
        ///     Writes a log entry.
        /// </summary>
        /// <param name="logLevel"> The log level to use. </param>
        /// <param name="eventId"> Id of the event. </param>
        /// <param name="state"> State associated with the event. </param>
        /// <param name="exception"> The exception related to this event, or null if none. </param>
        /// <param name="formatter"> Function to create a string message for the event. </param>
        void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            [CanBeNull] TState state,
            [CanBeNull] Exception exception,
            [NotNull] Func<TState, Exception, string> formatter);

        /// <summary>
        ///     Checks if the given <paramref name="logLevel" /> is enabled or the given event.
        /// </summary>
        /// <param name="eventId"> The event ID that will be logged, if enabled. </param>
        /// <param name="logLevel"> The logging level to which the event will be logged.</param>
        /// <returns> <c>true</c> if enabled. </returns>
        bool IsEnabled(EventId eventId, LogLevel logLevel);

        /// <summary>
        ///     Begins a logical operation scope.
        /// </summary>
        /// <param name="state"> The identifier for the scope. </param>
        /// <returns> An IDisposable that ends the logical operation scope on dispose. </returns>
        IDisposable BeginScope<TState>([CanBeNull] TState state);

        /// <summary>
        ///     Entity Framework logging options.
        /// </summary>
        ILoggingOptions Options { get; }

        /// <summary>
        ///     Gets a value indicating whether sensitive information should be written
        ///     to the underlying logger. This also has the side effect of writing a warning
        ///     to the log the first time sensitive data is logged.
        /// </summary>
        bool ShouldLogSensitiveData([NotNull] IDiagnosticsLogger<TLoggerCategory> diagnostics);
    }
}
