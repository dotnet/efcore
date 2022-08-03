// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload for
///     <see cref="SqliteEventId.UnexpectedConnectionTypeWarning" />.
/// </summary>
public class UnexpectedConnectionTypeEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="connectionType">The connection type.</param>
    public UnexpectedConnectionTypeEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Type connectionType)
        : base(eventDefinition, messageGenerator)
    {
        ConnectionType = connectionType;
    }

    /// <summary>
    ///     The connection type.
    /// </summary>
    public virtual Type ConnectionType { get; }
}
