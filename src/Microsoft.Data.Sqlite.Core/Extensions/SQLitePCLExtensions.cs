// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        private static readonly Dictionary<string, bool> _knownLibraries = new Dictionary<string, bool>
        {
            { "e_sqlcipher", true },
            { "e_sqlite3", false },
            { "sqlcipher", true },
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
