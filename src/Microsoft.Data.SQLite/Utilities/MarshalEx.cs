// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.SQLite.Interop;

namespace Microsoft.Data.SQLite.Utilities
{
    internal static class MarshalEx
    {
        public static SQLiteException GetExceptionForRC(int errorCode)
        {
            if (errorCode == NativeMethods.SQLITE_OK)
                return null;

            return new SQLiteException(NativeMethods.sqlite3_errstr(errorCode), errorCode);
        }

        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            var i = 0;
            while (Marshal.ReadByte(ptr, i) != 0)
                i++;

            var bytes = new byte[i];
            Marshal.Copy(ptr, bytes, 0, i);

            return Encoding.UTF8.GetString(bytes, 0, i);
        }

        public static IntPtr StringToHGlobalUTF8(string s)
        {
            if (s == null)
                return IntPtr.Zero;

            var bytes = Encoding.UTF8.GetBytes(s);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);

            return ptr;
        }

        public static void ThrowExceptionForRC(int errorCode)
        {
            if (errorCode == NativeMethods.SQLITE_OK)
                return;

            throw GetExceptionForRC(errorCode);
        }
    }
}
