// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderClosing" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class DataReaderClosingEventData : DataReaderEventData
{
    /// <summary>
    ///     Constructs a <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.DataReaderClosing" />.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="command">The <see cref="DbCommand" /> that created the reader.</param>
    /// <param name="dataReader">The <see cref="DbDataReader" /> that is being disposed.</param>
    /// <param name="context">The <see cref="DbContext" /> currently being used, to null if not known.</param>
    /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="async">Indicates whether or not the command was executed asynchronously.</param>
    /// <param name="recordsAffected">Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.</param>
    /// <param name="readCount">Gets the number of read operations performed by this reader.</param>
    /// <param name="startTime">The time when the data reader was created.</param>
    public DataReaderClosingEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbCommand command,
        DbDataReader dataReader,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        bool async,
        int recordsAffected,
        int readCount,
        DateTimeOffset startTime)
        : base(
            eventDefinition, messageGenerator, command, dataReader, context, commandId, connectionId, recordsAffected, readCount, startTime)
    {
        IsAsync = async;
    }

    /// <summary>
    ///     Indicates whether or not the operation is being executed asynchronously.
    /// </summary>
    public virtual bool IsAsync { get; }
}
