// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.Tools.TestUtilities;

public class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = IOPath.Combine(IOPath.GetTempPath(), IOPath.GetRandomFileName());
        Assert.False(Directory.Exists(Path), $"Temporary directory '{Path}' already exists.");
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                Directory.Delete(Path, recursive: true);
                return;
            }
            catch when (stopwatch.ElapsedMilliseconds < 30000)
            {
                Thread.Sleep(150);
            }
        }
    }
}
