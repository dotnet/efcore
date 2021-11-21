// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for error events that reference
///     a <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class DbContextErrorEventData : DbContextEventData, IErrorEventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="context">The current <see cref="DbContext" />.</param>
    /// <param name="exception">The exception that triggered this event.</param>
    public DbContextErrorEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        DbContext context,
        Exception exception)
        : base(eventDefinition, messageGenerator, context)
    {
        Exception = exception;
    }

    /// <summary>
    ///     The exception that triggered this event.
    /// </summary>
    public virtual Exception Exception { get; }
}
