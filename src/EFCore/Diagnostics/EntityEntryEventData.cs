// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference
///     a <see cref="EntityEntry" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class EntityEntryEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entity entry.</param>
    public EntityEntryEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry)
        : base(eventDefinition, messageGenerator)
    {
        EntityEntry = entityEntry;
    }

    /// <summary>
    ///     The entity entry.
    /// </summary>
    public virtual EntityEntry EntityEntry { get; }
}
