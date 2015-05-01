// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if NET45 || DNX451 || DNXCORE50

using Microsoft.Framework.Internal;

#if DNX451 || DNXCORE50
using System.IO;
using System.Reflection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Microsoft.Framework.DependencyInjection;
#endif

#endif

namespace Microsoft.Data.Sqlite.Interop
{
    internal static class NativeMethods
    {
#if NET45 || DNX451 || DNXCORE50
        static NativeMethods()
        {
            try
            {
                var loaded = NativeLibraryLoader.TryLoad("sqlite3");

#if DNX451 || DNXCORE50
                if (!loaded)
                {
                    var library = CallContextServiceLocator
                        .Locator
                        .ServiceProvider
                        .GetRequiredService<ILibraryManager>()
                        .GetLibraryInformation(typeof(NativeMethods).GetTypeInfo().Assembly.GetName().Name);

                    var installPath = library.Path;
                    if (library.Type == "Project")
                    {
                        installPath = Path.GetDirectoryName(installPath);
                    }

                    loaded =  NativeLibraryLoader.TryLoad("sqlite3", Path.Combine(installPath, "redist"));
                }
#endif

                Debug.Assert(loaded, "loaded is false.");
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
#endif

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_blob(Sqlite3StmtHandle pStmt, int i, byte[] zData, int nData, IntPtr xDel);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_double(Sqlite3StmtHandle pStmt, int i, double rValue);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_int64(Sqlite3StmtHandle pStmt, int i, long iValue);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_null(Sqlite3StmtHandle pStmt, int i);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_bind_parameter_index(Sqlite3StmtHandle pStmt, IntPtr zName);

        public static int sqlite3_bind_parameter_index(Sqlite3StmtHandle pStmt, string zName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zName);
            try
            {
                return sqlite3_bind_parameter_index(pStmt, ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int sqlite3_bind_text16(Sqlite3StmtHandle pStmt, int i, string zData, int n, IntPtr xDel);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_changes(Sqlite3Handle db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_blob_raw(Sqlite3StmtHandle pStmt, int iCol);

        public static byte[] sqlite3_column_blob(Sqlite3StmtHandle pStmt, int iCol)
        {
            var ptr = sqlite3_column_blob_raw(pStmt, iCol);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var bytes = sqlite3_column_bytes(pStmt, iCol);

            var result = new byte[bytes];
            Marshal.Copy(ptr, result, 0, bytes);

            return result;
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_column_bytes(Sqlite3StmtHandle pStmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_count(Sqlite3StmtHandle stmt);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_decltype16", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_column_decltype16_raw(Sqlite3StmtHandle stmt, int iCol);

        public static string sqlite3_column_decltype16(Sqlite3StmtHandle stmt, int iCol) => Marshal.PtrToStringUni(sqlite3_column_decltype16_raw(stmt, iCol));

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern double sqlite3_column_double(Sqlite3StmtHandle stmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern long sqlite3_column_int64(Sqlite3StmtHandle stmt, int iCol);

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_name16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_name16_raw(Sqlite3StmtHandle stmt, int iCol);

        public static string sqlite3_column_name16(Sqlite3StmtHandle stmt, int iCol) => Marshal.PtrToStringUni(sqlite3_column_name16_raw(stmt, iCol));

        [DllImport("sqlite3", EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_text16_raw(Sqlite3StmtHandle stmt, int iCol);

        public static string sqlite3_column_text16(Sqlite3StmtHandle stmt, int iCol) => Marshal.PtrToStringUni(sqlite3_column_text16_raw(stmt, iCol));

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_type(Sqlite3StmtHandle stmt, int iCol);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_db_filename(Sqlite3Handle db, IntPtr zDbName);

        public static string sqlite3_db_filename(Sqlite3Handle db, string zDbName)
        {
            var ptr = MarshalEx.StringToHGlobalUTF8(zDbName);
            try
            {
                return MarshalEx.PtrToStringUTF8(sqlite3_db_filename(db, ptr));
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
        
        [DllImport("sqlite3", EntryPoint = "sqlite3_errmsg16", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_errmsg16_raw(Sqlite3Handle db);

        public static string sqlite3_errmsg16(Sqlite3Handle db) => Marshal.PtrToStringUni(sqlite3_errmsg16_raw(db));

        [DllImport("sqlite3", EntryPoint = "sqlite3_errstr", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_errstr_raw(int rc);

        public static string sqlite3_errstr(int rc) => MarshalEx.PtrToStringUTF8(sqlite3_errstr_raw(rc));

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_finalize(IntPtr pStmt);

        [DllImport("sqlite3", EntryPoint = "sqlite3_libversion", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_libversion_raw();

        public static string sqlite3_libversion() => MarshalEx.PtrToStringUTF8(sqlite3_libversion_raw());

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int sqlite3_open16(string filename, out Sqlite3Handle ppDb);

        // sqlite3.dll converts this to UTF-8 to parse
        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_prepare16_v2(Sqlite3Handle db, IntPtr zSql, int nByte, out Sqlite3StmtHandle ppStmt, out IntPtr pzTail);

        public static int sqlite3_prepare16_v2(Sqlite3Handle db, string zSql, int nByte, out Sqlite3StmtHandle ppStmt, out string pzTail)
        {
            var zSqlPtr = Marshal.StringToHGlobalUni(zSql);
            try
            {
                // TODO: Something fancy with indexes?
                IntPtr pzTailPtr;
                var rc = sqlite3_prepare16_v2(db, zSqlPtr, nByte, out ppStmt, out pzTailPtr);
                pzTail = Marshal.PtrToStringUni(pzTailPtr);

                return rc;
            }
            finally
            {
                Marshal.FreeHGlobal(zSqlPtr);
            }
        }

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_step(Sqlite3StmtHandle stmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_stmt_readonly(Sqlite3StmtHandle pStmt);
    }
}
