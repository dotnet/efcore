// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class CodeWriter
    {
        private const string DefaultFileExtension = ".cs";

        public CodeWriter([NotNull] IFileService fileService)
        {
            Check.NotNull(fileService, nameof(fileService));

            FileService = fileService;
        }

        public virtual IFileService FileService { get;[param: NotNull] set; }
        public virtual string FileExtension { get;[param: NotNull] set; } = DefaultFileExtension;

        public virtual List<string> ReadOnlyOutputFiles(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IModel metadataModel)
        {
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(metadataModel, nameof(metadataModel));

            var readOnlyFiles = new List<string>();

            if (!FileService.DirectoryExists(outputPath))
            {
                return readOnlyFiles;
            }

            var filesToTest = new List<string>
            {
                dbContextClassName + FileExtension
            };
            filesToTest.AddRange(metadataModel.EntityTypes
                .Select(entityType => entityType.DisplayName() + FileExtension));

            foreach (var fileName in filesToTest)
            {
                if (FileService.IsFileReadOnly(outputPath, fileName))
                {
                    readOnlyFiles.Add(fileName);
                }
            }

            return readOnlyFiles;
        }

        public abstract Task<List<string>> WriteCodeAsync(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
