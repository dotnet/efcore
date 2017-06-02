// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a query expression.
    /// </summary>
    public class QueryExpressionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="queryExpression"> The <see cref="Expression" />. </param>
        /// <param name="expressionPrinter"> An <see cref="IExpressionPrinter" /> that can be used to render the <see cref="Expression" />. </param>
        public QueryExpressionEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] Expression queryExpression,
            [NotNull] IExpressionPrinter expressionPrinter)
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
        ///     An <see cref="IExpressionPrinter" /> that can be used to render the <see cref="Expression" />.
        /// </summary>
        public virtual IExpressionPrinter ExpressionPrinter { get; }
    }
}
