// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class SqliteTestEnvironment
{
    private static readonly Lazy<bool> SpatialiteAvailableLazy
        = new(() =>
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            return SpatialiteLoader.TryLoad(connection);
        });

    private static readonly Lazy<Version?> CurrentVersionLazy
        = new(() =>
        {
            var connection = new SqliteConnection("Data Source=:memory:;");
            return connection.ServerVersion is null ? null : new Version(connection.ServerVersion);
        });

    // ---- Conditional* helpers consumed by [ConditionalFact(typeof(TestEnvironment), nameof(...))] ----

    public static bool SpatialiteAvailable => SpatialiteAvailableLazy.Value;

    /// <summary>
    ///     SQLite version >= 3.35.0 (required for STRICT tables / RETURNING).
    /// </summary>
    public static bool VersionAtLeast3_35
        => CurrentVersionLazy.Value is { } v && v >= new Version(3, 35, 0);

    /// <summary>
    ///     SQLite version >= 3.44.0 (required for ORDER BY inside aggregate functions, e.g. group_concat).
    /// </summary>
    public static bool VersionAtLeast3_44
        => CurrentVersionLazy.Value is { } v && v >= new Version(3, 44, 0);
}
