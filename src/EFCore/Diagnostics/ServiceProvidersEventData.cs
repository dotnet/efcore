// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference
///     multiple <see cref="IServiceProvider" /> containers.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ServiceProvidersEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="serviceProviders">The <see cref="IServiceProvider" />s.</param>
    public ServiceProvidersEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        ICollection<IServiceProvider> serviceProviders)
        : base(eventDefinition, messageGenerator)
    {
        ServiceProviders = serviceProviders;
    }

    /// <summary>
    ///     The <see cref="IServiceProvider" />s.
    /// </summary>
    public virtual ICollection<IServiceProvider> ServiceProviders { get; }
}
