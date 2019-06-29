// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
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

        private static bool _looked;

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
            Check.NotNull(connection, nameof(connection));

            var opened = false;
            if (connection.State != ConnectionState.Open)
            {
                // NB: If closed, LoadExtension won't throw immediately
                connection.Open();
                opened = true;
            }
            try
            {
                Load(connection);

                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                return false;
            }
            finally
            {
                if (opened)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Loads the mod_spatialite extension into the specified connection.
        ///     </para>
        ///     <para>
        ///         The extension will be loaded from native NuGet assets when available.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        public static void Load([NotNull] DbConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            FindExtension();

            if (connection is SqliteConnection sqliteConnection)
            {
                sqliteConnection.LoadExtension("mod_spatialite");
            }
            else
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT load_extension('mod_spatialite');";
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void FindExtension()
        {
            if (_looked)
            {
                return;
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

            _looked = true;
        }
    }
}
