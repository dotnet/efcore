// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that reference
///     debug information on service provider creation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ServiceProviderDebugInfoEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="newDebugInfo">The debug information for the new provider.</param>
    /// <param name="cachedDebugInfos">The debug information for existing providers.</param>
    public ServiceProviderDebugInfoEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        IDictionary<string, string> newDebugInfo,
        IList<IDictionary<string, string>> cachedDebugInfos)
        : base(eventDefinition, messageGenerator)
    {
        NewDebugInfo = newDebugInfo;
        CachedDebugInfos = cachedDebugInfos;
    }

    /// <summary>
    ///     The debug information for the new provider.
    /// </summary>
    public virtual IDictionary<string, string> NewDebugInfo { get; }

    /// <summary>
    ///     The debug information for existing providers.
    /// </summary>
    public virtual IList<IDictionary<string, string>> CachedDebugInfos { get; }
}
