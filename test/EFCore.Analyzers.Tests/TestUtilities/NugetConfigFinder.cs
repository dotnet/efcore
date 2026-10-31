// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

internal static class NugetConfigFinder
{
    private static bool _searched;
    private static string? _nugetConfigPath;

    public static string? Find()
    {
        if (_searched)
        {
            return _nugetConfigPath;
        }

        var path = AppContext.BaseDirectory;
        var currentDirectory = path;
        while (currentDirectory is not null)
        {
            var nugetConfigPath = Path.Combine(currentDirectory, "NuGet.config");

            if (File.Exists(nugetConfigPath))
            {
                _nugetConfigPath = nugetConfigPath;
                break;
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.ToString();
        }

        _searched = true;
        return _nugetConfigPath;
    }
}
