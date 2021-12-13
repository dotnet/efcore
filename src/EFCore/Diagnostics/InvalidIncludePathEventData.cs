// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     invalid include path information.
/// </summary>
public class InvalidIncludePathEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="navigationChain">Navigation chain included to this point.</param>
    /// <param name="navigationName">The name of the invalid navigation.</param>
    public InvalidIncludePathEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        string navigationChain,
        string navigationName)
        : base(eventDefinition, messageGenerator)
    {
        NavigationChain = navigationChain;
        NavigationName = navigationName;
    }

    /// <summary>
    ///     Navigation chain included to this point.
    /// </summary>
    public virtual string NavigationChain { get; }

    /// <summary>
    ///     The name of the invalid navigation.
    /// </summary>
    public virtual string NavigationName { get; }
}
