// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> command end events.
    /// </summary>
    public class CommandEndEventData : CommandEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="command">
        ///     The <see cref="DbCommand" />.
        /// </param>
        /// <param name="executeMethod">
        ///     The <see cref="DbCommand" /> method.
        /// </param>
        /// <param name="commandId">
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="async">
        ///     Indicates whether or not the command was executed asynchronously.
        /// </param>
        /// <param name="logParameterValues">
        ///     Indicates whether or not the application allows logging of parameter values.
        /// </param>
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        /// <param name="duration">
        ///     The duration this event.
        /// </param>
        public CommandEndEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            bool logParameterValues,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(eventDefinition, messageGenerator, command, executeMethod, commandId, connectionId, async, logParameterValues, startTime)
            => Duration = duration;

        /// <summary>
        ///     The duration this event.
        /// </summary>
        public virtual TimeSpan Duration { get; }
    }
}
