// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for creating a new check constraint.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} CHECK")]
    public class AddCheckConstraintOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     The name of the check constraint.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The table of the check constraint.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The table schema that contains the check constraint, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The logical sql expression used in a CHECK constraint and returns TRUE or FALSE.
        ///     SQL used with CHECK constraints cannot reference another table
        ///     but can reference other columns in the same table for the same row.
        ///     The expression cannot reference an alias data type.
        /// </summary>
        public virtual string Sql { get; [param: NotNull] set; }

        /// <summary>
        ///     Creates a new <see cref="AddCheckConstraintOperation" /> from the specified check constraint.
        /// </summary>
        /// <param name="checkConstraint"> The check constraint. </param>
        /// <returns> The operation. </returns>
        public static AddCheckConstraintOperation CreateFrom([NotNull] ICheckConstraint checkConstraint)
        {
            Check.NotNull(checkConstraint, nameof(checkConstraint));

            var operation = new AddCheckConstraintOperation
            {
                Name = checkConstraint.Name,
                Sql = checkConstraint.Sql,
                Schema = checkConstraint.EntityType.GetSchema(),
                Table = checkConstraint.EntityType.GetTableName()
            };
            operation.AddAnnotations(checkConstraint.GetAnnotations());

            return operation;
        }
    }
}
