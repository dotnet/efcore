// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        /// <param name="migrator">
        ///     The <see cref="IMigrator" /> in use.
        /// </param>
        /// <param name="migration">
        ///     The <see cref="Migration" /> being processed.
        /// </param>
        public MigrationEventData(
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
            : base(migrator)
        {
            Migration = migration;
        }

        /// <summary>
        ///     The <see cref="Migration" /> being processed.
        /// </summary>
        public virtual Migration Migration { get; }
    }
}
