// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for creating a new table.
    /// </summary>
    public class CreateTableOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the table.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The <see cref="AddPrimaryKeyOperation" /> representing the creation of the primary key for the table.
        /// </summary>
        public virtual AddPrimaryKeyOperation PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     An ordered list of <see cref="AddColumnOperation" /> for adding columns to the table.
        /// </summary>
        public virtual List<AddColumnOperation> Columns { get; } = new List<AddColumnOperation>();

        /// <summary>
        ///     A list of <see cref="AddForeignKeyOperation" /> for creating foreign key constraints in the table.
        /// </summary>
        public virtual List<AddForeignKeyOperation> ForeignKeys { get; } = new List<AddForeignKeyOperation>();

        /// <summary>
        ///     A list of <see cref="AddUniqueConstraintOperation" /> for creating unique constraints in the table.
        /// </summary>
        public virtual List<AddUniqueConstraintOperation> UniqueConstraints { get; } = new List<AddUniqueConstraintOperation>();
    }
}
