// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a query expression.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information.
    /// </remarks>
    public class QueryExpressionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="queryExpression"> The <see cref="Expression" />. </param>
        /// <param name="expressionPrinter"> An <see cref="ExpressionPrinter" /> that can be used to render the <see cref="Expression" />. </param>
        public QueryExpressionEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            Expression queryExpression,
            ExpressionPrinter expressionPrinter)
            : base(eventDefinition, messageGenerator)
        {
            Expression = queryExpression;
            ExpressionPrinter = expressionPrinter;
        }

        /// <summary>
        ///     The <see cref="Expression" />.
        /// </summary>
        public virtual Expression Expression { get; }

        /// <summary>
        ///     An <see cref="ExpressionPrinter" /> that can be used to render the <see cref="Expression" />.
        /// </summary>
        public virtual ExpressionPrinter ExpressionPrinter { get; }
    }
}
