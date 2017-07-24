// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for updating seed data in an existing table.
    /// </summary>
    public class UpdateDataOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the table in which data will be updated.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that will be used to identify
        ///     the rows that should be updated.
        /// </summary>
        public virtual string[] KeyColumns { get; [param: NotNull] set; }
        
        /// <summary>
        ///     The rows to be updated, represented as a list of key value arrays where each
        ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
        /// </summary>
        public virtual object[,] KeyValues { get; [param: NotNull] set; }

        /// <summary>
        ///     A list of column names that represent the columns that contain data to be updated.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }
        
        /// <summary>
        ///     The data to be updated, represented as a list of value arrays where each
        ///     value in the array corresponds to a column in the <see cref="Columns" /> property.
        /// </summary>
        public virtual object[,] Values { get; [param: NotNull] set; }
    }
}
