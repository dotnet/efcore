// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.CodeGeneration;
using Microsoft.Framework.CodeGeneration.Templating;
using Microsoft.Framework.Runtime;
using System.Text;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class ReverseEngineeringGenerator
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ICodeGeneratorActionsService _codeGeneratorActionsService;
        private readonly ITemplating _templatingService;

        public ReverseEngineeringGenerator(
            ILibraryManager libraryManager,
            IApplicationEnvironment applicationEnvironment,
            ICodeGeneratorActionsService codeGeneratorActionsService,
            ITemplating templatingService)
        {
            _libraryManager = libraryManager;
            _applicationEnvironment = applicationEnvironment;
            _codeGeneratorActionsService = codeGeneratorActionsService;
            _templatingService = templatingService;
        }

        public virtual IEnumerable<string> TemplateFolders
        {
            get
            {
                var path = _applicationEnvironment.ApplicationBasePath + @"\Templates\ReverseEngineering";
                return new[] { path };
            }
        }

        public async Task GenerateFromTemplate(
            ReverseEngineeringGeneratorModel commandLineModel,
            IDatabaseMetadataModelProvider provider)
        {
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

            // generate context class
            await _codeGeneratorActionsService.AddFileFromTemplateAsync(
                commandLineModel.OutputPath + @"\" + commandLineModel.ContextClassName + ".cs",
                "ContextTemplate.cshtml",
                TemplateFolders,
                contextTemplateModel);

            // generate poco class for each Entity Type
            var entityTypeTemplateModel = new EntityTypeTemplateModel()
            {
                Namespace = commandLineModel.Namespace,
                ProviderAssembly = commandLineModel.ProviderAssembly.FullName,
                ConnectionString = commandLineModel.ConnectionString,
                Filters = (commandLineModel.Filters ?? ""),
            };
            foreach (var entityType in metadataModel.EntityTypes)
            {
                entityTypeTemplateModel.EntityType = entityType;

                await _codeGeneratorActionsService.AddFileFromTemplateAsync(
                    commandLineModel.OutputPath + @"\" + entityType.SimpleName + ".cs",
                    "PocoTemplate.cshtml",
                    TemplateFolders,
                    entityTypeTemplateModel);
            }
        }

        public async Task GenerateFromTemplateResource(
            ReverseEngineeringGeneratorModel commandLineModel,
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

            //TODO - output result to file

            // generate poco class for each Entity Type
            var entityTypeTemplateModel = new EntityTypeTemplateModel()
            {
                Namespace = commandLineModel.Namespace,
                ProviderAssembly = commandLineModel.ProviderAssembly.FullName,
                ConnectionString = commandLineModel.ConnectionString,
                Filters = (commandLineModel.Filters ?? ""),
            };
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

                //TODO - output result to file
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

        private static void CheckGeneratorModel(ReverseEngineeringGeneratorModel generatorModel)
        {
            if (string.IsNullOrEmpty(generatorModel.ProviderAssembly.FullName))
            {
                throw new ArgumentException("ProviderAssembly is required to generate code.");
            }

            if (string.IsNullOrEmpty(generatorModel.ConnectionString))
            {
                throw new ArgumentException("ConnectionString is required to generate code.");
            }

            if (string.IsNullOrEmpty(generatorModel.OutputPath))
            {
                throw new ArgumentException("OutputPath is required to generate code.");
            }

            if (string.IsNullOrEmpty(generatorModel.Namespace))
            {
                throw new ArgumentException("Namespace is required to generate code.");
            }

            if (string.IsNullOrEmpty(generatorModel.ContextClassName))
            {
                throw new ArgumentException("ContextClassName is required to generate code.");
            }
        }
    }
}
