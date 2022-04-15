// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     two unmapped property collections.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class TwoUnmappedPropertyCollectionsEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="firstPropertyCollection">The first property collection.</param>
    /// <param name="secondPropertyCollection">The second property collection.</param>
    public TwoUnmappedPropertyCollectionsEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IEnumerable<Tuple<MemberInfo?, Type>> firstPropertyCollection,
        IEnumerable<Tuple<MemberInfo?, Type>> secondPropertyCollection)
        : base(eventDefinition, messageGenerator)
    {
        FirstPropertyCollection = firstPropertyCollection;
        SecondPropertyCollection = secondPropertyCollection;
    }

    /// <summary>
    ///     The first property collection.
    /// </summary>
    public virtual IEnumerable<Tuple<MemberInfo?, Type>> FirstPropertyCollection { get; }

    /// <summary>
    ///     The second property collection.
    /// </summary>
    public virtual IEnumerable<Tuple<MemberInfo?, Type>> SecondPropertyCollection { get; }
}
