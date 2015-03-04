// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
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
            CSharpCodeGeneratorHelper = serviceProvider.GetRequiredService<CSharpCodeGeneratorHelper>();
            ModelUtilities = serviceProvider.GetRequiredService<ModelUtilities>();
        }

        public virtual string FileExtension { get; [param: NotNull] set; } = DefaultFileExtension;

        public virtual CSharpCodeGeneratorHelper CSharpCodeGeneratorHelper { get; [param: NotNull] set; }

        public virtual ModelUtilities ModelUtilities { get; [param: NotNull] set; }

        public virtual ILogger Logger { get; }

        public virtual async Task GenerateAsync(
            [NotNull] ReverseEngineeringConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(configuration, nameof(configuration));

            CheckConfiguration(configuration);

            var providerAssembly = configuration.ProviderAssembly;
            var provider = GetProvider(providerAssembly);
            var metadataModel = GetMetadataModel(provider, configuration);

            var dbContextGeneratorModel = new DbContextGeneratorModel
            {
                ClassName = configuration.ContextClassName,
                Namespace = configuration.Namespace,
                ProviderAssembly = configuration.ProviderAssembly.FullName,
                ConnectionString = configuration.ConnectionString,
                MetadataModel = metadataModel
            };

            //TODO - check to see whether user has an override class for this in the current project first
            var dbContextCodeGenerator =
                provider.GetContextModelCodeGenerator(this, dbContextGeneratorModel);
            if (dbContextCodeGenerator == null)
            {
                throw new InvalidOperationException(
                    Strings.NoContextModelCodeGenerator(provider.GetType().FullName));
            }

            CheckOutputFiles(configuration.OutputPath, dbContextCodeGenerator.ClassName, metadataModel);

            var contextStringBuilder = new IndentedStringBuilder();
            dbContextCodeGenerator.Generate(contextStringBuilder);

            // output DbContext .cs file
            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(contextStringBuilder.ToString())))
            {
                await OutputFile(configuration.OutputPath, dbContextCodeGenerator.ClassName + FileExtension, sourceStream);
            }

            foreach (var entityType in metadataModel.EntityTypes)
            {
                var entityTypeGeneratorModel = new EntityTypeGeneratorModel()
                {
                    EntityType = entityType,
                    Namespace = configuration.Namespace,
                    ProviderAssembly = configuration.ProviderAssembly.FullName,
                    ConnectionString = configuration.ConnectionString,
                };

                //TODO - check to see whether user has an override class for this in the current project first
                var entityTypeCodeGenerator =
                    provider.GetEntityTypeModelCodeGenerator(
                        this,
                        entityTypeGeneratorModel);
                if (entityTypeCodeGenerator == null)
                {
                    throw new InvalidOperationException(
                        Strings.NoEntityTypeModelCodeGenerator(provider.GetType().FullName));
                }

                var entityTypeStringBuilder = new IndentedStringBuilder();
                entityTypeCodeGenerator.Generate(entityTypeStringBuilder);

                // output EntityType poco .cs file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(entityTypeStringBuilder.ToString())))
                {
                    await OutputFile(configuration.OutputPath,
                        entityType.Name + FileExtension,
                        sourceStream);
                }
            }
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

            if (!Directory.Exists(outputDirectoryName))
            {
                return;
            }

            var filesToTest = new List<string>()
                {
                    dbContextClassName + FileExtension
                };
            filesToTest.AddRange(metadataModel.EntityTypes
                .Select(entityType => entityType.Name + FileExtension));

            var readOnlyFiles = new List<string>();
            foreach (var fileName in filesToTest)
            {
                var fullFileName = Path.Combine(outputDirectoryName, fileName);
                if (File.Exists(fullFileName))
                {
                    var attributes = File.GetAttributes(fullFileName);
                    if (attributes.HasFlag(FileAttributes.ReadOnly))
                    {
                        readOnlyFiles.Add(fileName);
                    }
                }
            }

            if (readOnlyFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    Strings.ReadOnlyFiles(
                        outputDirectoryName, string.Join(", ", readOnlyFiles)));
            }
        }

        private async Task OutputFile(string outputDirectoryName, string outputFileName, Stream sourceStream)
        {
            Directory.CreateDirectory(outputDirectoryName);

            var fullFileName = Path.Combine(outputDirectoryName, outputFileName);
            using (var writeStream = new FileStream(fullFileName, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(writeStream);
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
