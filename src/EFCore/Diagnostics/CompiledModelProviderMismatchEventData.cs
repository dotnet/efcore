// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for compiled model provider mismatch warnings.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class CompiledModelProviderMismatchEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="compiledProviderName">The provider name stored in the compiled model.</param>
    /// <param name="currentProviderName">The provider name currently configured.</param>
    public CompiledModelProviderMismatchEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        string compiledProviderName,
        string currentProviderName)
        : base(eventDefinition, messageGenerator)
    {
        CompiledProviderName = compiledProviderName;
        CurrentProviderName = currentProviderName;
    }

    /// <summary>
    ///     The provider name stored in the compiled model.
    /// </summary>
    public virtual string CompiledProviderName { get; }

    /// <summary>
    ///     The provider name currently configured.
    /// </summary>
    public virtual string CurrentProviderName { get; }
}
