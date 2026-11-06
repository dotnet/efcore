// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools.TestUtilities;

public class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = Directory.CreateTempSubdirectory().FullName;
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
