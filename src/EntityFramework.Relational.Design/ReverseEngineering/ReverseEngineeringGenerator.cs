// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private const string DefaultFileExtension = ".cs";
        private readonly IServiceProvider _serviceProvider;
        private ILogger _logger;

        public ReverseEngineeringGenerator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            if (_logger == null)
            {
                throw new ArgumentException(typeof(ReverseEngineeringGenerator).Name + " cannot find a service of type " + typeof(ILogger).Name);
            }
        }

        public virtual string FileExtension { get; set; } = DefaultFileExtension;

        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public async Task Generate(ReverseEngineeringConfiguration configuration)
        {
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
                    "Provider " + provider.GetType().FullName
                    + " did not provide a ContextModelCodeGeneratorContext");
            }

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
                var entityTypeCodeGeneratorContext =
                    provider.GetEntityTypeModelCodeGenerator(
                        this,
                        entityTypeGeneratorModel);
                if (entityTypeCodeGeneratorContext == null)
                {
                    throw new InvalidOperationException(
                        "Provider " + provider.GetType().FullName
                        + " did not provide a EntityTypeModelCodeGeneratorContext");
                }

                var entityTypeStringBuilder = new IndentedStringBuilder();
                entityTypeCodeGeneratorContext.Generate(entityTypeStringBuilder);

                // output EntityType poco .cs file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(entityTypeStringBuilder.ToString())))
                {
                    await OutputFile(configuration.OutputPath,
                        entityType.Name + FileExtension,
                        sourceStream);
                }
            }
        }

        public IDatabaseMetadataModelProvider GetProvider(Assembly providerAssembly)
        {
            var type = providerAssembly.GetExportedTypes()
                .FirstOrDefault(t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
            if (type == null)
            {
                throw new InvalidOperationException(
                    "Assembly " + providerAssembly.FullName
                    + " does not contain a type which extends "
                    + typeof(IDatabaseMetadataModelProvider).FullName);
            }

            return (IDatabaseMetadataModelProvider)Activator.CreateInstance(type, _serviceProvider);
        }

        public IModel GetMetadataModel(
            IDatabaseMetadataModelProvider provider, ReverseEngineeringConfiguration configuration)
        {
            var metadataModel = provider
                .GenerateMetadataModel(configuration.ConnectionString);
            if (metadataModel == null)
            {
                throw new InvalidOperationException("Model returned is null. Provider class " + provider.GetType()
                    + ", connection string: " + configuration.ConnectionString);
            }

            return metadataModel;
        }

        private async Task OutputFile(string outputDirectoryName, string outputFileName, Stream sourceStream)
        {
            Directory.CreateDirectory(outputDirectoryName);

            var fullFileName = Path.Combine(outputDirectoryName, outputFileName);
            if (File.Exists(fullFileName))
            {
                //ensure file is writeable
                FileAttributes attributes = File.GetAttributes(fullFileName);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(fullFileName, attributes & ~FileAttributes.ReadOnly);
                }
            }

            using (var writeStream = new FileStream(fullFileName, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(writeStream);
            }
        }

        private static void CheckConfiguration(ReverseEngineeringConfiguration configuration)
        {
            if (configuration.ProviderAssembly == null)
            {
                throw new ArgumentException("ProviderAssembly is required to generate code.");
            }

            if (string.IsNullOrEmpty(configuration.ConnectionString))
            {
                throw new ArgumentException("ConnectionString is required to generate code.");
            }

            if (string.IsNullOrEmpty(configuration.OutputPath))
            {
                throw new ArgumentException("OutputPath is required to generate code.");
            }

            if (string.IsNullOrEmpty(configuration.Namespace))
            {
                throw new ArgumentException("Namespace is required to generate code.");
            }
        }
    }
}
