// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Migrations;

[SqlServerCondition(SqlServerCondition.IsNotAzureSql | SqlServerCondition.IsNotCI)]
public class RuntimeMigrationSqlServerTest : IAsyncLifetime
{
    private SqlServerTestStore? _testStore;
    private string _tempDirectory = null!;

    public async Task InitializeAsync()
    {
        _testStore = await SqlServerTestStore.CreateInitializedAsync(
            "RuntimeMigrationTest_" + Guid.NewGuid().ToString("N")[..8]);
        _tempDirectory = Path.Combine(Path.GetTempPath(), "RuntimeMigrationSqlServerTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task DisposeAsync()
    {
        if (_testStore != null)
        {
            await _testStore.DisposeAsync();
        }

        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [ConditionalFact]
    public void Can_create_and_apply_initial_migration()
    {
        using var context = CreateContext();

        // Ensure the database is clean (no tables)
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        // Drop everything again so we can test migration from scratch
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act - Create and apply migration
        var result = runtimeMigrationService.CreateAndApplyMigration(
            "InitialCreate",
            new RuntimeMigrationOptions
            {
                PersistToDisk = true,
                ProjectDirectory = _tempDirectory,
                OutputDirectory = "Migrations"
            });

        // Assert
        Assert.NotNull(result);
        Assert.Contains("InitialCreate", result.MigrationId);
        Assert.True(result.Applied);
        Assert.NotEmpty(result.SqlCommands);
        Assert.NotNull(result.MigrationFilePath);
        Assert.True(File.Exists(result.MigrationFilePath));

        // Verify the table was created
        var tableExists = _testStore!.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestEntities'");
        Assert.Equal(1, tableExists);
    }

    [ConditionalFact]
    public async Task Can_create_and_apply_initial_migration_async()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        await context.Database.EnsureDeletedAsync();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act - Create and apply migration
        var result = await runtimeMigrationService.CreateAndApplyMigrationAsync(
            "InitialCreateAsync",
            new RuntimeMigrationOptions
            {
                PersistToDisk = true,
                ProjectDirectory = _tempDirectory,
                OutputDirectory = "Migrations"
            });

        // Assert
        Assert.NotNull(result);
        Assert.Contains("InitialCreateAsync", result.MigrationId);
        Assert.True(result.Applied);
        Assert.NotEmpty(result.SqlCommands);

        // Verify the table was created
        var tableExists = await _testStore!.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestEntities'");
        Assert.Equal(1, tableExists);
    }

    [ConditionalFact]
    public void Can_create_migration_with_dry_run()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act - Create migration in dry run mode
        var result = runtimeMigrationService.CreateAndApplyMigration(
            "DryRunMigration",
            new RuntimeMigrationOptions
            {
                PersistToDisk = false,
                DryRun = true
            });

        // Assert
        Assert.NotNull(result);
        Assert.Contains("DryRunMigration", result.MigrationId);
        Assert.False(result.Applied); // Should not be applied
        Assert.NotEmpty(result.SqlCommands); // Should still have SQL commands
        Assert.Null(result.MigrationFilePath); // Should not persist to disk
        Assert.False(result.PersistedToDisk);

        // Verify the table was NOT created
        var tableCount = _testStore!.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestEntities'");
        Assert.Equal(0, tableCount);
    }

    [ConditionalFact]
    public void CreateAndApplyMigration_generates_valid_sql_commands()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act
        var result = runtimeMigrationService.CreateAndApplyMigration(
            "ValidSqlMigration",
            new RuntimeMigrationOptions
            {
                PersistToDisk = false
            });

        // Assert - SQL commands should include CREATE TABLE
        Assert.Contains(result.SqlCommands, sql => sql.Contains("CREATE TABLE"));
        Assert.Contains(result.SqlCommands, sql => sql.Contains("TestEntities"));
    }

    [ConditionalFact]
    public void Can_check_for_pending_model_changes()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act
        var hasPendingChanges = runtimeMigrationService.HasPendingModelChanges();

        // Assert - Should have pending changes since we have no migrations yet
        Assert.True(hasPendingChanges);
    }

    [ConditionalFact]
    public void Applied_migration_appears_in_migration_history()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act
        var result = runtimeMigrationService.CreateAndApplyMigration(
            "HistoryTestMigration",
            new RuntimeMigrationOptions
            {
                PersistToDisk = false
            });

        // Assert - Check migration history table
        var migrationId = _testStore!.ExecuteScalar<string>(
            "SELECT MigrationId FROM __EFMigrationsHistory WHERE MigrationId LIKE '%HistoryTestMigration'");
        Assert.NotNull(migrationId);
        Assert.Contains("HistoryTestMigration", migrationId);
    }

    [ConditionalFact]
    public void Can_create_migration_with_custom_namespace()
    {
        using var context = CreateContext();

        // Ensure the database is clean
        context.Database.EnsureDeleted();

        using var freshContext = CreateContext();

        // Create the design-time service provider
        using var serviceProvider = CreateDesignTimeServiceProvider(freshContext);
        using var scope = serviceProvider.CreateScope();

        var runtimeMigrationService = scope.ServiceProvider.GetRequiredService<IRuntimeMigrationService>();

        // Act - use a sub-namespace (the Namespace parameter is the sub-namespace added to root)
        var result = runtimeMigrationService.CreateAndApplyMigration(
            "CustomNamespaceMigration",
            new RuntimeMigrationOptions
            {
                PersistToDisk = true,
                ProjectDirectory = _tempDirectory,
                OutputDirectory = "Migrations",
                Namespace = "CustomMigrations"
            });

        // Assert
        Assert.NotNull(result);
        Assert.True(File.Exists(result.MigrationFilePath));

        // Verify the file was created and contains namespace declaration
        var migrationContent = File.ReadAllText(result.MigrationFilePath!);
        Assert.Contains("namespace", migrationContent);
        Assert.Contains("CustomMigrations", migrationContent);
    }

    private TestDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        _testStore!.AddProviderOptions(optionsBuilder);

        return new TestDbContext(optionsBuilder.Options);
    }

    private ServiceProvider CreateDesignTimeServiceProvider(DbContext context)
    {
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);

        // Add SQL Server design-time services
        new SqlServerDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);

        // Add additional required services for RuntimeMigrationService
        serviceCollection.AddScoped(_ => context.GetService<IMigrationsSqlGenerator>());
        serviceCollection.AddScoped(_ => context.GetService<IRelationalDatabaseCreator>());
        serviceCollection.AddScoped(_ => context.GetService<IMigrationCommandExecutor>());
        serviceCollection.AddScoped(_ => context.GetService<IRelationalConnection>());
        serviceCollection.AddScoped(_ => context.GetService<IRawSqlCommandBuilder>());
        serviceCollection.AddScoped(_ => context.GetService<IRelationalCommandDiagnosticsLogger>());

        return serviceCollection.BuildServiceProvider(validateScopes: true);
    }

    // Test DbContext with entities - no existing migrations/snapshot
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

