// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Enables configuration of global SQLite settings.
    /// </summary>
    public static class SqliteEngine
    {
        /// <summary>
        /// Configures Microsoft.Data.Sqlite to use winsqlite3.dll. This is a version of SQLite that ships in Windows
        /// 10. This method must be called before any other interaction with SQLite.
        /// </summary>
        public static void UseWinSqlite3()
            => NativeMethods.SetDllName("winsqlite3");
    }
}
