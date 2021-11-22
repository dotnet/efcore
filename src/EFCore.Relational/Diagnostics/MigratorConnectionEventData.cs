// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="RelationalEventId" /> migration connection events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class MigratorConnectionEventData : MigratorEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="migrator">
    ///     The <see cref="IMigrator" /> in use.
    /// </param>
    /// <param name="connection">
    ///     The <see cref="DbConnection" />.
    /// </param>
    /// <param name="connectionId">
    ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
    /// </param>
    public MigratorConnectionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IMigrator migrator,
        DbConnection connection,
        Guid connectionId)
        : base(eventDefinition, messageGenerator, migrator)
    {
        Connection = connection;
        ConnectionId = connectionId;
    }

    /// <summary>
    ///     The <see cref="DbConnection" />.
    /// </summary>
    public virtual DbConnection Connection { get; }

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
    /// </summary>
    public virtual Guid ConnectionId { get; }
}
