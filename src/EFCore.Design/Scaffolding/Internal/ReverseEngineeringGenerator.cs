// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ReverseEngineeringGenerator
    {
        private readonly ConfigurationFactory _configurationFactory;
        private readonly IScaffoldingModelFactory _factory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReverseEngineeringGenerator(
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] ConfigurationFactory configurationFactory,
            [NotNull] CodeWriter codeWriter)
        {
            Check.NotNull(scaffoldingModelFactory, nameof(scaffoldingModelFactory));
            Check.NotNull(configurationFactory, nameof(configurationFactory));
            Check.NotNull(codeWriter, nameof(codeWriter));

            _factory = scaffoldingModelFactory;
            _configurationFactory = configurationFactory;
            CodeWriter = codeWriter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual CodeWriter CodeWriter { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<ReverseEngineerFiles> GenerateAsync(
            [NotNull] ReverseEngineeringConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(configuration, nameof(configuration));

            cancellationToken.ThrowIfCancellationRequested();

            configuration.CheckValidity();

            var metadataModel = GetMetadataModel(configuration);

            var outputPathsAndNamespace = ConstructNamespaceAndCanonicalizedPaths(
                configuration.ProjectRootNamespace,
                configuration.ProjectPath, configuration.OutputPath);

            var customConfiguration = _configurationFactory
                .CreateCustomConfiguration(
                    configuration.ConnectionString, configuration.ContextClassName,
                    outputPathsAndNamespace.Namespace, configuration.UseFluentApiOnly);
            var modelConfiguration = _configurationFactory
                .CreateModelConfiguration(metadataModel, customConfiguration);

            var dbContextClassName =
                string.IsNullOrWhiteSpace(customConfiguration.ContextClassName)
                    ? modelConfiguration.ClassName()
                    : customConfiguration.ContextClassName;

            CheckOutputFiles(outputPathsAndNamespace.CanonicalizedFullOutputPath,
                dbContextClassName, metadataModel, configuration.OverwriteFiles);

            return CodeWriter.WriteCodeAsync(
                modelConfiguration, outputPathsAndNamespace.CanonicalizedFullOutputPath,
                dbContextClassName, cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel GetMetadataModel([NotNull] ReverseEngineeringConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));

            var metadataModel = _factory.Create(
                configuration.ConnectionString, configuration.TableSelectionSet);
            if (metadataModel == null)
            {
                throw new InvalidOperationException(
                    RelationalDesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().ShortDisplayName()));
            }

            return metadataModel;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        private static readonly char[] _directorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        ///     Construct canonicalized paths from the project path and output path and a namespace
        ///     based om the root namespace for the project.
        /// </summary>
        /// <param name="rootNamespace"> the root namespace for the project, must not be empty </param>
        /// <param name="projectPath"> path to the project, must not be empty, can be absolute or relative </param>
        /// <param name="outputPath"> path to output directory, can be null or empty, can be absolute or relative (to the project path) </param>
        /// <returns>
        ///     an <see cref="NamespaceAndOutputPaths" /> object containing the canonicalized paths
        ///     and the namespace
        /// </returns>
        internal static NamespaceAndOutputPaths ConstructNamespaceAndCanonicalizedPaths(
            [NotNull] string rootNamespace,
            [NotNull] string projectPath,
            [CanBeNull] string outputPath)
        {
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectPath, nameof(projectPath));

            // Strip off any directory separator chars at end of project path
            // to ensure that when we strip off the canonicalized project path
            // later to form the canonicalized relative path we strip off the
            // correct number of characters.
            for (var projectPathLastChar = projectPath.Last();
                 _directorySeparatorChars.Contains(projectPathLastChar);
                 projectPathLastChar = projectPath.Last())
            {
                projectPath = projectPath.Substring(0, projectPath.Length - 1);
            }

            var canonicalizedFullProjectPath = Path.GetFullPath(projectPath);
            var canonicalizedFullOutputPath =
                string.IsNullOrEmpty(outputPath)
                    ? canonicalizedFullProjectPath
                    : Path.GetFullPath(Path.Combine(projectPath, outputPath));

            var canonicalizedRelativeOutputPath =
                canonicalizedFullOutputPath == canonicalizedFullProjectPath
                    ? string.Empty
                    : canonicalizedFullOutputPath.StartsWith(canonicalizedFullProjectPath, StringComparison.Ordinal)
                        ? canonicalizedFullOutputPath
                            .Substring(canonicalizedFullProjectPath.Length + 1)
                        : null;

            var @namespace = rootNamespace;
            if (!string.IsNullOrEmpty(canonicalizedRelativeOutputPath))
            {
                foreach (var pathPart in canonicalizedRelativeOutputPath
                    .Split(_directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries))
                {
                    @namespace += "." + CSharpUtilities.Instance.GenerateCSharpIdentifier(pathPart, null);
                }
            }

            return new NamespaceAndOutputPaths(@namespace,
                canonicalizedFullOutputPath, canonicalizedRelativeOutputPath);
        }

        internal class NamespaceAndOutputPaths
        {
            internal NamespaceAndOutputPaths(
                [NotNull] string @namespace,
                [NotNull] string canonicalizedFullOutputPath,
                [CanBeNull] string canonicalizedRelativeOutputPath)
            {
                Check.NotEmpty(@namespace, nameof(@namespace));
                Check.NotEmpty(canonicalizedFullOutputPath, nameof(canonicalizedFullOutputPath));

                Namespace = @namespace;
                CanonicalizedFullOutputPath = canonicalizedFullOutputPath;
                CanonicalizedRelativeOutputPath = canonicalizedRelativeOutputPath;
            }

            internal string Namespace { get; }
            internal string CanonicalizedFullOutputPath { get; }
            internal string CanonicalizedRelativeOutputPath { get; }
        }
    }
}
