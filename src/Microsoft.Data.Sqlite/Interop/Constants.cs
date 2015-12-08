// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Sqlite.Interop
{
    internal static class Constants
    {
        public const int SQLITE_OK = 0;

        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;

        public const int SQLITE_INTEGER = 1;
        public const int SQLITE_FLOAT = 2;
        public const int SQLITE_TEXT = 3;
        public const int SQLITE_BLOB = 4;
        public const int SQLITE_NULL = 5;

        public const int SQLITE_OPEN_READONLY = 0x00000001;
        public const int SQLITE_OPEN_READWRITE = 0x00000002;
        public const int SQLITE_OPEN_CREATE = 0x00000004;
        public const int SQLITE_OPEN_URI = 0x00000040;
        public const int SQLITE_OPEN_MEMORY = 0x00000080;
        public const int SQLITE_OPEN_SHAREDCACHE = 0x00020000;
        public const int SQLITE_OPEN_PRIVATECACHE = 0x00040000;

        public const int SQLITE_LOCKED = 6;

        public static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);
    }
}
