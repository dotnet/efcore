// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     a property that has been uniquified.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class UniquifiedPropertyEventData : PropertyEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="property">The property.</param>
    /// <param name="basePropertyName">The property name that was uniquified.</param>
    public UniquifiedPropertyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IReadOnlyProperty property,
        string basePropertyName)
        : base(eventDefinition, messageGenerator, property)
    {
        BasePropertyName = basePropertyName;
    }

    /// <summary>
    ///     The property name that was uniquified.
    /// </summary>
    public virtual string BasePropertyName { get; }
}
