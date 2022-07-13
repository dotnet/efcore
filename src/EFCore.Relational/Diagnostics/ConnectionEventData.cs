// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload base class for
///     <see cref="RelationalEventId" /> connection events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ConnectionEventData : DbContextEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="connection">The <see cref="DbConnection" />.</param>
    /// <param name="context">The <see cref="DbContext" /> currently being used, to null if not known.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="async">Indicates whether or not the operation is happening asynchronously.</param>
    /// <param name="startTime">The start time of this event.</param>
    public ConnectionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbConnection connection,
        DbContext? context,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime)
        : base(eventDefinition, messageGenerator, context)
    {
        Connection = connection;
        ConnectionId = connectionId;
        IsAsync = async;
        StartTime = startTime;
    }

    /// <summary>
    ///     The <see cref="DbConnection" />.
    /// </summary>
    public virtual DbConnection Connection { get; }

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
    /// </summary>
    public virtual Guid ConnectionId { get; }

    /// <summary>
    ///     Indicates whether or not the operation is happening asynchronously.
    /// </summary>
    public virtual bool IsAsync { get; }

    /// <summary>
    ///     The start time of this event.
    /// </summary>
    public virtual DateTimeOffset StartTime { get; }
}
