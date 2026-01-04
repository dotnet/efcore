// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

/// <summary>
///     Base class for runtime migration tests. These tests validate the ability to
///     scaffold, compile, and apply migrations at runtime without using the CLI.
///     Each test gets its own isolated database to prevent state leakage.
/// </summary>
public abstract class RuntimeMigrationTestBase
{
    protected abstract ITestStoreFactory TestStoreFactory { get; }
    protected abstract Assembly ProviderAssembly { get; }

    #region Model Classes

    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    public class RuntimeMigrationDbContext : DbContext
    {
        public RuntimeMigrationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(b =>
            {
                b.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<Post>(b =>
            {
                b.Property(e => e.Title).HasMaxLength(300);
                b.HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId);
            });
        }
    }

    #endregion

    #region Test Infrastructure

    /// <summary>
    ///     Creates an isolated test context with its own database.
    ///     Uses a fixed database name and cleans tables between tests.
    /// </summary>
    protected TestContext CreateTestContext()
    {
        var testStore = TestStoreFactory.Create("RuntimeMigration");

        var builder = new DbContextOptionsBuilder<RuntimeMigrationDbContext>();
        ConfigureOptions(builder);
        builder.EnableServiceProviderCaching(false);

        var context = new RuntimeMigrationDbContext(builder.Options);
        // Clean the database to ensure we start with empty schema
        CleanDatabase(context);

        return new TestContext(context, testStore, ProviderAssembly);
    }

    /// <summary>
    ///     Configures provider-specific options for the DbContext.
    /// </summary>
    protected virtual void ConfigureOptions(DbContextOptionsBuilder builder)
    {
        // Default: let the test store configure options
        var testStore = TestStoreFactory.Create("RuntimeMigration");
        testStore.AddProviderOptions(builder);
    }

    /// <summary>
    ///     Cleans the database to ensure a fresh start for each test.
    ///     Provider-specific implementation may be required.
    /// </summary>
    protected virtual void CleanDatabase(RuntimeMigrationDbContext context)
    {
        // Default: use EnsureDeleted to start fresh
        context.Database.EnsureDeleted();
    }

    public sealed class TestContext : IDisposable
    {
        public RuntimeMigrationDbContext Context { get; }
        private readonly TestStore _testStore;
        private readonly ServiceProvider _designTimeServices;

        public TestContext(RuntimeMigrationDbContext context, TestStore testStore, Assembly providerAssembly)
        {
            Context = context;
            _testStore = testStore;

            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddDbContextDesignTimeServices(context);
            ((IDesignTimeServices)Activator.CreateInstance(
                    providerAssembly.GetType(
                        providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                        throwOnError: true))!)
                .ConfigureDesignTimeServices(serviceCollection);
            _designTimeServices = serviceCollection.BuildServiceProvider(validateScopes: true);
        }

        public IServiceScope CreateScope()
            => _designTimeServices.CreateScope();

        public void Dispose()
        {
            _designTimeServices.Dispose();
            Context.Dispose();
            _testStore.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    #endregion

    #region Tests

    [ConditionalFact]
    public void Can_scaffold_migration()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

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
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();

        var migration = scaffolder.ScaffoldMigration(
            "CompiledMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());

        Assert.NotNull(compiledAssembly);

        var migrationType = compiledAssembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);
    }

    [ConditionalFact]
    public void Can_register_and_apply_compiled_migration()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

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

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        Assert.Contains(migration.MigrationId, migrationsAssembly.Migrations.Keys);

        migrator.Migrate(migration.MigrationId);

