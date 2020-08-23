// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to add a new foreign key.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} PRIMARY KEY")]
    public class AddPrimaryKeyOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table to which the key should be added.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The name of the foreign key constraint.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered-list of column names for the columns that make up the primary key.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }

        /// <summary>
        ///     Creates a new <see cref="AddPrimaryKeyOperation" /> from the specified primary key.
        /// </summary>
        /// <param name="primaryKey"> The primary key. </param>
        /// <returns> The operation. </returns>
        public static AddPrimaryKeyOperation CreateFrom([NotNull] IPrimaryKeyConstraint primaryKey)
        {
            Check.NotNull(primaryKey, nameof(primaryKey));

            var operation = new AddPrimaryKeyOperation
            {
                Schema = primaryKey.Table.Schema,
                Table = primaryKey.Table.Name,
                Name = primaryKey.Name,
                Columns = primaryKey.Columns.Select(c => c.Name).ToArray()
            };
            operation.AddAnnotations(primaryKey.GetAnnotations());

            return operation;
        }
    }
}
