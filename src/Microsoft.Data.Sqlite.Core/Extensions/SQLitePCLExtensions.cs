// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SQLitePCL
{
    internal static class SQLitePCLExtensions
    {
        public static void Dispose2(this sqlite3 db)
            => raw.sqlite3_close_v2(db);
    }
}
