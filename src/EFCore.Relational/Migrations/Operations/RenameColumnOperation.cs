// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for renaming an existing column.
    /// </summary>
    public class RenameColumnOperation : MigrationOperation
    {
        /// <summary>
        ///     The old name of the column.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: NotNull] set; }

        /// <summary>
        ///     The name of the table that contains the column.
        /// </summary>
        public virtual string Table { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The new name for the column.
        /// </summary>
        public virtual string NewName { get; [param: NotNull] set; }
    }
}
