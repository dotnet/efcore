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

#if NET451 || K10
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Net.Runtime;
using Microsoft.Net.Runtime.Infrastructure;

namespace Microsoft.Data.SQLite.Utilities
{
    internal static class NativeLibraryLoader
    {
        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 8;

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void Load(string dllName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(dllName), "dllName is null or empty.");

            if (IsLoaded(dllName))
                return;

            var currentAssembly = typeof(NativeLibraryLoader).GetTypeInfo().Assembly;

            try
            {
                if (TryLoadUnderKRuntime(currentAssembly, dllName))
                    return;
            }
            catch (FileNotFoundException)
            {
                // Ignore. Running outside of Project K
            }

#if NET451
            if (TryLoadFromDirectory(dllName, new Uri(AppDomain.CurrentDomain.BaseDirectory).LocalPath))
                return;
#endif

            Debug.Fail(dllName + " was not loaded.");
        }

        private static bool TryLoadUnderKRuntime(Assembly currentAssembly, string dllName)
        {
            var serviceProvider = CallContextServiceLocator.Locator.ServiceProvider;

            var libraryManager = serviceProvider.GetService<ILibraryManager>();
            if (libraryManager != null)
            {
                var library = libraryManager.GetLibraryInformation(currentAssembly.GetName().Name);
                var libraryPath = library.Path;
                if (library.Type == "Project")
                    libraryPath = Path.GetDirectoryName(libraryPath);

                if (TryLoadFromDirectory(dllName, Path.Combine(libraryPath, "redist")))
                    return true;
            }

            var applicationEnvironment = serviceProvider.GetService<IApplicationEnvironment>();
            
            return applicationEnvironment != null
                && TryLoadFromDirectory(dllName, applicationEnvironment.ApplicationBasePath);
        }

        private static bool IsLoaded(string dllName)
        {
            var ptr = IntPtr.Zero;
            try
            {
                ptr = GetModuleHandle(dllName);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

            return ptr != IntPtr.Zero;
        }

        private static bool TryLoadFromDirectory(string dllName, string baseDirectory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(dllName), "dllName is null or empty.");
            Debug.Assert(!string.IsNullOrWhiteSpace(baseDirectory), "baseDirectory is null or empty.");
            Debug.Assert(Path.IsPathRooted(baseDirectory), "baseDirectory is not rooted.");

            var architecture = IntPtr.Size == 4
                ? "x86"
                : "x64";

            if (!dllName.EndsWith(".dll"))
                dllName = dllName + ".dll";

            var dllPath = Path.Combine(baseDirectory, architecture, dllName);
            if (!File.Exists(dllPath))
                return false;

            var ptr = IntPtr.Zero;
            try
            {
                ptr = LoadLibraryEx(dllPath, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

            return ptr != IntPtr.Zero;
        }
    }
}
#endif
