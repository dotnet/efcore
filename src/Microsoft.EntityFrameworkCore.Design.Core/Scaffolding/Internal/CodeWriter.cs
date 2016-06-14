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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class CodeWriter
    {
        private const string DefaultFileExtension = ".cs";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected CodeWriter([NotNull] IFileService fileService)
        {
            Check.NotNull(fileService, nameof(fileService));

            FileService = fileService;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IFileService FileService { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string FileExtension { get; [param: NotNull] set; } = DefaultFileExtension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract Task<ReverseEngineerFiles> WriteCodeAsync(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
