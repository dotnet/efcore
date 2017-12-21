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
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IForeignKey foreignKey)
            : base(eventDefinition, messageGenerator)
        {
            ForeignKey = foreignKey;
        }

        /// <summary>
        ///     The foreign key.
        /// </summary>
        public virtual IForeignKey ForeignKey { get; }
    }
}
