// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A base class for all Entity Framework <see cref="DiagnosticSource" /> event payloads.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class EventData
{
    private readonly EventDefinitionBase _eventDefinition;
    private readonly Func<EventDefinitionBase, EventData, string> _messageGenerator;

    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    public EventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator)
    {
        _eventDefinition = eventDefinition;
        _messageGenerator = messageGenerator;
    }

    /// <summary>
    ///     The <see cref="EventId" /> that defines the message ID and name.
    /// </summary>
    public virtual EventId EventId
        => _eventDefinition.EventId;

    /// <summary>
    ///     The <see cref="LogLevel" /> that would be used to log message for this event.
    /// </summary>
    public virtual LogLevel LogLevel
        => _eventDefinition.Level;

    /// <summary>
    ///     A string representing the code where this event is defined.
    /// </summary>
    public virtual string EventIdCode
        => _eventDefinition.EventIdCode;

    /// <summary>
    ///     A logger message describing this event.
    /// </summary>
    /// <returns>A logger message describing this event.</returns>
    public override string ToString()
        => _messageGenerator(_eventDefinition, this);
}
