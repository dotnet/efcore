// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private readonly IServiceProvider _serviceProvider;

        private Dictionary<IEntityType, string> _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();
        private Dictionary<IProperty, string> _propertyToPropertyNameMap = new Dictionary<IProperty, string>();

        public ReverseEngineeringGenerator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Dictionary<IEntityType, string> EntityTypeToClassNameMap
        {
            get
            {
                return _entityTypeToClassNameMap;
            }
        }

        public Dictionary<IProperty, string> PropertyToPropertyNameMap
        {
            get
            {
                return _propertyToPropertyNameMap;
            }
        }

        public async Task Generate(ReverseEngineeringConfiguration configuration)
        {
            CheckConfiguration(configuration);

            var providerAssembly = configuration.ProviderAssembly;
            var provider = GetProvider(providerAssembly);
            var metadataModel = GetMetadataModel(provider, configuration);

            ConstructGlobalNameMaps(metadataModel);

            // generate DbContext code
            var dbContextGeneratorModel = new DbContextGeneratorModel()
            {
                ClassName = configuration.ContextClassName,
                Namespace = configuration.Namespace,
                ProviderAssembly = configuration.ProviderAssembly.FullName,
                ConnectionString = configuration.ConnectionString,
                Filters = (configuration.Filters ?? ""),
                MetadataModel = metadataModel
            };

            //TODO - check to see whether user has one in current project first
            var dbContextCodeGeneratorContext =
                provider.GetContextModelCodeGenerator(this, dbContextGeneratorModel);
            if (dbContextCodeGeneratorContext == null)
            {
                throw new InvalidProgramException(
                    "Provider " + provider.GetType().FullName
                    + " did not provide a ContextModelCodeGeneratorContext");
            }

            var contextStringBuilder = new IndentedStringBuilder();
            dbContextCodeGeneratorContext.Generate(contextStringBuilder);

            // output context file
            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(contextStringBuilder.ToString())))
            {
                await OutputFile(configuration.OutputPath, configuration.ContextClassName + ".cs", sourceStream);
            }

            // generate EntityType code for each Entity Type
            foreach (var entityType in metadataModel.EntityTypes)
            {
                var entityTypeGeneratorModel = new EntityTypeGeneratorModel()
                {
                    EntityType = entityType,
                    Namespace = configuration.Namespace,
                    ProviderAssembly = configuration.ProviderAssembly.FullName,
                    ConnectionString = configuration.ConnectionString,
                    Filters = (configuration.Filters ?? ""),
                };

                //TODO - check to see whether user has one in current project first
                var entityTypeCodeGeneratorContext =
                    provider.GetEntityTypeModelCodeGenerator(
                        this
                        , entityTypeGeneratorModel);
                if (entityTypeCodeGeneratorContext == null)
                {
                    throw new InvalidProgramException(
                        "Provider " + provider.GetType().FullName
                        + " did not provide a EntityTypeModelCodeGeneratorContext");
                }

                var entityTypeStringBuilder = new IndentedStringBuilder();
                entityTypeCodeGeneratorContext.Generate(entityTypeStringBuilder);

                // output EntityType poco file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(entityTypeStringBuilder.ToString())))
                {
                    await OutputFile(configuration.OutputPath
                        , EntityTypeToClassNameMap[entityType] + ".cs"
                        , sourceStream);
                }
            }
        }

        public IDatabaseMetadataModelProvider GetProvider(Assembly providerAssembly)
        {
            var type = providerAssembly.GetExportedTypes()
                .FirstOrDefault(t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
            if (type == null)
            {
                throw new InvalidProgramException(
                    "Assembly " + providerAssembly.FullName
                    + " does not contain a type which extends "
                    + typeof(IDatabaseMetadataModelProvider).FullName);
            }

            IDatabaseMetadataModelProvider metadataModelProvider = null;
            try
            {
                metadataModelProvider = (IDatabaseMetadataModelProvider)Activator.CreateInstance(type, _serviceProvider);
            }
            catch (Exception e)
            {
                throw new InvalidProgramException(
                    "Unable to instantiate type " + type.FullName
                    + " in assembly " + providerAssembly.FullName
                    + ". Exception message: " + e.Message);
            }

            return metadataModelProvider;
        }

        public static IModel GetMetadataModel(
            IDatabaseMetadataModelProvider provider, ReverseEngineeringConfiguration configuration)
        {
            var metadataModel = provider
                .GenerateMetadataModel(configuration.ConnectionString, configuration.Filters);
            if (metadataModel == null)
            {
                throw new InvalidProgramException("Model returned is null. Provider class " + provider.GetType()
                    + ", connection string: " + configuration.ConnectionString
                    + ", filters " + configuration.Filters);
            }

            if (metadataModel.EntityTypes.Count() == 0)
            {
                throw new InvalidProgramException("Model returned contains no EntityTypes. Provider class " + provider.GetType()
                    + ", connection string: " + configuration.ConnectionString
                    + ", filters " + configuration.Filters);
            }

            return metadataModel;
        }

        private void ConstructGlobalNameMaps(IModel model)
        {
            _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();
            foreach (var entityType in model.EntityTypes)
            {
                _entityTypeToClassNameMap[entityType] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        entityType.SimpleName, _entityTypeToClassNameMap.Values);
                InitializePropertyNames(entityType);
            }
        }

        private void InitializePropertyNames(IEntityType entityType)
        {
            // use local propertyToPropertyNameMap to ensure no clashes in Property names
            // within an EntityType but to allow them for properties in different EntityTypes
            var propertyToPropertyNameMap = new Dictionary<IProperty, string>();
            foreach (var property in entityType.Properties)
            {
                propertyToPropertyNameMap[property] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        property.Name, propertyToPropertyNameMap.Values);
            }

            foreach (var keyValuePair in propertyToPropertyNameMap)
            {
                _propertyToPropertyNameMap.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private async Task OutputFile(string outputDirectoryName, string outputFileName, Stream sourceStream)
        {
            if (!Directory.Exists(outputDirectoryName))
            {
                Directory.CreateDirectory(outputDirectoryName);
            }

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

            if (string.IsNullOrEmpty(configuration.ContextClassName))
            {
                throw new ArgumentException("ContextClassName is required to generate code.");
            }
        }
    }
}
