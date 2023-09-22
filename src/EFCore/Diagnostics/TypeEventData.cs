// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference a <see cref="Type" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class TypeEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="clrType">The <see cref="Type" /> associated with this event.</param>
    public TypeEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Type clrType)
        : base(eventDefinition, messageGenerator)
    {
        ClrType = clrType;
    }

    /// <summary>
    ///     The <see cref="Type" /> associated with this event.
    /// </summary>
    public virtual Type ClrType { get; }
}
