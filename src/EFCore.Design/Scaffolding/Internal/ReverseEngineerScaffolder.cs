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
using Microsoft.EntityFrameworkCore.Storage.Internal;
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
        private readonly ICSharpHelper _code;
        private readonly INamedConnectionStringResolver _connectionStringResolver;
        private const string DbContextSuffix = "Context";
        private const string DefaultDbContextName = "Model" + DbContextSuffix;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReverseEngineerScaffolder(
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] IScaffoldingModelFactory scaffoldingModelFactory,
            [NotNull] IModelCodeGeneratorSelector modelCodeGeneratorSelector,
            [NotNull] ICSharpUtilities cSharpUtilities,
            [NotNull] ICSharpHelper cSharpHelper,
            [NotNull] INamedConnectionStringResolver connectionStringResolver)
        {
            Check.NotNull(databaseModelFactory, nameof(databaseModelFactory));
            Check.NotNull(scaffoldingModelFactory, nameof(scaffoldingModelFactory));
            Check.NotNull(modelCodeGeneratorSelector, nameof(modelCodeGeneratorSelector));
            Check.NotNull(cSharpHelper, nameof(cSharpHelper));

            _databaseModelFactory = databaseModelFactory;
            _factory = scaffoldingModelFactory;
            ModelCodeGeneratorSelector = modelCodeGeneratorSelector;
            _cSharpUtilities = cSharpUtilities;
            _code = cSharpHelper;
            _connectionStringResolver = connectionStringResolver;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private IModelCodeGeneratorSelector ModelCodeGeneratorSelector { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ScaffoldedModel ScaffoldModel(
            string connectionString,
            IEnumerable<string> tables,
            IEnumerable<string> schemas,
            string rootNamespace,
            string modelNamespace,
            string contextNamespace,
            string language,
            string contextDir,
            string contextName,
            ModelReverseEngineerOptions modelOptions,
            ModelCodeGenerationOptions codeOptions)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));
            Check.NotEmpty(modelNamespace, nameof(modelNamespace));
            Check.NotEmpty(contextNamespace, nameof(contextNamespace));
            Check.NotNull(modelOptions, nameof(modelOptions));
            Check.NotNull(codeOptions, nameof(codeOptions));

            if (!string.IsNullOrWhiteSpace(contextName)
                && (!_cSharpUtilities.IsValidIdentifier(contextName)
                    || _cSharpUtilities.IsCSharpKeyword(contextName)))
            {
                throw new ArgumentException(
                    DesignStrings.ContextClassNotValidCSharpIdentifier(contextName));
            }

            var resolvedConnectionString = _connectionStringResolver.ResolveConnectionString(connectionString);
            if (resolvedConnectionString != connectionString)
            {
                codeOptions.SuppressConnectionStringWarning = true;
            }

            var databaseModel = _databaseModelFactory.Create(resolvedConnectionString, tables, schemas);
            var model = _factory.Create(databaseModel, modelOptions.UseDatabaseNames);

            if (model == null)
            {
                throw new InvalidOperationException(
                    DesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().ShortDisplayName()));
            }

            if (string.IsNullOrEmpty(contextName))
            {
                contextName = DefaultDbContextName;

                var annotatedName = model.Scaffolding().DatabaseName;
                if (!string.IsNullOrEmpty(annotatedName))
                {
                    contextName = _code.Identifier(annotatedName + DbContextSuffix);
                }
            }

            var codeGenerator = ModelCodeGeneratorSelector.Select(language);

            return codeGenerator.GenerateModel(model, rootNamespace, modelNamespace, contextNamespace, contextDir ?? string.Empty, contextName, connectionString, codeOptions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual SavedModelFiles Save(
            ScaffoldedModel scaffoldedModel,
            string outputDir,
            bool overwriteFiles)
        {
            CheckOutputFiles(scaffoldedModel, outputDir, overwriteFiles);

            Directory.CreateDirectory(outputDir);

            var contextPath = Path.GetFullPath(Path.Combine(outputDir, scaffoldedModel.ContextFile.Path));
            Directory.CreateDirectory(Path.GetDirectoryName(contextPath));
            File.WriteAllText(contextPath, scaffoldedModel.ContextFile.Code, Encoding.UTF8);

            var additionalFiles = new List<string>();
            foreach (var entityTypeFile in scaffoldedModel.AdditionalFiles)
            {
                var additionalFilePath = Path.Combine(outputDir, entityTypeFile.Path);
                File.WriteAllText(additionalFilePath, entityTypeFile.Code, Encoding.UTF8);
                additionalFiles.Add(additionalFilePath);
            }

            return new SavedModelFiles(contextPath, additionalFiles);
        }

        private static void CheckOutputFiles(
            ScaffoldedModel scaffoldedModel,
            string outputDir,
            bool overwriteFiles)
        {
            var paths = scaffoldedModel.AdditionalFiles.Select(f => f.Path).ToList();
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

            if (!overwriteFiles
                && existingFiles.Count != 0)
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
