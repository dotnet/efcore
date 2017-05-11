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
    ///     <see cref="RelationalEventId" /> command events.
    /// </summary>
    public class CommandData
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
        /// <param name="startTime">
        ///     The start time of this event.
        /// </param>
        public CommandData(
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
        {
            Command = command;
            CommandId = commandId;
            ConnectionId = connectionId;
            ExecuteMethod = executeMethod;
            Async = async;
            StartTime = startTime;
        }

        /// <summary>
        ///     The <see cref="DbCommand" />.
        /// </summary>
        public virtual DbCommand Command { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </summary>
        public virtual Guid CommandId { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }

        /// <summary>
        ///     The <see cref="DbCommand" /> method.
        /// </summary>
        public virtual DbCommandMethod ExecuteMethod { get; }

        /// <summary>
        ///     Indicates whether or not the operation is being executed asyncronously.
        /// </summary>
        public virtual bool Async { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }
    }
}
