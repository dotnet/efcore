// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

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
        string projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[]? args)
        : this(
            reporter,
            assembly,
            startupAssembly,
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
    public virtual IReadOnlyList<string> Optimize(string? outputDir, string? modelNamespace, string? contextTypeName)
    {
        using var context = CreateContext(contextTypeName);
        var contextType = context.GetType();

        var services = _servicesBuilder.Build(context);
        var scaffolder = services.GetRequiredService<ICompiledModelScaffolder>();

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

        var finalModelNamespace = modelNamespace ?? GetNamespaceFromOutputPath(outputDir) ?? "";

        var scaffoldedFiles = scaffolder.ScaffoldModel(
            context.GetService<IDesignTimeModel>().Model,
            outputDir,
            new CompiledModelCodeGenerationOptions
            {
                ContextType = contextType,
                ModelNamespace = finalModelNamespace,
                Language = _language,
                UseNullableReferenceTypes = _nullable
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
    public virtual DbContext CreateContext(string? contextType)
    {
        var factory = FindContextType(contextType).Value;
        try
        {
            var context = factory();
            _reporter.WriteVerbose(DesignStrings.UseContext(context.GetType().ShortDisplayName()));

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

            throw new OperationException(DesignStrings.CannotCreateContextInstance(contextType, ex.Message), ex);
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

    private IDictionary<Type, Func<DbContext>> FindContextTypes()
    {
        _reporter.WriteVerbose(DesignStrings.FindingContexts);

        var contexts = new Dictionary<Type, Func<DbContext>>();

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

            // Look for DbContext classes registered in the service provider
            var appServices = _appServicesFactory.Create(_args);
            var registeredContexts = appServices.GetServices<DbContextOptions>()
                .Select(o => o.ContextType);
            foreach (var context in registeredContexts.Where(c => !contexts.ContainsKey(c)))
            {
                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context)
                    ?? FindContextFromRuntimeDbContextFactory(appServices, context)
                    ?? (() => (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(appServices, context)));
            }

            // Look for DbContext classes in assemblies
            _reporter.WriteVerbose(DesignStrings.FindingReferencedContexts);
            var types = _startupAssembly.GetConstructibleTypes()
                .Concat(_assembly.GetConstructibleTypes())
                .ToList();

            var contextTypes = types.Where(t => typeof(DbContext).IsAssignableFrom(t)).Select(
                    t => t.AsType())
                .Concat(
                    types.Where(t => typeof(Migration).IsAssignableFrom(t))
                        .Select(t => t.GetCustomAttribute<DbContextAttribute>()?.ContextType)
                        .Where(t => t != null)
                        .Cast<Type>())
                .Distinct();

            foreach (var context in contextTypes.Where(c => !contexts.ContainsKey(c)))
            {
                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context)
                    ?? (() => (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(appServices, context)));
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

        return contexts;
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

    private static Func<DbContext>? FindContextFromRuntimeDbContextFactory(IServiceProvider appServices, Type contextType)
    {
        var factoryInterface = typeof(IDbContextFactory<>).MakeGenericType(contextType);
        var service = appServices.GetService(factoryInterface);
        return service == null
            ? null
            : () => (DbContext)factoryInterface
                .GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))
                !.Invoke(service, null)!;
    }

    private Func<DbContext>? FindContextFactory(Type contextType)
    {
        var factoryInterface = typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType);
        var factory = contextType.Assembly.GetConstructibleTypes()
            .FirstOrDefault(t => factoryInterface.IsAssignableFrom(t));
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
        var types = FindContextTypes();

        if (string.IsNullOrEmpty(name))
        {
            if (types.Count == 0)
            {
                throw new OperationException(DesignStrings.NoContext(_assembly.GetName().Name));
            }

            if (types.Count == 1)
            {
                return types.First();
            }

            throw new OperationException(DesignStrings.MultipleContexts);
        }

        var candidates = FilterTypes(types, name, ignoreCase: true);
        if (candidates.Count == 0)
        {
            throw new OperationException(DesignStrings.NoContextWithName(name));
        }

        if (candidates.Count == 1)
        {
            return candidates.First();
        }

        // Disambiguate using case
        candidates = FilterTypes(candidates, name);
        if (candidates.Count == 0)
        {
            throw new OperationException(DesignStrings.MultipleContextsWithName(name));
        }

        if (candidates.Count == 1)
        {
            return candidates.First();
        }

        // Allow selecting types in the default namespace
        candidates = candidates.Where(t => t.Key.Namespace == null).ToDictionary();
        if (candidates.Count == 0)
        {
            throw new OperationException(DesignStrings.MultipleContextsWithQualifiedName(name));
        }

        Check.DebugAssert(candidates.Count == 1, $"candidates.Count is {candidates.Count}");

        return candidates.First();
    }

    private static IDictionary<Type, Func<DbContext>> FilterTypes(
        IDictionary<Type, Func<DbContext>> types,
        string name,
        bool ignoreCase = false)
    {
        var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return types
            .Where(
                t => string.Equals(t.Key.Name, name, comparisonType)
                    || string.Equals(t.Key.FullName, name, comparisonType)
                    || string.Equals(t.Key.AssemblyQualifiedName, name, comparisonType))
            .ToDictionary();
    }
}
