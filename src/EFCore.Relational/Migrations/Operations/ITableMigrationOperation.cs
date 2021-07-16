// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        string? Schema { get; }

        /// <summary>
        ///     The table that contains the target of this operation.
        /// </summary>
        string Table { get; }
    }
}
