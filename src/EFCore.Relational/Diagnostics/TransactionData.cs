// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload base class for
    ///     <see cref="RelationalEventId" /> transaction events.
    /// </summary>
    public class TransactionData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="DbTransaction" />.
        /// </param>
        /// <param name="transactionId">
        ///     A correlation ID that identifies the Entity Framework transaction being used.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="timestamp">
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used for timing.
        /// </param>
        public TransactionData(
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            Guid connectionId,
            long timestamp)
        {
            Transaction = transaction;
            TransactionId = transactionId;
            ConnectionId = connectionId;
            Timestamp = timestamp;
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
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used for timing.
        /// </summary>
        public virtual long Timestamp { get; }
    }
}
