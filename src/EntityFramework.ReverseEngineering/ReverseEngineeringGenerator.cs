// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Framework.CodeGeneration.Templating;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private readonly ITemplating _templatingService;

        public ReverseEngineeringGenerator(
            ITemplating templatingService)
        {
            _templatingService = templatingService;
        }

        public async Task GenerateFromTemplateResource(ReverseEngineeringConfiguration configuration)
        {
            CheckConfiguration(configuration);

            var providerAssembly = configuration.ProviderAssembly;
            var provider = GetProvider(providerAssembly);
            //var contextTemplateResourceName = provider.GetContextTemplateResourceName();
            //var entityTypeTemplateResourceName = provider.GetEntityTypeTemplateResourceName();
            //var contextTemplateContent = GetTemplateContent(providerAssembly, contextTemplateResourceName);
            //var entityTypeTemplateContent = GetTemplateContent(providerAssembly, entityTypeTemplateResourceName);
            var contextTemplateContent = provider.GetContextTemplate();
            var entityTypeTemplateContent = provider.GetEntityTypeTemplate();

            var metadataModel = GetMetadataModel(provider, configuration);

            // run context template
            var contextTemplateModel = new ContextTemplateModel()
            {
                ClassName = configuration.ContextClassName,
                Namespace = configuration.Namespace,
                ProviderAssembly = configuration.ProviderAssembly.FullName,
                ConnectionString = configuration.ConnectionString,
                Filters = (configuration.Filters ?? ""),
                MetadataModel = metadataModel
            };
            var contextTemplatingHelper = 
                provider.GetContextTemplateHelper(contextTemplateModel) ?? new ContextTemplatingHelper(contextTemplateModel);
            contextTemplateModel.Helper = contextTemplatingHelper;

            var contextTemplateResult = await _templatingService.RunTemplateAsync(contextTemplateContent, contextTemplateModel);
            if (contextTemplateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(
                    "There was an error running the context template. Error: "
                    + contextTemplateResult.ProcessingException.Message);
            }

            // output context file
            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(contextTemplateResult.GeneratedText)))
            {
                await OutputFile(configuration.OutputPath, configuration.ContextClassName + ".cs", sourceStream);
            }

            // run EntityType template for each Entity Type
            var entityTypeTemplateModel = new EntityTypeTemplateModel()
            {
                Namespace = configuration.Namespace,
                ProviderAssembly = configuration.ProviderAssembly.FullName,
                ConnectionString = configuration.ConnectionString,
                Filters = (configuration.Filters ?? ""),
            };
            foreach (var entityType in metadataModel.EntityTypes)
            {
                entityTypeTemplateModel.EntityType = entityType;
                var entityTypeTemplatingHelper =
                    provider.GetEntityTypeTemplateHelper(entityTypeTemplateModel)
                        ?? new EntityTypeTemplatingHelper(entityTypeTemplateModel);
                entityTypeTemplateModel.Helper = entityTypeTemplatingHelper;

                var entityTypeTemplateResult = await _templatingService
                    .RunTemplateAsync(entityTypeTemplateContent, entityTypeTemplateModel);
                if (entityTypeTemplateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(
                        "There was an error running the EntityType template. Error: "
                        + entityTypeTemplateResult.ProcessingException.Message);
                }

                // output EntityType poco file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(entityTypeTemplateResult.GeneratedText)))
                {
                    await OutputFile(configuration.OutputPath, entityType.SimpleName + ".cs", sourceStream);
                }
            }
        }

        public static string GetTemplateContent(Assembly providerAssembly, string templateResourceName)
        {
            if (!providerAssembly.GetManifestResourceNames().Contains(templateResourceName))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " does not contain template resource with name " + templateResourceName +
                    ". It only contains [" + string.Join(",", providerAssembly.GetManifestResourceNames()) + "]");
            }

            string templateContent = null;
            using (var templateStream = providerAssembly.GetManifestResourceStream(templateResourceName))
            {
                using (var reader = new StreamReader(templateStream, Encoding.UTF8))
                {
                    templateContent = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrWhiteSpace(templateContent))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " template name " + templateResourceName + " is empty or contains only whitespace.");
            }

            return templateContent;
        }

        public static Type GetTemplateHelperClass(Assembly providerAssembly, string templateHelperClassName)
        {
            var type = providerAssembly.GetExportedTypes()
                .FirstOrDefault(t => t.Name == templateHelperClassName);
            if (type == null)
            {
                throw new InvalidProgramException(
                    "Assembly " + providerAssembly.FullName
                    + " does not contain a type matching name " + templateHelperClassName);
            }

            if (!typeof(BaseTemplatingHelper).IsAssignableFrom(type))
            {
                throw new InvalidProgramException(
                    "Class " + templateHelperClassName
                    + " from assembly " + providerAssembly.FullName
                    + " does not extend " + typeof(BaseTemplatingHelper).FullName);
            }

            return type;
        }

        public static IDatabaseMetadataModelProvider GetProvider(Assembly providerAssembly)
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
                metadataModelProvider = (IDatabaseMetadataModelProvider)Activator.CreateInstance(type);
            }
            catch (Exception)
            {
                throw new InvalidProgramException(
                    "Unable to instantiate type " + type.FullName
                    + " in assembly " + providerAssembly.FullName);
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
