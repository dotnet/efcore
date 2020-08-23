// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class BundleInitializer
    {
        private const int SQLITE_WIN32_DATA_DIRECTORY_TYPE = 1;
        private const int SQLITE_WIN32_TEMP_DIRECTORY_TYPE = 2;

        public static void Initialize()
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(new AssemblyName("SQLitePCLRaw.batteries_v2"));
            }
            catch
            {
            }

            if (assembly != null)
            {
                assembly.GetType("SQLitePCL.Batteries_V2").GetTypeInfo().GetDeclaredMethod("Init")
                    .Invoke(null, null);
            }

            if (ApplicationDataHelper.CurrentApplicationData != null)
            {
                var rc = sqlite3_win32_set_directory(
                    SQLITE_WIN32_DATA_DIRECTORY_TYPE,
                    ApplicationDataHelper.LocalFolderPath);
                SqliteException.ThrowExceptionForRC(rc, db: null);

                rc = sqlite3_win32_set_directory(
                    SQLITE_WIN32_TEMP_DIRECTORY_TYPE,
                    ApplicationDataHelper.TemporaryFolderPath);
                SqliteException.ThrowExceptionForRC(rc, db: null);
            }
        }
    }
}
