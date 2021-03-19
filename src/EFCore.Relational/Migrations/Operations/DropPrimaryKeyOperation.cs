// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping a primary key.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} DROP CONSTRAINT {Name}")]
    public class DropPrimaryKeyOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The name of the primary key constraint.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The table that contains the primary key.
        /// </summary>
        public virtual string Table { get; set; } = null!;
    }
}
