// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class DesignTimeTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : DesignTimeTestBase<TFixture>.DesignTimeFixtureBase
{
    protected TFixture Fixture { get; } = fixture;

    protected abstract Assembly ProviderAssembly { get; }

    [ConditionalFact]
    public void Can_get_reverse_engineering_services()
    {
        using var context = Fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices();
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var reverseEngineerScaffolder = services.CreateScope().ServiceProvider.GetService<IReverseEngineerScaffolder>();

        Assert.NotNull(reverseEngineerScaffolder);
    }

    [ConditionalFact]
    public void Can_get_migrations_services()
    {
        using var context = Fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var migrationsScaffolder = services.CreateScope().ServiceProvider.GetService<IMigrationsScaffolder>();

        Assert.NotNull(migrationsScaffolder);
    }

    [ConditionalFact]
    public void Can_scaffold_migration()
    {
        using var context = Fixture.CreateContext();
        using var services = CreateDesignTimeServices(context);
        using var scope = services.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();

        var migration = scaffolder.ScaffoldMigration(
            "TestMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        Assert.NotNull(migration);
        Assert.Contains("TestMigration", migration.MigrationId);
        Assert.NotEmpty(migration.MigrationCode);
        Assert.NotEmpty(migration.MetadataCode);
        Assert.NotEmpty(migration.SnapshotCode);
    }

    [ConditionalFact]
    public void Can_compile_migration()
    {
        using var context = Fixture.CreateContext();
        using var services = CreateDesignTimeServices(context);
        using var scope = services.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();

        var migration = scaffolder.ScaffoldMigration(
            "CompiledMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());

        Assert.NotNull(compiledAssembly);

        var migrationType = compiledAssembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);
    }

    [ConditionalFact]
    public void Can_register_and_apply_compiled_migration()
    {
        using var context = Fixture.CreateContext();
        using var services = CreateDesignTimeServices(context);
        using var scope = services.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        var migration = scaffolder.ScaffoldMigration(
            "AppliedMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        Assert.Contains(migration.MigrationId, migrationsAssembly.Migrations.Keys);

        migrator.Migrate(migration.MigrationId);

        var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration.MigrationId, appliedMigrations);
    }

    [ConditionalFact]
    public void Compiled_migration_generates_valid_sql()
    {
        using var context = Fixture.CreateContext();
        using var services = CreateDesignTimeServices(context);
        using var scope = services.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var sqlGenerator = scope.ServiceProvider.GetRequiredService<IMigrationsSqlGenerator>();

        var migration = scaffolder.ScaffoldMigration(
            "SqlGenMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        var migrationTypeInfo = migrationsAssembly.Migrations[migration.MigrationId];
        var migrationInstance = migrationsAssembly.CreateMigration(
            migrationTypeInfo,
            context.Database.ProviderName);

        var commands = sqlGenerator.Generate(
            migrationInstance.UpOperations,
            context.Model).ToList();

        Assert.NotEmpty(commands);
    }

    [ConditionalFact]
    public void HasPendingModelChanges_returns_true_for_new_model()
    {
        using var context = Fixture.CreateContext();
        using var services = CreateDesignTimeServices(context);
        using var scope = services.CreateScope();

        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        Assert.True(migrator.HasPendingModelChanges());
    }

    protected ServiceProvider CreateDesignTimeServices(DbContext context)
    {
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        return serviceCollection.BuildServiceProvider(validateScopes: true);
    }

    public abstract class DesignTimeFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "DesignTimeTest";
    }
}
