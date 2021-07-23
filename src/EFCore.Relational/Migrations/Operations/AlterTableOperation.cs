// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing table.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Name}")]
    public class AlterTableOperation : TableOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     An operation representing the table as it was before being altered.
        /// </summary>
        public virtual TableOperation OldTable { get; set; } = new CreateTableOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
            => OldTable;
    }
}
