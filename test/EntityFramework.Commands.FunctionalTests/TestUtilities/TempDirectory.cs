// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using IOPath = System.IO.Path;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    internal class TempDirectory : IDisposable
    {
        private readonly string _path;

        public TempDirectory()
        {
            _path = IOPath.Combine(IOPath.GetTempPath(), IOPath.GetRandomFileName());
            Directory.CreateDirectory(_path);
        }

        public string Path
        {
            get { return _path; }
        }

        public void Dispose()
        {
            Directory.Delete(_path, recursive: true);
        }
    }
}
