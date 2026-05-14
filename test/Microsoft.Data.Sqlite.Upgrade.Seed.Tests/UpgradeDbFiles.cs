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

    public static void EnsureDirectory()
        => System.IO.Directory.CreateDirectory(Directory);

    public static void Delete(params string[] fileNames)
    {
        EnsureDirectory();
        foreach (var fileName in fileNames)
        {
            var full = GetPath(fileName);
            if (File.Exists(full))
            {
                File.Delete(full);
            }

            foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            {
                var sidecar = full + suffix;
                if (File.Exists(sidecar))
                {
                    File.Delete(sidecar);
                }
            }
        }
    }
}
