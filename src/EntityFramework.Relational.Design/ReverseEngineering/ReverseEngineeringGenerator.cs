// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private readonly ModelConfigurationFactory _modelConfigurationFactory;
        private readonly IDatabaseMetadataModelProvider _provider;

        public ReverseEngineeringGenerator(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IFileService fileService,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] IDatabaseMetadataModelProvider metadataModelProvider,
            [NotNull] ModelConfigurationFactory modelConfigurationFactory,
            [NotNull] CodeWriter codeWriter)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(fileService, nameof(fileService));
            Check.NotNull(modelUtilities, nameof(modelUtilities));
            Check.NotNull(metadataModelProvider, nameof(metadataModelProvider));
            Check.NotNull(modelConfigurationFactory, nameof(modelConfigurationFactory));
            Check.NotNull(codeWriter, nameof(codeWriter));

            Logger = loggerFactory.CreateCommandsLogger();
            _provider = metadataModelProvider;
            _modelConfigurationFactory = modelConfigurationFactory;
            CodeWriter = codeWriter;
        }

        public virtual ILogger Logger { get; }

        public virtual CodeWriter CodeWriter { get; }

        public virtual Task<ReverseEngineerFiles> GenerateAsync(
            [NotNull] ReverseEngineeringConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(configuration, nameof(configuration));

            cancellationToken.ThrowIfCancellationRequested();

            configuration.CheckValidity();

            var metadataModel = GetMetadataModel(configuration);
            var @namespace = ConstructNamespace(configuration.ProjectRootNamespace,
                    configuration.ProjectPath, configuration.RelativeOutputPath);

            var customConfiguration = new CustomConfiguration()
            {
                ConnectionString = configuration.ConnectionString,
                ContextClassName = configuration.ContextClassName,
                Namespace = @namespace,
                UseFluentApiOnly = configuration.UseFluentApiOnly,
            };
            var modelConfiguration = _modelConfigurationFactory
                .CreateModelConfiguration(metadataModel, customConfiguration);

            var dbContextClassName =
                customConfiguration.ContextClassName ?? modelConfiguration.ClassName();

            CheckOutputFiles(configuration.ProjectPath,
                configuration.RelativeOutputPath, dbContextClassName, metadataModel);

            var outputPath = configuration.RelativeOutputPath == null
                ? configuration.ProjectPath
                : Path.Combine(configuration.ProjectPath, configuration.RelativeOutputPath);

            return CodeWriter.WriteCodeAsync(
                modelConfiguration, outputPath, dbContextClassName, cancellationToken);
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

            var readOnlyFiles = CodeWriter.GetReadOnlyFilePaths(
                outputPath, dbContextClassName, metadataModel.EntityTypes);
            if (readOnlyFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    Strings.ReadOnlyFiles(
                        outputPath, string.Join(", ", readOnlyFiles)));
            }
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
