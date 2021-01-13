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
    ///     <see cref="RelationalEventId" /> transaction end events.
    /// </summary>
    public class TransactionEndEventData : TransactionEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="transaction"> The <see cref="DbTransaction" />. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, or <see langword="null" /> if not known. </param>
        /// <param name="transactionId"> A correlation ID that identifies the Entity Framework transaction being used. </param>
        /// <param name="connectionId"> A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="async"> Indicates whether or not the transaction is being used asynchronously. </param>
        /// <param name="startTime"> The start time of this event. </param>
        /// <param name="duration"> The duration this event. </param>
        public TransactionEndEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbTransaction transaction,
            [CanBeNull] DbContext context,
            Guid transactionId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(eventDefinition, messageGenerator, transaction, context, transactionId, connectionId, async, startTime)
            => Duration = duration;

        /// <summary>
        ///     The duration of this event.
        /// </summary>
        public virtual TimeSpan Duration { get; }
    }
}
