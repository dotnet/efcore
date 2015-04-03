// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Sqlite.Interop
{
    internal static class Constants
    {
        public const int SQLITE_OK = 0;

        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;

        public const int SQLITE_OPEN_READONLY = 1;
        public const int SQLITE_OPEN_READWRITE = 2;
        public const int SQLITE_OPEN_CREATE = 4;
        public const int SQLITE_OPEN_URI = 0x40;
        public const int SQLITE_OPEN_MEMORY = 0x80;
        public const int SQLITE_OPEN_NOMUTEX = 0x8000;
        public const int SQLITE_OPEN_FULLMUTEX = 0x10000;
        public const int SQLITE_OPEN_SHAREDCACHE = 0x20000;
        public const int SQLITE_OPEN_PRIVATECACHE = 0x40000;

        public const int SQLITE_INTEGER = 1;
        public const int SQLITE_FLOAT = 2;
        public const int SQLITE_TEXT = 3;
        public const int SQLITE_BLOB = 4;
        public const int SQLITE_NULL = 5;

        public static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);
    }
}
