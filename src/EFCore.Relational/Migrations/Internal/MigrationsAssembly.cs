// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MigrationsAssembly : IMigrationsAssembly
{
    private readonly IMigrationsIdGenerator _idGenerator;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
    private IReadOnlyDictionary<string, TypeInfo>? _migrations;
    private ModelSnapshot? _modelSnapshot;
    private readonly Type _contextType;
    private readonly List<Assembly> _additionalAssemblies = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
    {
        _contextType = currentContext.Context.GetType();

        var optionsExtension = RelationalOptionsExtension.Extract(options);
        var assemblyName = optionsExtension.MigrationsAssembly;
        var assemblyObject = optionsExtension.MigrationsAssemblyObject;

        Assembly = assemblyName == null
            ? assemblyObject ?? _contextType.Assembly
            : Assembly.Load(new AssemblyName(assemblyName));

        _idGenerator = idGenerator;
        _logger = logger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, TypeInfo> Migrations
    {
        get
        {
            IReadOnlyDictionary<string, TypeInfo> Create()
            {
                var result = new SortedList<string, TypeInfo>();

                // Get migrations from the main assembly
                AddMigrationsFromAssembly(Assembly, result);

                // Get migrations from additional assemblies
                foreach (var additionalAssembly in _additionalAssemblies)
                {
                    AddMigrationsFromAssembly(additionalAssembly, result);
                }

                return result;
            }

            return _migrations ??= Create();
        }
    }

    private void AddMigrationsFromAssembly(Assembly assembly, SortedList<string, TypeInfo> result)
    {
        var items
            = from t in assembly.GetConstructibleTypes()
              where t.IsSubclassOf(typeof(Migration))
                  && t.GetCustomAttribute<DbContextAttribute>(inherit: false)?.ContextType == _contextType
              let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
              orderby id
              select (id, t);

        foreach (var (id, t) in items)
        {
            if (id == null)
            {
                _logger.MigrationAttributeMissingWarning(t);

                continue;
            }

            result[id] = t;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ModelSnapshot? ModelSnapshot
    {
        get
        {
            if (_modelSnapshot != null)
            {
                return _modelSnapshot;
            }

            // Check additional assemblies first (in reverse order, so latest added is checked first)
            for (var i = _additionalAssemblies.Count - 1; i >= 0; i--)
            {
                var snapshot = GetModelSnapshotFromAssembly(_additionalAssemblies[i]);
                if (snapshot != null)
                {
                    _modelSnapshot = snapshot;
                    return _modelSnapshot;
                }
            }

            // Fall back to main assembly
            _modelSnapshot = GetModelSnapshotFromAssembly(Assembly);
            return _modelSnapshot;
        }
    }

    private ModelSnapshot? GetModelSnapshotFromAssembly(Assembly assembly)
        => (from t in assembly.GetConstructibleTypes()
            where t.IsSubclassOf(typeof(ModelSnapshot))
                && t.GetCustomAttribute<DbContextAttribute>(inherit: false)?.ContextType == _contextType
            select (ModelSnapshot)Activator.CreateInstance(t.AsType())!)
        .FirstOrDefault();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Assembly Assembly { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? FindMigrationId(string nameOrId)
        => Migrations.Keys
            .Where(
                _idGenerator.IsValidId(nameOrId)
                    // ReSharper disable once ImplicitlyCapturedClosure
                    ? id => string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase)
                    : id => string.Equals(_idGenerator.GetName(id), nameOrId, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
    {
        var migration = (Migration)Activator.CreateInstance(migrationClass.AsType())!;
        migration.ActiveProvider = activeProvider;

        return migration;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddMigrations(Assembly additionalMigrationsAssembly)
    {
        _additionalAssemblies.Add(additionalMigrationsAssembly);

        // Reset cached data so it will be recomputed with the new assembly
        _migrations = null;
        _modelSnapshot = null;
    }
}
