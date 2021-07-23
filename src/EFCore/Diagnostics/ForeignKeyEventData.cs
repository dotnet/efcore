// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a foreign key.
    /// </summary>
    public class ForeignKeyEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="foreignKey"> The foreign key. </param>
        public ForeignKeyEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IReadOnlyForeignKey foreignKey)
            : base(eventDefinition, messageGenerator)
        {
            ForeignKey = foreignKey;
        }

        /// <summary>
        ///     The foreign key.
        /// </summary>
        public virtual IReadOnlyForeignKey ForeignKey { get; }
    }
}
