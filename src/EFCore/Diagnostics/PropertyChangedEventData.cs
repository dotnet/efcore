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
public class PropertyChangedEventData : PropertyEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="entityEntry">The entry for the entity instance on which the property value has changed.</param>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public PropertyChangedEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        EntityEntry entityEntry,
        IProperty property,
        object? oldValue,
        object? newValue)
        : base(eventDefinition, messageGenerator, property)
    {
        EntityEntry = entityEntry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    ///     The entry for the entity instance on which the property value has changed.
    /// </summary>
    public virtual EntityEntry EntityEntry { get; }

    /// <summary>
    ///     The property.
    /// </summary>
    public new virtual IProperty Property
        => (IProperty)base.Property;

    /// <summary>
    ///     The old value.
    /// </summary>
    public virtual object? OldValue { get; }

    /// <summary>
    ///     The new value.
    /// </summary>
    public virtual object? NewValue { get; }
}
