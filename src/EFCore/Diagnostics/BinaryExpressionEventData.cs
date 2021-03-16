// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a query expression.
    /// </summary>
    public class BinaryExpressionEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="left"> The left <see cref="Expression" />. </param>
        /// <param name="right"> The right <see cref="Expression" />. </param>
        public BinaryExpressionEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] Expression left,
            [NotNull] Expression right)
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
}
