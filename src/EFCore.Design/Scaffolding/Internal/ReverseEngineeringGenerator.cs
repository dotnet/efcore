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
        private static readonly char[] _directorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

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
            [NotNull] string connectionString,
            [NotNull] TableSelectionSet tableSelectionSet,
            [NotNull] string projectPath,
            [CanBeNull] string outputPath,
            [NotNull] string rootNamespace,
            [CanBeNull] string contextName,
            bool useDataAnnotations,
            bool overwriteFiles,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));
            Check.NotEmpty(projectPath, nameof(projectPath));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(contextName)
                && (!CSharpUtilities.Instance.IsValidIdentifier(contextName)
                    || CSharpUtilities.Instance.IsCSharpKeyword(contextName)))
            {
                throw new ArgumentException(
                    DesignStrings.ContextClassNotValidCSharpIdentifier(contextName));
            }

            var model = _factory.Create(connectionString, tableSelectionSet);

            if (model == null)
            {
                throw new InvalidOperationException(
                    RelationalDesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().ShortDisplayName()));
            }

            projectPath = projectPath.TrimEnd(_directorySeparatorChars);

            var fullProjectPath = Path.GetFullPath(projectPath);
            var fullOutputPath = string.IsNullOrEmpty(outputPath)
                ? fullProjectPath
                : Path.GetFullPath(Path.Combine(projectPath, outputPath));

            var @namespace = rootNamespace;
            if (!string.Equals(fullOutputPath, fullProjectPath)
                && fullOutputPath.StartsWith(fullProjectPath, StringComparison.Ordinal))
            {
                var relativeOutputPath = fullOutputPath.Substring(fullProjectPath.Length + 1);
                @namespace += "." + string.Join(
                                  ".", relativeOutputPath
                                      .Split(_directorySeparatorChars, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(p => CSharpUtilities.Instance.GenerateCSharpIdentifier(p, null)));
            }

            //var customConfiguration = _configurationFactory.CreateCustomConfiguration(connectionString, contextName, @namespace, useDataAnnotations);
            var modelConfiguration = _configurationFactory.CreateModelConfiguration(model, connectionString, contextName, @namespace, useDataAnnotations);

            var dbContextClassName =
                string.IsNullOrWhiteSpace(contextName)
                    ? modelConfiguration.ClassName()
                    : contextName;

            CheckOutputFiles(fullOutputPath, dbContextClassName, model, overwriteFiles);

            return CodeWriter.WriteCodeAsync(modelConfiguration, fullOutputPath, dbContextClassName, cancellationToken);
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
    }
}
