// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BuildFileResult
{
    public BuildFileResult(string targetPath)
    {
        TargetPath = targetPath;
        TargetDir = Path.GetDirectoryName(targetPath);
        TargetName = Path.GetFileNameWithoutExtension(targetPath);
    }

    public string TargetPath { get; }

    public string TargetDir { get; }

    public string TargetName { get; }
}
