// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload base class for events that
    ///     references two <see cref="SqlExpression" />.
    /// </summary>
    public class TwoSqlExpressionsEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="left"> The left SqlExpression. </param>
        /// <param name="right"> The right SqlExpression. </param>
        public TwoSqlExpressionsEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right)
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
}
