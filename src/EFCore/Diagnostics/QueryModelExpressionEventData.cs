// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a query model and an expression.
    /// </summary>
    public class QueryModelClientEvalEventData : QueryModelEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="queryModel"> The <see cref="QueryModel" />. </param>
        /// <param name="queryModelElement"> The query model element requiring client-eval. </param>
        public QueryModelClientEvalEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] QueryModel queryModel,
            [NotNull] object queryModelElement)
            : base(eventDefinition, messageGenerator, queryModel)
        {
            QueryModelElement = queryModelElement;
        }

        /// <summary>
        ///     The expression.
        /// </summary>
        public virtual object QueryModelElement { get; }
    }
}
