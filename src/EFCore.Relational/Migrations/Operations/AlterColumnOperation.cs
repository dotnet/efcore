// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information.
    /// </remarks>
    [DebuggerDisplay("ALTER TABLE {Table} ALTER COLUMN {Name}")]
    public class AlterColumnOperation : ColumnOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     An operation representing the column as it was before being altered.
        /// </summary>
        public virtual ColumnOperation OldColumn { get; set; } = new AddColumnOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
            => OldColumn;
    }
}
