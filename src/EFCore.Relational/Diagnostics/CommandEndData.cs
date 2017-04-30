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
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> command end events.
    /// </summary>
    public class CommandEndData : CommandData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="command">
        ///     The <see cref="DbCommand" />.
        /// </param>
        /// <param name="executeMethod">
        ///     The <see cref="DbCommand" /> method.
        /// </param>
        /// <param name="commandId">
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="async">
        ///     Indicates whether or not the command was executed asyncronously.
        /// </param>
        /// <param name="timestamp">
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used for timing.
        /// </param>
        /// <param name="duration">
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </param>
        public CommandEndData(
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            long timestamp,
            long duration)
            : base(command, executeMethod, commandId, connectionId, async, timestamp)
        {
            Duration = duration;
        }

        /// <summary>
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </summary>
        public virtual long Duration { get; }
    }
}
