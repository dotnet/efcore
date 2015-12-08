// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class NativeLibraryLoader
    {
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 8;
        private const short PROCESSOR_ARCHITECTURE_INTEL = 0;
        private const short PROCESSOR_ARCHITECTURE_ARM = 5;
        private const short PROCESSOR_ARCHITECTURE_AMD64 = 9;

        [DllImport("api-ms-win-core-libraryloader-l1-1-0", SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("api-ms-win-core-libraryloader-l1-1-0", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("api-ms-win-core-libraryloader-l1-1-0")]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        public static bool TryLoad(string dllName)
        {
            if (dllName == null)
            {
                throw new ArgumentNullException(nameof(dllName));
            }
            if (IsLoaded(dllName))
            {
                return true;
            }

            if (!dllName.EndsWith(".dll"))
            {
                dllName += ".dll";
            }

            var applicationBase = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(Path.Combine(applicationBase, dllName)))
            {
                return true;
            }

            var dllPath = Path.Combine(applicationBase, GetArchitecture(), dllName);

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
            if (dllName == null)
            {
                throw new ArgumentNullException(nameof(dllName));
            }
            if (IsDNX() || IsMono())
            {
                return true;
            }

            var handle = GetModuleHandle(dllName);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            return handle != IntPtr.Zero;
        }

        private static bool IsDNX()
        {
            var platformServices = Type.GetType(
                "Microsoft.Extensions.PlatformAbstractions.PlatformServices, Microsoft.Extensions.PlatformAbstractions");
            if (platformServices == null)
            {
                return false;
            }

            var defaultPlatformServices = platformServices.GetProperty("Default").GetValue(null);
            var application = defaultPlatformServices.GetType().GetProperty("Application").GetValue(defaultPlatformServices);
            var runtimeFramework = (FrameworkName)application.GetType().GetProperty("RuntimeFramework").GetValue(application);

            return runtimeFramework.Identifier == "DNX";
        }

        private static bool IsMono() => Type.GetType("Mono.Runtime") != null;

        private static string GetArchitecture()
        {
            SYSTEM_INFO systemInfo;
            GetSystemInfo(out systemInfo);

            switch (systemInfo.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return "x64";

                case PROCESSOR_ARCHITECTURE_ARM:
                    return "arm";

                default:
                    Debug.Assert(
                        systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL,
                        "Unexpected processor architecture: " + systemInfo.wProcessorArchitecture);
                    return "x86";
            }
        }

        private struct SYSTEM_INFO
        {
            public short wProcessorArchitecture;
            public short wReserved;
            public int dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public int dwNumberOfProcessors;
            public int dwProcessorType;
            public int dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }
    }
}

#endif
