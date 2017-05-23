// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
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
}
