// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Data.SQLite.Interop
{
    internal static class MarshalEx
    {
        public static SQLiteException GetExceptionForRC(int errorCode)
        {
            if (errorCode == Constants.SQLITE_OK)
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
            int size;
            return StringToHGlobalUTF8(s, out size);
        }

        public static IntPtr StringToHGlobalUTF8(string s, out int size)
        {
            if (s == null)
            {
                size = 0;
                return IntPtr.Zero;
            }

            var bytes = Encoding.UTF8.GetBytes(s);
            size = bytes.Length + 1;
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);

            return ptr;
        }

        public static void ThrowExceptionForRC(int errorCode)
        {
            if (errorCode == Constants.SQLITE_OK)
                return;

            throw GetExceptionForRC(errorCode);
        }
    }
}
