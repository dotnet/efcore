// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for type loading errors.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class TypeLoadingEventData : AssemblyEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="assembly">The assembly from which types are being loaded.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    public TypeLoadingEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Assembly assembly,
        string exceptionMessage)
        : base(eventDefinition, messageGenerator, assembly)
    {
        ExceptionMessage = exceptionMessage;
    }

    /// <summary>
    ///     The exception message.
    /// </summary>
    public virtual string ExceptionMessage { get; }
}
