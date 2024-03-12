// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for type loading errors.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class TypeLoadingEventData : AssemblyEventData, IErrorEventData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TypeLoadingEventData"/> class.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="assembly">The assembly from which types are being loaded.</param>
    /// <param name="exception">The exception message.</param>
    public TypeLoadingEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Assembly assembly,
        Exception exception)
        : base(eventDefinition, messageGenerator, assembly)
    {
        Exception = exception;
    }

    /// <summary>
    ///     Gets the type-loading exception.
    /// </summary>
    public virtual Exception Exception { get; }
}
