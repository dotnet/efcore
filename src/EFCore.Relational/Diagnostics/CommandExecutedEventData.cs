// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.CommandExecuted" />.
    /// </summary>
    public class CommandExecutedEventData : CommandEndEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="connection"> The <see cref="DbConnection" /> being used. </param>
        /// <param name="command"> The <see cref="DbCommand" /> that was executing when it failed. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> The <see cref="DbCommand" /> method that was used to execute the command. </param>
        /// <param name="commandId"> A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="connectionId"> A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="result"> The result of executing the operation. </param>
        /// <param name="async"> Indicates whether or not the command was executed asynchronously. </param>
        /// <param name="logParameterValues"> Indicates whether or not the application allows logging of parameter values. </param>
        /// <param name="startTime"> The start time of this event. </param>
        /// <param name="duration"> The duration this event. </param>
        public CommandExecutedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] object result,
            bool async,
            bool logParameterValues,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(
                eventDefinition,
                messageGenerator,
                connection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                async,
                logParameterValues,
                startTime,
                duration)
            => Result = result;

        /// <summary>
        ///     The result of executing the command.
        /// </summary>
        public virtual object Result { get; }
    }
}
