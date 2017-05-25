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
    public class DbContextScaffolder
    {
        private readonly IScaffoldingModelFactory _factory;
        private readonly CSharpUtilities _cSharpUtilities;
        private static readonly char[] _directorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private const string DbContextSuffix = "Context";
        private const string DefaultDbContextName = "Model" + DbContextSuffix;
        
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbContextScaffolder(
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] ScaffoldingCodeGenerator scaffoldingCodeGenerator,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(scaffoldingModelFactory, nameof(scaffoldingModelFactory));
            Check.NotNull(scaffoldingCodeGenerator, nameof(scaffoldingCodeGenerator));

            _factory = scaffoldingModelFactory;
            ScaffoldingCodeGenerator = scaffoldingCodeGenerator;
            _cSharpUtilities = cSharpUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private ScaffoldingCodeGenerator ScaffoldingCodeGenerator { get; }

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
                && (!_cSharpUtilities.IsValidIdentifier(contextName)
                    || _cSharpUtilities.IsCSharpKeyword(contextName)))
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
                                      .Select(p => _cSharpUtilities.GenerateCSharpIdentifier(p, existingIdentifiers: null)));
            }

            if (string.IsNullOrEmpty(contextName))
            {
                contextName = DefaultDbContextName;

                var annotatedName = model.Scaffolding().DatabaseName;
                if (!string.IsNullOrEmpty(annotatedName))
                {
                    contextName = _cSharpUtilities.GenerateCSharpIdentifier(annotatedName + DbContextSuffix, existingIdentifiers: null);
                }
            }

            CheckOutputFiles(fullOutputPath, contextName, model, overwriteFiles);

            return ScaffoldingCodeGenerator.WriteCodeAsync(model, fullOutputPath, @namespace, contextName, connectionString, useDataAnnotations, cancellationToken);
        }

        private void CheckOutputFiles(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IModel metadataModel,
            bool overwriteFiles)
        {
            Check.NotEmpty(outputPath, nameof(outputPath));
            Check.NotEmpty(dbContextClassName, nameof(dbContextClassName));
            Check.NotNull(metadataModel, nameof(metadataModel));

            var readOnlyFiles = ScaffoldingCodeGenerator.GetReadOnlyFilePaths(
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
                var existingFiles = ScaffoldingCodeGenerator.GetExistingFilePaths(
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
