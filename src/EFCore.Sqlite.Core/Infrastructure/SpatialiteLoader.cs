// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Finds and loads SpatiaLite.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SpatialiteLoader
{
    private static readonly string? SharedLibraryExtension;
    private static readonly string? PathVariableName;

    private static bool _looked;

    static SpatialiteLoader()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SharedLibraryExtension = ".dll";
            PathVariableName = "PATH";
        }
        else
        {
            // NB: The PATH trick we use won't work on Linux. Changing LD_LIBRARY_PATH has
            //     no effect after the first library is loaded
            _looked = true;
        }
    }

    /// <summary>
    ///     Tries to load the mod_spatialite extension into the specified connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns><see langword="true" /> if the extension was loaded; otherwise, <see langword="false" />.</returns>
    public static bool TryLoad(DbConnection connection)
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
    ///     Loads the mod_spatialite extension into the specified connection.
    /// </summary>
    /// <remarks>
    ///     The extension will be loaded from native NuGet assets when available.
    /// </remarks>
    /// <param name="connection">The connection.</param>
    public static void Load(DbConnection connection)
    {
        Check.NotNull(connection, nameof(connection));

        FindExtension();

        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.LoadExtension("mod_spatialite");
        }
        else
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT load_extension('mod_spatialite');";
            command.ExecuteNonQuery();
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
            var candidateAssets = new Dictionary<(string, string), int>();
            var rid = RuntimeInformation.RuntimeIdentifier;
            var rids = DependencyContext.Default!.RuntimeGraph.FirstOrDefault(g => g.Runtime == rid)?.Fallbacks.ToList()
                ?? [];
            rids.Insert(0, rid);

            foreach (var library in DependencyContext.Default.RuntimeLibraries)
            {
                foreach (var group in library.NativeLibraryGroups)
                {
                    foreach (var file in group.RuntimeFiles)
                    {
                        if (string.Equals(
                                Path.GetFileName(file.Path),
                                "mod_spatialite" + SharedLibraryExtension,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            var fallbacks = rids.IndexOf(group.Runtime);
                            if (fallbacks != -1 && library.Path is not null)
                            {
                                candidateAssets.Add((library.Path, file.Path), fallbacks);
                            }
                        }
                    }
                }
            }

            var assetPath = candidateAssets.OrderBy(p => p.Value)
                .Select(p => p.Key).FirstOrDefault();
            if (assetPath != default)
            {
                string? assetDirectory;
                if (File.Exists(Path.Combine(AppContext.BaseDirectory, assetPath.Item2)))
                {
                    // NB: This enables framework-dependent deployments
                    assetDirectory = Path.Combine(
                        AppContext.BaseDirectory,
                        Path.GetDirectoryName(assetPath.Item2.Replace('/', Path.DirectorySeparatorChar))!);
                }
                else
                {
                    string? assetFullPath = null;
                    var probingDirectories = ((string)AppDomain.CurrentDomain.GetData("PROBING_DIRECTORIES")!)
                        .Split(Path.PathSeparator);
                    foreach (var directory in probingDirectories)
                    {
                        var candidateFullPath = Path.Combine(
                            directory,
                            (assetPath.Item1 + "/" + assetPath.Item2).Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(candidateFullPath))
                        {
                            assetFullPath = candidateFullPath;
                        }
                    }

                    Check.DebugAssert(assetFullPath != null, "assetFullPath is null");

                    assetDirectory = Path.GetDirectoryName(assetFullPath);
                }

                Check.DebugAssert(assetDirectory != null, "assetDirectory is null");

                // GetEnvironmentVariable can sometimes return null when there is a race condition
                // with another thread setting it. Therefore we do a bit of back off and retry here.
                // Note that the result can be null if no path is set on the system.
                var delay = 1;
                var currentPath = Environment.GetEnvironmentVariable(PathVariableName!);
                while (currentPath == null && delay < 1000)
                {
                    Thread.Sleep(delay);
                    delay *= 2;
                    currentPath = Environment.GetEnvironmentVariable(PathVariableName!);
                }

                if (currentPath == null
                    || !currentPath.Split(Path.PathSeparator).Any(
                        p => string.Equals(
                            p.TrimEnd(Path.DirectorySeparatorChar),
                            assetDirectory,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    Environment.SetEnvironmentVariable(
                        PathVariableName!,
                        assetDirectory + Path.PathSeparator + currentPath);
                }
            }
        }

        _looked = true;
    }
}
