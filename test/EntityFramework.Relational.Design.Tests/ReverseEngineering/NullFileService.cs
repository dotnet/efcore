// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class NullFileService : IFileService
    {
        public bool DirectoryExists([NotNull] string directoryName)
        {
            throw new NotImplementedException();
        }

        public bool FileExists([NotNull] string directoryName, [NotNull] string fileName)
        {
            throw new NotImplementedException();
        }

        public bool IsFileReadOnly([NotNull] string directoryName, [NotNull] string fileName)
        {
            throw new NotImplementedException();
        }

        public string OutputFile([NotNull] string directoryName, [NotNull] string fileName, [NotNull] string contents)
        {
            throw new NotImplementedException();
        }

        public string RetrieveFileContents([NotNull] string directoryName, [NotNull] string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
