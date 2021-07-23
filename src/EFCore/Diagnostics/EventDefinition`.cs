// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Defines metadata for an event with one parameter and a cached delegate to log the
    ///     event with reduced allocations.
    /// </summary>
    public class EventDefinition<TParam> : EventDefinitionBase
    {
        private readonly Action<ILogger, TParam, Exception?> _logAction;

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
            ILoggingOptions loggingOptions,
            EventId eventId,
            LogLevel level,
            string eventIdCode,
            Func<LogLevel, Action<ILogger, TParam, Exception?>> logActionFunc)
            : base(loggingOptions, eventId, level, eventIdCode)
        {
            Check.NotNull(logActionFunc, nameof(logActionFunc));

            _logAction = logActionFunc(Level);
        }

        /// <summary>
        ///     Generates the message that would be logged without logging it.
        ///     Typically used for throwing an exception in warning-as-error cases.
        /// </summary>
        /// <param name="arg"> The message argument. </param>
        /// <returns> The message string. </returns>
        public virtual string GenerateMessage(
            TParam arg)
        {
            var extractor = new MessageExtractingLogger();
            _logAction(extractor, arg, null);
            return extractor.Message;
        }

        /// <summary>
        ///     Logs the event, or throws if the event has been configured to be treated as an error.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event should be logged. </param>
        /// <param name="arg"> Message argument. </param>
        public virtual void Log<TLoggerCategory>(
            IDiagnosticsLogger<TLoggerCategory> logger,
            TParam arg)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            switch (WarningBehavior)
            {
                case WarningBehavior.Log:
                    _logAction(logger.Logger, arg, null);
                    break;
                case WarningBehavior.Throw:
                    throw WarningAsError(GenerateMessage(arg));
            }
        }
    }
}
