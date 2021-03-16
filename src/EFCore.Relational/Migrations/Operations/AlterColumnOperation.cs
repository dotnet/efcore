// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing column.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} ALTER COLUMN {Name}")]
    public class AlterColumnOperation : ColumnOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     An operation representing the column as it was before being altered.
        /// </summary>
        public virtual ColumnOperation OldColumn { get; [param: NotNull] set; } = new AddColumnOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
            => OldColumn;
    }
}
