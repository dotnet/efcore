// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for renaming an existing column.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} RENAME COLUMN {Name} TO {NewName}")]
    public class RenameColumnOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The old name of the column.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The name of the table that contains the column.
        /// </summary>
        public virtual string Table { get; set; } = null!;

        /// <summary>
        ///     The new name for the column.
        /// </summary>
        public virtual string NewName { get; set; } = null!;
    }
}
