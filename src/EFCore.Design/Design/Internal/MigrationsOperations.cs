// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MigrationsOperations
{
    private readonly IOperationReporter _reporter;
    private readonly Assembly _assembly;
    private readonly string _projectDir;
    private readonly string? _rootNamespace;
    private readonly string? _language;
    private readonly DesignTimeServicesBuilder _servicesBuilder;
    private readonly DbContextOperations _contextOperations;

    // Track applied dynamic migrations for revert support
    private readonly List<string> _appliedDynamicMigrationIds = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MigrationsOperations(
        IOperationReporter reporter,
        Assembly assembly,
        Assembly startupAssembly,
        string projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[]? args)
    {
        _reporter = reporter;
        _assembly = assembly;
        _projectDir = projectDir;
        _rootNamespace = rootNamespace;
        _language = language;
        args ??= [];
        _contextOperations = new DbContextOperations(
            reporter,
            assembly,
            startupAssembly,
            project: "",
            projectDir,
            rootNamespace,
            language,
            nullable,
            args);

        _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, args);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MigrationFiles AddMigration(
        string name,
        string? outputDir,
        string? contextType,
        string? @namespace,
        bool dryRun)
    {
        ValidateMigrationName(name);

        if (outputDir != null)
        {
            outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));
        }

        var subNamespace = SubnamespaceFromOutputPath(outputDir);

        using var context = _contextOperations.CreateContext(contextType);
        ValidateMigrationNameNotContextName(name, context);

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);
        EnsureMigrationsAssembly(services);

        using var scope = services.CreateScope();
        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var migration =
            string.IsNullOrEmpty(@namespace)
                // TODO: Honor _nullable (issue #18950)
                ? scaffolder.ScaffoldMigration(name, _rootNamespace ?? string.Empty, subNamespace, _language, dryRun)
                : scaffolder.ScaffoldMigration(name, null, @namespace, _language, dryRun);
        return scaffolder.Save(_projectDir, migration, outputDir, dryRun);
    }

    // if outputDir is a subfolder of projectDir, then use each subfolder as a sub-namespace
    // --output-dir $(projectFolder)/A/B/C
    // => "namespace $(rootnamespace).A.B.C"
    private string? SubnamespaceFromOutputPath(string? outputDir)
    {
        var fullPath = Path.GetFullPath(_projectDir);
        if (outputDir?.StartsWith(fullPath, StringComparison.Ordinal) != true)
        {
            return null;
        }

        var subPath = outputDir[fullPath.Length..];

        return !string.IsNullOrWhiteSpace(subPath)
            ? string.Join(
                ".",
                subPath.Split(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                    StringSplitOptions.RemoveEmptyEntries))
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<MigrationInfo> GetMigrations(
        string? contextType,
        string? connectionString,
        bool noConnect)
    {
        using var context = _contextOperations.CreateContext(contextType);

        if (connectionString != null)
        {
            context.Database.SetConnectionString(connectionString);
        }

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);

        var migrationsAssembly = services.GetRequiredService<IMigrationsAssembly>();
        var idGenerator = services.GetRequiredService<IMigrationsIdGenerator>();

        HashSet<string>? appliedMigrations = null;
        if (!noConnect)
        {
            try
            {
                appliedMigrations = new HashSet<string>(
                    context.Database.GetAppliedMigrations(),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _reporter.WriteVerbose(ex.ToString());
                _reporter.WriteWarning(DesignStrings.ErrorConnecting(ex.Message));
            }
        }

        return from id in migrationsAssembly.Migrations.Keys
               select new MigrationInfo
               {
                   Id = id,
                   Name = idGenerator.GetName(id),
                   Applied = appliedMigrations?.Contains(id)
               };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string ScriptMigration(
        string? fromMigration,
        string? toMigration,
        MigrationsSqlGenerationOptions options,
        string? contextType)
    {
        using var context = _contextOperations.CreateContext(contextType);
        var services = _servicesBuilder.Build(context);
        EnsureServices(services);

        var migrator = services.GetRequiredService<IMigrator>();

        return migrator.GenerateScript(fromMigration, toMigration, options);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateDatabase(
        string? targetMigration,
        string? connectionString,
        string? contextType)
    {
        using (var context = _contextOperations.CreateContext(contextType))
        {
            if (connectionString != null)
            {
                context.Database.SetConnectionString(connectionString);
            }

            var services = _servicesBuilder.Build(context);
            EnsureServices(services);

            var migrator = services.GetRequiredService<IMigrator>();
            migrator.Migrate(targetMigration);
        }

        _reporter.WriteInformation(DesignStrings.Done);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MigrationFiles RemoveMigration(
        string? contextType,
        bool force,
        bool dryRun)
    {
        using var context = _contextOperations.CreateContext(contextType);
        var services = _servicesBuilder.Build(context);
        EnsureServices(services);
        EnsureMigrationsAssembly(services);

        using var scope = services.CreateScope();
        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();

        var files = scaffolder.RemoveMigration(_projectDir, _rootNamespace, force, _language, dryRun);

        _reporter.WriteInformation(DesignStrings.Done);

        return files;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void HasPendingModelChanges(string? contextType)
    {
        using var context = _contextOperations.CreateContext(contextType);

        var hasPendingModelChanges = context.Database.HasPendingModelChanges();

        if (hasPendingModelChanges)
        {
            throw new OperationException(DesignStrings.PendingModelChanges);
        }

        _reporter.WriteInformation(DesignStrings.NoPendingModelChanges);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    public virtual MigrationFiles AddAndApplyMigration(
        string name,
        string? outputDir,
        string? contextType,
        string? @namespace,
        string? connectionString)
    {
        ValidateMigrationName(name);

        if (outputDir != null)
        {
            outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));
        }

        var subNamespace = SubnamespaceFromOutputPath(outputDir);

        using var context = _contextOperations.CreateContext(contextType);
        ValidateMigrationNameNotContextName(name, context);

        if (connectionString != null)
        {
            context.Database.SetConnectionString(connectionString);
        }

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);
        EnsureMigrationsAssembly(services);

        using var scope = services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();

        // Check for pending model changes
        if (!migrator.HasPendingModelChanges())
        {
            // No new migration needed, just apply existing migrations
            _reporter.WriteInformation(DesignStrings.NoPendingModelChanges);
            migrator.Migrate(null);
            _reporter.WriteInformation(DesignStrings.Done);
            return new MigrationFiles();
        }

        _reporter.WriteInformation(DesignStrings.CreatingAndApplyingMigration(name));

        // Scaffold migration
        var migration =
            string.IsNullOrEmpty(@namespace)
                ? scaffolder.ScaffoldMigration(name, _rootNamespace ?? string.Empty, subNamespace, _language, dryRun: true)
                : scaffolder.ScaffoldMigration(name, null, @namespace, _language, dryRun: true);

        // Save to disk
        var files = scaffolder.Save(_projectDir, migration, outputDir, dryRun: false);

        // Compile and register
        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        // Track for revert support
        _appliedDynamicMigrationIds.Add(migration.MigrationId);

        // Apply migration
        migrator.Migrate(migration.MigrationId);

        _reporter.WriteInformation(DesignStrings.MigrationCreatedAndApplied(migration.MigrationId));

        return files;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresDynamicCode("Runtime migration requires dynamic code generation.")]
    public virtual IReadOnlyList<string> RevertMigration(
        string? contextType,
        string? migrationId = null)
    {
        if (_appliedDynamicMigrationIds.Count == 0)
        {
            throw new OperationException(DesignStrings.NoDynamicMigrationsToRevert);
        }

        var idToRevert = migrationId ?? _appliedDynamicMigrationIds[^1];

        if (!_appliedDynamicMigrationIds.Contains(idToRevert))
        {
            throw new OperationException(DesignStrings.DynamicMigrationNotFound(idToRevert));
        }

        using var context = _contextOperations.CreateContext(contextType);

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);

        using var scope = services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var sqlGenerator = scope.ServiceProvider.GetRequiredService<IMigrationsSqlGenerator>();
        var historyRepository = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<IMigrationCommandExecutor>();
        var connection = scope.ServiceProvider.GetRequiredService<IRelationalConnection>();
        var rawSqlCommandBuilder = scope.ServiceProvider.GetRequiredService<IRawSqlCommandBuilder>();
        var commandLogger = scope.ServiceProvider.GetRequiredService<IRelationalCommandDiagnosticsLogger>();

        // Get the migration and execute Down operations
        var migrationTypeInfo = migrationsAssembly.Migrations[idToRevert];
        var migration = migrationsAssembly.CreateMigration(
            migrationTypeInfo,
            context.Database.ProviderName!);

        connection.Open();
        try
        {
            // Build the history delete command
            var deleteCommand = rawSqlCommandBuilder.Build(
                historyRepository.GetDeleteScript(idToRevert));

            // Generate migration commands from Down operations and append history delete
            var migrationCommands = sqlGenerator.Generate(
                migration.DownOperations,
                context.Model).ToList();
            migrationCommands.Add(new MigrationCommand(deleteCommand, context, commandLogger));

            // Execute all commands
            commandExecutor.ExecuteNonQuery(migrationCommands, connection);

            _appliedDynamicMigrationIds.Remove(idToRevert);

            // Return the SQL that was executed (excluding history delete for cleaner output)
            return migrationCommands
                .Take(migrationCommands.Count - 1)
                .Select(c => c.CommandText)
                .ToList();
        }
        finally
        {
            connection.Close();
        }
    }

    private static void ValidateMigrationName(string name)
    {
        var invalidPathChars = Path.GetInvalidFileNameChars();
        if (name.Any(c => invalidPathChars.Contains(c)))
        {
            throw new OperationException(
                DesignStrings.BadMigrationName(name, string.Join("','", invalidPathChars)));
        }
    }

    private static void ValidateMigrationNameNotContextName(string name, DbContext context)
    {
        var contextClassName = context.GetType().Name;
        if (string.Equals(name, contextClassName, StringComparison.Ordinal))
        {
            throw new OperationException(
                DesignStrings.ConflictingContextAndMigrationName(name));
        }
    }

    private static void EnsureServices(IServiceProvider services)
    {
        var migrator = services.GetService<IMigrator>();
        if (migrator == null)
        {
            var databaseProvider = services.GetService<IDatabaseProvider>();
            throw new OperationException(DesignStrings.NonRelationalProvider(databaseProvider?.Name ?? "Unknown"));
        }
    }

    private void EnsureMigrationsAssembly(IServiceProvider services)
    {
        var assemblyName = _assembly.GetName();
        var options = services.GetRequiredService<IDbContextOptions>();
        var contextType = services.GetRequiredService<ICurrentDbContext>().Context.GetType();
        var optionsExtension = RelationalOptionsExtension.Extract(options);
        if (optionsExtension.MigrationsAssemblyObject == null
            || optionsExtension.MigrationsAssemblyObject != _assembly)
        {
            var migrationsAssemblyName = optionsExtension.MigrationsAssembly
                ?? optionsExtension.MigrationsAssemblyObject?.GetName().Name
                ?? contextType.Assembly.GetName().Name;
            if (assemblyName.Name != migrationsAssemblyName
                && assemblyName.FullName != migrationsAssemblyName)
            {
                throw new OperationException(
                    DesignStrings.MigrationsAssemblyMismatch(assemblyName.Name, migrationsAssemblyName));
            }
        }
    }
}
