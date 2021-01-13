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
    ///     A <see cref="MigrationOperation" /> to add a new unique constraint.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} UNIQUE")]
    public class AddUniqueConstraintOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table to which the constraint should be added.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The name of the constraint.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered-list of column names for the columns that make up the constraint.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }

        /// <summary>
        ///     Creates a new <see cref="AddUniqueConstraintOperation" /> from the specified unique constraint.
        /// </summary>
        /// <param name="uniqueConstraint"> The unique constraint. </param>
        /// <returns> The operation. </returns>
        public static AddUniqueConstraintOperation CreateFrom([NotNull] IUniqueConstraint uniqueConstraint)
        {
            Check.NotNull(uniqueConstraint, nameof(uniqueConstraint));

            var operation = new AddUniqueConstraintOperation
            {
                Schema = uniqueConstraint.Table.Schema,
                Table = uniqueConstraint.Table.Name,
                Name = uniqueConstraint.Name,
                Columns = uniqueConstraint.Columns.Select(c => c.Name).ToArray()
            };
            operation.AddAnnotations(uniqueConstraint.GetAnnotations());

            return operation;
        }
    }
}
