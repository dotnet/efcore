// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SQLitePCL;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class BundleInitializer
    {
        public static void Initialize()
        {
            SQLite3Provider_dynamic_cdecl.Setup("sqlite3", new MyGetFunctionPointer());
            SQLitePCL.raw.SetProvider(new SQLite3Provider_dynamic_cdecl());

            //Assembly assembly;
            //try
            //{
            //    assembly = Assembly.Load(new AssemblyName("SQLitePCLRaw.batteries_v2"));
            //}
            //catch
            //{
            //    return;
            //}

            //assembly.GetType("SQLitePCL.Batteries_V2").GetTypeInfo().GetDeclaredMethod("Init")
            //    .Invoke(null, null);
        }

        private class MyGetFunctionPointer : IGetFunctionPointer
        {
            private readonly IntPtr _dll;

            public MyGetFunctionPointer()
            {
                Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", "/usr/local/opt/sqlite/lib" + Path.PathSeparator + Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH"));            
                _dll = NativeLibrary.Load("sqlite3");
            }

            public IntPtr GetFunctionPointer(string name)
                => NativeLibrary.TryGetExport(_dll, name, out var f)
                    ? f
                    : IntPtr.Zero;
        }
    }
}
