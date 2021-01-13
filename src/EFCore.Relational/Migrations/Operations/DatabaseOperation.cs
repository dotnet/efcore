// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for operations on databases.
    ///     See also <see cref="AlterDatabaseOperation" />.
    /// </summary>
    public abstract class DatabaseOperation : MigrationOperation
    {
        /// <summary>
        ///     The collation for the database, or <see langword="null" /> to use the default collation of the instance of SQL Server.
        /// </summary>
        public virtual string Collation { get; [param: CanBeNull] set; }
    }
}
