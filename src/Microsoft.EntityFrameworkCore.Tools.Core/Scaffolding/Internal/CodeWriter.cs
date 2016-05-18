// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public abstract class CodeWriter
    {
        private const string DefaultFileExtension = ".cs";

        protected CodeWriter([NotNull] IFileService fileService)
        {
            Check.NotNull(fileService, nameof(fileService));

            FileService = fileService;
        }

        public virtual IFileService FileService { get; }
        public virtual string FileExtension { get; [param: NotNull] set; } = DefaultFileExtension;

        /// <summary>
        ///     Returns a list of the files which would be output by this class where
        ///     the file already exists.
        /// </summary>
        /// <param name="outputPath"> directory where the files are to be output </param>
        /// <param name="dbContextClassName"> name of the <see cref="DbContext" /> class </param>
        /// <param name="entityTypes"> a list of the <see cref="IEntityType" /> classes to be output </param>
        /// <returns> A list of paths to the output files which already exist </returns>
        public virtual IList<string> GetExistingFilePaths(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IEnumerable<IEntityType> entityTypes)
        {
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(entityTypes, nameof(entityTypes));

            var existingFiles = new List<string>();

            if (!FileService.DirectoryExists(outputPath))
            {
                return existingFiles;
            }

            var filesToTest = new List<string>
            {
                dbContextClassName + FileExtension
            };
            filesToTest.AddRange(entityTypes
                .Select(entityType => entityType.DisplayName() + FileExtension));

            foreach (var fileName in filesToTest)
            {
                if (FileService.FileExists(outputPath, fileName))
                {
                    existingFiles.Add(fileName);
                }
            }

            return existingFiles;
        }

        /// <summary>
        ///     Returns a list of the files which would be output by this class but
        ///     which currently exist and would not be able to be overwritten due to
        ///     being read-only.
        /// </summary>
        /// <param name="outputPath"> directory where the files are to be output </param>
        /// <param name="dbContextClassName"> name of the <see cref="DbContext" /> class </param>
        /// <param name="entityTypes"> a list of the <see cref="IEntityType" /> classes to be output </param>
        /// <returns> A list of paths to the output files which currently exist and are read-only </returns>
        public virtual IList<string> GetReadOnlyFilePaths(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IEnumerable<IEntityType> entityTypes)
        {
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(entityTypes, nameof(entityTypes));

            var readOnlyFiles = new List<string>();

            var filesToTest = GetExistingFilePaths(outputPath, dbContextClassName, entityTypes);
            foreach (var fileName in filesToTest)
            {
                if (FileService.IsFileReadOnly(outputPath, fileName))
                {
                    readOnlyFiles.Add(fileName);
                }
            }

            return readOnlyFiles;
        }

        public abstract Task<ReverseEngineerFiles> WriteCodeAsync(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
