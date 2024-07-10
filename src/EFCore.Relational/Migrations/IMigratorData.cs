// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A class that holds the results from the last migrations application.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public interface IMigratorData
{
    /// <summary>
    ///     The migrations that were applied to the database.
    /// </summary>
    public IReadOnlyList<Migration> AppliedMigrations { get; }

    /// <summary>
    ///     The migrations that were reverted from the database.
    /// </summary>
    public IReadOnlyList<Migration> RevertedMigrations { get; }

    /// <summary>
    ///     The target migration.
    ///     <see langword="null" /> if all migrations were reverted or no target migration was specified.
    /// </summary>
    public Migration? TargetMigration { get; }
}
