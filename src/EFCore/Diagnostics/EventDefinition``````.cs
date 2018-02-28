// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Defines metadata for an event with three parameters and a cached delegate to log the
    ///     event with reduced allocations.
    /// </summary>
    public class EventDefinition<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : EventDefinitionBase
    {
        private readonly Action<ILogger, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Exception> _logAction;

        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="eventId"> The <see cref="EventId" />. </param>
        /// <param name="level"> The <see cref="LogLevel" /> at which the event will be logged. </param>
        /// <param name="logAction"> A cached delegate for logging the event. </param>
        public EventDefinition(
            EventId eventId,
            LogLevel level,
            [NotNull] Action<ILogger, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Exception> logAction)
            : this(eventId, level, null, logAction)
        {
        }

        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="eventId"> The <see cref="EventId" />. </param>
        /// <param name="level"> The <see cref="LogLevel" /> at which the event will be logged. </param>
        /// <param name="eventIdCode"> A string representing the code that should be passed to ConfigureWanings. </param>
        /// <param name="logAction"> A cached delegate for logging the event. </param>
        public EventDefinition(
            EventId eventId,
            LogLevel level,
            [CanBeNull] string eventIdCode,
            [NotNull] Action<ILogger, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Exception> logAction)
            : base(eventId, level, eventIdCode)
        {
            Check.NotNull(logAction, nameof(logAction));

            _logAction = logAction;
        }

        /// <summary>
        ///     Generates the message that would be logged without logging it.
        ///     Typically used for throwing an exception in warning-as-error cases.
        /// </summary>
        /// <param name="arg1"> The first message argument. </param>
        /// <param name="arg2"> The second message argument. </param>
        /// <param name="arg3"> The third message argument. </param>
        /// <param name="arg4"> The fourth message argument. </param>
        /// <param name="arg5"> The fifth message argument. </param>
        /// <param name="arg6"> The sixth message argument. </param>
        /// <param name="exception"> Optional exception associated with this event. </param>
        /// <returns> The message string. </returns>
        public virtual string GenerateMessage(
            [CanBeNull] TParam1 arg1,
            [CanBeNull] TParam2 arg2,
            [CanBeNull] TParam3 arg3,
            [CanBeNull] TParam4 arg4,
            [CanBeNull] TParam5 arg5,
            [CanBeNull] TParam6 arg6,
            [CanBeNull] Exception exception = null)
        {
            var extractor = new MessageExtractingLogger();
            _logAction(extractor, arg1, arg2, arg3, arg4, arg5, arg6, exception);
            return extractor.Message;
        }

        /// <summary>
        ///     Logs the event, or throws if the event has been configured to be treated as an error.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event should be logged. </param>
        /// <param name="arg1"> The first message argument. </param>
        /// <param name="arg2"> The second message argument. </param>
        /// <param name="arg3"> The third message argument. </param>
        /// <param name="arg4"> The fourth message argument. </param>
        /// <param name="arg5"> The fifth message argument. </param>
        /// <param name="arg6"> The sixth message argument. </param>
        /// <param name="exception"> Optional exception associated with the event. </param>
        [Obsolete("Use the other overload")]
        public virtual void Log<TLoggerCategory>(
            [NotNull] IDiagnosticsLogger<TLoggerCategory> logger,
            [CanBeNull] TParam1 arg1,
            [CanBeNull] TParam2 arg2,
            [CanBeNull] TParam3 arg3,
            [CanBeNull] TParam4 arg4,
            [CanBeNull] TParam5 arg5,
            [CanBeNull] TParam6 arg6,
            [CanBeNull] Exception exception = null)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => Log(logger, GetLogBehavior(logger), arg1, arg2, arg3, arg4, arg5, arg6, exception);

        /// <summary>
        ///     Logs the event, or throws if the event has been configured to be treated as an error.
        /// </summary>
        /// <typeparam name="TLoggerCategory"> The <see cref="DbLoggerCategory" />. </typeparam>
        /// <param name="logger"> The logger to which the event should be logged. </param>
        /// <param name="warningBehavior"> Whether the event should be logged, thrown as an exception or ignored. </param>
        /// <param name="arg1"> The first message argument. </param>
        /// <param name="arg2"> The second message argument. </param>
        /// <param name="arg3"> The third message argument. </param>
        /// <param name="arg4"> The fourth message argument. </param>
        /// <param name="arg5"> The fifth message argument. </param>
        /// <param name="arg6"> The sixth message argument. </param>
        /// <param name="exception"> Optional exception associated with the event. </param>
        public virtual void Log<TLoggerCategory>(
            [NotNull] IDiagnosticsLogger<TLoggerCategory> logger,
            WarningBehavior warningBehavior,
            [CanBeNull] TParam1 arg1,
            [CanBeNull] TParam2 arg2,
            [CanBeNull] TParam3 arg3,
            [CanBeNull] TParam4 arg4,
            [CanBeNull] TParam5 arg5,
            [CanBeNull] TParam6 arg6,
            [CanBeNull] Exception exception = null)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            switch (warningBehavior)
            {
                case WarningBehavior.Log:
                    _logAction(logger.Logger, arg1, arg2, arg3, arg4, arg5, arg6, exception);
                    break;
                case WarningBehavior.Throw:
                    throw WarningAsError(GenerateMessage(arg1, arg2, arg3, arg4, arg5, arg6, exception));
            }
        }
    }
}
