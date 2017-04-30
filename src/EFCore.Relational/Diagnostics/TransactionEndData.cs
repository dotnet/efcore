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
    ///     <see cref="RelationalEventId" /> transaction end events.
    /// </summary>
    public class TransactionEndData : TransactionData
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
        /// <param name="duration">
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </param>
        public TransactionEndData(
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            Guid connectionId,
            long timestamp,
            long duration)
            : base(transaction, transactionId, connectionId, timestamp)
        {
            Duration = duration;
        }

        /// <summary>
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </summary>
        public virtual long Duration { get; }
    }
}
