// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     SQLite specific extension methods for <see cref="MigrationBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqliteMigrationBuilderExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the SQLite provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="migrationBuilder">
    ///     The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
    ///     <see cref="Migration.Down(MigrationBuilder)" />.
    /// </param>
    /// <returns><see langword="true" /> if SQLite is being used; <see langword="false" /> otherwise.</returns>
    public static bool IsSqlite(this MigrationBuilder migrationBuilder)
        => string.Equals(
            migrationBuilder.ActiveProvider,
            typeof(SqliteOptionsExtension).Assembly.GetName().Name,
            StringComparison.Ordinal);
}
