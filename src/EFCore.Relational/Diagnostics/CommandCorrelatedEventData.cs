// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for events correlated with a <see cref="DbCommand" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CommandCorrelatedEventData : DbContextEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="connection">The <see cref="DbConnection" /> being used.</param>
    /// <param name="context">The <see cref="DbContext" /> currently being used, to null if not known.</param>
    /// <param name="executeMethod">The <see cref="DbCommand" /> method.</param>
    /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="async">Indicates whether or not the command was executed asynchronously.</param>
    /// <param name="startTime">The start time of this event.</param>
    /// <param name="commandSource">Source of the command.</param>
    public CommandCorrelatedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbConnection connection,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        CommandSource commandSource)
        : base(eventDefinition, messageGenerator, context)
    {
        Connection = connection;
        CommandId = commandId;
        ConnectionId = connectionId;
        ExecuteMethod = executeMethod;
        IsAsync = async;
        StartTime = startTime;
        CommandSource = commandSource;
    }

    /// <summary>
    ///     The <see cref="DbConnection" />.
    /// </summary>
    public virtual DbConnection Connection { get; }

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
    /// </summary>
    public virtual Guid CommandId { get; }

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
    /// </summary>
    public virtual Guid ConnectionId { get; }

    /// <summary>
    ///     The <see cref="DbCommandMethod" /> method.
    /// </summary>
    public virtual DbCommandMethod ExecuteMethod { get; }

    /// <summary>
    ///     Indicates whether or not the operation is being executed asynchronously.
    /// </summary>
    public virtual bool IsAsync { get; }

    /// <summary>
    ///     The start time of this event.
    /// </summary>
    public virtual DateTimeOffset StartTime { get; }

    /// <summary>
    ///     Source of the command.
    /// </summary>
    public virtual CommandSource CommandSource { get; }
}
