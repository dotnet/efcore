// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class BuildFileResult(string targetPath)
{
    public string TargetPath { get; } = targetPath;

    public string TargetDir { get; } = Path.GetDirectoryName(targetPath);

    public string TargetName { get; } = Path.GetFileNameWithoutExtension(targetPath);
}
