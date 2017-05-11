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
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        /// <param name="duration">
        ///     The duration this event.
        /// </param>
        public TransactionEndData(
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            Guid connectionId,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(transaction, transactionId, connectionId, startTime) 
            => Duration = duration;

        /// <summary>
        ///     The duration this event.
        /// </summary>
        public virtual TimeSpan Duration { get; }
    }
}
