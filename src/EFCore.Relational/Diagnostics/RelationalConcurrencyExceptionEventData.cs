// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload used when a <see cref="DbUpdateConcurrencyException" /> is being thrown
///     from a relational database provider.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class RelationalConcurrencyExceptionEventData : ConcurrencyExceptionEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="context">The current <see cref="DbContext" />.</param>
    /// <param name="connection">The <see cref="DbConnection" /> being used.</param>
    /// <param name="command">The <see cref="DbCommand" /> being used.</param>
    /// <param name="dataReader">The <see cref="DbDataReader" /> being used.</param>
    /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="entries">The entries that were involved in the concurrency violation.</param>
    /// <param name="exception">The exception that will be thrown, unless throwing is suppressed.</param>
    public RelationalConcurrencyExceptionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbContext context,
        DbConnection connection,
        DbCommand command,
        DbDataReader dataReader,
        Guid commandId,
        Guid connectionId,
        IReadOnlyList<IUpdateEntry> entries,
        DbUpdateConcurrencyException exception)
        : base(eventDefinition, messageGenerator, context, entries, exception)
    {
        Connection = connection;
        Command = command;
        DataReader = dataReader;
        CommandId = commandId;
        ConnectionId = connectionId;
    }

    /// <summary>
    ///     The <see cref="DbConnection" /> being used.
    /// </summary>
    public virtual DbConnection Connection { get; }

    /// <summary>
    ///     The <see cref="DbCommand" /> being used.
    /// </summary>
    public virtual DbCommand Command { get; }

    /// <summary>
    ///     The <see cref="DbDataReader" /> being used.
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
}
