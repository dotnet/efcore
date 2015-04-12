// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NETCORE451

using System;
using System.IO;
using System.Runtime.InteropServices;

#if DNX451 || DNXCORE50
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Microsoft.Framework.DependencyInjection;
#endif

namespace Microsoft.Framework.Internal
{
    internal static class NativeLibraryLoader
    {
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 8;

        [DllImport("api-ms-win-core-libraryloader-l1-1-0", SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("api-ms-win-core-libraryloader-l1-1-0", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static bool TryLoad(string dllName)
        {
            string applicationBase;
#if NET451
            applicationBase = AppDomain.CurrentDomain.BaseDirectory;
#elif DNX451 || DNXCORE50
            applicationBase = CallContextServiceLocator
                .Locator
                .ServiceProvider
                .GetRequiredService<IApplicationEnvironment>()
                .ApplicationBasePath;
#else
            applicationBase = AppContext.BaseDirectory;
#endif

            return TryLoad(dllName, applicationBase);
        }

        public static bool TryLoad(string dllName, string applicationBase)
        {
            if (dllName == null)
            {
                throw new ArgumentNullException(nameof(dllName));
            }
            if (applicationBase == null)
            {
                throw new ArgumentNullException(nameof(applicationBase));
            }
            if (IsLoaded(dllName))
            {
                return true;
            }

            // TODO: Use GetSystemInfo (in api-ms-win-core-sysinfo-l1-1-0.dll) to detect ARM
            var architecture = IntPtr.Size == 4 ? "x86" : "x64";

            if (!dllName.EndsWith(".dll"))
            {
                dllName += ".dll";
            }

            var dllPath = Path.Combine(applicationBase, architecture, dllName);

            if (!File.Exists(dllPath))
            {
                return false;
            }

            var handle = LoadLibraryEx(dllPath, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            return handle != IntPtr.Zero;
        }

        public static bool IsLoaded(string dllName)
        {
            var handle = GetModuleHandle(dllName);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            return handle != IntPtr.Zero;
        }
    }
}

#endif
