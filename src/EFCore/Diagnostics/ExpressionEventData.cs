// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a query expression.
    /// </summary>
    public class ExpressionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="expression"> The <see cref="Expression" />. </param>
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
}
