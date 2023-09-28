// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for <see cref="RelationalEventId.ConnectionCreated" /> events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ConnectionCreatedEventData : DbContextEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="connection">The <see cref="DbConnection" />.</param>
    /// <param name="context">The <see cref="DbContext" /> currently being used, to null if not known.</param>
    /// <param name="connectionId">A correlation ID that identifies the <see cref="DbConnection" /> instance being used.</param>
    /// <param name="startTime">The start time of this event.</param>
    /// <param name="duration">The duration this event.</param>
    public ConnectionCreatedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbConnection connection,
        DbContext? context,
        Guid connectionId,
        DateTimeOffset startTime,
        TimeSpan duration)
        : base(eventDefinition, messageGenerator, context)
    {
        Connection = connection;
        ConnectionId = connectionId;
        StartTime = startTime;
        Duration = duration;
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
    ///     The start time of this event.
    /// </summary>
    public virtual DateTimeOffset StartTime { get; }

    /// <summary>
    ///     The duration of this event.
    /// </summary>
    public virtual TimeSpan Duration { get; }
}
