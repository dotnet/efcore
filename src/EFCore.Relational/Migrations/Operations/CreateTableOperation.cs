// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for creating a new table.
    /// </summary>
    [DebuggerDisplay("CREATE TABLE {Name}")]
    public class CreateTableOperation : TableOperation
    {
        /// <summary>
        ///     The <see cref="AddPrimaryKeyOperation" /> representing the creation of the primary key for the table.
        /// </summary>
        public virtual AddPrimaryKeyOperation? PrimaryKey { get; set; }

        /// <summary>
        ///     An ordered list of <see cref="AddColumnOperation" /> for adding columns to the table.
        /// </summary>
        public virtual List<AddColumnOperation> Columns { get; } = new();

        /// <summary>
        ///     A list of <see cref="AddForeignKeyOperation" /> for creating foreign key constraints in the table.
        /// </summary>
        public virtual List<AddForeignKeyOperation> ForeignKeys { get; } = new();

        /// <summary>
        ///     A list of <see cref="AddUniqueConstraintOperation" /> for creating unique constraints in the table.
        /// </summary>
        public virtual List<AddUniqueConstraintOperation> UniqueConstraints { get; } = new();

        /// <summary>
        ///     A list of <see cref="AddCheckConstraintOperation" /> for creating check constraints in the table.
        /// </summary>
        public virtual List<AddCheckConstraintOperation> CheckConstraints { get; } = new();
    }
}
