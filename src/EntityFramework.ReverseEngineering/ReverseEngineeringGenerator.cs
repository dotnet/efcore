// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.CodeGeneration;
using Microsoft.Framework.CodeGeneration.CommandLine;
using Microsoft.Framework.Runtime;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    [Alias("reverseEngineer")]
    public class ReverseEngineeringGenerator : ICodeGenerator
    {
        public const string AssemblyName = "EntityFramework.ReverseEngineering";

        private readonly ILibraryManager _libraryManager;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ICodeGeneratorActionsService _codeGeneratorActionsService;

        public ReverseEngineeringGenerator(
            ILibraryManager libraryManager,
            IApplicationEnvironment applicationEnvironment,
            ICodeGeneratorActionsService codeGeneratorActionsService)
        {
            _libraryManager = libraryManager;
            _applicationEnvironment = applicationEnvironment;
            _codeGeneratorActionsService = codeGeneratorActionsService;
        }

        public virtual IEnumerable<string> TemplateFolders
        {
            get
            {
                return TemplateFoldersUtilities.GetTemplateFolders(
                    containingProject: "EntityFramework.ReverseEngineering",
                    applicationBasePath: _applicationEnvironment.ApplicationBasePath,
                    baseFolders: new[] { "ReverseEngineering" },
                    libraryManager: _libraryManager);
            }
        }

        public async Task GenerateCode(ReverseEngineeringGeneratorModel generatorModel)
        {
            CheckGeneratorModel(generatorModel);

            var provider = GetFromAssembly(generatorModel.ProviderAssembly);

            await GenerateFromTemplate(generatorModel, provider);
        }

        private async Task GenerateFromTemplate(
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
                    ProviderAssembly = commandLineModel.ProviderAssembly,
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
                ProviderAssembly = commandLineModel.ProviderAssembly,
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

        private IDatabaseMetadataModelProvider GetFromAssembly(string assemblyFilePath)
        {
            var assemblies = GetCandidateAssemblies(assemblyFilePath);
            if (assemblies == null)
            {
                return null;
            }

            var type = assemblies
                .SelectMany(a => a.GetExportedTypes())
                .FirstOrDefault(
                    t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
            if (type != null)
            {
                return (IDatabaseMetadataModelProvider)(Activator.CreateInstance(type));
            }

            return null;
        }

        private IEnumerable<Assembly> GetCandidateAssemblies(string assemblyFilePath)
        {
            var assembly = Assembly.LoadFrom(assemblyFilePath);
            return new List<Assembly>() { assembly };
            //return _libraryManager.GetReferencingLibraries(AssemblyName)
            //    .Distinct()
            //    .Where(l => l.Name == assemblyFilePath)
            //    .SelectMany(l => l.LoadableAssemblies)
            //    .Select(Load);
        }

        private static Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        private static void CheckGeneratorModel(ReverseEngineeringGeneratorModel generatorModel)
        {
            if (string.IsNullOrEmpty(generatorModel.ProviderAssembly))
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
