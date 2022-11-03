// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     an entity is being deleted because its parent entity has been deleted.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CascadeDeleteEventData : EntityEntryEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entity entry for the entity that is being deleted.</param>
    /// <param name="parentEntry">The entity entry for the parent that trigger the cascade.</param>
    /// <param name="state">The state that the child is transitioning to--usually 'Deleted'.</param>
    public CascadeDeleteEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        EntityEntry parentEntry,
        EntityState state)
        : base(eventDefinition, messageGenerator, entityEntry)
    {
        ParentEntityEntry = parentEntry;
        State = state;
    }

    /// <summary>
    ///     The state that the child is transitioning to--usually 'Deleted'.
    /// </summary>
    public virtual EntityState State { get; }

    /// <summary>
    ///     The entity entry for the parent that trigger the cascade.
    /// </summary>
    public virtual EntityEntry ParentEntityEntry { get; }
}
