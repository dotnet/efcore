// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.Data.Sqlite.Upgrade.Tests;

internal static class UpgradeDbFiles
{
    public const string DirectoryName = "upgrade-dbs";

    public static string Directory
        => Path.Combine(AppContext.BaseDirectory, DirectoryName);

    public static string GetPath(string fileName)
        => Path.Combine(Directory, fileName);

    public static void EnsureExists(string fileName)
    {
        var full = GetPath(fileName);
        if (!File.Exists(full))
        {
            Assert.Fail(
                $"Expected seeded database '{full}' not found. "
                + "Run test/run-sqlite-upgrade-tests.cmd (or .sh) first so the e_sqlite3 seed stage produces the DB files.");
        }
    }
}
