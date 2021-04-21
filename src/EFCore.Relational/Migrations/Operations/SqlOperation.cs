// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
