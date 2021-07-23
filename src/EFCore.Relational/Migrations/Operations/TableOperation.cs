// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for operations on tables.
    ///     See also <see cref="CreateTableOperation" /> and <see cref="AlterTableOperation" />.
    /// </summary>
    public abstract class TableOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The name of the table.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     Comment for this table
        /// </summary>
        public virtual string? Comment { get; set; }

        /// <inheritdoc />
        string ITableMigrationOperation.Table
            => Name;
    }
}
