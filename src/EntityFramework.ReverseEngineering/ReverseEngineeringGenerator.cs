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

        public async Task GenerateFromTemplateResource(
            ReverseEngineeringConfiguration configuration,
            IDatabaseMetadataModelProvider provider,
            string contextTemplateResourceName,
            string entityTypeTemplateResourceName)
        {
            CheckGeneratorModel(configuration);

            var providerAssembly = configuration.ProviderAssembly;
            var contextTemplateContent = GetTemplateContent(providerAssembly, contextTemplateResourceName);
            var entityTypeTemplateContent = GetTemplateContent(providerAssembly, entityTypeTemplateResourceName);

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
            var contextTemplatingHelper = new ContextTemplatingHelper(contextTemplateModel);
            contextTemplateModel.Helper = contextTemplatingHelper;

            var contextTemplateResult = await _templatingService.RunTemplateAsync(contextTemplateContent, contextTemplateModel);
            if (contextTemplateResult.ProcessingException != null)
            {
                throw new InvalidOperationException(string.Format(
                    "There was an error running the template named {0}: {1}",
                    contextTemplateResourceName,
                    contextTemplateResult.ProcessingException.Message));
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
                var entityTypeTemplatingHelper = new EntityTypeTemplatingHelper(entityTypeTemplateModel);
                entityTypeTemplateModel.Helper = entityTypeTemplatingHelper;

                var pocoTemplateResult = await _templatingService
                    .RunTemplateAsync(entityTypeTemplateContent, entityTypeTemplateModel);
                if (pocoTemplateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "There was an error running the template named {0}: {1}",
                        entityTypeTemplateResourceName,
                        pocoTemplateResult.ProcessingException.Message));
                }

                // output poco file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(pocoTemplateResult.GeneratedText)))
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

        private static void CheckGeneratorModel(ReverseEngineeringConfiguration configuration)
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
