// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class FileSystemFileService : IFileService
    {
        public virtual bool DirectoryExists(string directoryName)
            => Directory.Exists(directoryName);

        public virtual bool FileExists(string directoryName, string fileName)
            => File.Exists(Path.Combine(directoryName, fileName));

        public virtual bool IsFileReadOnly(string directoryName, string fileName)
        {
            var fullFileName = Path.Combine(directoryName, fileName);
            return File.Exists(fullFileName)
                   && File.GetAttributes(fullFileName).HasFlag(FileAttributes.ReadOnly);
        }

        public virtual string RetrieveFileContents(string directoryName, string fileName)
            => File.ReadAllText(Path.Combine(directoryName, fileName));

        public virtual string OutputFile(string directoryName, string fileName, string contents)
        {
            Directory.CreateDirectory(directoryName);
            var fullFileName = Path.Combine(directoryName, fileName);
            File.WriteAllText(fullFileName, contents, Encoding.UTF8);

            return fullFileName;
        }
    }
}
