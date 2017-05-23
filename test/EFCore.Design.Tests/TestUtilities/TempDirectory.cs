// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = IOPath.Combine(IOPath.GetTempPath(), IOPath.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
            => Directory.Delete(Path, recursive: true);
    }
}
