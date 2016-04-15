// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.ReverseEngineering
{
    public class FileSet
    {
        public List<string> Files = new List<string>();
        private readonly IFileService _fileService;
        private readonly Func<string, string> _contentsReplacementFunc;

        public FileSet(IFileService fileService, string directory)
        {
            _fileService = fileService;
            Directory = directory;
        }

        public FileSet(IFileService fileService, string directory, Func<string, string> contentsReplacementFunc)
        {
            _fileService = fileService;
            Directory = directory;
            _contentsReplacementFunc = contentsReplacementFunc;
        }

        public string Directory { get; }

        public bool Exists(int index) => Exists(Files[index]);
        public bool Exists(string filepath) => _fileService.FileExists(Directory, filepath);
        public string Contents(int index) => Contents(Files[index]);

        public string Contents(string filepath) =>
            _contentsReplacementFunc == null
                ? _fileService.RetrieveFileContents(Directory, filepath)
                : _contentsReplacementFunc(_fileService.RetrieveFileContents(Directory, filepath));
    }
}
