// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    public class DesignTimeProjectLoadContext : AssemblyLoadContext
    {
        private readonly IDictionary<AssemblyName, string> _assemblyPaths;
        private readonly IDictionary<string, string> _nativeLibraries;
        private readonly IEnumerable<string> _searchPaths;

        private static readonly string NativeLibraryPrefix;
        private static readonly string NativeLibrarySuffix;
        private static readonly string[] ManagedAssemblyExtensions = new[]
        {
            ".dll",
            ".ni.dll",
            ".exe",
            ".ni.exe"
        };

        static DesignTimeProjectLoadContext()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                NativeLibraryPrefix = string.Empty;
                NativeLibrarySuffix = ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                NativeLibraryPrefix = "lib";
                NativeLibrarySuffix = ".dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                NativeLibraryPrefix = "lib";
                NativeLibrarySuffix = ".so";
            }
            else
            {
                NativeLibraryPrefix = string.Empty;
                NativeLibrarySuffix = string.Empty;
            }
        }

        public DesignTimeProjectLoadContext(
                [NotNull] IDictionary<AssemblyName, string> assemblyPaths,
                [NotNull] IDictionary<string, string> nativeLibraries,
                [NotNull] IEnumerable<string> searchPaths)
        {
            _assemblyPaths = assemblyPaths;
            _nativeLibraries = nativeLibraries;
            _searchPaths = searchPaths;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string path;
            if (_assemblyPaths.TryGetValue(assemblyName, out path) || SearchForLibrary(assemblyName.Name, out path, ManagedAssemblyExtensions))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        private bool SearchAlternateNativeLibraryName(string dllFileName, out string path)
        {
            if (_nativeLibraries.Count == 0)
            {
                path = null;
                return false;
            }

            var alternates = new [] {
                Path.GetFileNameWithoutExtension(dllFileName),
                $"{NativeLibraryPrefix}{dllFileName}",
                $"{NativeLibraryPrefix}{dllFileName}{NativeLibrarySuffix}",
                $"{dllFileName}{NativeLibrarySuffix}"
            };

            foreach (var dllName in alternates)
            {
                if (_nativeLibraries.ContainsKey(dllName))
                {
                    path = _nativeLibraries[dllName];
                    return true;
                }
            }

            path = null;
            return false;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string path;
            if (_nativeLibraries.TryGetValue(unmanagedDllName, out path)
                || SearchAlternateNativeLibraryName(unmanagedDllName, out path)
                || SearchForLibrary(unmanagedDllName, out path, NativeLibrarySuffix))
            {
                return LoadUnmanagedDllFromPath(path);
            }

            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private bool SearchForLibrary(string name, out string path, params string[] extensions)
        {
            foreach (var searchPath in _searchPaths)
            {
                foreach (var extension in extensions)
                {
                    var candidate = Path.Combine(searchPath, name + extension);
                    if (File.Exists(candidate))
                    {
                        path = candidate;
                        return true;
                    }
                }
            }
            path = null;
            return false;
        }
    }
}