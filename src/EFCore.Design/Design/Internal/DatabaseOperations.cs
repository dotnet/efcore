// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DatabaseOperations
{
    private readonly string _projectDir;
    private readonly string? _rootNamespace;
    private readonly string? _language;
    private readonly bool _nullable;
    private readonly DesignTimeServicesBuilder _servicesBuilder;
    private readonly string[] _args;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DatabaseOperations(
        IOperationReporter reporter,
        Assembly assembly,
        Assembly startupAssembly,
        string projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[]? args)
    {
        _projectDir = projectDir;
        _rootNamespace = rootNamespace;
        _language = language;
        _nullable = nullable;
        _args = args ?? [];

        _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, _args);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SavedModelFiles ScaffoldContext(
        string provider,
        string connectionString,
        string? outputDir,
        string? outputContextDir,
        string? dbContextClassName,
        IEnumerable<string> schemas,
        IEnumerable<string> tables,
        string? modelNamespace,
        string? contextNamespace,
        bool useDataAnnotations,
        bool overwriteFiles,
        bool useDatabaseNames,
        bool suppressOnConfiguring,
        bool noPluralize)
    {
        outputDir = outputDir != null
            ? Path.GetFullPath(Path.Combine(_projectDir, outputDir))
            : _projectDir;

        outputContextDir = outputContextDir != null
            ? Path.GetFullPath(Path.Combine(_projectDir, outputContextDir))
            : outputDir;

        var services = _servicesBuilder.Build(provider);
        using var scope = services.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IReverseEngineerScaffolder>();

        var finalModelNamespace = modelNamespace ?? GetNamespaceFromOutputPath(outputDir);
        var finalContextNamespace =
            contextNamespace ?? modelNamespace ?? GetNamespaceFromOutputPath(outputContextDir);

        var scaffoldedModel = scaffolder.ScaffoldModel(
            connectionString,
            new DatabaseModelFactoryOptions(tables, schemas),
            new ModelReverseEngineerOptions { UseDatabaseNames = useDatabaseNames, NoPluralize = noPluralize },
            new ModelCodeGenerationOptions
            {
                UseDataAnnotations = useDataAnnotations,
                RootNamespace = _rootNamespace,
                ModelNamespace = finalModelNamespace,
                ContextNamespace = finalContextNamespace,
                Language = _language,
                UseNullableReferenceTypes = _nullable,
                ContextDir = MakeDirRelative(outputDir, outputContextDir),
                ContextName = dbContextClassName,
                SuppressOnConfiguring = suppressOnConfiguring,
                ProjectDir = _projectDir
            });

        return scaffolder.Save(
            scaffoldedModel,
            outputDir,
            overwriteFiles);
    }

    private string? GetNamespaceFromOutputPath(string directoryPath)
    {
        var subNamespace = SubnamespaceFromOutputPath(_projectDir, directoryPath);
        return string.IsNullOrEmpty(subNamespace)
            ? _rootNamespace
            : string.IsNullOrEmpty(_rootNamespace)
                ? subNamespace
                : _rootNamespace + "." + subNamespace;
    }

    // if outputDir is a subfolder of projectDir, then use each subfolder as a sub-namespace
    // --output-dir $(projectFolder)/A/B/C
    // => "namespace $(rootnamespace).A.B.C"
    private static string? SubnamespaceFromOutputPath(string projectDir, string outputDir)
    {
        if (!outputDir.StartsWith(projectDir, StringComparison.Ordinal))
        {
            return null;
        }

        var subPath = outputDir[projectDir.Length..];

        return !string.IsNullOrWhiteSpace(subPath)
            ? string.Join(
                ".",
                subPath.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
            : null;
    }

    private static string MakeDirRelative(string root, string path)
    {
        var relativeUri = new Uri(NormalizeDir(root)).MakeRelativeUri(new Uri(NormalizeDir(path)));

        return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    private static string NormalizeDir(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var last = path[^1];
        return last == Path.DirectorySeparatorChar
            || last == Path.AltDirectorySeparatorChar
                ? path
                : path + Path.DirectorySeparatorChar;
    }
}
