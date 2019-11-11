// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Defines metadata for an event with no parameters and a cached delegate to log the
    ///     event with reduced allocations.
    /// </summary>
    public class EventDefinition : EventDefinitionBase
    {
        private readonly Action<ILogger, Exception> _logAction;

        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="loggingOptions"> Logging options. </param>
        /// <param name="eventId"> The <see cref="EventId" />. </param>
        /// <param name="level"> The <see cref="LogLevel" /> at which the event will be logged. </param>
        /// <param name="logActionFunc"> Function to create a cached delegate for logging the event. </param>
        /// <param name="eventIdCode">
        ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
        /// </param>
        public EventDefinition(
            [NotNull] ILoggingOptions loggingOptions,
            EventId eventId,
            LogLevel level,
            [NotNull] string eventIdCode,
            [NotNull] Func<LogLevel, Action<ILogger, Exception>> logActionFunc)
            : base(loggingOptions, eventId, level, eventIdCode)
        {
            Check.NotNull(logActionFunc, nameof(logActionFunc));

            _logAction = logActionFunc(Level);
        }

        /// <summary>
        ///     Generates the message that would be logged without logging it.
        ///     Typically used for throwing an exception in warning-as-error cases.
        /// </summary>
        /// <returns> The message string. </returns>
        public virtual string GenerateMessage()
        {
            var extractor = new MessageExtractingLogger();
            _logAction(extractor, null);
            return extractor.Message;
        }

        /// <summary>
        ///     Logs the event, or throws if the event has been configured to be treated as an error.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event should be logged. </param>
        /// <param name="exception"> Optional exception associated with the event. </param>
        public virtual void Log<TLoggerCategory>(
            [NotNull] IDiagnosticsLogger<TLoggerCategory> logger,
            [CanBeNull] Exception exception = null)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            switch (WarningBehavior)
            {
                case WarningBehavior.Log:
                    _logAction(logger.Logger, exception);
                    break;
                case WarningBehavior.Throw:
                    throw WarningAsError(GenerateMessage());
            }
        }
    }
}
