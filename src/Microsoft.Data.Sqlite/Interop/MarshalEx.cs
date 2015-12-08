// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite.Interop
{
    internal static class MarshalEx
    {
        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var i = 0;
            while (Marshal.ReadByte(ptr, i) != 0)
            {
                i++;
            }

            var bytes = new byte[i];
            Marshal.Copy(ptr, bytes, 0, i);

            return Encoding.UTF8.GetString(bytes, 0, i);
        }

        public static IntPtr StringToHGlobalUTF8(string s, out int length)
        {
            if (s == null)
            {
                length = 0;
                return IntPtr.Zero;
            }

            var bytes = Encoding.UTF8.GetBytes(s);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            length = bytes.Length;

            return ptr;
        }

        public static IntPtr StringToHGlobalUTF8(string s)
        {
            int temp;
            return StringToHGlobalUTF8(s, out temp);
        }

        public static void ThrowExceptionForRC(int rc, Sqlite3Handle db)
        {
            if (rc == SQLITE_OK
                || rc == SQLITE_ROW
                || rc == SQLITE_DONE)
            {
                return;
            }

            var message = db == null || db.IsInvalid
                ? VersionedMethods.GetErrorString(rc)
                : NativeMethods.sqlite3_errmsg(db);

            throw new SqliteException(Strings.FormatSqliteNativeError(rc, message), rc);
        }
    }
}
