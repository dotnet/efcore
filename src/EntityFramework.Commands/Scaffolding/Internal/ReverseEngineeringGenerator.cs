// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class ReverseEngineeringGenerator
    {
        private readonly ConfigurationFactory _configurationFactory;
        private readonly IScaffoldingModelFactory _factory;

        public ReverseEngineeringGenerator(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] ConfigurationFactory configurationFactory,
            [NotNull] CodeWriter codeWriter)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(scaffoldingModelFactory, nameof(scaffoldingModelFactory));
            Check.NotNull(configurationFactory, nameof(configurationFactory));
            Check.NotNull(codeWriter, nameof(codeWriter));

            Logger = loggerFactory.CreateCommandsLogger();
            _factory = scaffoldingModelFactory;
            _configurationFactory = configurationFactory;
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

            var outputPaths = ConstructCanonicalizedPaths(configuration.ProjectPath, configuration.OutputPath);

            var @namespace = ConstructNamespace(configuration.ProjectRootNamespace,
                    outputPaths.CanonicalizedRelativeOutputPath);

            var customConfiguration = _configurationFactory
                .CreateCustomConfiguration(
                    configuration.ConnectionString, configuration.ContextClassName,
                    @namespace, configuration.UseFluentApiOnly);
            var modelConfiguration = _configurationFactory
                .CreateModelConfiguration(metadataModel, customConfiguration);

            var dbContextClassName =
                string.IsNullOrWhiteSpace(customConfiguration.ContextClassName)
                ? modelConfiguration.ClassName()
                : customConfiguration.ContextClassName;

            CheckOutputFiles(outputPaths.CanonicalizedFullOutputPath,
                dbContextClassName, metadataModel, configuration.OverwriteFiles);

            return CodeWriter.WriteCodeAsync(
                modelConfiguration, outputPaths.CanonicalizedFullOutputPath,
                dbContextClassName, cancellationToken);
        }

        public virtual IModel GetMetadataModel([NotNull] ReverseEngineeringConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));

            var metadataModel = _factory.Create(
                configuration.ConnectionString, configuration.TableSelectionSet);
            if (metadataModel == null)
            {
                throw new InvalidOperationException(
                    RelationalDesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().FullName));
            }

            return metadataModel;
        }

        public virtual void CheckOutputFiles(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IModel metadataModel,
            bool overwriteFiles)
        {
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(metadataModel, nameof(metadataModel));

            var readOnlyFiles = CodeWriter.GetReadOnlyFilePaths(
                outputPath, dbContextClassName, metadataModel.GetEntityTypes());
            if (readOnlyFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    RelationalDesignStrings.ReadOnlyFiles(
                        outputPath,
                        string.Join(
                            CultureInfo.CurrentCulture.TextInfo.ListSeparator, readOnlyFiles)));
            }

            if (!overwriteFiles)
            {
                var existingFiles = CodeWriter.GetExistingFilePaths(
                    outputPath, dbContextClassName, metadataModel.GetEntityTypes());
                if (existingFiles.Count > 0)
                {
                    throw new InvalidOperationException(
                        RelationalDesignStrings.ExistingFiles(
                            outputPath,
                            string.Join(
                                CultureInfo.CurrentCulture.TextInfo.ListSeparator, existingFiles)));
                }
            }
        }

        private static char[] directorySeparatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public static string ConstructNamespace(
            [NotNull] string rootNamespace, [CanBeNull] string canonicalizedRelativeOutputPath)
        {
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            if (string.IsNullOrEmpty(canonicalizedRelativeOutputPath))
            {
                // canonicalized output path is outside of or is the same
                // as the project dir - so just use root namespace
                return rootNamespace;
            }

            var @namespace = rootNamespace;
            foreach (var pathPart in canonicalizedRelativeOutputPath
                .Split(directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries))
            {
                @namespace += "." + CSharpUtilities.Instance.GenerateCSharpIdentifier(pathPart, null);
            }

            return @namespace;
        }

        /// <summary>
        /// Construct canonicalized paths from the project path and output path.
        /// </summary>
        /// <param name="projectPath"> path to the project, must not be empty, can be absolute or relative </param>
        /// <param name="outputPath"> path to output directory, can be null or empty, can be absolute or relative (to the project path) </param>
        /// <returns>
        ///  a <see cref="CanonicalizedOutputPaths"> object containing the canonicalized full output path
        ///  and the canonicalized relative output path
        /// </returns>
        public static CanonicalizedOutputPaths ConstructCanonicalizedPaths(
            [NotNull] string projectPath, [CanBeNull] string outputPath)
        {
            Check.NotEmpty(projectPath, nameof(projectPath));

            // strip off any directory separator chars at end of project path
            for (var projectPathLastChar = projectPath.Last();
                directorySeparatorChars.Contains(projectPathLastChar);
                projectPathLastChar = projectPath.Last())
            {
                projectPath = projectPath.Substring(0, projectPath.Length - 1);
            }

            var canonicalizedFullProjectPath = Path.GetFullPath(projectPath);
            var canonicalizedFullOutputPath =
                string.IsNullOrEmpty(outputPath)
                    ? canonicalizedFullProjectPath
                    : Path.IsPathRooted(outputPath)
                        ? Path.GetFullPath(outputPath)
                        : Path.GetFullPath(Path.Combine(projectPath, outputPath));

            var canonicalizedRelativeOutputPath =
                canonicalizedFullOutputPath == canonicalizedFullProjectPath
                ? string.Empty
                : canonicalizedFullOutputPath.StartsWith(canonicalizedFullProjectPath)
                    ? canonicalizedFullOutputPath
                        .Substring(canonicalizedFullProjectPath.Count() + 1)
                    : null;

            return new CanonicalizedOutputPaths(
                canonicalizedFullOutputPath, canonicalizedRelativeOutputPath);
        }

        public class CanonicalizedOutputPaths
        {
            public CanonicalizedOutputPaths(
                [NotNull] string canonicalizedFullOutputPath,
                [CanBeNull] string canonicalizedRelativeOutputPath)
            {
                Check.NotEmpty(canonicalizedFullOutputPath, nameof(canonicalizedFullOutputPath));

                CanonicalizedFullOutputPath = canonicalizedFullOutputPath;
                CanonicalizedRelativeOutputPath = canonicalizedRelativeOutputPath;
            }

            public virtual string CanonicalizedFullOutputPath { get; }
            public virtual string CanonicalizedRelativeOutputPath { get; }
        }
    }
}
