// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="DiagnosticSource" /> event payload class for events that have
///     a query expression.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class ExpressionEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="expression">The <see cref="Expression" />.</param>
    public ExpressionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Expression expression)
        : base(eventDefinition, messageGenerator)
    {
        Expression = expression;
    }

    /// <summary>
    ///     The <see cref="Expression" />.
    /// </summary>
    public virtual Expression Expression { get; }
}
