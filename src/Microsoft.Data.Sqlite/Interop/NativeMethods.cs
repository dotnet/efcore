// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Data.Sqlite.Interop
{
    internal static partial class NativeMethods
    {
        private static string _dllName = "sqlite3";
        private static Lazy<ISqlite3> _sqlite3 = new Lazy<ISqlite3>(() => Load(_dllName));

        public static void SetDllName(string dllName)
        {
            if (_sqlite3.IsValueCreated)
            {
                throw new InvalidOperationException(Strings.AlreadyLoaded);
            }

            _dllName = dllName;
        }

        private static ISqlite3 Sqlite3 => _sqlite3.Value;

        public static int sqlite3_bind_blob(Sqlite3StmtHandle pStmt, int i, byte[] zData, int nData, IntPtr xDel)
            => Sqlite3.bind_blob(pStmt, i, zData, nData, xDel);

        public static int sqlite3_bind_double(Sqlite3StmtHandle pStmt, int i, double rValue)
            => Sqlite3.bind_double(pStmt, i, rValue);

        public static int sqlite3_bind_int64(Sqlite3StmtHandle pStmt, int i, long iValue)
            => Sqlite3.bind_int64(pStmt, i, iValue);

        public static int sqlite3_bind_null(Sqlite3StmtHandle pStmt, int i)
            => Sqlite3.bind_null(pStmt, i);

        public static int sqlite3_bind_parameter_count(Sqlite3StmtHandle stmt)
            => Sqlite3.bind_parameter_count(stmt);

        public static string sqlite3_bind_parameter_name(Sqlite3StmtHandle stmt, int i)
            => MarshalEx.PtrToStringUTF8(Sqlite3.bind_parameter_name(stmt, i));

        public static int sqlite3_bind_parameter_index(Sqlite3StmtHandle pStmt, string zName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zName);
            try
            {
                return Sqlite3.bind_parameter_index(pStmt, ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static int sqlite3_bind_text(Sqlite3StmtHandle pStmt, int i, string data, IntPtr xDel)
        {
            int nLen;
            var zData = MarshalEx.StringToHGlobalUTF8(data, out nLen);
            try
            {
                return Sqlite3.bind_text(pStmt, i, zData, nLen, xDel);
            }
            finally
            {
                if (zData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zData);
                }
            }
        }

        public static int sqlite3_busy_timeout(Sqlite3Handle db, int ms)
            => Sqlite3.busy_timeout(db, ms);

        public static int sqlite3_changes(Sqlite3Handle db)
            => Sqlite3.changes(db);

        public static int sqlite3_close(IntPtr db)
            => Sqlite3.close(db);

        public static int sqlite3_close_v2(IntPtr db)
            => Sqlite3.close_v2(db);

        // TODO can be Array.Empty<T>() when upgrading to net46
        private static byte[] EmptyByteArray = new byte[0];

        public static byte[] sqlite3_column_blob(Sqlite3StmtHandle pStmt, int iCol)
        {
            var ptr = Sqlite3.column_blob(pStmt, iCol);
            if (ptr == IntPtr.Zero)
            {
                return EmptyByteArray;
            }

            var bytes = Sqlite3.column_bytes(pStmt, iCol);

            var result = new byte[bytes];
            Marshal.Copy(ptr, result, 0, bytes);

            return result;
        }

        public static int sqlite3_column_count(Sqlite3StmtHandle stmt)
            => Sqlite3.column_count(stmt);

        public static string sqlite3_column_decltype(Sqlite3StmtHandle stmt, int iCol)
            => MarshalEx.PtrToStringUTF8(Sqlite3.column_decltype(stmt, iCol));

        public static double sqlite3_column_double(Sqlite3StmtHandle stmt, int iCol)
            => Sqlite3.column_double(stmt, iCol);

        public static long sqlite3_column_int64(Sqlite3StmtHandle stmt, int iCol)
            => Sqlite3.column_int64(stmt, iCol);

        public static string sqlite3_column_name(Sqlite3StmtHandle stmt, int iCol)
            => MarshalEx.PtrToStringUTF8(Sqlite3.column_name(stmt, iCol));

        public static string sqlite3_column_text(Sqlite3StmtHandle stmt, int iCol)
            => MarshalEx.PtrToStringUTF8(Sqlite3.column_text(stmt, iCol));

        public static int sqlite3_column_type(Sqlite3StmtHandle stmt, int iCol)
            => Sqlite3.column_type(stmt, iCol);

        public static string sqlite3_db_filename(Sqlite3Handle db, string zDbName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zDbName);
            try
            {
                return MarshalEx.PtrToStringUTF8(Sqlite3.db_filename(db, ptr));
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static int sqlite3_enable_load_extension(Sqlite3Handle db, int onoff)
            => Sqlite3.enable_load_extension(db, onoff);

        public static string sqlite3_errmsg(Sqlite3Handle db)
            => MarshalEx.PtrToStringUTF8(Sqlite3.errmsg(db));

        public static string sqlite3_errstr(int rc)
            => MarshalEx.PtrToStringUTF8(Sqlite3.errstr(rc));

        public static int sqlite3_finalize(IntPtr pStmt)
            => Sqlite3.finalize(pStmt);

        public static string sqlite3_libversion()
            => MarshalEx.PtrToStringUTF8(Sqlite3.libversion());

        public static int sqlite3_open_v2(string filename, out Sqlite3Handle ppDb, int flags, string vfs)
        {
            var zFilename = MarshalEx.StringToHGlobalUTF8(filename);
            var zVfs = string.IsNullOrEmpty(vfs) ? IntPtr.Zero : MarshalEx.StringToHGlobalUTF8(vfs);
            try
            {
                return Sqlite3.open_v2(zFilename, out ppDb, flags, zVfs);
            }
            finally
            {
                if (zFilename != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zFilename);
                }
                if (zVfs != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zVfs);
                }
            }
        }

        public static int sqlite3_prepare_v2(Sqlite3Handle db, string zSql, out Sqlite3StmtHandle ppStmt, out string pzTail)
        {
            int nByte;
            var zSqlPtr = MarshalEx.StringToHGlobalUTF8(zSql, out nByte);
            try
            {
                IntPtr pzTailPtr;
                var rc = Sqlite3.prepare_v2(db, zSqlPtr, nByte, out ppStmt, out pzTailPtr);
                pzTail = MarshalEx.PtrToStringUTF8(pzTailPtr);

                return rc;
            }
            finally
            {
                Marshal.FreeHGlobal(zSqlPtr);
            }
        }

        public static int sqlite3_reset(Sqlite3StmtHandle stmt)
            => Sqlite3.reset(stmt);

        public static int sqlite3_step(Sqlite3StmtHandle stmt)
            => Sqlite3.step(stmt);

        public static int sqlite3_stmt_readonly(Sqlite3StmtHandle pStmt)
            => Sqlite3.stmt_readonly(pStmt);

        private interface ISqlite3
        {
            int bind_blob(Sqlite3StmtHandle pStmt, int i, byte[] zData, int nData, IntPtr xDel);
            int bind_double(Sqlite3StmtHandle pStmt, int i, double rValue);
            int bind_int64(Sqlite3StmtHandle pStmt, int i, long iValue);
            int bind_null(Sqlite3StmtHandle pStmt, int i);
            int bind_parameter_count(Sqlite3StmtHandle stmt);
            int bind_parameter_index(Sqlite3StmtHandle pStmt, IntPtr zName);
            IntPtr bind_parameter_name(Sqlite3StmtHandle stmt, int i);
            int bind_text(Sqlite3StmtHandle pStmt, int i, IntPtr zData, int n, IntPtr xDel);
            int busy_timeout(Sqlite3Handle db, int ms);
            int changes(Sqlite3Handle db);
            int close(IntPtr db);
            int close_v2(IntPtr db);
            IntPtr column_blob(Sqlite3StmtHandle pStmt, int iCol);
            int column_bytes(Sqlite3StmtHandle pStmt, int iCol);
            int column_count(Sqlite3StmtHandle stmt);
            IntPtr column_decltype(Sqlite3StmtHandle stmt, int iCol);
            double column_double(Sqlite3StmtHandle stmt, int iCol);
            long column_int64(Sqlite3StmtHandle stmt, int iCol);
            IntPtr column_name(Sqlite3StmtHandle stmt, int iCol);
            IntPtr column_text(Sqlite3StmtHandle stmt, int iCol);
            int column_type(Sqlite3StmtHandle stmt, int iCol);
            IntPtr db_filename(Sqlite3Handle db, IntPtr zDbName);
            int enable_load_extension(Sqlite3Handle db, int onoff);
            IntPtr errmsg(Sqlite3Handle db);
            IntPtr errstr(int rc);
            int finalize(IntPtr pStmt);
            IntPtr libversion();
            int open_v2(IntPtr filename, out Sqlite3Handle ppDb, int flags, IntPtr vfs);
            int prepare_v2(Sqlite3Handle db, IntPtr zSql, int nByte, out Sqlite3StmtHandle ppStmt, out IntPtr pzTail);
            int reset(Sqlite3StmtHandle stmt);
            int step(Sqlite3StmtHandle stmt);
            int stmt_readonly(Sqlite3StmtHandle pStmt);
        }
    }
}
