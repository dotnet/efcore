// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a property.
    /// </summary>
    public class ConflictingValueGenerationStrategiesEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="sqlServerValueGenerationStrategy"> The SQL Server value generation strategy. </param>
        /// <param name="otherValueGenerationStrategy"> The other value generation strategy. </param>
        /// <param name="property"> The property. </param>
        public ConflictingValueGenerationStrategiesEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            SqlServerValueGenerationStrategy sqlServerValueGenerationStrategy,
            [NotNull] string otherValueGenerationStrategy,
            [NotNull] IProperty property)
            : base(eventDefinition, messageGenerator)
        {
            SqlServerValueGenerationStrategy = sqlServerValueGenerationStrategy;
            OtherValueGenerationStrategy = otherValueGenerationStrategy;
            Property = property;
        }

        /// <summary>
        ///     The SQL Server value generation strategy.
        /// </summary>
        public virtual SqlServerValueGenerationStrategy SqlServerValueGenerationStrategy { get; }

        /// <summary>
        ///     The other value generation strategy.
        /// </summary>
        public virtual string OtherValueGenerationStrategy { get; }

        /// <summary>
        ///     The property.
        /// </summary>
        public virtual IProperty Property { get; }
    }
}
