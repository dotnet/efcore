// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StringBuilderCodeWriter : CodeWriter
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbContextWriter DbContextWriter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityTypeWriter EntityTypeWriter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public StringBuilderCodeWriter(
            [NotNull] IFileService fileService,
            [NotNull] DbContextWriter dbContextWriter,
            [NotNull] EntityTypeWriter entityTypeWriter)
            : base(fileService)
        {
            Check.NotNull(dbContextWriter, nameof(dbContextWriter));
            Check.NotNull(entityTypeWriter, nameof(entityTypeWriter));

            DbContextWriter = dbContextWriter;
            EntityTypeWriter = entityTypeWriter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<ReverseEngineerFiles> WriteCodeAsync(
            ModelConfiguration modelConfiguration,
            string outputPath,
            string dbContextClassName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));

            cancellationToken.ThrowIfCancellationRequested();

            var resultingFiles = new ReverseEngineerFiles();

            var generatedCode = DbContextWriter.WriteCode(modelConfiguration);

            // output DbContext .cs file
            var dbContextFileName = dbContextClassName + FileExtension;
            var dbContextFileFullPath = FileService.OutputFile(
                outputPath, dbContextFileName, generatedCode);
            resultingFiles.ContextFile = dbContextFileFullPath;

            foreach (var entityConfig in modelConfiguration.EntityConfigurations)
            {
                generatedCode = EntityTypeWriter.WriteCode(entityConfig);

                // output EntityType poco .cs file
                var entityTypeFileName = entityConfig.EntityType.DisplayName() + FileExtension;
                var entityTypeFileFullPath = FileService.OutputFile(
                    outputPath, entityTypeFileName, generatedCode);
                resultingFiles.EntityTypeFiles.Add(entityTypeFileFullPath);
            }

            return Task.FromResult(resultingFiles);
        }
    }
}
