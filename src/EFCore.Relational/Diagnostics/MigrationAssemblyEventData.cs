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
    ///     <see cref="RelationalEventId" /> migrations assembly events.
    /// </summary>
    public class MigrationAssemblyEventData : MigratorEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="migrator"> The <see cref="IMigrator" /> in use. </param>
        /// <param name="migrationsAssembly"> The <see cref="IMigrationsAssembly" /> in use. </param>
        public MigrationAssemblyEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IMigrator migrator,
            [NotNull] IMigrationsAssembly migrationsAssembly)
            : base(eventDefinition, messageGenerator, migrator)
            => MigrationsAssembly = migrationsAssembly;

        /// <summary>
        ///     The <see cref="IMigrationsAssembly" /> in use.
        /// </summary>
        public virtual IMigrationsAssembly MigrationsAssembly { get; }
    }
}
