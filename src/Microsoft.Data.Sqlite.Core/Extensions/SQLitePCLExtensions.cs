// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming
namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        public static bool EncryptionNotSupported()
            => raw.GetNativeLibraryName() == "e_sqlite3";
    }
}
