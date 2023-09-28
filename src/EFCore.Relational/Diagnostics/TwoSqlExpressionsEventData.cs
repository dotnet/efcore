// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The <see cref="DiagnosticSource" /> event payload base class for events that
///     references two <see cref="SqlExpression" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public class TwoSqlExpressionsEventData : EventData
{
    /// <summary>
    ///     Constructs the event payload.
    /// </summary>
    /// <param name="eventDefinition">The event definition.</param>
    /// <param name="messageGenerator">A delegate that generates a log message for this event.</param>
    /// <param name="left">The left SqlExpression.</param>
    /// <param name="right">The right SqlExpression.</param>
    public TwoSqlExpressionsEventData(
        EventDefinitionBase eventDefinition,
        Func<EventDefinitionBase, EventData, string> messageGenerator,
        SqlExpression left,
        SqlExpression right)
        : base(eventDefinition, messageGenerator)
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    ///     The left SqlExpression.
    /// </summary>
    public virtual SqlExpression Left { get; }

    /// <summary>
    ///     The right SqlExpression.
    /// </summary>
    public virtual SqlExpression Right { get; }
}
