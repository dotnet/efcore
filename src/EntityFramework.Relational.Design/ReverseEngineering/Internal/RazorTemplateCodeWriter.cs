// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal
{
    public class RazorTemplateCodeWriter : CodeWriter
    {
        public static readonly string DbContextTemplateResourceName =
            typeof(RazorTemplateCodeWriter).GetTypeInfo().Assembly.GetName().Name
            + ".ReverseEngineering.Internal.Templates.DbContextTemplate.cshtml";

        public static readonly string EntityTypeTemplateResourceName =
            typeof(RazorTemplateCodeWriter).GetTypeInfo().Assembly.GetName().Name
            + ".ReverseEngineering.Internal.Templates.EntityTypeTemplate.cshtml";

        public RazorTemplateCodeWriter(
            [NotNull] IFileService fileService,
            [NotNull] RazorTemplating templateEngine)
            : base(fileService)
        {
            Check.NotNull(templateEngine, nameof(templateEngine));

            TemplateEngine = templateEngine;
        }

        protected string DbContextTemplate =
            ReadFromResource(
                typeof(RelationalMetadataModelProvider).GetTypeInfo().Assembly,
                DbContextTemplateResourceName);

        protected string EntityTypeTemplate =
            ReadFromResource(
                typeof(RelationalMetadataModelProvider).GetTypeInfo().Assembly,
                EntityTypeTemplateResourceName);

        protected virtual RazorTemplating TemplateEngine { get;[param: NotNull] set; }

        public async override Task<ReverseEngineerFiles> WriteCodeAsync(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));

            cancellationToken.ThrowIfCancellationRequested();

            var resultingFiles = new ReverseEngineerFiles();

            var templateResult = await TemplateEngine.RunTemplateAsync(
                DbContextTemplate, modelConfiguration, cancellationToken);
            if (templateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(
                    Strings.ErrorRunningDbContextTemplate(templateResult.ProcessingException.Message));
            }

            // output DbContext .cs file
            var dbContextFileName = dbContextClassName + FileExtension;
            var dbContextFileFullPath = FileService.OutputFile(
                outputPath, dbContextFileName, templateResult.GeneratedText);
            resultingFiles.ContextFile = dbContextFileFullPath;

            foreach (var entityConfig in modelConfiguration.EntityConfigurations)
            {
                templateResult = await TemplateEngine.RunTemplateAsync(
                    EntityTypeTemplate, entityConfig, cancellationToken);
                if (templateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(
                        Strings.ErrorRunningEntityTypeTemplate(templateResult.ProcessingException.Message));
                }

                // output EntityType poco .cs file
                var entityTypeFileName = entityConfig.EntityType.DisplayName() + FileExtension;
                var entityTypeFileFullPath = FileService.OutputFile(
                    outputPath, entityTypeFileName, templateResult.GeneratedText);
                resultingFiles.EntityTypeFiles.Add(entityTypeFileFullPath);
            }

            return resultingFiles;
        }

        protected static string ReadFromResource([NotNull] Assembly assembly, [NotNull] string resourceName)
        {
            Check.NotNull(assembly, nameof(assembly));
            Check.NotEmpty(resourceName, nameof(resourceName));

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

    }
}
