// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for events correlated with a <see cref="DbCommand"/>.
    /// </summary>
    public class CommandCorrelatedEventData : DbContextEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="connection"> The <see cref="DbConnection"/> being used. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> The <see cref="DbCommand" /> method. </param>
        /// <param name="commandId"> A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="connectionId"> A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="async"> Indicates whether or not the command was executed asynchronously. </param>
        /// <param name="startTime"> The start time of this event. </param>
        public CommandCorrelatedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbConnection connection,
            [CanBeNull] DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
            : base(eventDefinition, messageGenerator, context)
        {
            Connection = connection;
            CommandId = commandId;
            ConnectionId = connectionId;
            ExecuteMethod = executeMethod;
            IsAsync = async;
            StartTime = startTime;
        }

        /// <summary>
        ///     The <see cref="DbConnection" />.
        /// </summary>
        public virtual DbConnection Connection { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </summary>
        public virtual Guid CommandId { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }

        /// <summary>
        ///     The <see cref="DbCommandMethod" /> method.
        /// </summary>
        public virtual DbCommandMethod ExecuteMethod { get; }

        /// <summary>
        ///     Indicates whether or not the operation is being executed asynchronously.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }
    }
}
