// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.ConnectionError" />.
    /// </summary>
    public class ConnectionErrorEventData : ConnectionEndEventData, IErrorEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="connection"> The <see cref="DbConnection" />. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="exception"> The exception that was thrown when the connection failed. </param>
        /// <param name="async"> Indicates whether or not the operation is happening asynchronously. </param>
        /// <param name="startTime"> The start time of this event. </param>
        /// <param name="duration"> The duration this event.  </param>
        public ConnectionErrorEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbConnection connection,
            [CanBeNull] DbContext context,
            Guid connectionId,
            [NotNull] Exception exception,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(eventDefinition, messageGenerator, connection, context, connectionId, async, startTime, duration)
            => Exception = exception;

        /// <summary>
        ///     The exception that was thrown when the connection failed.
        /// </summary>
        public virtual Exception Exception { get; }
    }
}
