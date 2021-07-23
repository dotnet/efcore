// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> command events.
    /// </summary>
    public class CommandEventData : CommandCorrelatedEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="connection"> The <see cref="DbConnection" /> being used. </param>
        /// <param name="command"> The <see cref="DbCommand" />. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> The <see cref="DbCommand" /> method. </param>
        /// <param name="commandId"> A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="connectionId"> A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="async"> Indicates whether or not the command was executed asynchronously. </param>
        /// <param name="logParameterValues"> Indicates whether or not the application allows logging of parameter values. </param>
        /// <param name="startTime"> The start time of this event. </param>
        public CommandEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            DbConnection connection,
            DbCommand command,
            DbContext? context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            bool logParameterValues,
            DateTimeOffset startTime)
            : base(
                eventDefinition,
                messageGenerator,
                connection,
                context,
                executeMethod,
                commandId,
                connectionId,
                async,
                startTime)
        {
            Command = command;
            LogParameterValues = logParameterValues;
        }

        /// <summary>
        ///     The <see cref="DbCommand" />.
        /// </summary>
        public virtual DbCommand Command { get; }

        /// <summary>
        ///     Indicates whether or not the application allows logging of parameter values.
        /// </summary>
        public virtual bool LogParameterValues { get; }
    }
}
