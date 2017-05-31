// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     Base class for event definitions.
    /// </summary>
    public abstract class EventDefinitionBase
    {
        /// <summary>
        ///     Creates an event definition instance.
        /// </summary>
        /// <param name="eventId"> The <see cref="EventId" />. </param>
        /// <param name="level"> The <see cref="Microsoft.Extensions.Logging.LogLevel" /> at which the event will be logged. </param>
        protected EventDefinitionBase(EventId eventId, LogLevel level)
        {
            EventId = eventId;
            Level = level;
        }

        /// <summary>
        ///     The <see cref="EventId" />.
        /// </summary>
        public virtual EventId EventId { [DebuggerStepThrough] get; }

        /// <summary>
        ///     The <see cref="Microsoft.Extensions.Logging.LogLevel" /> at which the event will be logged.
        /// </summary>
        public virtual LogLevel Level { [DebuggerStepThrough] get; }

        /// <summary>
        ///     Returns a warning-as-error exception wrapping the given message for this event.
        /// </summary>
        /// <param name="message"> The message to wrap. </param>
        protected virtual Exception WarningAsError([NotNull] string message)
            => new InvalidOperationException(CoreStrings.WarningAsErrorTemplate(EventId.ToString(), message));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected sealed class MessageExtractingLogger : ILogger
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string Message { get; [param: CanBeNull] private set; }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            void ILogger.Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                [CanBeNull] TState state,
                [CanBeNull] Exception exception,
                [NotNull] Func<TState, Exception, string> formatter)
            {
                Message = formatter(state, exception);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            bool ILogger.IsEnabled(LogLevel logLevel) => true;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            IDisposable ILogger.BeginScope<TState>([CanBeNull] TState state) => throw new NotImplementedException();
        }
    }
}
