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
    ///     The <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.ConnectionError" />.
    /// </summary>
    public class ConnectionErrorData : ConnectionEndData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="connection">
        ///     The <see cref="DbConnection" />.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown when the connection failed.
        /// </param>
        /// <param name="async">
        ///     Indicates whether or not the operation is happening asyncronously.
        /// </param>
        /// <param name="timestamp">
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used for timing.
        /// </param>
        /// <param name="duration">
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </param>
        public ConnectionErrorData(
            [NotNull] DbConnection connection,
            Guid connectionId,
            [NotNull] Exception exception,
            bool async,
            long timestamp,
            long duration)
            : base(connection, connectionId, async, timestamp, duration)
        {
            Exception = exception;
        }

        /// <summary>
        ///     The exception that was thrown when the connection failed.
        /// </summary>
        public virtual Exception Exception { get; }
    }
}