        var appliedMigrations = test.Context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration.MigrationId, appliedMigrations);
    }

    [ConditionalFact]
    public void Compiled_migration_generates_valid_sql()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

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

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);

        var migrationTypeInfo = migrationsAssembly.Migrations[migration.MigrationId];
        var migrationInstance = migrationsAssembly.CreateMigration(
            migrationTypeInfo,
            test.Context.Database.ProviderName);

        var commands = sqlGenerator.Generate(
            migrationInstance.UpOperations,
            test.Context.Model).ToList();

        Assert.NotEmpty(commands);
    }

    [ConditionalFact]
    public void HasPendingModelChanges_returns_true_for_new_model()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        Assert.True(migrator.HasPendingModelChanges());
    }

    [ConditionalFact]
    public void HasPendingModelChanges_returns_false_after_migration()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Initially has pending changes
        Assert.True(migrator.HasPendingModelChanges());

        // Apply migration
        var migration = scaffolder.ScaffoldMigration(
            "PendingChangesTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        // Should not have pending changes after migration
        Assert.False(migrator.HasPendingModelChanges());
    }

    [ConditionalFact]
    public void Compiled_migration_contains_correct_operations()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();

        var migration = scaffolder.ScaffoldMigration(
            "OperationsTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());

        var migrationType = compiledAssembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);

        var migrationInstance = (Migration)Activator.CreateInstance(migrationType);
        var upOperations = migrationInstance.UpOperations.ToList();

        Assert.NotEmpty(upOperations);
        Assert.Contains(upOperations, op => op is CreateTableOperation);
    }

    [ConditionalFact]
    public void Can_scaffold_and_save_migration_to_disk()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "EFCoreMigrationTest_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);

            using var test = CreateTestContext();
            using var scope = test.CreateScope();

            var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();

            var migration = scaffolder.ScaffoldMigration(
                "SaveToDisk",
                rootNamespace: "TestNamespace",
                subNamespace: "Migrations",
                language: "C#",
                dryRun: true);

            var files = scaffolder.Save(tempDirectory, migration, outputDir: null, dryRun: false);

            Assert.NotNull(files.MigrationFile);
            Assert.NotNull(files.MetadataFile);
            Assert.NotNull(files.SnapshotFile);
            Assert.True(File.Exists(files.MigrationFile));
            Assert.True(File.Exists(files.MetadataFile));
            Assert.True(File.Exists(files.SnapshotFile));

            var migrationContent = File.ReadAllText(files.MigrationFile);
            Assert.Contains("CreateTable", migrationContent);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                try { Directory.Delete(tempDirectory, recursive: true); }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }

    [ConditionalFact]
    public void Can_apply_multiple_migrations_sequentially()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // First migration
        var migration1 = scaffolder.ScaffoldMigration(
            "FirstSequential",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, test.Context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        var appliedAfterFirst = test.Context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration1.MigrationId, appliedAfterFirst);

        // Second migration (empty since model hasn't changed, but should still work)
        var migration2 = scaffolder.ScaffoldMigration(
            "SecondSequential",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, test.Context.GetType());
        migrationsAssembly.AddMigrations(assembly2);
        migrator.Migrate(migration2.MigrationId);

        var appliedAfterSecond = test.Context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration1.MigrationId, appliedAfterSecond);
        Assert.Contains(migration2.MigrationId, appliedAfterSecond);
    }

    [ConditionalFact]
    public void Migration_down_reverses_up()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Scaffold and apply the migration (Up)
        var migration = scaffolder.ScaffoldMigration(
            "SymmetryTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        // Verify tables were created
        var connection = test.Context.Database.GetDbConnection();
        connection.Open();
        var tablesAfterUp = GetTableNames(connection);
        Assert.Contains("Blogs", tablesAfterUp);
        Assert.Contains("Posts", tablesAfterUp);

        // Run Down to revert
        migrator.Migrate("0"); // Migrate to "0" reverts all migrations

        // Verify tables were dropped
        var tablesAfterDown = GetTableNames(connection);
        Assert.DoesNotContain("Blogs", tablesAfterDown);
        Assert.DoesNotContain("Posts", tablesAfterDown);
        connection.Close();
    }

    /// <summary>
    ///     Gets table names from the database. Provider-specific implementation required.
    /// </summary>
    protected abstract List<string> GetTableNames(System.Data.Common.DbConnection connection);

    [ConditionalFact]
    public void Can_revert_migration_using_down_operations()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
        var sqlGenerator = scope.ServiceProvider.GetRequiredService<IMigrationsSqlGenerator>();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<IMigrationCommandExecutor>();
        var connection = scope.ServiceProvider.GetRequiredService<IRelationalConnection>();

        var migration = scaffolder.ScaffoldMigration(
            "RevertTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        var appliedBefore = test.Context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration.MigrationId, appliedBefore);

        var migrationTypeInfo = migrationsAssembly.Migrations[migration.MigrationId];
        var migrationInstance = migrationsAssembly.CreateMigration(
            migrationTypeInfo,
            test.Context.Database.ProviderName);

        var downCommands = sqlGenerator.Generate(
            migrationInstance.DownOperations,
            test.Context.Model).ToList();

        connection.Open();
        try
        {
            commandExecutor.ExecuteNonQuery(downCommands, connection);
        }
        finally
        {
            connection.Close();
        }
    }

    [ConditionalFact]
    public void Applied_migration_is_recorded_in_history()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        var initialApplied = test.Context.Database.GetAppliedMigrations().ToList();

        var migration = scaffolder.ScaffoldMigration(
            "HistoryTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        var afterApplied = test.Context.Database.GetAppliedMigrations().ToList();
        Assert.Equal(initialApplied.Count + 1, afterApplied.Count);
        Assert.Contains(migration.MigrationId, afterApplied);
    }

    [ConditionalFact]
    public void Compiled_migration_has_matching_up_and_down_table_operations()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();

        var migration = scaffolder.ScaffoldMigration(
            "UpDownTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());

        var migrationType = compiledAssembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);

        var migrationInstance = (Migration)Activator.CreateInstance(migrationType);

        var upTableCount = migrationInstance.UpOperations.OfType<CreateTableOperation>().Count();
        var downTableCount = migrationInstance.DownOperations.OfType<DropTableOperation>().Count();
        Assert.Equal(upTableCount, downTableCount);
    }

    #endregion
}
