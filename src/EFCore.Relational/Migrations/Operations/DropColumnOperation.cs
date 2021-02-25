// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping an existing column.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} DROP COLUMN {Name}")]
    public class DropColumnOperation : MigrationOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DropColumnOperation" />.
        /// </summary>
        // ReSharper disable once VirtualMemberCallInConstructor
        public DropColumnOperation()
            => IsDestructiveChange = true;

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table that contains that column.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }
    }
}
