// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = IOPath.Combine(IOPath.GetTempPath(), IOPath.GetRandomFileName());
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public static implicit operator string(TempDirectory dir)
        => dir.Path;

    public void Dispose()
        => Directory.Delete(Path, recursive: true);
}
