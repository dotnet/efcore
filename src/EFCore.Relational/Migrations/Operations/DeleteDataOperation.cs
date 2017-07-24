// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for deleting seed data from an existing table.
    /// </summary>
    public class DeleteDataOperation : MigrationOperation
    {
        /// <summary>
        ///     The table from which data will be deleted.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be deleted.
        /// </summary>
        public virtual string[] KeyColumns { get; [param: NotNull] set; }

        /// <summary>
        ///     The rows to be deleted, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object[,] KeyValues { get; [param: NotNull] set; }
    }
}
