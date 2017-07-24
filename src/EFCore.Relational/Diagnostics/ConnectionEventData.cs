// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload base class for
    ///     <see cref="RelationalEventId" /> connection events.
    /// </summary>
    public class ConnectionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="connection">
        ///     The <see cref="DbConnection" />.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="async">
        ///     Indicates whether or not the operation is happening asynchronously.
        /// </param>
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        public ConnectionEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbConnection connection,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
            : base(eventDefinition, messageGenerator)
        {
            Connection = connection;
            ConnectionId = connectionId;
            IsAsync = async;
            StartTime = startTime;
        }

        /// <summary>
        ///     The <see cref="DbConnection" />.
        /// </summary>
        public virtual DbConnection Connection { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }

        /// <summary>
        ///     Indicates whether or not the operation is happening asynchronously.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }
    }
}
