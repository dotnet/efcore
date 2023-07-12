// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     a change of a tracked entity from one <see cref="EntityState" /> to another.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class StateChangedEventData : EntityEntryEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entity entry.</param>
    /// <param name="oldState">The old state.</param>
    /// <param name="newState">The new state.</param>
    public StateChangedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        EntityState oldState,
        EntityState newState)
        : base(eventDefinition, messageGenerator, entityEntry)
    {
        OldState = oldState;
        NewState = newState;
    }

    /// <summary>
    ///     The old state.
    /// </summary>
    public virtual EntityState OldState { get; }

    /// <summary>
    ///     The new state.
    /// </summary>
    public virtual EntityState NewState { get; }
}
