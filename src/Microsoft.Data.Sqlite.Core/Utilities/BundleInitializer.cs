// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
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
            Assembly? assembly = null;
            try
            {
                assembly = Assembly.Load(new AssemblyName("SQLitePCLRaw.batteries_v2"));
            }
            catch
            {
            }

            if (assembly != null)
            {
                assembly.GetType("SQLitePCL.Batteries_V2", throwOnError: true)!.GetMethod("Init", Type.EmptyTypes)!
                    .Invoke(null, null);
            }

            if (ApplicationDataHelper.CurrentApplicationData != null)
            {
                var rc = sqlite3_win32_set_directory(
                    SQLITE_WIN32_DATA_DIRECTORY_TYPE,
                    ApplicationDataHelper.LocalFolderPath);
                Debug.Assert(rc == SQLITE_OK);

                rc = sqlite3_win32_set_directory(
                    SQLITE_WIN32_TEMP_DIRECTORY_TYPE,
                    ApplicationDataHelper.TemporaryFolderPath);
                Debug.Assert(rc == SQLITE_OK);
            }
        }
    }
}
