// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     Represents a migration operation on a table.
    /// </summary>
    public interface ITableMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The table that contains the target of this operation.
        /// </summary>
        string Table { get; }
    }
}
