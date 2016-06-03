// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite.Internal
{
    /// <summary>
    /// Enables configuration of global SQLite settings. This API may change or be removed in future releases.
    /// </summary>
    public static class SqliteEngine
    {
        /// <summary>
        /// Configures Microsoft.Data.Sqlite to use winsqlite3.dll. This is a version of SQLite that ships in Windows
        /// 10. This method must be called before any other interaction with SQLite. This API may change or be removed
        /// in future releases.
        /// </summary>
        public static void UseWinSqlite3()
            => NativeMethods.SetDllName("winsqlite3");
    }
}
