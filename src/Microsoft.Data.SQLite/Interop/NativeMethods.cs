// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
#if NET451 || K10
using Microsoft.Data.SQLite.Utilities;
#endif

namespace Microsoft.Data.SQLite.Interop
{
    // TODO: Consider using UTF-16 overloads #Perf
    // TODO: Consider using function pointers instead of SQLITE_TRANSIENT #Perf
    internal static class NativeMethods
    {
#if NET451 || K10
        static NativeMethods()
        {
            NativeLibraryLoader.Load("sqlite3");
        }
#endif

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_bind_blob(StatementHandle pStmt, int i, IntPtr zData, int nData, IntPtr xDel);

        public static int sqlite3_bind_blob(StatementHandle pStmt, int i, byte[] zData)
        {
            var zDataPtr = Marshal.AllocHGlobal(zData.Length);
            try
            {
                Marshal.Copy(zData, 0, zDataPtr, zData.Length);

                return sqlite3_bind_blob(pStmt, i, zDataPtr, zData.Length, Constants.SQLITE_TRANSIENT);
            }
            finally
            {
                Marshal.FreeHGlobal(zDataPtr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_bind_double(StatementHandle pStmt, int i, double rValue);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_bind_int64(StatementHandle pStmt, int i, long iValue);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_bind_null(StatementHandle pStmt, int i);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_bind_parameter_index(StatementHandle pStmt, IntPtr zName);

        public static int sqlite3_bind_parameter_index(StatementHandle pStmt, string zName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zName);
            try
            {
                return sqlite3_bind_parameter_index(pStmt, ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_bind_text(StatementHandle pStmt, int i, IntPtr zData, int n, IntPtr xDel);

        public static int sqlite3_bind_text(StatementHandle pStmt, int i, string zData)
        {
            int size;
            var ptr = MarshalEx.StringToHGlobalUTF8(zData, out size);
            try
            {
                return sqlite3_bind_text(pStmt, i, ptr, size - 1, Constants.SQLITE_TRANSIENT);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_changes(DatabaseHandle db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_clear_bindings(StatementHandle pStmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_column_blob_raw(StatementHandle pStmt, int iCol);

        public static byte[] sqlite3_column_blob(StatementHandle pStmt, int iCol)
        {
            var ptr = sqlite3_column_blob_raw(pStmt, iCol);
            if (ptr == IntPtr.Zero)
                return null;

            var bytes = sqlite3_column_bytes(pStmt, iCol);

            var result = new byte[bytes];
            Marshal.Copy(ptr, result, 0, bytes);

            return result;
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_column_bytes(StatementHandle pStmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_column_count(StatementHandle pStmt);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_decltype", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_column_decltype_raw(StatementHandle pStmt, int N);

        public static string sqlite3_column_decltype(StatementHandle pStmt, int N)
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_column_decltype_raw(pStmt, N));
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double sqlite3_column_double(StatementHandle pStmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern long sqlite3_column_int64(StatementHandle pStmt, int iCol);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_column_name_raw(StatementHandle pStmt, int N);

        public static string sqlite3_column_name(StatementHandle pStmt, int N)
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_column_name_raw(pStmt, N));
        }

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_column_text_raw(StatementHandle pStmt, int iCol);

        public static string sqlite3_column_text(StatementHandle pStmt, int iCol)
        {
            return MarshalEx.PtrToStringUTF8(sqlite3_column_text_raw(pStmt, iCol));
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_column_type(StatementHandle pStmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_db_filename(DatabaseHandle db, IntPtr zDbName);

        public static string sqlite3_db_filename(DatabaseHandle db, string zDbName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zDbName);
            try
            {
                return MarshalEx.PtrToStringUTF8(sqlite3_db_filename(db, ptr));
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

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
            out StatementHandle ppStmt,
            out string pzTail)
        {
            int nByte;
            var zSqlPtr = MarshalEx.StringToHGlobalUTF8(zSql, out nByte);
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

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int sqlite3_stmt_readonly(StatementHandle pStmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr sqlite3_vfs_find(IntPtr zVfsName);
    }
}
