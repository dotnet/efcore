// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A SQL Server-specific <see cref="MigrationOperation" /> to create a database.
    /// </summary>
    [DebuggerDisplay("CREATE DATABASE {Name}")]
    public class SqlServerCreateDatabaseOperation : DatabaseOperation
    {
        /// <summary>
        ///     The name of the database.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The filename to use for the database, or <see langword="null" /> to let SQL Server choose.
        /// </summary>
        public virtual string? FileName { get; set; }
    }
}
