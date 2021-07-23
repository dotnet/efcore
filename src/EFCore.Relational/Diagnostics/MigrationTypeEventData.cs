// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> migration events.
    /// </summary>
    public class MigrationTypeEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="migrationType"> The migration type. </param>
        public MigrationTypeEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            TypeInfo migrationType)
            : base(eventDefinition, messageGenerator)
        {
            MigrationType = migrationType;
        }

        /// <summary>
        ///     The migration type.
        /// </summary>
        public virtual TypeInfo MigrationType { get; }
    }
}
