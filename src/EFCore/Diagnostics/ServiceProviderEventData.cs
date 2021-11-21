// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference
///     a <see cref="IServiceProvider" /> container.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ServiceProviderEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    public ServiceProviderEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IServiceProvider serviceProvider)
        : base(eventDefinition, messageGenerator)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    ///     The <see cref="IServiceProvider" />.
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; }
}
