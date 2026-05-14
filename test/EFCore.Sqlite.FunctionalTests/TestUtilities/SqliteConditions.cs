// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Static helpers consumed by Arcade's <see cref="Xunit.ConditionalFactAttribute" /> /
///     <see cref="Xunit.ConditionalTheoryAttribute" /> via <c>typeof(SqliteConditions)</c> +
///     <c>nameof(...)</c>.
/// </summary>
public static class SqliteConditions
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

    public static bool SpatialiteAvailable
        => SpatialiteAvailableLazy.Value;

    /// <summary>
    ///     SQLite version >= 3.35.0 (used by tests that depend on STRICT tables / RETURNING).
    /// </summary>
    public static bool VersionAtLeast3_35
        => CurrentVersionLazy.Value is { } v && v >= new Version(3, 35, 0);

    // Re-exported universal helpers so that combined skips can use a single calleeType.
    public static bool NotOnHelix
        => TestConditions.NotOnHelix;

    public static bool NotOnCI
        => TestConditions.NotOnCI;

    public static bool NotOnMac
        => TestConditions.NotOnMac;

    public static bool NotOnLinux
        => TestConditions.NotOnLinux;

    public static bool NotOnWindows
        => TestConditions.NotOnWindows;

    public static bool NotOnLinuxOrMac
        => TestConditions.NotOnLinuxOrMac;
}
