// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
///     a skip collection navigation property has had entities added and/or removed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class SkipCollectionChangedEventData : SkipNavigationEventData, ICollectionChangedEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entry for the entity instance on which the property value has changed.</param>
    /// <param name="navigation">The navigation property.</param>
    /// <param name="added">The entities added to the collection.</param>
    /// <param name="removed">The entities removed from the collection.</param>
    public SkipCollectionChangedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        ISkipNavigation navigation,
        IEnumerable<object> added,
        IEnumerable<object> removed)
        : base(eventDefinition, messageGenerator, navigation)
    {
        EntityEntry = entityEntry;
        Added = added;
        Removed = removed;
    }

    /// <summary>
    ///     The entry for the entity instance on which the navigation property has been added
    ///     to or removed from.
    /// </summary>
    public virtual EntityEntry EntityEntry { get; }

    /// <summary>
    ///     The navigation.
    /// </summary>
    public new virtual ISkipNavigation Navigation
        => (ISkipNavigation)base.Navigation;

    /// <summary>
    ///     The entities added to the collection.
    /// </summary>
    public virtual IEnumerable<object> Added { get; }

    /// <summary>
    ///     The entities removed from the collection.
    /// </summary>
    public virtual IEnumerable<object> Removed { get; }
}
