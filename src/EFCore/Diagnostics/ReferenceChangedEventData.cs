// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     a changed property value.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ReferenceChangedEventData : NavigationEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entry for the entity instance on which the property value has changed.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="oldReferencedEntity">The old referenced entity.</param>
    /// <param name="newReferencedEntity">The new referenced entity.</param>
    public ReferenceChangedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        INavigation navigation,
        object? oldReferencedEntity,
        object? newReferencedEntity)
        : base(eventDefinition, messageGenerator, navigation)
    {
        EntityEntry = entityEntry;
        OldReferencedEntity = oldReferencedEntity;
        NewReferencedEntity = newReferencedEntity;
    }

    /// <summary>
    ///     The entry for the entity instance on which the navigation property value has changed.
    /// </summary>
    public virtual EntityEntry EntityEntry { get; }

    /// <summary>
    ///     The navigation.
    /// </summary>
    public new virtual INavigation Navigation
        => (INavigation)base.Navigation;

    /// <summary>
    ///     The old referenced entity.
    /// </summary>
    public virtual object? OldReferencedEntity { get; }

    /// <summary>
    ///     The new referenced entity.
    /// </summary>
    public virtual object? NewReferencedEntity { get; }
}
