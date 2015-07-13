// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class FileSystemFileService : IFileService
    {
        public virtual bool DirectoryExists([NotNull] string directoryName)
        {
            return Directory.Exists(directoryName);
        }

        public virtual bool IsFileReadOnly([NotNull] string directoryName, [NotNull] string fileName)
        {
            var fullFileName = Path.Combine(directoryName, fileName);
            if (!File.Exists(fullFileName))
            {
                return false;
            }

            var attributes = File.GetAttributes(fullFileName);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                return true;
            }

            return false;
        }

        public virtual string RetrieveFileContents([NotNull] string directoryName,
            [NotNull] string fileName)
        {
            var fullFileName = Path.Combine(directoryName, fileName);
            return File.ReadAllText(fullFileName);
        }

        public virtual string OutputFile([NotNull] string directoryName,
            [NotNull] string fileName, [NotNull] string contents)
        {
            Directory.CreateDirectory(directoryName);
            var fullFileName = Path.Combine(directoryName, fileName);
            File.WriteAllText(fullFileName, contents);

            return fullFileName;
        }
    }
}
