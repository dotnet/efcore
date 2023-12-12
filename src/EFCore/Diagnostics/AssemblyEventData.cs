// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for assembly events.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class AssemblyEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="assembly">The assembly from which types are being loaded.</param>
    public AssemblyEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Assembly assembly)
        : base(eventDefinition, messageGenerator)
    {
        Assembly = assembly;
    }

    /// <summary>
    ///     The assembly.
    /// </summary>
    public virtual Assembly Assembly { get; }
}
