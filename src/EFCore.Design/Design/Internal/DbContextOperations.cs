// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbContextOperations
{
    private readonly IOperationReporter _reporter;
    private readonly Assembly _assembly;
    private readonly Assembly _startupAssembly;
    private readonly string _project;
    private readonly string _projectDir;
    private readonly string? _rootNamespace;
    private readonly string? _language;
    private readonly bool _nullable;
    private readonly string[] _args;
    private readonly AppServiceProviderFactory _appServicesFactory;
    private readonly DesignTimeServicesBuilder _servicesBuilder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DbContextOperations(
        IOperationReporter reporter,
        Assembly assembly,
        Assembly startupAssembly,
        string project,
        string projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[]? args)
        : this(
            reporter,
            assembly,
            startupAssembly,
            project,
            projectDir,
            rootNamespace,
            language,
            nullable,
            args,
            new AppServiceProviderFactory(startupAssembly, reporter))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected DbContextOperations(
        IOperationReporter reporter,
        Assembly assembly,
        Assembly startupAssembly,
        string project,
        string projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[]? args,
        AppServiceProviderFactory appServicesFactory)
    {
        _reporter = reporter;
        _assembly = assembly;
        _startupAssembly = startupAssembly;
        _project = project;
        _projectDir = projectDir;
        _rootNamespace = rootNamespace;
        _language = language;
        _nullable = nullable;
        _args = args ?? [];
        _appServicesFactory = appServicesFactory;
        _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, _args);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DropDatabase(string? contextType)
    {
        using var context = CreateContext(contextType);
        var connection = context.Database.GetDbConnection();
        _reporter.WriteInformation(DesignStrings.DroppingDatabase(connection.Database, connection.DataSource));
        _reporter.WriteInformation(
            context.Database.EnsureDeleted()
                ? DesignStrings.DatabaseDropped(connection.Database)
                : DesignStrings.NotExistDatabase(connection.Database));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string ScriptDbContext(string? contextType)
    {
        using var context = CreateContext(contextType);
        return context.Database.GenerateCreateScript();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<string> Optimize(
        string? outputDir,
        string? modelNamespace,
        string? contextTypeName,
        string? suffix,
        bool scaffoldModel,
        bool precompileQueries,
        bool nativeAot)
    {
        var optimizeAllInAssembly = contextTypeName == "*";
        var contexts = optimizeAllInAssembly ? CreateAllContexts() : [CreateContext(contextTypeName)];

        List<string> generatedFiles = [];
        HashSet<string> generatedFileNames = [];
        var contextOptimized = false;
        foreach (var context in contexts)
        {
            using (context)
            {
                Optimize(
                    outputDir,
                    modelNamespace,
                    suffix,
                    scaffoldModel,
                    precompileQueries,
                    context,
                    optimizeAllInAssembly,
                    nativeAot,
                    generatedFiles,
                    generatedFileNames);
                contextOptimized = true;
            }
        }

        if (optimizeAllInAssembly)
        {
            if (!contextOptimized)
            {
                throw new OperationException(DesignStrings.NoContextsToOptimize);
            }

            if (generatedFiles.Count == 0)
            {
                _reporter.WriteWarning(DesignStrings.OptimizeNoFilesGenerated);
            }
        }

        return generatedFiles;
    }

    private void Optimize(
        string? outputDir,
        string? modelNamespace,
        string? suffix,
        bool scaffoldModel,
        bool precompileQueries,
        DbContext context,
        bool optimizeAllInAssembly,
        bool nativeAot,
        List<string> generatedFiles,
        HashSet<string> generatedFileNames)
    {
        var contextType = context.GetType();
        var services = _servicesBuilder.Build(context);

        IReadOnlyDictionary<MemberInfo, QualifiedName>? memberAccessReplacements = null;

        if (scaffoldModel
            && (!optimizeAllInAssembly || contextType.Assembly == _assembly))
        {
            generatedFiles.AddRange(
                ScaffoldCompiledModel(
                    outputDir, modelNamespace, context, suffix, nativeAot, services, generatedFileNames));
            if (precompileQueries)
            {
                memberAccessReplacements = ((IRuntimeModel)context.GetService<IDesignTimeModel>().Model).GetUnsafeAccessors();
            }
        }

        if (precompileQueries)
        {
            generatedFiles.AddRange(
                PrecompileQueries(
                    outputDir,
                    context,
                    suffix,
                    services,
                    memberAccessReplacements ?? ((IRuntimeModel)context.Model).GetUnsafeAccessors(),
                    generatedFileNames));
        }
    }

    private IReadOnlyList<string> ScaffoldCompiledModel(
        string? outputDir,
        string? modelNamespace,
        DbContext context,
        string? suffix,
        bool nativeAot,
        IServiceProvider services,
        ISet<string> generatedFileNames)
    {
        var contextType = context.GetType();
        if (contextType.Assembly != _assembly)
        {
            _reporter.WriteWarning(
                DesignStrings.ContextAssemblyMismatch(
                    _assembly.GetName().Name, contextType.ShortDisplayName(), contextType.Assembly.GetName().Name));
        }

        if (outputDir == null)
        {
            var contextSubNamespace = contextType.Namespace ?? "";
            if (!string.IsNullOrEmpty(_rootNamespace)
                && contextSubNamespace.StartsWith(_rootNamespace, StringComparison.Ordinal))
            {
                contextSubNamespace = contextSubNamespace[_rootNamespace.Length..];
            }

            outputDir = Path.Combine(contextSubNamespace.Replace('.', Path.DirectorySeparatorChar), "CompiledModels");
        }

        outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));

        var scaffolder = services.GetRequiredService<ICompiledModelScaffolder>();

        var finalModelNamespace = modelNamespace ?? GetNamespaceFromOutputPath(outputDir) ?? "";

        var scaffoldedFiles = scaffolder.ScaffoldModel(
            context.GetService<IDesignTimeModel>().Model,
            outputDir,
            new CompiledModelCodeGenerationOptions
            {
                ContextType = contextType,
                ModelNamespace = finalModelNamespace,
                Language = _language,
                UseNullableReferenceTypes = _nullable,
                Suffix = suffix,
                ForNativeAot = nativeAot,
                GeneratedFileNames = generatedFileNames
            });

        var fullName = contextType.ShortDisplayName() + "Model";
        if (!string.IsNullOrEmpty(modelNamespace))
        {
            fullName = modelNamespace + "." + fullName;
        }

        _reporter.WriteInformation(DesignStrings.CompiledModelGenerated($"options.UseModel({fullName}.Instance)"));

        var cacheKeyFactory = context.GetService<IModelCacheKeyFactory>();
        if (cacheKeyFactory is not ModelCacheKeyFactory)
        {
            _reporter.WriteWarning(DesignStrings.CompiledModelCustomCacheKeyFactory(cacheKeyFactory.GetType().ShortDisplayName()));
        }

        return scaffoldedFiles;
    }

    private IReadOnlyList<string> PrecompileQueries(
        string? outputDir,
        DbContext context,
        string? suffix,
        IServiceProvider services,
        IReadOnlyDictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        ISet<string> generatedFileNames)
    {
        outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir ?? "Generated"));

        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        // TODO: pass through properties
        var workspace = MSBuildWorkspace.Create();
        workspace.LoadMetadataForReferencedProjects = true;
        var project = workspace.OpenProjectAsync(_project).GetAwaiter().GetResult();
        if (!project.SupportsCompilation)
        {
            throw new NotSupportedException(DesignStrings.UncompilableProject(_project));
        }

        var compilation = project.GetCompilationAsync().GetAwaiter().GetResult()!;
        var errorDiagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errorDiagnostics.Any())
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine(DesignStrings.CompilationErrors);
            foreach (var diagnostic in errorDiagnostics)
            {
                errorBuilder.AppendLine(diagnostic.ToString());
            }

            throw new InvalidOperationException(errorBuilder.ToString());
        }

        var syntaxGenerator = SyntaxGenerator.GetGenerator(
            workspace, _language == "VB" ? LanguageNames.VisualBasic : _language ?? LanguageNames.CSharp);

        var precompiledQueryCodeGenerator = services.GetRequiredService<IPrecompiledQueryCodeGeneratorSelector>().Select(_language);

        var precompilationErrors = new List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>();
        var generatedFiles = precompiledQueryCodeGenerator.GeneratePrecompiledQueries(
            compilation,
            syntaxGenerator,
            context,
            memberAccessReplacements,
            precompilationErrors,
            generatedFileNames,
            assembly: _assembly,
            suffix);

        if (precompilationErrors.Count > 0)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine(DesignStrings.QueryPrecompilationErrors);
            foreach (var error in precompilationErrors)
            {
                errorBuilder.AppendLine(error.ToString());
            }

            throw new InvalidOperationException(errorBuilder.ToString());
        }

        var writtenFiles = new List<string>();
        return CompiledModelScaffolder.WriteFiles(generatedFiles, outputDir);
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ContextInfo GetContextInfo(string? contextType)
    {
        using var context = CreateContext(contextType);
        var info = new ContextInfo { Type = context.GetType().FullName! };

        var provider = context.GetService<IDatabaseProvider>();
        info.ProviderName = provider.Name;

        if (((IDatabaseFacadeDependenciesAccessor)context.Database).Dependencies is IRelationalDatabaseFacadeDependencies)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                info.DataSource = connection.DataSource;
                info.DatabaseName = connection.Database;
            }
            catch (Exception exception)
            {
                info.DataSource = info.DatabaseName = DesignStrings.BadConnection(exception.Message);
            }
        }
        else
        {
            info.DataSource = info.DatabaseName = DesignStrings.NoRelationalConnection;
        }

        var options = context.GetService<IDbContextOptions>();
        info.Options = options.BuildOptionsFragment().Trim();

        return info;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContext CreateContext(string? contextType)
    {
        EF.IsDesignTime = true;
        return CreateContext(contextType, FindContextType(contextType));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<DbContext> CreateAllContexts()
    {
        EF.IsDesignTime = true;
        var types = FindContextTypes(useServiceProvider: false);
        foreach (var contextPair in types)
        {
            yield return CreateContext(null, contextPair);
        }
    }

    private DbContext CreateContext(string? contextType, KeyValuePair<Type, Func<DbContext>> contextPair)
    {
        var factory = contextPair.Value;
        try
        {
            var context = factory();
            contextType = context.GetType().ShortDisplayName();
            _reporter.WriteVerbose(DesignStrings.UseContext(contextType));

            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new OperationLoggerProvider(_reporter));

            return context;
        }
        catch (Exception ex)
        {
            if (ex is TargetInvocationException)
            {
                ex = ex.InnerException!;
            }

            throw new OperationException(
                DesignStrings.CannotCreateContextInstance(
                    contextType ?? contextPair.Key.GetType().ShortDisplayName(), ex.Message), ex);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Type> GetContextTypes()
        => FindContextTypes().Keys;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type GetContextType(string? name)
        => FindContextType(name).Key;

    private IDictionary<Type, Func<DbContext>> FindContextTypes(string? name = null, bool useServiceProvider = true)
    {
        _reporter.WriteVerbose(DesignStrings.FindingContexts);

        AppServiceProviderFactory.SetEnvironment(_reporter);

        var contexts = new Dictionary<Type, Func<DbContext>?>();

        try
        {
            // Look for IDesignTimeDbContextFactory implementations
            _reporter.WriteVerbose(DesignStrings.FindingContextFactories);
            var contextFactories = _startupAssembly.GetConstructibleTypes()
                .Where(t => typeof(IDesignTimeDbContextFactory<DbContext>).IsAssignableFrom(t));
            foreach (var factory in contextFactories)
            {
                _reporter.WriteVerbose(DesignStrings.FoundContextFactory(factory.ShortDisplayName()));
                var manufacturedContexts =
                    from i in factory.ImplementedInterfaces
                    where i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>)
                    select i.GenericTypeArguments[0];
                foreach (var context in manufacturedContexts)
                {
                    _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                    contexts.Add(
                        context,
                        () => CreateContextFromFactory(factory.AsType(), context));
                }
            }

            // Look for DbContextAttribute on the assembly
            foreach (var contextAttribute in _startupAssembly.GetCustomAttributes<DbContextAttribute>())
            {
                var context = contextAttribute.ContextType;
                if (contexts.ContainsKey(context))
                {
                    continue;
                }

                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context));
            }

            // Look for DbContext classes in assemblies
            _reporter.WriteVerbose(DesignStrings.FindingReferencedContexts);
            var types = _startupAssembly.GetConstructibleTypes()
                .Concat(_assembly.GetConstructibleTypes())
                .ToList();

            var contextTypes = types.Where(t => typeof(DbContext).IsAssignableFrom(t)).Select(
                    t => t.AsType())
                .Concat<Type>(
                    types.Where(t => typeof(Migration).IsAssignableFrom(t))
                        .Select(t => t.GetCustomAttribute<DbContextAttribute>()?.ContextType)
                        .Where(t => t != null)!)
                .Distinct();

            foreach (var context in contextTypes)
            {
                if (contexts.ContainsKey(context))
                {
                    continue;
                }

                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context));
            }

            if (!string.IsNullOrEmpty(name))
            {
                contexts = FilterTypes(contexts, name, throwOnEmpty: false);
            }

            if (contexts.Values.All(f => f != null)
                && (!useServiceProvider || contexts.Count == 1))
            {
                return contexts!;
            }

            // Look for DbContext classes registered in the service provider
            var appServices = _appServicesFactory.Create(_args);
            foreach (var options in appServices.GetServices<DbContextOptions>())
            {
                var context = options.ContextType;
                if (contexts.ContainsKey(context))
                {
                    continue;
                }

                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context));
            }

            if (!string.IsNullOrEmpty(name))
            {
                contexts = FilterTypes(contexts, name, throwOnEmpty: true);
            }

            foreach (var contextPair in contexts)
            {
                if (contextPair.Value == null)
                {
                    var context = contextPair.Key;
                    contexts[context] = CreateContextFromServiceProvider(appServices, context);
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is OperationException)
            {
                throw;
            }

            if (ex is TargetInvocationException)
            {
                ex = ex.InnerException!;
            }

            throw new OperationException(DesignStrings.CannotFindDbContextTypes(ex.Message), ex);
        }

        return contexts!;
    }

    private static Func<DbContext> CreateContextFromServiceProvider(IServiceProvider appServices, Type contextType)
    {
        var factoryInterface = typeof(IDbContextFactory<>).MakeGenericType(contextType);
        var factoryService = appServices.GetService(factoryInterface);
        return factoryService == null
            ? () => (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(appServices, contextType)
            : () => (DbContext)factoryInterface
                .GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))
                !.Invoke(factoryService, null)!;
    }

    private Func<DbContext>? FindContextFactory(Type contextType)
    {
        var factoryInterface = typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType);
        var factory = contextType.Assembly.GetConstructibleTypes()
            .FirstOrDefault(factoryInterface.IsAssignableFrom);
        return factory == null ? null : (() => CreateContextFromFactory(factory.AsType(), contextType));
    }

    private DbContext CreateContextFromFactory(Type factory, Type contextType)
    {
        _reporter.WriteVerbose(DesignStrings.UsingDbContextFactory(factory.ShortDisplayName()));

        return (DbContext)typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType)
            .GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext), [typeof(string[])])!
            .Invoke(Activator.CreateInstance(factory), [_args])!;
    }

    private KeyValuePair<Type, Func<DbContext>> FindContextType(string? name)
    {
        var types = FindContextTypes(name);
        return !string.IsNullOrEmpty(name)
            ? types.First()
            : types.Count switch
            {
                0 => throw new OperationException(DesignStrings.NoContext(_assembly.GetName().Name)),
                1 => types.First(),
                _ => throw new OperationException(DesignStrings.MultipleContexts)
            };
    }

    private Dictionary<Type, Func<DbContext>?> FilterTypes(
        Dictionary<Type, Func<DbContext>?> types,
        string name,
        bool throwOnEmpty)
    {
        var candidates = FilterTypes(types, name, StringComparison.OrdinalIgnoreCase);
        if (candidates.Count == 0)
        {
            return throwOnEmpty
                ? throw new OperationException(DesignStrings.NoContextWithName(name))
                : candidates;
        }

        if (candidates.Count == 1)
        {
            return candidates;
        }

        // Disambiguate using case
        candidates = FilterTypes(candidates, name, StringComparison.Ordinal);
        if (candidates.Count == 0)
        {
            throw new OperationException(DesignStrings.MultipleContextsWithName(name));
        }

        if (candidates.Count == 1)
        {
            return candidates;
        }

        // Allow selecting types in the default namespace
        candidates = candidates.Where(t => t.Key.Namespace == null).ToDictionary();
        if (candidates.Count == 0)
        {
            throw new OperationException(DesignStrings.MultipleContextsWithQualifiedName(name));
        }

        Check.DebugAssert(candidates.Count == 1, $"candidates.Count is {candidates.Count}");

        return candidates;
    }

    private static Dictionary<Type, Func<DbContext>?> FilterTypes(
        Dictionary<Type, Func<DbContext>?> types,
        string name,
        StringComparison comparisonType)
        => types
            .Where(
                t => string.Equals(t.Key.Name, name, comparisonType)
                    || string.Equals(t.Key.FullName, name, comparisonType)
                    || string.Equals(t.Key.AssemblyQualifiedName, name, comparisonType))
            .ToDictionary();
}
