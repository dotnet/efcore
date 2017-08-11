// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModelScaffolder : IModelScaffolder
    {
        private readonly IScaffoldingModelFactory _factory;
        private readonly ICSharpUtilities _cSharpUtilities;
        private static readonly char[] _directorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private const string DbContextSuffix = "Context";
        private const string DefaultDbContextName = "Model" + DbContextSuffix;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModelScaffolder(
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] IScaffoldingCodeGenerator scaffoldingCodeGenerator,
            [NotNull] ICSharpUtilities cSharpUtilities)
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
        private IScaffoldingCodeGenerator ScaffoldingCodeGenerator { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ReverseEngineerFiles Generate(
            string connectionString,
            IEnumerable<string> tables,
            IEnumerable<string> schemas,
            string projectPath,
            string outputPath,
            string rootNamespace,
            string contextName,
            bool useDataAnnotations,
            bool overwriteFiles,
            bool useDatabaseNames)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));
            Check.NotEmpty(projectPath, nameof(projectPath));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));

            if (!string.IsNullOrWhiteSpace(contextName)
                && (!_cSharpUtilities.IsValidIdentifier(contextName)
                    || _cSharpUtilities.IsCSharpKeyword(contextName)))
            {
                throw new ArgumentException(
                    DesignStrings.ContextClassNotValidCSharpIdentifier(contextName));
            }

            var model = _factory.Create(connectionString, tables, schemas, useDatabaseNames);

            if (model == null)
            {
                throw new InvalidOperationException(
                    DesignStrings.ProviderReturnedNullModel(
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
                                      .Select(
                                          p => _cSharpUtilities.GenerateCSharpIdentifier(
                                              p,
                                              existingIdentifiers: null,
                                              singularizePluralizer: null)));
            }

            if (string.IsNullOrEmpty(contextName))
            {
                contextName = DefaultDbContextName;

                var annotatedName = model.Scaffolding().DatabaseName;
                if (!string.IsNullOrEmpty(annotatedName))
                {
                    contextName = _cSharpUtilities.GenerateCSharpIdentifier(
                        annotatedName + DbContextSuffix,
                        existingIdentifiers: null,
                        singularizePluralizer: null);
                }
            }

            CheckOutputFiles(fullOutputPath, contextName, model, overwriteFiles);

            return ScaffoldingCodeGenerator.WriteCode(model, fullOutputPath, @namespace, contextName, connectionString, useDataAnnotations);
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
                    DesignStrings.ReadOnlyFiles(
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
                        DesignStrings.ExistingFiles(
                            outputPath,
                            string.Join(
                                CultureInfo.CurrentCulture.TextInfo.ListSeparator, existingFiles)));
                }
            }
        }
    }
}
