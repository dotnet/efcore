// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    public class BuildFileResult
    {
        private readonly string _targetPath;
        private readonly string _targetDir;
        private readonly string _targetName;

        public BuildFileResult(string targetPath)
        {
            _targetPath = targetPath;
            _targetDir = Path.GetDirectoryName(targetPath);
            _targetName = Path.GetFileNameWithoutExtension(targetPath);
        }

        public string TargetPath
        {
            get { return _targetPath; }
        }

        public string TargetDir
        {
            get { return _targetDir; }
        }

        public string TargetName
        {
            get { return _targetName; }
        }
    }
}
