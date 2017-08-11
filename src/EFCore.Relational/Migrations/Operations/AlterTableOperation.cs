// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing table.
    /// </summary>
    public class AlterTableOperation : MigrationOperation, IAlterMigrationOperation
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
        ///     An operation representing the table as it was before being altered.
        /// </summary>
        public virtual Annotatable OldTable { get; [param: NotNull] set; } = new Annotatable();

        /// <summary>
        ///     The <see cref="OldTable" /> exposed to examine annotations.
        /// </summary>
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldTable;
    }
}
