// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    public static class SqliteEngine
    {
        public static void UseWinSqlite3()
            => NativeMethods.SetDllName("winsqlite3");
    }
}
