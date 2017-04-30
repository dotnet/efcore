// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> migration events.
    /// </summary>
    public class MigratorData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="migrator">
        ///     The <see cref="IMigrator" /> in use.
        /// </param>
        public MigratorData([NotNull] IMigrator migrator)
        {
            Migrator = migrator;
        }

        /// <summary>
        ///     The <see cref="IMigrator" /> in use.
        /// </summary>
        public virtual IMigrator Migrator { get; }
    }
}
