// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design.Internal;

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
        using var context = _contextOperations.CreateContext(contextType);
        var services = PrepareForMigration(name, context);

        using var scope = services.CreateScope();
        var (_, files) =
            CreateScaffoldedMigration(name, outputDir, @namespace, dryRun, scope.ServiceProvider);
        return files;
    }

    /// <summary>
    ///     Creates and saves a scaffolded migration using the provided services.
    /// </summary>
    public virtual (ScaffoldedMigration Migration, MigrationFiles Files)
        CreateScaffoldedMigration(
            string name,
            string? outputDir,
            string? @namespace,
            bool dryRun,
            IServiceProvider services)
    {
        EnsureMigrationsAssembly(services);

        if (outputDir != null)
        {
            outputDir = Path.GetFullPath(Path.Combine(_projectDir, outputDir));
        }

        var subNamespace = SubnamespaceFromOutputPath(outputDir);

        var scaffolder = services.GetRequiredService<IMigrationsScaffolder>();
        var migration =
            string.IsNullOrEmpty(@namespace)
                // TODO: Honor _nullable (issue #18950)
                ? scaffolder.ScaffoldMigration(name, _rootNamespace ?? string.Empty, subNamespace, _language, dryRun)
                : scaffolder.ScaffoldMigration(name, null, @namespace, _language, dryRun);

        var files = scaffolder.Save(_projectDir, migration, outputDir, dryRun);

        return (migration, files);
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
        bool offline,
        bool dryRun,
        string? connectionString)
    {
        using var context = _contextOperations.CreateContext(contextType);

        if (connectionString != null)
        {
            context.Database.SetConnectionString(connectionString);
        }

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);
        EnsureMigrationsAssembly(services);

        using var scope = services.CreateScope();
        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();

        var files = scaffolder.RemoveMigration(_projectDir, _rootNamespace, force, _language, dryRun, offline);

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
        using var context = _contextOperations.CreateContext(contextType);
        var services = PrepareForMigration(name, context);

        if (connectionString != null)
        {
            context.Database.SetConnectionString(connectionString);
        }

        using var scope = services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        if (!migrator.HasPendingModelChanges())
        {
            _reporter.WriteInformation(DesignStrings.NoPendingModelChanges);
            migrator.Migrate(null);
            _reporter.WriteInformation(DesignStrings.Done);
            // Return empty MigrationFiles to indicate no migration was created.
            // When serialized to JSON (with --json), all file path properties will be null.
            return new MigrationFiles();
        }

        _reporter.WriteInformation(DesignStrings.CreatingAndApplyingMigration(name));

        var (migration, files) =
            CreateScaffoldedMigration(name, outputDir, @namespace, dryRun: false, scope.ServiceProvider);

        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        migrator.Migrate(migration.MigrationId);

        _reporter.WriteInformation(DesignStrings.MigrationCreatedAndApplied(migration.MigrationId));

        return files;
    }

    /// <summary>
    ///     Prepares common resources for migration operations.
    /// </summary>
    public virtual IServiceProvider PrepareForMigration(string name, DbContext context)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new OperationException(DesignStrings.MigrationNameRequired);
        }

        var invalidPathChars = Path.GetInvalidFileNameChars();
        if (name.Any(c => invalidPathChars.Contains(c)))
        {
            throw new OperationException(
                DesignStrings.BadMigrationName(name, string.Join("','", invalidPathChars)));
        }

        var contextClassName = context.GetType().Name;
        if (string.Equals(name, contextClassName, StringComparison.Ordinal))
        {
            throw new OperationException(
                DesignStrings.ConflictingContextAndMigrationName(name));
        }

        var services = _servicesBuilder.Build(context);
        EnsureServices(services);

        return services;
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
