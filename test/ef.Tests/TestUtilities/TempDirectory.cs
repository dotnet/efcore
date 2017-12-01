// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;
using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.Tools.TestUtilities
{
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
}
