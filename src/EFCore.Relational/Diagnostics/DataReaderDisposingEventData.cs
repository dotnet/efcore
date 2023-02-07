// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class DataReaderDisposingEventData : DataReaderEventData
{
    /// <summary>
    ///     Constructs a <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderDisposing" />.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="command">The <see cref="DbCommand" /> that created the reader.</param>
    /// <param name="dataReader">The <see cref="DbDataReader" /> that is being disposed.</param>
    /// <param name="context">The <see cref="DbContext" /> currently being used, to null if not known.</param>
    /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="recordsAffected">Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.</param>
    /// <param name="readCount">Gets the number of read operations performed by this reader.</param>
    /// <param name="startTime">The time when the data reader was created.</param>
    /// <param name="duration">
    ///     The duration from the time the data reader is created until it is disposed. This corresponds to the time reading
    ///     for reading results of a query.
    /// </param>
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
        : base(
            eventDefinition, messageGenerator, command, dataReader, context, commandId, connectionId, recordsAffected, readCount, startTime)
    {
        Duration = duration;
    }

    /// <summary>
    ///     The duration from the time the data reader is created until it is disposed. This corresponds to the time reading
    ///     for reading results of a query.
    /// </summary>
    public virtual TimeSpan Duration { get; }
}
