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
    ///     <see cref="RelationalEventId" /> migration scripting events.
    /// </summary>
    public class MigrationScriptingEventData : MigrationEventData
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
        /// <param name="fromMigration">
        ///     The migration that scripting is starting from.
        /// </param>
        /// <param name="toMigration">
        ///     The migration that scripting is going to.
        /// </param>
        /// <param name="idempotent">
        ///     Indicates whether or not the script is idempotent.
        /// </param>
        public MigrationScriptingEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent)
            : base(eventDefinition, messageGenerator, migrator, migration)
        {
            FromMigration = fromMigration;
            ToMigration = toMigration;
            IsIdempotent = idempotent;
        }

        /// <summary>
        ///     The migration that scripting is starting from.
        /// </summary>
        public virtual string FromMigration { get; }

        /// <summary>
        ///     The migration that scripting is going to.
        /// </summary>
        public virtual string ToMigration { get; }

        /// <summary>
        ///     Indicates whether or not the script is idempotent.
        /// </summary>
        public virtual bool IsIdempotent { get; }
    }
}
