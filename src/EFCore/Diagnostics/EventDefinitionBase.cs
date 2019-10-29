// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Base class for event definitions.
    /// </summary>
    public abstract class EventDefinitionBase
    {
        private readonly WarningBehavior _warningBehavior;

        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="loggingOptions"> Logging options. </param>
        /// <param name="eventId"> The <see cref="Microsoft.Extensions.Logging.EventId" />. </param>
        /// <param name="level"> The <see cref="LogLevel" /> at which the event will be logged. </param>
        /// <param name="eventIdCode">
        ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
        /// </param>
        protected EventDefinitionBase(
            [NotNull] ILoggingOptions loggingOptions,
            EventId eventId,
            LogLevel level,
            [NotNull] string eventIdCode)
        {
            Check.NotNull(loggingOptions, nameof(loggingOptions));
            Check.NotEmpty(eventIdCode, nameof(eventIdCode));

            EventId = eventId;
            EventIdCode = eventIdCode;

            var warningsConfiguration = loggingOptions.WarningsConfiguration;

            if (warningsConfiguration != null)
            {
                var levelOverride = warningsConfiguration.GetLevel(eventId);
                if (levelOverride.HasValue)
                {
                    level = levelOverride.Value;
                }

                var behavior = warningsConfiguration.GetBehavior(eventId);
                _warningBehavior = behavior
                    ?? (level == LogLevel.Warning
                        && warningsConfiguration.DefaultBehavior == WarningBehavior.Throw
                            ? WarningBehavior.Throw
                            : WarningBehavior.Log);
            }
            else
            {
                _warningBehavior = WarningBehavior.Log;
            }

            Level = level;
        }

        /// <summary>
        ///     The <see cref="EventId" />.
        /// </summary>
        public virtual EventId EventId { [DebuggerStepThrough] get; }

        /// <summary>
        ///     The <see cref="LogLevel" /> at which the event will be logged.
        /// </summary>
        public virtual LogLevel Level { [DebuggerStepThrough] get; }

        /// <summary>
        ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" /> to suppress this event
        ///     as an error.
        /// </summary>
        public virtual string EventIdCode { get; }

        /// <summary>
        ///     Returns a warning-as-error exception wrapping the given message for this event.
        /// </summary>
        /// <param name="message"> The message to wrap. </param>
        protected virtual Exception WarningAsError([NotNull] string message)
            => new InvalidOperationException(
                CoreStrings.WarningAsErrorTemplate(EventId.ToString(), message, EventIdCode));

        /// <summary>
        ///     Gets the log behavior for this event. This determines whether it should be logged, thrown as an exception or ignored.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event would be logged. </param>
        /// <returns> Whether the event should be logged, thrown as an exception or ignored. </returns>
        public virtual WarningBehavior GetLogBehavior<TLoggerCategory>(
            [NotNull] IDiagnosticsLogger<TLoggerCategory> logger)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => _warningBehavior == WarningBehavior.Log
                ? logger.Logger.IsEnabled(Level) ? WarningBehavior.Log : WarningBehavior.Ignore
                : _warningBehavior;

        internal sealed class MessageExtractingLogger : ILogger
        {
            private string _message;

            public string Message
            {
                get => _message ?? throw new InvalidOperationException();
                private set => _message = value;
            }

            void ILogger.Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                [CanBeNull] TState state,
                [CanBeNull] Exception exception,
                [NotNull] Func<TState, Exception, string> formatter)
            {
                Message = formatter(state, exception);
            }

            bool ILogger.IsEnabled(LogLevel logLevel) => true;

            IDisposable ILogger.BeginScope<TState>([CanBeNull] TState state) => throw new NotImplementedException();
        }
    }
}
