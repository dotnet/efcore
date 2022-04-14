// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have an <see cref="ISkipNavigation" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class SkipNavigationEventData : EventData, INavigationBaseEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="navigation">The navigation.</param>
    public SkipNavigationEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IReadOnlySkipNavigation navigation)
        : base(eventDefinition, messageGenerator)
    {
        Navigation = navigation;
    }

    /// <summary>
    ///     The navigation.
    /// </summary>
    public virtual IReadOnlySkipNavigation Navigation { get; }

    /// <summary>
    ///     The navigation.
    /// </summary>
    INavigationBase INavigationBaseEventData.NavigationBase
        => (INavigationBase)Navigation;
}
