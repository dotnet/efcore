// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     A service that creates and applies migrations at runtime without requiring recompilation.
/// </summary>
/// <remarks>
///     <para>
///         This service orchestrates the workflow of scaffolding a migration, compiling it
///         dynamically using Roslyn, registering it with the migrations assembly, and applying
///         it to the database.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public class RuntimeMigrationService : IRuntimeMigrationService
{
    private readonly ICurrentDbContext _currentContext;
    private readonly IMigrationsScaffolder _scaffolder;
    private readonly IMigrationCompiler _compiler;
    private readonly IDynamicMigrationsAssembly _dynamicMigrationsAssembly;
    private readonly IMigrator _migrator;
    private readonly IMigrationsSqlGenerator _sqlGenerator;
    private readonly IRelationalDatabaseCreator _databaseCreator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeMigrationService" /> class.
    /// </summary>
    /// <param name="currentContext">The current DbContext.</param>
    /// <param name="scaffolder">The migrations scaffolder.</param>
    /// <param name="compiler">The migration compiler.</param>
    /// <param name="dynamicMigrationsAssembly">The dynamic migrations assembly.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="sqlGenerator">The SQL generator.</param>
    /// <param name="databaseCreator">The database creator.</param>
    public RuntimeMigrationService(
        ICurrentDbContext currentContext,
        IMigrationsScaffolder scaffolder,
        IMigrationCompiler compiler,
        IDynamicMigrationsAssembly dynamicMigrationsAssembly,
        IMigrator migrator,
        IMigrationsSqlGenerator sqlGenerator,
        IRelationalDatabaseCreator databaseCreator)
    {
        _currentContext = currentContext;
        _scaffolder = scaffolder;
        _compiler = compiler;
        _dynamicMigrationsAssembly = dynamicMigrationsAssembly;
        _migrator = migrator;
        _sqlGenerator = sqlGenerator;
        _databaseCreator = databaseCreator;
    }

    /// <inheritdoc />
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    public virtual RuntimeMigrationResult CreateAndApplyMigration(
        string migrationName,
        RuntimeMigrationOptions? options = null)
    {
        options ??= new RuntimeMigrationOptions();

        // Step 1: Check for pending changes
        if (!HasPendingModelChanges())
        {
            throw new InvalidOperationException(DesignStrings.NoPendingModelChanges);
        }

        var contextType = _currentContext.Context.GetType();
        var rootNamespace = contextType.Namespace ?? string.Empty;

        // Step 2: Scaffold the migration
        var scaffoldedMigration = _scaffolder.ScaffoldMigration(
            migrationName,
            rootNamespace,
            options.Namespace,
            language: "C#",
            dryRun: true); // Always generate code without writing

        // Step 3: Optionally persist to disk
        MigrationFiles? savedFiles = null;
        if (options.PersistToDisk)
        {
            var projectDir = options.ProjectDirectory ?? Directory.GetCurrentDirectory();
            savedFiles = _scaffolder.Save(
                projectDir,
                scaffoldedMigration,
                options.OutputDirectory,
                dryRun: false);
        }

        // Step 4: Compile the migration
        var compiledMigration = _compiler.CompileMigration(scaffoldedMigration, contextType);

        // Step 5: Register with dynamic migrations assembly
        _dynamicMigrationsAssembly.RegisterDynamicMigration(compiledMigration);

        // Step 6: Generate SQL commands for reporting
        var sqlCommands = GenerateSqlCommands(compiledMigration);

        // Step 7: Apply the migration (unless dry run)
        var applied = false;
        if (!options.DryRun)
        {
            _migrator.Migrate(compiledMigration.MigrationId);
            applied = true;
        }

        return new RuntimeMigrationResult(
            compiledMigration.MigrationId,
            applied,
            sqlCommands,
            savedFiles?.MigrationFile,
            savedFiles?.MetadataFile,
            savedFiles?.SnapshotFile);
    }

    /// <inheritdoc />
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    public virtual async Task<RuntimeMigrationResult> CreateAndApplyMigrationAsync(
        string migrationName,
        RuntimeMigrationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RuntimeMigrationOptions();

        // Step 1: Check for pending changes
        if (!HasPendingModelChanges())
        {
            throw new InvalidOperationException(DesignStrings.NoPendingModelChanges);
        }

        var contextType = _currentContext.Context.GetType();
        var rootNamespace = contextType.Namespace ?? string.Empty;

        // Step 2: Scaffold the migration (synchronous - no async API)
        var scaffoldedMigration = _scaffolder.ScaffoldMigration(
            migrationName,
            rootNamespace,
            options.Namespace,
            language: "C#",
            dryRun: true);

        // Step 3: Optionally persist to disk
        MigrationFiles? savedFiles = null;
        if (options.PersistToDisk)
        {
            var projectDir = options.ProjectDirectory ?? Directory.GetCurrentDirectory();
            savedFiles = _scaffolder.Save(
                projectDir,
                scaffoldedMigration,
                options.OutputDirectory,
                dryRun: false);
        }

        // Step 4: Compile the migration (synchronous - Roslyn is synchronous)
        var compiledMigration = _compiler.CompileMigration(scaffoldedMigration, contextType);

        // Step 5: Register with dynamic migrations assembly
        _dynamicMigrationsAssembly.RegisterDynamicMigration(compiledMigration);

        // Step 6: Generate SQL commands for reporting
        var sqlCommands = GenerateSqlCommands(compiledMigration);

        // Step 7: Apply the migration (unless dry run)
        var applied = false;
        if (!options.DryRun)
        {
            await _migrator.MigrateAsync(compiledMigration.MigrationId, cancellationToken)
                .ConfigureAwait(false);
            applied = true;
        }

        return new RuntimeMigrationResult(
            compiledMigration.MigrationId,
            applied,
            sqlCommands,
            savedFiles?.MigrationFile,
            savedFiles?.MetadataFile,
            savedFiles?.SnapshotFile);
    }

    /// <inheritdoc />
    public virtual bool HasPendingModelChanges()
        => _migrator.HasPendingModelChanges();

    /// <summary>
    ///     Generates the SQL commands that would be executed for the migration.
    /// </summary>
    /// <param name="compiledMigration">The compiled migration.</param>
    /// <returns>The list of SQL command strings.</returns>
    protected virtual IReadOnlyList<string> GenerateSqlCommands(CompiledMigration compiledMigration)
    {
        var migration = _dynamicMigrationsAssembly.CreateMigration(
            compiledMigration.MigrationTypeInfo,
            _currentContext.Context.Database.ProviderName!);

        var commands = _sqlGenerator.Generate(
            migration.UpOperations,
            _currentContext.Context.Model);

        return commands.Select(c => c.CommandText).ToList();
    }
}
