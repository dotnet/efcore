// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> events of a specific migration.
    /// </summary>
    public class MigrationEventData : MigratorEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="migrator">
        ///     The <see cref="IMigrator" /> in use.
        /// </param>
        /// <param name="migration">
        ///     The <see cref="Migration" /> being processed.
        /// </param>
        public MigrationEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
            : base(eventDefinition, messageGenerator, migrator)
        {
            Migration = migration;
        }

        /// <summary>
        ///     The <see cref="Migration" /> being processed.
        /// </summary>
        public virtual Migration Migration { get; }
    }
}
