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
    ///     <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
    /// </summary>
    public class DataReaderDisposingData
    {
        /// <summary>
        ///     Constructs a <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
        /// </summary>
        /// <param name="command">
        ///     The <see cref="DbCommand" /> that created the reader.
        /// </param>
        /// <param name="dataReader">
        ///     The <see cref="DbDataReader" /> that is being disposed.
        /// </param>
        /// <param name="commandId">
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        /// <param name="recordsAffected">
        ///     Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </param>
        /// <param name="timestamp">
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used
        ///     with <see cref="RelationalEventId.CommandExecuting" /> to time execution.
        /// </param>
        /// <param name="duration">
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </param>
        public DataReaderDisposingData(
            [NotNull] DbCommand command,
            [NotNull] DbDataReader dataReader,
            Guid commandId,
            Guid connectionId,
            int recordsAffected,
            long timestamp,
            long duration)
        {
            Command = command;
            DataReader = dataReader;
            CommandId = commandId;
            ConnectionId = connectionId;
            RecordsAffected = recordsAffected;
            Timestamp = timestamp;
            Duration = duration;
        }

        /// <summary>
        ///     The <see cref="DbCommand" /> that created the reader.
        /// </summary>
        public virtual DbCommand Command { get; }

        /// <summary>
        ///     The <see cref="DbDataReader" /> that is being disposed.
        /// </summary>
        public virtual DbDataReader DataReader { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
        /// </summary>
        public virtual Guid CommandId { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }

        /// <summary>
        ///     Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public virtual int RecordsAffected { get; }

        /// <summary>
        ///     A timestamp from <see cref="Stopwatch.GetTimestamp" /> that can be used
        ///     with <see cref="RelationalEventId.CommandExecuting" /> to time execution.
        /// </summary>
        public virtual long Timestamp { get; }

        /// <summary>
        ///     The duration of execution as ticks from <see cref="Stopwatch.GetTimestamp" />.
        /// </summary>
        public virtual long Duration { get; }
    }
}
