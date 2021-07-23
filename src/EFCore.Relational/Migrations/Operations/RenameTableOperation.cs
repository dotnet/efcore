// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for renaming an existing table.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Name} RENAME TO {NewName}")]
    public class RenameTableOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The old name of the table.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The new schema name, or <see langword="null" /> to use the default schema.
        /// </summary>
        public virtual string? NewSchema { get; set; }

        /// <summary>
        ///     The new table name or <see langword="null" /> if only the schema has changed.
        /// </summary>
        public virtual string? NewName { get; set; }

        /// <inheritdoc />
        string ITableMigrationOperation.Table
            => Name;
    }
}
