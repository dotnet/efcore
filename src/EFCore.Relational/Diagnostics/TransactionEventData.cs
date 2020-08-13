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
    public class TransactionEventData : DbContextEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="transaction"> The <see cref="DbTransaction" />. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently in use, or null if not known. </param>
        /// <param name="transactionId"> A correlation ID that identifies the Entity Framework transaction being used. </param>
        /// <param name="connectionId"> A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="async"> Indicates whether or not the transaction is being used asynchronously. </param>
        /// <param name="startTime"> The start time of this event. </param>
        public TransactionEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbTransaction transaction,
            [CanBeNull] DbContext context,
            Guid transactionId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
            : base(eventDefinition, messageGenerator, context)
        {
            Transaction = transaction;
            TransactionId = transactionId;
            ConnectionId = connectionId;
            IsAsync = async;
            StartTime = startTime;
        }

        /// <summary>
        ///     The <see cref="DbTransaction" />, or null if it has not yet been created.
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
        ///     Indicates whether or not the transaction is being used asynchronously.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }
    }
}
