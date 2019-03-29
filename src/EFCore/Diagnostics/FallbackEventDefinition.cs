// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Defines metadata for an event with more than six parameters such that it has to have
    ///     special handling.
    /// </summary>
    public class FallbackEventDefinition : EventDefinitionBase
    {
        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="loggingOptions"> Logging options. </param>
        /// <param name="eventId"> The <see cref="EventId" />. </param>
        /// <param name="level"> The <see cref="LogLevel" /> at which the event will be logged. </param>
        /// <param name="eventIdCode"> A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings"/>. </param>
        /// <param name="messageFormat"> The parameterized message definition. </param>
        public FallbackEventDefinition(
            [NotNull] ILoggingOptions loggingOptions,
            EventId eventId,
            LogLevel level,
            [NotNull] string eventIdCode,
            [NotNull] string messageFormat)
            : base(loggingOptions, eventId, level, eventIdCode)
        {
            Check.NotEmpty(messageFormat, nameof(messageFormat));

            MessageFormat = messageFormat;
        }

        /// <summary>
        ///     Generates the message that would be logged without logging it.
        ///     Typically used for throwing an exception in warning-as-error cases.
        /// </summary>
        /// <param name="logAction"> A delegate that will log the message to an <see cref="ILogger" />. </param>
        /// <returns> The message string. </returns>
        public virtual string GenerateMessage([NotNull] Action<ILogger> logAction)
        {
            Check.NotNull(logAction, nameof(logAction));

            var extractor = new MessageExtractingLogger();
            logAction(extractor);
            return extractor.Message;
        }

        /// <summary>
        ///     Logs the event, or throws if the event has been configured to be treated as an error.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event should be logged. </param>
        /// <param name="warningBehavior"> Whether the event should be logged, thrown as an exception or ignored. </param>
        /// <param name="logAction"> A delegate that will log the message to an <see cref="ILogger" />. </param>
        public virtual void Log<TLoggerCategory>(
            [NotNull] IDiagnosticsLogger<TLoggerCategory> logger,
            WarningBehavior warningBehavior,
            [NotNull] Action<ILogger> logAction)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            switch (warningBehavior)
            {
                case WarningBehavior.Log:
                    logAction(logger.Logger);
                    break;
                case WarningBehavior.Throw:
                    throw WarningAsError(GenerateMessage(logAction));
            }
        }

        /// <summary>
        ///     The parameterized message definition.
        /// </summary>
        public virtual string MessageFormat { get; }
    }
}
