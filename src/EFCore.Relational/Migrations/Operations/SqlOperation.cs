// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for raw SQL commands.
    /// </summary>
    [DebuggerDisplay("{Sql}")]
    public class SqlOperation : MigrationOperation
    {
        /// <summary>
        ///     The SQL string to be executed to perform this operation.
        /// </summary>
        public virtual string Sql { get; set; } = null!;

        /// <summary>
        ///     Indicates whether or not transactions will be suppressed while executing the SQL.
        /// </summary>
        public virtual bool SuppressTransaction { get; set; }
    }
}
