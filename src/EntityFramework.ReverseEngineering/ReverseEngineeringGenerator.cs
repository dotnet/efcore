// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Framework.CodeGeneration.Templating;

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
            ReverseEngineeringCommandLineModel commandLineModel,
            IDatabaseMetadataModelProvider provider,
            string contextTemplateResourceName,
            string pocoTemplateResourceName)
        {
            var providerAssembly = commandLineModel.ProviderAssembly;
            if (!providerAssembly.GetManifestResourceNames().Contains(contextTemplateResourceName))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " does not contain template resource with name " + contextTemplateResourceName +
                    ". It only contains [" + string.Join(",", providerAssembly.GetManifestResourceNames()) + "]");
            }
            if (!providerAssembly.GetManifestResourceNames().Contains(pocoTemplateResourceName))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " does not contain template resource with name " + pocoTemplateResourceName +
                    ". It only contains [" + string.Join(",", providerAssembly.GetManifestResourceNames()) + "]");
            }

            var metadataModel = provider.GenerateMetadataModel(commandLineModel.ConnectionString, commandLineModel.Filters);
            if (metadataModel == null)
            {
                throw new InvalidProgramException("Model returned is null. Provider class " + provider.GetType()
                    + ", connection string: " + commandLineModel.ConnectionString
                    + ", filters " + commandLineModel.Filters);
            }

            if (metadataModel.EntityTypes.Count() == 0)
            {
                throw new InvalidProgramException("Model returned contains no EntityTypes. Provider class " + provider.GetType()
                    + ", connection string: " + commandLineModel.ConnectionString
                    + ", filters " + commandLineModel.Filters);
            }

            var contextTemplateModel = new ContextTemplateModel()
            {
                ClassName = commandLineModel.ContextClassName,
                Namespace = commandLineModel.Namespace,
                ProviderAssembly = commandLineModel.ProviderAssembly.FullName,
                ConnectionString = commandLineModel.ConnectionString,
                Filters = (commandLineModel.Filters ?? ""),
                MetadataModel = metadataModel
            };
            var contextTemplatingHelper = new ContextTemplatingHelper(contextTemplateModel);
            contextTemplateModel.Helper = contextTemplatingHelper;

            // get context template content
            string contextTemplateContent = null;
            using (var templateStream = providerAssembly.GetManifestResourceStream(contextTemplateResourceName))
            {
                using (var reader = new StreamReader(templateStream, Encoding.UTF8))
                {
                    contextTemplateContent = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrWhiteSpace(contextTemplateContent))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " template name " + contextTemplateResourceName + " is empty or contains only whitespace.");
            }

            // get poco template content
            string pocoTemplateContent = null;
            using (var templateStream = providerAssembly.GetManifestResourceStream(pocoTemplateResourceName))
            {
                using (var reader = new StreamReader(templateStream, Encoding.UTF8))
                {
                    pocoTemplateContent = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrWhiteSpace(pocoTemplateContent))
            {
                throw new InvalidProgramException("Assembly " + providerAssembly.FullName +
                    " template name " + pocoTemplateResourceName + " is empty or contains only whitespace.");
            }

            // generate context class
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
                await OutputFile(commandLineModel.OutputPath, commandLineModel.ContextClassName + ".cs", sourceStream);
            }

            // generate poco class for each Entity Type
            var entityTypeTemplateModel = new EntityTypeTemplateModel()
            {
                Namespace = commandLineModel.Namespace,
                ProviderAssembly = commandLineModel.ProviderAssembly.FullName,
                ConnectionString = commandLineModel.ConnectionString,
                Filters = (commandLineModel.Filters ?? ""),
            };
            var entityTypeTemplatingHelper = new EntityTypeTemplatingHelper(entityTypeTemplateModel);
            entityTypeTemplateModel.Helper = entityTypeTemplatingHelper;
            foreach (var entityType in metadataModel.EntityTypes)
            {
                entityTypeTemplateModel.EntityType = entityType;

                var pocoTemplateResult = await _templatingService.RunTemplateAsync(pocoTemplateContent, entityTypeTemplateModel);
                if (pocoTemplateResult.ProcessingException != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "There was an error running the template named {0}: {1}",
                        pocoTemplateResourceName,
                        pocoTemplateResult.ProcessingException.Message));
                }

                // output poco file
                using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(pocoTemplateResult.GeneratedText)))
                {
                    await OutputFile(commandLineModel.OutputPath, entityType.SimpleName + ".cs", sourceStream);
                }
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

        //private IDatabaseMetadataModelProvider GetFromAssembly(string assemblyFilePath)
        //{
        //    var assemblies = GetCandidateAssemblies(assemblyFilePath);
        //    if (assemblies == null)
        //    {
        //        return null;
        //    }

        //    var type = assemblies
        //        .SelectMany(a => a.GetExportedTypes())
        //        .FirstOrDefault(
        //            t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
        //    if (type != null)
        //    {
        //        return (IDatabaseMetadataModelProvider)(Activator.CreateInstance(type));
        //    }

        //    return null;
        //}

        //private IEnumerable<Assembly> GetCandidateAssemblies(string assemblyFilePath)
        //{
        //    var assembly = Assembly.LoadFrom(assemblyFilePath);
        //    return new List<Assembly>() { assembly };
        //    //return _libraryManager.GetReferencingLibraries(AssemblyName)
        //    //    .Distinct()
        //    //    .Where(l => l.Name == assemblyFilePath)
        //    //    .SelectMany(l => l.LoadableAssemblies)
        //    //    .Select(Load);
        //}

        //private static Assembly Load(AssemblyName assemblyName)
        //{
        //    return Assembly.Load(assemblyName);
        //}

        private static void CheckGeneratorModel(ReverseEngineeringCommandLineModel commandLineModel)
        {
            if (commandLineModel.ProviderAssembly == null)
            {
                throw new ArgumentException("ProviderAssembly is required to generate code.");
            }

            if (string.IsNullOrEmpty(commandLineModel.ConnectionString))
            {
                throw new ArgumentException("ConnectionString is required to generate code.");
            }

            if (string.IsNullOrEmpty(commandLineModel.OutputPath))
            {
                throw new ArgumentException("OutputPath is required to generate code.");
            }

            if (string.IsNullOrEmpty(commandLineModel.Namespace))
            {
                throw new ArgumentException("Namespace is required to generate code.");
            }

            if (string.IsNullOrEmpty(commandLineModel.ContextClassName))
            {
                throw new ArgumentException("ContextClassName is required to generate code.");
            }
        }
    }
}
