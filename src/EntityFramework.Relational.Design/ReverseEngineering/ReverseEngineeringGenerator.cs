// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.Templating;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private const string DefaultFileExtension = ".cs";
        private readonly IServiceProvider _serviceProvider;

        public ReverseEngineeringGenerator([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILogger>();
            FileService = serviceProvider.GetRequiredService<IFileService>();
            CSharpCodeGeneratorHelper = serviceProvider.GetRequiredService<CSharpCodeGeneratorHelper>();
            ModelUtilities = serviceProvider.GetRequiredService<ModelUtilities>();
        }

        public virtual string FileExtension { get; [param: NotNull] set; } = DefaultFileExtension;

        public virtual CSharpCodeGeneratorHelper CSharpCodeGeneratorHelper { get; [param: NotNull] set; }

        public virtual ModelUtilities ModelUtilities { get; [param: NotNull] set; }

        public virtual ILogger Logger { get; }

        public virtual IFileService FileService { get; }

        public virtual async Task<IReadOnlyList<string>> GenerateAsync(
            [NotNull] ReverseEngineeringConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(configuration, nameof(configuration));

            cancellationToken.ThrowIfCancellationRequested();

            CheckConfiguration(configuration);

            var resultingFiles = new List<string>();
            var providerAssembly = configuration.ProviderAssembly;
            var provider = GetProvider(providerAssembly);
            var metadataModel = GetMetadataModel(provider, configuration);

            var dbContextGeneratorModel = new DbContextGeneratorModel
            {
                ClassName = configuration.ContextClassName,
                Namespace = configuration.Namespace,
                ProviderAssembly = configuration.ProviderAssembly.FullName,
                ConnectionString = configuration.ConnectionString,
                Generator = this,
                MetadataModel = metadataModel
            };
            var dbContextCodeGeneratorHelper = provider.DbContextCodeGeneratorHelper(dbContextGeneratorModel);
            dbContextGeneratorModel.Helper = dbContextCodeGeneratorHelper;

            var dbContextClassName = configuration.ContextClassName
                                     ?? dbContextCodeGeneratorHelper.ClassName(configuration.ConnectionString);
            CheckOutputFiles(configuration.OutputPath, dbContextClassName, metadataModel);

            var templating = _serviceProvider.GetRequiredService<ITemplating>();
            var dbContextTemplate = provider.DbContextTemplate;
            var templateResult = await templating.RunTemplateAsync(
                dbContextTemplate, dbContextGeneratorModel, provider, cancellationToken);
            if (templateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(
                    Strings.ErrorRunningDbContextTemplate(templateResult.ProcessingException.Message));
            }

            // output DbContext .cs file
            var dbContextFileName = dbContextClassName + FileExtension;
            var dbContextFileFullPath = FileService.OutputFile(
                configuration.OutputPath, dbContextFileName, templateResult.GeneratedText);
            resultingFiles.Add(dbContextFileFullPath);

            var entityTypeTemplate = provider.EntityTypeTemplate;
            foreach (var entityType in metadataModel.EntityTypes)
            {
                var entityTypeGeneratorModel = new EntityTypeGeneratorModel
                {
                    EntityType = entityType,
                    Namespace = configuration.Namespace,
                    ProviderAssembly = configuration.ProviderAssembly.FullName,
                    ConnectionString = configuration.ConnectionString,
                    Generator = this
                };
                var entityTypeCodeGeneratorHelper = provider.EntityTypeCodeGeneratorHelper(entityTypeGeneratorModel);
                entityTypeGeneratorModel.Helper = entityTypeCodeGeneratorHelper;

                templateResult = await templating.RunTemplateAsync(
                    entityTypeTemplate, entityTypeGeneratorModel, provider, cancellationToken);
                if (templateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(
                        Strings.ErrorRunningEntityTypeTemplate(templateResult.ProcessingException.Message));
                }

                // output EntityType poco .cs file
                var entityTypeFileName = entityType.DisplayName() + FileExtension;
                var entityTypeFileFullPath = FileService.OutputFile(
                    configuration.OutputPath, entityTypeFileName, templateResult.GeneratedText);
                resultingFiles.Add(entityTypeFileFullPath);
            }

            return resultingFiles;
        }

        public virtual IDatabaseMetadataModelProvider GetProvider([NotNull] Assembly providerAssembly)
        {
            Check.NotNull(providerAssembly, nameof(providerAssembly));

            var type = providerAssembly.GetExportedTypes()
                .FirstOrDefault(t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
            if (type == null)
            {
                throw new InvalidOperationException(
                    Strings.AssemblyDoesNotContainMetadataModelProvider(
                        providerAssembly.FullName,
                        typeof(IDatabaseMetadataModelProvider).FullName));
            }

            return (IDatabaseMetadataModelProvider)Activator.CreateInstance(type, _serviceProvider);
        }

        public virtual IModel GetMetadataModel(
            [NotNull] IDatabaseMetadataModelProvider provider,
            [NotNull] ReverseEngineeringConfiguration configuration)
        {
            Check.NotNull(provider, nameof(provider));
            Check.NotNull(configuration, nameof(configuration));

            var metadataModel = provider
                .GenerateMetadataModel(configuration.ConnectionString);
            if (metadataModel == null)
            {
                throw new InvalidOperationException(
                    Strings.ProviderReturnedNullModel(
                        provider.GetType().FullName,
                        configuration.ConnectionString));
            }

            return metadataModel;
        }

        public virtual void CheckOutputFiles(
            [NotNull] string outputDirectoryName,
            [NotNull] string dbContextClassName,
            [NotNull] IModel metadataModel)
        {
            Check.NotEmpty(outputDirectoryName, nameof(outputDirectoryName));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(metadataModel, nameof(metadataModel));

            if (!FileService.DirectoryExists(outputDirectoryName))
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
                if (FileService.IsFileReadOnly(outputDirectoryName, fileName))
                {
                    readOnlyFiles.Add(fileName);
                }
            }

            if (readOnlyFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    Strings.ReadOnlyFiles(
                        outputDirectoryName, string.Join(", ", readOnlyFiles)));
            }
        }

        private static void CheckConfiguration(ReverseEngineeringConfiguration configuration)
        {
            if (configuration.ProviderAssembly == null)
            {
                throw new ArgumentException(Strings.ProviderAssemblyRequired);
            }

            if (string.IsNullOrEmpty(configuration.ConnectionString))
            {
                throw new ArgumentException(Strings.ConnectionStringRequired);
            }

            if (string.IsNullOrEmpty(configuration.OutputPath))
            {
                throw new ArgumentException(Strings.OutputPathRequired);
            }

            if (string.IsNullOrEmpty(configuration.Namespace))
            {
                throw new ArgumentException(Strings.NamespaceRequired);
            }
        }
    }
}
