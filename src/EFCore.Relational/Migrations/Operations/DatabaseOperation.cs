// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for operations on databases.
///     See also <see cref="AlterDatabaseOperation" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public abstract class DatabaseOperation : MigrationOperation
{
    /// <summary>
    ///     The collation for the database, or <see langword="null" /> to use the default collation of the instance of SQL Server.
    /// </summary>
    public virtual string? Collation { get; set; }
}
