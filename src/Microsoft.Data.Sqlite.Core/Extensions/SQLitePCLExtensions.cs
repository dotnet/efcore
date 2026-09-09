// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        private static readonly Dictionary<string, bool> _knownLibraries = new()
        {
            { "e_sqlcipher", true },
            { "e_sqlite3", false },
            { "e_sqlite3mc", true },
            { "sqlcipher", true },
            { "sqlite3mc", true },
            { "winsqlite3", false }
        };

        public static bool? EncryptionSupported()
            => EncryptionSupported(out _);

        public static bool? EncryptionSupported(out string libraryName)
        {
            libraryName = raw.GetNativeLibraryName();

            return _knownLibraries.TryGetValue(libraryName, out var supported)
                ? supported
                : default(bool?);
        }
    }
}
