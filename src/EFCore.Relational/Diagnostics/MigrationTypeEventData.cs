// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

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
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] TypeInfo migrationType)
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
