// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyModel;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Finds and loads SpatiaLite.
    /// </summary>
    public static class SpatialiteLoader
    {
        private static readonly string _sharedLibraryExtension;
        private static readonly string _pathVariableName;

        private static string _extension;

        static SpatialiteLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _sharedLibraryExtension = ".dll";
                _pathVariableName = "PATH";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _sharedLibraryExtension = ".dylib";
                _pathVariableName = "DYLD_LIBRARY_PATH";
            }
            else
            {
                _sharedLibraryExtension = ".so";
                _pathVariableName = "LD_LIBRARY_PATH";
            }
        }

        /// <summary>
        ///     Tries to load the mod_spatialite extension into the specified connection.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <returns> true if the extension was loaded; otherwise, false. </returns>
        public static bool TryLoad([NotNull] DbConnection connection)
        {
            try
            {
                Load(connection);

                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                return false;
            }
        }

        /// <summary>
        ///     <para>
        ///         Loads the mod_spatialite extension into the specified connection.
        ///     </para>
        ///     <para>
        ///         The the extension will be loaded from native NuGet assets when available.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        public static void Load([NotNull] DbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var extension = FindExtension();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT load_extension('" + extension + "');";
                command.ExecuteNonQuery();
            }
        }

        private static string FindExtension()
        {
            if (_extension != null)
            {
                return _extension;
            }

            bool hasDependencyContext;
            try
            {
                hasDependencyContext = DependencyContext.Default != null;
            }
            catch (Exception ex) // Work around dotnet/core-setup#4556
            {
                Debug.Fail(ex.ToString());
                hasDependencyContext = false;
            }

            if (hasDependencyContext)
            {
                var candidateAssets = new Dictionary<string, int>();
                var rid = RuntimeEnvironment.GetRuntimeIdentifier();
                var rids = DependencyContext.Default.RuntimeGraph.First(g => g.Runtime == rid).Fallbacks.ToList();
                rids.Insert(0, rid);

                foreach (var library in DependencyContext.Default.RuntimeLibraries)
                {
                    foreach (var group in library.NativeLibraryGroups)
                    {
                        foreach (var file in group.RuntimeFiles)
                        {
                            if (string.Equals(
                                Path.GetFileName(file.Path),
                                "mod_spatialite" + _sharedLibraryExtension,
                                StringComparison.OrdinalIgnoreCase))
                            {
                                var fallbacks = rids.IndexOf(group.Runtime);
                                if (fallbacks != -1)
                                {
                                    candidateAssets.Add(library.Path + "/" + file.Path, fallbacks);
                                }
                            }
                        }
                    }
                }

                var assetPath = candidateAssets.OrderBy(p => p.Value)
                    .Select(p => p.Key.Replace('/', Path.DirectorySeparatorChar)).FirstOrDefault();
                if (assetPath != null)
                {
                    string assetFullPath = null;
                    var probingDirectories = ((string)AppDomain.CurrentDomain.GetData("PROBING_DIRECTORIES"))
                        .Split(Path.PathSeparator);
                    foreach (var directory in probingDirectories)
                    {
                        var candidateFullPath = Path.Combine(directory, assetPath);
                        if (File.Exists(candidateFullPath))
                        {
                            assetFullPath = candidateFullPath;
                        }
                    }
                    Debug.Assert(assetFullPath != null);

                    var assetDirectory = Path.GetDirectoryName(assetFullPath);

                    var currentPath = Environment.GetEnvironmentVariable(_pathVariableName);
                    if (!currentPath.Split(Path.PathSeparator).Any(
                        p => string.Equals(
                            p.TrimEnd(Path.DirectorySeparatorChar),
                            assetDirectory,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        Environment.SetEnvironmentVariable(
                            _pathVariableName,
                            assetDirectory + Path.PathSeparator + currentPath);
                    }
                }
            }

            var extension = "mod_spatialite";

            // Workaround ericsink/SQLitePCL.raw#225
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                extension += _sharedLibraryExtension;
            }

            return _extension = extension;
        }
    }
}
