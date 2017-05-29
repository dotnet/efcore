// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Combines <see cref="ILogger" /> and <see cref="DiagnosticSource" />
    ///         for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
    ///         for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
    ///     </para>
    ///     <para>
    ///         Also intercepts messages such that warnings
    ///         can be either logged or thrown, and such that a decision as to whether to log
    ///         sensitive data or not can be made.
    ///     </para>
    /// </summary>
    public interface IDiagnosticsLogger<TLoggerCategory>
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        /// <summary>
        ///     Checks if the given <paramref name="logLevel" /> is enabled or the given event, and,
        ///     if so, whether the event should be logged or thrown.
        /// </summary>
        /// <param name="eventId"> The event ID that will be logged, if enabled. </param>
        /// <param name="logLevel"> The logging level to which the event will be logged.</param>
        /// <returns> One of Log, Throw, or Ignore. </returns>
        WarningBehavior GetLogBehavior(EventId eventId, LogLevel logLevel);

        /// <summary>
        ///     Entity Framework logging options.
        /// </summary>
        ILoggingOptions Options { get; }

        /// <summary>
        ///     Gets a value indicating whether sensitive information should be written
        ///     to the underlying logger. This also has the side effect of writing a warning
        ///     to the log the first time sensitive data is logged.
        /// </summary>
        bool ShouldLogSensitiveData();

        /// <summary>
        ///     The underlying <see cref="ILogger" />.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        ///     The <see cref="DiagnosticSource" />.
        /// </summary>
        DiagnosticSource DiagnosticSource { get; }
    }
}
