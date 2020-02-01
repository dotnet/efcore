// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for creating a new check constraint.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} CHECK")]
    public class CreateCheckConstraintOperation : MigrationOperation
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
        ///     The table schema that contains the check constraint, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The logical sql expression used in a CHECK constraint and returns TRUE or FALSE.
        ///     Sql used with CHECK constraints cannot reference another table
        ///     but can reference other columns in the same table for the same row.
        ///     The expression cannot reference an alias data type.
        /// </summary>
        public virtual string Sql { get; [param: NotNull] set; }
    }
}
