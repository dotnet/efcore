// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        public static bool EncryptionNotSupported()
            => SQLitePCL.raw.GetNativeLibraryName() == "e_sqlite3";
    }
}
