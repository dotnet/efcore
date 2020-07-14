// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

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
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The filename to use for the database, or <see langword="null" /> to let SQL Server choose.
        /// </summary>
        public virtual string FileName { get; [param: CanBeNull] set; }
    }
}
