// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> migration connection events.
    /// </summary>
    public class MigratorConnectionData : MigratorData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="migrator">
        ///     The <see cref="IMigrator" /> in use.
        /// </param>
        /// <param name="connection">
        ///     The <see cref="DbConnection" />.
        /// </param>
        /// <param name="connectionId">
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </param>
        public MigratorConnectionData(
            [NotNull] IMigrator migrator,
            [NotNull] DbConnection connection,
            Guid connectionId)
            : base(migrator)
        {
            Connection = connection;
            ConnectionId = connectionId;
        }

        /// <summary>
        ///     The <see cref="DbConnection" />.
        /// </summary>
        public virtual DbConnection Connection { get; }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual Guid ConnectionId { get; }
    }
}
