// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Transactions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> transaction enlisted events.
    /// </summary>
    public class TransactionEnlistedEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="transaction"> The <see cref="Transaction" />. </param>
        /// <param name="connection"> The <see cref="DbConnection" />. </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        public TransactionEnlistedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] Transaction transaction,
            [NotNull] DbConnection connection,
            Guid connectionId)
            : base(eventDefinition, messageGenerator)
        {
            Transaction = transaction;
            Connection = connection;
            ConnectionId = connectionId;
        }

        /// <summary>
        ///     The <see cref="Transaction" />.
        /// </summary>
        public virtual Transaction Transaction { get; }

        /// <summary>
        ///     The <see cref="DbConnection" />.
        /// </summary>
        public virtual DbConnection Connection { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }
    }
}
