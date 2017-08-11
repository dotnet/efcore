// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an <see cref="EntityFrameworkQueryableExtensions.Include{TEntity,TProperty}" /> specification.
    /// </summary>
    public class IncludeEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="includeResultOperator"> The <see cref="IncludeResultOperator" />. </param>
        public IncludeEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IncludeResultOperator includeResultOperator)
            : base(eventDefinition, messageGenerator)
        {
            IncludeResultOperator = includeResultOperator;
        }

        /// <summary>
        ///     The <see cref="EntityFrameworkQueryableExtensions.Include{TEntity,TProperty}" /> result operator.
        /// </summary>
        public virtual IncludeResultOperator IncludeResultOperator { get; }
    }
}
