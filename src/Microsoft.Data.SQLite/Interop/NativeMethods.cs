// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.SQLite.Interop
{
    internal static class NativeMethods
    {
        public const int SQLITE_OK = 0;

        [DllImport("sqlite3")]
        public static extern int sqlite3_changes(IntPtr db);

        [DllImport("sqlite3")]
        public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport("sqlite3", EntryPoint = "sqlite3_errstr")]
        private static extern IntPtr sqlite3_errstr_raw(int rc);

        public static string sqlite3_errstr(int rc)
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_errstr_raw(rc));
        }

        [DllImport("sqlite3")]
        public static extern int sqlite3_finalize(IntPtr pStmt);

        [DllImport("sqlite3", EntryPoint = "sqlite3_libversion")]
        private static extern IntPtr sqlite3_libversion_raw();

        public static string sqlite3_libversion()
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_libversion_raw());
        }

        [DllImport("sqlite3")]
        private static extern int sqlite3_open(IntPtr filename, out IntPtr ppDb);

        public static int sqlite3_open(string filename, out IntPtr ppDb)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(filename);
            try
            {
                return sqlite3_open(ptr, out ppDb);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport("sqlite3")]
        private static extern int sqlite3_prepare_v2(
            IntPtr db,
            IntPtr zSql,
            int nByte,
            out IntPtr ppStmt,
            IntPtr pzTail);

        public static int sqlite3_prepare_v2(
            IntPtr db,
            string zSql,
            int nByte,
            out IntPtr ppStmt,
            // TODO: ref string?
            IntPtr pzTail)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zSql);
            try
            {
                return sqlite3_prepare_v2(db, ptr, nByte, out ppStmt, pzTail);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport("sqlite3")]
        public static extern int sqlite3_reset(IntPtr pStmt);

        [DllImport("sqlite3")]
        public static extern int sqlite3_step(IntPtr pStmt);
    }
}
