// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Data.SQLite.Interop;

namespace Microsoft.Data.SQLite
{
    public static class SQLiteEngine
    {
        public static void DeleteDatabase(string filename)
        {
            // TODO: Consider passing in vfs name
            var ptrVfs = NativeMethods.sqlite3_vfs_find(IntPtr.Zero);
            var ptrFilename = MarshalEx.StringToHGlobalUTF8(filename);
            try
            {
                var vfs = Marshal.PtrToStructure<sqlite3_vfs>(ptrVfs);
                vfs.xDelete(ptrVfs, ptrFilename, 1);
            }
            finally
            {
                if (ptrFilename != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrFilename);
            }
        }
    }
}
