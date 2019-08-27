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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private IModelCodeGeneratorSelector ModelCodeGeneratorSelector { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ScaffoldedModel ScaffoldModel(
            string connectionString,
            DatabaseModelFactoryOptions databaseOptions,
            ModelReverseEngineerOptions modelOptions,
            ModelCodeGenerationOptions codeOptions)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(databaseOptions, nameof(databaseOptions));
            Check.NotNull(modelOptions, nameof(modelOptions));
            Check.NotNull(codeOptions, nameof(codeOptions));

            if (!string.IsNullOrWhiteSpace(codeOptions.ContextName)
                && (!_cSharpUtilities.IsValidIdentifier(codeOptions.ContextName)
                    || _cSharpUtilities.IsCSharpKeyword(codeOptions.ContextName)))
            {
                throw new ArgumentException(
                    DesignStrings.ContextClassNotValidCSharpIdentifier(codeOptions.ContextName));
            }

            var resolvedConnectionString = _connectionStringResolver.ResolveConnectionString(connectionString);
            if (resolvedConnectionString != connectionString)
            {
                codeOptions.SuppressConnectionStringWarning = true;
            }

            if (codeOptions.ConnectionString == null)
            {
                codeOptions.ConnectionString = connectionString;
            }

            var databaseModel = _databaseModelFactory.Create(resolvedConnectionString, databaseOptions);
            var model = _factory.Create(databaseModel, modelOptions.UseDatabaseNames);

            if (model == null)
            {
                throw new InvalidOperationException(
                    DesignStrings.ProviderReturnedNullModel(
                        _factory.GetType().ShortDisplayName()));
            }

            if (string.IsNullOrEmpty(codeOptions.ContextName))
            {
                var annotatedName = model.GetDatabaseName();
                codeOptions.ContextName = !string.IsNullOrEmpty(annotatedName)
                    ? _code.Identifier(annotatedName + DbContextSuffix)
                    : DefaultDbContextName;
            }

            var codeGenerator = ModelCodeGeneratorSelector.Select(codeOptions.Language);

            return codeGenerator.GenerateModel(model, codeOptions);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
