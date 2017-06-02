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
    ///     a query model.
    /// </summary>
    public class QueryModelEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="queryModel"> The <see cref="QueryModel" />. </param>
        public QueryModelEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] QueryModel queryModel)
            : base(eventDefinition, messageGenerator)
        {
            QueryModel = queryModel;
        }

        /// <summary>
        ///     The <see cref="QueryModel" />.
        /// </summary>
        public virtual QueryModel QueryModel { get; }
    }
}
