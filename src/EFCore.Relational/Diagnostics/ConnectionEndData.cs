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
    ///     The <see cref="DiagnosticSource" /> event payload class for
    ///     <see cref="RelationalEventId" /> connection ending events.
    /// </summary>
    public class ConnectionEndData : ConnectionData
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
        /// <param name="async">
        ///     Indicates whether or not the operation is happening asyncronously.
        /// </param>
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        /// <param name="duration">
        ///     The duration this event.
        /// </param>
        public ConnectionEndData(
            [NotNull] DbConnection connection,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(connection, connectionId, async, startTime)
            => Duration = duration;

        /// <summary>
        ///     The duration this event.
        /// </summary>
        public virtual TimeSpan Duration { get; }
    }
}
