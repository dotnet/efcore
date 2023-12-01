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

        var assemblyName = RelationalOptionsExtension.Extract(options).MigrationsAssembly;
        var assemblyObject = RelationalOptionsExtension.Extract(options).MigrationsAssemblyObject;

        Assembly = assemblyName == null
            ?   assemblyObject ?? _contextType.Assembly
            :   Assembly.Load(new AssemblyName(assemblyName));

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

                var items
                    = from t in Assembly.GetConstructibleTypes()
                      where t.IsSubclassOf(typeof(Migration))
                          && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
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

                    result.Add(id, t);
                }

                return result;
            }

            return _migrations ??= Create();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ModelSnapshot? ModelSnapshot
        => _modelSnapshot
            ??= (from t in Assembly.GetConstructibleTypes()
                 where t.IsSubclassOf(typeof(ModelSnapshot))
                     && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
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
}
