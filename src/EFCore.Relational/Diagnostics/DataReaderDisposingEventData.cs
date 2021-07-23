// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
    /// </summary>
    public class DataReaderDisposingEventData : DbContextEventData
    {
        /// <summary>
        ///     Constructs a <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="command"> The <see cref="DbCommand" /> that created the reader. </param>
        /// <param name="dataReader"> The <see cref="DbDataReader" /> that is being disposed. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used. </param>
        /// <param name="recordsAffected">Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. </param>
        /// <param name="readCount">Gets the number of read operations performed by this reader. </param>
        /// <param name="startTime">The start time of this event. </param>
        /// <param name="duration">The duration this event. </param>
        public DataReaderDisposingEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            DbCommand command,
            DbDataReader dataReader,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            int recordsAffected,
            int readCount,
            DateTimeOffset startTime,
            TimeSpan duration)
            : base(eventDefinition, messageGenerator, context)
        {
            Command = command;
            DataReader = dataReader;
            CommandId = commandId;
            ConnectionId = connectionId;
            RecordsAffected = recordsAffected;
            ReadCount = readCount;
            StartTime = startTime;
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
        ///     Gets the number of read operations performed by this reader.
        /// </summary>
        public virtual int ReadCount { get; }

        /// <summary>
        ///     The start time of this event.
        /// </summary>
        public virtual DateTimeOffset StartTime { get; }

        /// <summary>
        ///     The duration this event.
        /// </summary>
        public virtual TimeSpan Duration { get; }
    }
}
