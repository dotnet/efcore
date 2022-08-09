// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference
///     two <see cref="IEntityType" /> instances.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class SharedDependentEntityEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="firstEntityType">The first <see cref="IEntityType" />.</param>
    /// <param name="secondEntityType">The second <see cref="IEntityType" />.</param>
    public SharedDependentEntityEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEntityType firstEntityType,
        IEntityType secondEntityType)
        : base(eventDefinition, messageGenerator)
    {
        FirstEntityType = firstEntityType;
        SecondEntityType = secondEntityType;
    }

    /// <summary>
    ///     The first <see cref="IEntityType" />.
    /// </summary>
    public virtual IEntityType FirstEntityType { get; }

    /// <summary>
    ///     The second <see cref="IEntityType" />.
    /// </summary>
    public virtual IEntityType SecondEntityType { get; }
}
