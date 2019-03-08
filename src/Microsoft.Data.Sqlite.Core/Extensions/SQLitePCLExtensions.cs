// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

using static SQLitePCL.raw;

namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        public static void Dispose2(this sqlite3 db)
            => sqlite3_close_v2(db);

        public static string GetProviderName()
        {
            string providerName = null;
            try
            {
                providerName = typeof(raw)
                    .GetField("_imp", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
                    .GetType().Name;
            }
            catch
            {
            }

            return providerName;
        }

        public static bool EncryptionNotSupported()
            => GetProviderName() == "SQLite3Provider_e_sqlite3";
    }
}
