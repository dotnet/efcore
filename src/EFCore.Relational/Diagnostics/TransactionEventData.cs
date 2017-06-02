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
    ///     <see cref="RelationalEventId" /> transaction events.
    /// </summary>
    public class TransactionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="transaction">
        ///     The <see cref="DbTransaction" />.
        /// </param>
        /// <param name="transactionId">
        ///     A correlation ID that identifies the Entity Framework transaction being used.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        public TransactionEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            Guid connectionId,
            DateTimeOffset startTime)
            : base(eventDefinition, messageGenerator)
        {
            Transaction = transaction;
            TransactionId = transactionId;
            ConnectionId = connectionId;
            StartTime = startTime;
        }

        /// <summary>
        ///     The <see cref="DbTransaction" />.
        /// </summary>
        public virtual DbTransaction Transaction { get; }

        /// <summary>
        ///     A correlation ID that identifies the Entity Framework transaction being used.
        /// </summary>
        public virtual Guid TransactionId { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }
    }
}
