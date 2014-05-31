// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.SQLite.Interop
{
    internal delegate int SQLiteDeleteDelegate(IntPtr pVfs, IntPtr zName, int syncDir);

    internal struct sqlite3_vfs
    {
        public int iVersion;
        public int szOsFile;
        public int mxPathname;
        public IntPtr pNext;
        public IntPtr zName;
        public IntPtr pAppData;
        public IntPtr xOpen;
        public SQLiteDeleteDelegate xDelete;
        public IntPtr xAccess;
        public IntPtr xFullPathname;
        public IntPtr xDlOpen;
        public IntPtr xDlError;
        public IntPtr xDlSym;
        public IntPtr xDlClose;
        public IntPtr xRandomness;
        public IntPtr xSleep;
        public IntPtr xCurrentTime;
        public IntPtr xGetLastError;
    }
}
