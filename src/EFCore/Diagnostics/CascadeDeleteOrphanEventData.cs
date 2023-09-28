// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     an entity is being deleted because its required relationship to a parent has been severed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CascadeDeleteOrphanEventData : EntityEntryEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entity entry for the entity that is being deleted.</param>
    /// <param name="parentEntityTypes">The entity type to which the relationship was severed.</param>
    /// <param name="state">The state that the child is transitioning to--usually 'Deleted'.</param>
    public CascadeDeleteOrphanEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        IEntityType parentEntityTypes,
        EntityState state)
        : base(eventDefinition, messageGenerator, entityEntry)
    {
        ParentEntityType = parentEntityTypes;
        State = state;
    }

    /// <summary>
    ///     The state that the child is transitioning to--usually 'Deleted'.
    /// </summary>
    public virtual EntityState State { get; }

    /// <summary>
    ///     The entity type to which the relationship was severed.
    /// </summary>
    public virtual IEntityType ParentEntityType { get; }
}
