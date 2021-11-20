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
public class BinaryExpressionEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="left">The left <see cref="Expression" />.</param>
    /// <param name="right">The right <see cref="Expression" />.</param>
    public BinaryExpressionEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        Expression left,
        Expression right)
        : base(eventDefinition, messageGenerator)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The left <see cref="Expression" />.
    /// </summary>
    public virtual Expression Left { get; }

    /// <summary>
    ///     The right <see cref="Expression" />.
    /// </summary>
    public virtual Expression Right { get; }
}
