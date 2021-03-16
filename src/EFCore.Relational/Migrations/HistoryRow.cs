// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     An entity type that represents a row in the Migrations history table.
    /// </summary>
    public class HistoryRow
    {
        /// <summary>
        ///     Creates a new <see cref="HistoryRow" /> with the given migration identifier for
        ///     the given version of EF Core.
        /// </summary>
        /// <param name="migrationId"> The migration identifier. </param>
        /// <param name="productVersion">
        ///     The EF Core version, which is obtained from the <see cref="AssemblyInformationalVersionAttribute" />
        ///     of the EF Core assembly.
        /// </param>
        public HistoryRow([NotNull] string migrationId, [NotNull] string productVersion)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(productVersion, nameof(productVersion));

            MigrationId = migrationId;
            ProductVersion = productVersion;
        }

        /// <summary>
        ///     The migration identifier.
        /// </summary>
        public virtual string MigrationId { get; }

        /// <summary>
        ///     The EF Core version, as obtained from the <see cref="AssemblyInformationalVersionAttribute" />
        ///     of the EF Core assembly.
        /// </summary>
        public virtual string ProductVersion { get; }
    }
}
