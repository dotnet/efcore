// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

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
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Comment for this table
        /// </summary>
        public virtual string Comment { get; [param: CanBeNull] set; }

        /// <inheritdoc />
        string ITableMigrationOperation.Table
            => Name;
    }
}
