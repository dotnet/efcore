// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

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
    private readonly IDesignTimeConnectionStringResolver _connectionStringResolver;
    private readonly IOperationReporter _reporter;
    private const string DbContextSuffix = "Context";
    private const string DefaultDbContextName = "Model" + DbContextSuffix;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ReverseEngineerScaffolder(
        IDatabaseModelFactory databaseModelFactory,
        IScaffoldingModelFactory scaffoldingModelFactory,
        IModelCodeGeneratorSelector modelCodeGeneratorSelector,
        ICSharpUtilities cSharpUtilities,
        ICSharpHelper cSharpHelper,
        IDesignTimeConnectionStringResolver connectionStringResolver,
        IOperationReporter reporter)
    {
        _databaseModelFactory = databaseModelFactory;
        _factory = scaffoldingModelFactory;
        ModelCodeGeneratorSelector = modelCodeGeneratorSelector;
        _cSharpUtilities = cSharpUtilities;
        _code = cSharpHelper;
        _connectionStringResolver = connectionStringResolver;
        _reporter = reporter;
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
        else if (!codeOptions.SuppressOnConfiguring)
        {
            _reporter.WriteWarning(DesignStrings.SensitiveInformationWarning);
        }

        codeOptions.ConnectionString ??= connectionString;

        var databaseModel = _databaseModelFactory.Create(resolvedConnectionString, databaseOptions);
        var modelConnectionString = (string?)(databaseModel[ScaffoldingAnnotationNames.ConnectionString]);
        if (!string.IsNullOrEmpty(modelConnectionString))
        {
            codeOptions.ConnectionString = modelConnectionString;
        }

        var model = _factory.Create(databaseModel, modelOptions);
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

        var codeGenerator = ModelCodeGeneratorSelector.Select(codeOptions);

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
        Directory.CreateDirectory(Path.GetDirectoryName(contextPath)!);
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
