// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Templating;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private readonly CodeGeneratorHelperFactory _helperFactory;
        private readonly IDatabaseMetadataModelProvider _provider;
        public const string DbContextTemplateFileName = "DbContextTemplate.cshtml";
        public const string EntityTypeTemplateFileName = "EntityTypeTemplate.cshtml";
        private const string DefaultFileExtension = ".cs";

        public ReverseEngineeringGenerator(
            [NotNull] ILogger logger, 
            [NotNull] IFileService fileService,
            [NotNull] ModelUtilities modelUtilities, 
            [NotNull] ITemplating templating,
            [NotNull] IDatabaseMetadataModelProvider metadataModelProvider, 
            [NotNull] CodeGeneratorHelperFactory helperFactory)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(fileService, nameof(fileService));
            Check.NotNull(modelUtilities, nameof(modelUtilities));
            Check.NotNull(templating, nameof(templating));
            Check.NotNull(helperFactory, nameof(helperFactory));

            Logger = logger;
            FileService = fileService;
            Templating = templating;
            _provider = metadataModelProvider;
            _helperFactory = helperFactory;
        }

        public virtual string FileExtension { get; [param: NotNull] set; } = DefaultFileExtension;

        public virtual ILogger Logger { get; }

        public virtual IFileService FileService { get; }

        public virtual ITemplating Templating { get; }

        public virtual async Task<IReadOnlyList<string>> GenerateAsync(
            [NotNull] ReverseEngineeringConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(configuration, nameof(configuration));

            cancellationToken.ThrowIfCancellationRequested();

            configuration.CheckValidity();

            var resultingFiles = new List<string>();
            var metadataModel = GetMetadataModel(configuration);
            var @namespace = ConstructNamespace(configuration.ProjectRootNamespace,
                    configuration.ProjectPath, configuration.RelativeOutputPath);

            var dbContextGeneratorModel = new DbContextGeneratorModel
            {
                ClassName = configuration.ContextClassName,
                Namespace = @namespace,
                ConnectionString = configuration.ConnectionString,
                MetadataModel = metadataModel
            };
            var dbContextCodeGeneratorHelper = _helperFactory.DbContextHelper(dbContextGeneratorModel);
            dbContextGeneratorModel.Helper = dbContextCodeGeneratorHelper;

            var dbContextClassName = configuration.ContextClassName
                                     ?? dbContextCodeGeneratorHelper.ClassName(configuration.ConnectionString);
            CheckOutputFiles(configuration.ProjectPath, configuration.RelativeOutputPath, dbContextClassName, metadataModel);
            var outputPath = configuration.RelativeOutputPath == null
                ? configuration.ProjectPath
                : Path.Combine(configuration.ProjectPath, configuration.RelativeOutputPath);

            var dbContextTemplate = LoadTemplate(configuration.CustomTemplatePath,
                    GetDbContextTemplateFileName(), () => _provider.DbContextTemplate);
            var templateResult = await Templating.RunTemplateAsync(
                dbContextTemplate, dbContextGeneratorModel, _provider, cancellationToken);
            if (templateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(
                    Strings.ErrorRunningDbContextTemplate(templateResult.ProcessingException.Message));
            }

            // output DbContext .cs file
            var dbContextFileName = dbContextClassName + FileExtension;
            var dbContextFileFullPath = FileService.OutputFile(
                outputPath, dbContextFileName, templateResult.GeneratedText);
            resultingFiles.Add(dbContextFileFullPath);

            var entityTypeTemplate = LoadTemplate(configuration.CustomTemplatePath,
                GetEntityTypeTemplateFileName(), () => _provider.EntityTypeTemplate);
            foreach (var entityType in metadataModel.EntityTypes)
            {
                // TODO decouple models and helpers to remove the bidirectional references
                var entityTypeGeneratorModel = new EntityTypeGeneratorModel
                {
                    EntityType = entityType,
                    Namespace = @namespace,
                    ConnectionString = configuration.ConnectionString
                };
                
                var entityTypeCodeGeneratorHelper = _helperFactory.EntityTypeHelper(entityTypeGeneratorModel);
                entityTypeGeneratorModel.Helper = entityTypeCodeGeneratorHelper;

                templateResult = await Templating.RunTemplateAsync(
                    entityTypeTemplate, entityTypeGeneratorModel, _provider, cancellationToken);
                if (templateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(
                        Strings.ErrorRunningEntityTypeTemplate(templateResult.ProcessingException.Message));
                }

                // output EntityType poco .cs file
                var entityTypeFileName = entityType.DisplayName() + FileExtension;
                var entityTypeFileFullPath = FileService.OutputFile(
                    outputPath, entityTypeFileName, templateResult.GeneratedText);
                resultingFiles.Add(entityTypeFileFullPath);
            }

            return resultingFiles;
        }

        public virtual IReadOnlyList<string> Customize([NotNull] string projectDir)
        {
            var dbContextTemplate = _provider.DbContextTemplate;
            var entityTypeTemplate = _provider.EntityTypeTemplate;

            return new List<string>
                {
                    FileService.OutputFile(projectDir, GetDbContextTemplateFileName(), dbContextTemplate),
                    FileService.OutputFile(projectDir, GetEntityTypeTemplateFileName(), entityTypeTemplate)
                };
        }

        public virtual IModel GetMetadataModel([NotNull] ReverseEngineeringConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));

            var metadataModel = _provider
                .GenerateMetadataModel(configuration.ConnectionString);
            if (metadataModel == null)
            {
                throw new InvalidOperationException(
                    Strings.ProviderReturnedNullModel(
                        _provider.GetType().FullName,
                        configuration.ConnectionString));
            }

            return metadataModel;
        }

        public virtual void CheckOutputFiles(
            [NotNull] string projectPath,
            [CanBeNull] string relativeOutputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IModel metadataModel)
        {
            Check.NotEmpty(projectPath, nameof(projectPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(metadataModel, nameof(metadataModel));

            var outputPath = relativeOutputPath == null
                ? projectPath
                : Path.Combine(projectPath, relativeOutputPath);

            if (!FileService.DirectoryExists(outputPath))
            {
                return;
            }

            var filesToTest = new List<string>
            {
                dbContextClassName + FileExtension
            };
            filesToTest.AddRange(metadataModel.EntityTypes
                .Select(entityType => entityType.DisplayName() + FileExtension));

            var readOnlyFiles = new List<string>();
            foreach (var fileName in filesToTest)
            {
                if (FileService.IsFileReadOnly(outputPath, fileName))
                {
                    readOnlyFiles.Add(fileName);
                }
            }

            if (readOnlyFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    Strings.ReadOnlyFiles(
                        outputPath, string.Join(", ", readOnlyFiles)));
            }
        }

        public virtual string GetDbContextTemplateFileName() 
            => _provider.GetType().GetTypeInfo().Assembly.GetName().Name + "." + DbContextTemplateFileName;

        public virtual string GetEntityTypeTemplateFileName() 
            => _provider.GetType().GetTypeInfo().Assembly.GetName().Name + "." + EntityTypeTemplateFileName;

        private string LoadTemplate(string searchPath, string fileName, Func<string> providerTemplateFunc)
        {
            if (!string.IsNullOrEmpty(searchPath)
                && FileService.FileExists(searchPath, fileName))
            {
                Logger.LogInformation(Strings.UsingCustomTemplate(Path.Combine(searchPath,fileName)));
                return FileService.RetrieveFileContents(searchPath, fileName);
            }

            return providerTemplateFunc.Invoke();
        }

        private static char[] directorySeparatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private string ConstructNamespace(string rootNamespace, string projectPath, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return rootNamespace;
            }

            var @namespace = rootNamespace;
            var projectPathPrefixLength = Path.GetFullPath(projectPath).Count();
            var canonicalizedRelativePath = Path.GetFullPath(Path.Combine(projectPath, relativePath))
                .Substring(projectPathPrefixLength + 1);
            foreach (var pathPart in canonicalizedRelativePath
                .Split(directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries))
            {
                @namespace += "." + CSharpUtilities.Instance.GenerateCSharpIdentifier(pathPart, null);
            }

            return @namespace;
        }
    }
}
