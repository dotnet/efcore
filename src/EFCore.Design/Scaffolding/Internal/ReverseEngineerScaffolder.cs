// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ReverseEngineerScaffolder : IReverseEngineerScaffolder
    {
        private readonly IDatabaseModelFactory _databaseModelFactory;
        private readonly IScaffoldingModelFactory _factory;
        private readonly ICSharpUtilities _cSharpUtilities;
        private const string DbContextSuffix = "Context";
        private const string DefaultDbContextName = "Model" + DbContextSuffix;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReverseEngineerScaffolder(
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] ScaffoldingCodeGeneratorSelector scaffoldingCodeGeneratorSelector,
            [NotNull] ICSharpUtilities cSharpUtilities)
        {
            Check.NotNull(databaseModelFactory, nameof(databaseModelFactory));
            Check.NotNull(scaffoldingModelFactory, nameof(scaffoldingModelFactory));
            Check.NotNull(scaffoldingCodeGeneratorSelector, nameof(scaffoldingCodeGeneratorSelector));

            _databaseModelFactory = databaseModelFactory;
            _factory = scaffoldingModelFactory;
            ScaffoldingCodeGeneratorSelector = scaffoldingCodeGeneratorSelector;
            _cSharpUtilities = cSharpUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private ScaffoldingCodeGeneratorSelector ScaffoldingCodeGeneratorSelector { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ScaffoldedModel Generate(
            string connectionString,
            IEnumerable<string> tables,
            IEnumerable<string> schemas,
            string projectPath,
            string outputPath,
            string rootNamespace,
            string language,
            string contextName,
            bool useDataAnnotations,
            bool useDatabaseNames)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));
            Check.NotEmpty(projectPath, nameof(projectPath));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotNull(language, nameof(language));

            if (!string.IsNullOrWhiteSpace(contextName)
                && (!_cSharpUtilities.IsValidIdentifier(contextName)
                    || _cSharpUtilities.IsCSharpKeyword(contextName)))
            {
                throw new ArgumentException(
                    DesignStrings.ContextClassNotValidCSharpIdentifier(contextName));
            }

            var databaseModel = _databaseModelFactory.Create(connectionString, tables, schemas);
            var model = _factory.Create(databaseModel, useDatabaseNames);

            if (model == null)
            {
                throw new InvalidOperationException(
                    DesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().ShortDisplayName()));
            }

            outputPath = string.IsNullOrWhiteSpace(outputPath) ? null : outputPath;
            var subNamespace = SubnamespaceFromOutputPath(projectPath, outputPath);

            var @namespace = rootNamespace;

            if (!string.IsNullOrEmpty(subNamespace))
            {
                @namespace += "." + subNamespace;
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

            var codeGenerator = ScaffoldingCodeGeneratorSelector.Select(language);

            return codeGenerator.WriteCode(model, @namespace, contextName, connectionString, useDataAnnotations);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ReverseEngineerFiles Save(
            ScaffoldedModel scaffoldedModel,
            string projectPath,
            string outputPath,
            bool overwriteFiles)
        {
            Check.NotEmpty(projectPath, nameof(projectPath));

            var outputDir = outputPath == null
                ? projectPath
                : Path.GetFullPath(Path.Combine(projectPath, outputPath));

            CheckOutputFiles(scaffoldedModel, outputDir, overwriteFiles);

            var files = new ReverseEngineerFiles();
            Directory.CreateDirectory(outputDir);

            var contextPath = Path.Combine(outputDir, scaffoldedModel.ContextFile.Path);
            File.WriteAllText(contextPath, scaffoldedModel.ContextFile.Code, Encoding.UTF8);
            files.ContextFile = contextPath;

            foreach (var entityTypeFile in scaffoldedModel.EntityTypeFiles)
            {
                var additionalFilePath = Path.Combine(outputDir, entityTypeFile.Path);
                File.WriteAllText(additionalFilePath, entityTypeFile.Code, Encoding.UTF8);
                files.EntityTypeFiles.Add(additionalFilePath);
            }

            return files;
        }

        // if outputDir is a subfolder of projectDir, then use each subfolder as a subnamespace
        // --output-dir $(projectFolder)/A/B/C
        // => "namespace $(rootnamespace).A.B.C"
        private string SubnamespaceFromOutputPath(string projectDir, string outputDir)
        {
            if (outputDir == null
                || !outputDir.StartsWith(projectDir, StringComparison.Ordinal))
            {
                return null;
            }

            var subPath = outputDir.Substring(projectDir.Length);

            return !string.IsNullOrWhiteSpace(subPath)
                ? string.Join(".", subPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
                : null;
        }

        private static void CheckOutputFiles(
            ScaffoldedModel scaffoldedModel,
            string outputDir,
            bool overwriteFiles)
        {
            var paths = scaffoldedModel.EntityTypeFiles.Select(f => f.Path).ToList();
            paths.Insert(0, scaffoldedModel.ContextFile.Path);

            var existingFiles = new List<string>();
            var readOnlyFiles = new List<string>();
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(outputDir, path);

                if (File.Exists(fullPath))
                {
                    existingFiles.Add(path);

                    if (File.GetAttributes(fullPath).HasFlag(FileAttributes.ReadOnly))
                    {
                        readOnlyFiles.Add(path);
                    }
                }
            }

            if (!overwriteFiles && existingFiles.Count != 0)
            {
                throw new OperationException(
                    DesignStrings.ExistingFiles(
                        outputDir,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, existingFiles)));
            }
            if (readOnlyFiles.Count != 0)
            {
                throw new OperationException(
                    DesignStrings.ReadOnlyFiles(
                        outputDir,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, readOnlyFiles)));
            }
        }
    }
}
