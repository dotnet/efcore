// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.SQLite.Interop
{
    internal static class NativeMethods
    {
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_changes(DatabaseHandle db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport(
            "sqlite3",
            EntryPoint = "sqlite3_errstr",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        private static extern IntPtr sqlite3_errstr_raw(int rc);

        public static string sqlite3_errstr(int rc)
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_errstr_raw(rc));
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_finalize(IntPtr pStmt);

        [DllImport(
            "sqlite3",
            EntryPoint = "sqlite3_libversion",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true)]
        private static extern IntPtr sqlite3_libversion_raw();

        public static string sqlite3_libversion()
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_libversion_raw());
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_open_v2(IntPtr filename, out DatabaseHandle ppDb, int flags, IntPtr zVfs);

        public static int sqlite3_open_v2(string filename, out DatabaseHandle ppDb, int flags, string zVfs)
        {
            var filenamePtr = MarshalEx.StringToHGlobalUTF8(filename);
            var zVfsPtr = MarshalEx.StringToHGlobalUTF8(zVfs);
            try
            {
                return sqlite3_open_v2(filenamePtr, out ppDb, flags, zVfsPtr);
            }
            finally
            {
                if (filenamePtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(filenamePtr);
                if (zVfsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(zVfsPtr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_prepare_v2(
            DatabaseHandle db,
            IntPtr zSql,
            int nByte,
            out StatementHandle ppStmt,
            out IntPtr pzTail);

        public static int sqlite3_prepare_v2(
            DatabaseHandle db,
            string zSql,
            int nByte,
            out StatementHandle ppStmt,
            out string pzTail)
        {
            var zSqlPtr = MarshalEx.StringToHGlobalUTF8(zSql);
            try
            {
                IntPtr pzTailPtr;
                var rc = sqlite3_prepare_v2(db, zSqlPtr, nByte, out ppStmt, out pzTailPtr);
                pzTail = MarshalEx.PtrToStringUTF8(pzTailPtr);

                return rc;
            }
            finally
            {
                if (zSqlPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(zSqlPtr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_reset(StatementHandle pStmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_step(StatementHandle pStmt);
    }
}
