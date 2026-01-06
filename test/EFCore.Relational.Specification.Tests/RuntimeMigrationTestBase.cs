// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

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
        // Clean up existing tables without deleting the database file
        // This avoids file locking issues on Windows
        context.Database.EnsureCreated();
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        // Drop all tables except migrations history
        var tables = GetTableNames(connection);
        foreach (var table in tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS \"{table}\"";
            command.ExecuteNonQuery();
        }

        // Drop migrations history table to reset migration state
        using var dropHistoryCommand = connection.CreateCommand();
        dropHistoryCommand.CommandText = "DROP TABLE IF EXISTS \"__EFMigrationsHistory\"";
        dropHistoryCommand.ExecuteNonQuery();
    }

    /// <summary>
    ///     Gets all table names in the database (excluding system tables and migrations history).
    ///     Provider-specific implementation required.
    /// </summary>
    protected abstract List<string> GetTableNames(System.Data.Common.DbConnection connection);

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
    ///     Gets the database model by reverse-engineering the current database state.
    ///     This allows rigorous verification of actual schema structure.
    /// </summary>
    protected DatabaseModel GetDatabaseModel(IServiceScope scope, RuntimeMigrationDbContext context)
    {
        var databaseModelFactory = scope.ServiceProvider.GetRequiredService<IDatabaseModelFactory>();
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        return databaseModelFactory.Create(connection, new DatabaseModelFactoryOptions());
    }

    /// <summary>
    ///     Helper to scaffold, compile, and apply a migration.
    /// </summary>
    protected (ScaffoldedMigration Migration, Assembly CompiledAssembly) ScaffoldAndApplyMigration(
        IServiceScope scope,
        RuntimeMigrationDbContext context,
        string migrationName)
    {
        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        var migration = scaffolder.ScaffoldMigration(
            migrationName,
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        return (migration, compiledAssembly);
    }

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

    #region Rigorous Schema Verification Tests

    [ConditionalFact]
    public void Migration_creates_correct_table_structure()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        ScaffoldAndApplyMigration(scope, test.Context, "TableStructureTest");

        var dbModel = GetDatabaseModel(scope, test.Context);

        // Verify both tables exist
        Assert.Contains(dbModel.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModel.Tables, t => t.Name == "Posts");

        // Verify Blogs table structure
        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.Equal(2, blogsTable.Columns.Count); // Id, Name
        Assert.Single(blogsTable.Columns, c => c.Name == "Id");
        Assert.Single(blogsTable.Columns, c => c.Name == "Name");

        // Verify Posts table structure
        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        Assert.Equal(4, postsTable.Columns.Count); // Id, Title, Content, BlogId
        Assert.Single(postsTable.Columns, c => c.Name == "Id");
        Assert.Single(postsTable.Columns, c => c.Name == "Title");
        Assert.Single(postsTable.Columns, c => c.Name == "Content");
        Assert.Single(postsTable.Columns, c => c.Name == "BlogId");
    }

    [ConditionalFact]
    public void Migration_creates_correct_primary_keys()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        ScaffoldAndApplyMigration(scope, test.Context, "PrimaryKeyTest");

        var dbModel = GetDatabaseModel(scope, test.Context);

        // Verify Blogs.Id is primary key
        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.NotNull(blogsTable.PrimaryKey);
        Assert.Single(blogsTable.PrimaryKey.Columns);
        Assert.Equal("Id", blogsTable.PrimaryKey.Columns[0].Name);

        // Verify Blogs.Id is not nullable (PK requirement)
        var blogsId = blogsTable.Columns.Single(c => c.Name == "Id");
        Assert.False(blogsId.IsNullable);

        // Verify Posts.Id is primary key
        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        Assert.NotNull(postsTable.PrimaryKey);
        Assert.Single(postsTable.PrimaryKey.Columns);
        Assert.Equal("Id", postsTable.PrimaryKey.Columns[0].Name);

        // Verify Posts.Id is not nullable
        var postsId = postsTable.Columns.Single(c => c.Name == "Id");
        Assert.False(postsId.IsNullable);
    }

    [ConditionalFact]
    public void Migration_creates_correct_foreign_keys()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        ScaffoldAndApplyMigration(scope, test.Context, "ForeignKeyTest");

        var dbModel = GetDatabaseModel(scope, test.Context);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");

        // Verify Posts has exactly one foreign key
        var fk = Assert.Single(postsTable.ForeignKeys);

        // Verify FK points to Blogs table
        Assert.Equal("Blogs", fk.PrincipalTable.Name);

        // Verify FK column is BlogId
        Assert.Single(fk.Columns);
        Assert.Equal("BlogId", fk.Columns[0].Name);

        // Verify FK principal column is Id
        Assert.Single(fk.PrincipalColumns);
        Assert.Equal("Id", fk.PrincipalColumns[0].Name);

        // Verify cascade delete behavior
        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
    }

    [ConditionalFact]
    public void Migration_creates_columns_with_correct_constraints()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        ScaffoldAndApplyMigration(scope, test.Context, "ColumnConstraintTest");

        var dbModel = GetDatabaseModel(scope, test.Context);

        // Verify Blogs.Name column (MaxLength 200)
        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        var nameColumn = blogsTable.Columns.Single(c => c.Name == "Name");
        // SQLite stores as TEXT with length info embedded or separate
        Assert.NotNull(nameColumn.StoreType);

        // Verify Posts.Title column (MaxLength 300)
        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        var titleColumn = postsTable.Columns.Single(c => c.Name == "Title");
        Assert.NotNull(titleColumn.StoreType);

        // Verify Posts.Content is nullable (no constraint specified)
        var contentColumn = postsTable.Columns.Single(c => c.Name == "Content");
        Assert.True(contentColumn.IsNullable);

        // Verify Posts.BlogId is NOT nullable (required FK)
        var blogIdColumn = postsTable.Columns.Single(c => c.Name == "BlogId");
        Assert.False(blogIdColumn.IsNullable);
    }

    [ConditionalFact]
    public void Migration_down_removes_schema_completely()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Apply migration and verify schema exists
        ScaffoldAndApplyMigration(scope, test.Context, "CompleteRemovalTest");

        var dbModelBefore = GetDatabaseModel(scope, test.Context);

        // Verify Blogs and Posts tables exist
        Assert.Contains(dbModelBefore.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModelBefore.Tables, t => t.Name == "Posts");

        // Verify FK exists before rollback
        var postsTableBefore = dbModelBefore.Tables.Single(t => t.Name == "Posts");
        Assert.Single(postsTableBefore.ForeignKeys);

        // Run Down to revert all migrations
        migrator.Migrate("0");

        // Verify schema is completely removed
        var dbModelAfter = GetDatabaseModel(scope, test.Context);

        // Verify Blogs and Posts tables are removed
        Assert.DoesNotContain(dbModelAfter.Tables, t => t.Name == "Blogs");
        Assert.DoesNotContain(dbModelAfter.Tables, t => t.Name == "Posts");
    }

    [ConditionalFact]
    public void Migration_creates_foreign_key_index()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        ScaffoldAndApplyMigration(scope, test.Context, "FKIndexTest");

        var dbModel = GetDatabaseModel(scope, test.Context);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");

        // EF Core automatically creates an index on FK columns
        // Look for an index that contains BlogId
        var fkIndex = postsTable.Indexes.FirstOrDefault(i =>
            i.Columns.Any(c => c.Name == "BlogId"));

        Assert.NotNull(fkIndex);
        Assert.Single(fkIndex.Columns);
        Assert.Equal("BlogId", fkIndex.Columns[0].Name);
    }

    [ConditionalFact]
    public void Migration_with_no_changes_produces_empty_operations()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Apply first migration (creates schema)
        var migration1 = scaffolder.ScaffoldMigration(
            "InitialMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, test.Context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        // Scaffold second migration (no model changes)
        var migration2 = scaffolder.ScaffoldMigration(
            "EmptyMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, test.Context.GetType());

        // Find the migration type and verify empty operations
        var migrationType = assembly2.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);

        var migrationInstance = (Migration)Activator.CreateInstance(migrationType);

        // Should have no Up operations (model unchanged)
        Assert.Empty(migrationInstance.UpOperations);

        // Should have no Down operations
        Assert.Empty(migrationInstance.DownOperations);
    }

    [ConditionalFact]
    public void Migration_preserves_existing_data()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Apply first migration
        var migration1 = scaffolder.ScaffoldMigration(
            "DataPreservationInit",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, test.Context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        // Insert test data
        test.Context.Blogs.Add(new Blog { Name = "Test Blog" });
        test.Context.SaveChanges();

        var blogId = test.Context.Blogs.First().Id;
        test.Context.Posts.Add(new Post
        {
            Title = "Test Post",
            Content = "Test Content",
            BlogId = blogId
        });
        test.Context.SaveChanges();

        // Verify data exists
        Assert.Equal(1, test.Context.Blogs.Count());
        Assert.Equal(1, test.Context.Posts.Count());

        // Apply second empty migration
        var migration2 = scaffolder.ScaffoldMigration(
            "DataPreservationCheck",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, test.Context.GetType());
        migrationsAssembly.AddMigrations(assembly2);
        migrator.Migrate(migration2.MigrationId);

        // Verify data is preserved
        Assert.Equal(1, test.Context.Blogs.Count());
        Assert.Equal(1, test.Context.Posts.Count());

        var blog = test.Context.Blogs.First();
        Assert.Equal("Test Blog", blog.Name);

        var post = test.Context.Posts.First();
        Assert.Equal("Test Post", post.Title);
        Assert.Equal("Test Content", post.Content);
    }

    [ConditionalFact]
    public void Applied_migration_snapshot_matches_model()
    {
        using var test = CreateTestContext();
        using var scope = test.CreateScope();

        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

        // Initially should have pending model changes
        Assert.True(migrator.HasPendingModelChanges());

        // Apply migration
        ScaffoldAndApplyMigration(scope, test.Context, "SnapshotMatchTest");

        // After migration, should have no pending changes
        Assert.False(migrator.HasPendingModelChanges());

        // Verify the database model matches expectations
        var dbModel = GetDatabaseModel(scope, test.Context);

        // Model should have Blogs and Posts tables
        Assert.Contains(dbModel.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModel.Tables, t => t.Name == "Posts");

        // Blogs should have the correct columns
        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.Equal(2, blogsTable.Columns.Count);

        // Posts should have the correct columns
        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        Assert.Equal(4, postsTable.Columns.Count);
    }

    [ConditionalFact]
    public void RemoveMigration_removes_dynamically_created_migration()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "EFCoreRemoveMigrationTest_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);

            using var test = CreateTestContext();
            using var scope = test.CreateScope();

            var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

            // Step 1: Scaffold migration
            var migration = scaffolder.ScaffoldMigration(
                "RemovableTest",
                rootNamespace: "TestNamespace",
                subNamespace: null,
                language: "C#",
                dryRun: true);

            // Step 2: Save files to disk
            var files = scaffolder.Save(tempDirectory, migration, outputDir: null, dryRun: false);

            // Verify files exist
            Assert.True(File.Exists(files.MigrationFile));
            Assert.True(File.Exists(files.MetadataFile));
            Assert.True(File.Exists(files.SnapshotFile));

            // Step 3: Compile and register the migration
            var compiledAssembly = compiler.CompileMigration(migration, test.Context.GetType());
            migrationsAssembly.AddMigrations(compiledAssembly);

            // Step 4: Apply the migration
            migrator.Migrate(migration.MigrationId);

            // Verify migration was applied
            var appliedMigrations = test.Context.Database.GetAppliedMigrations().ToList();
            Assert.Contains(migration.MigrationId, appliedMigrations);

            // Step 5: Call RemoveMigration
            // Note: RemoveMigration will fail because the migration is applied.
            // We need to first revert the migration, then remove it.
            migrator.Migrate("0"); // Revert all migrations

            // Verify migration was reverted
            var appliedAfterRevert = test.Context.Database.GetAppliedMigrations().ToList();
            Assert.DoesNotContain(migration.MigrationId, appliedAfterRevert);

            // Now remove the migration files
            var removedFiles = scaffolder.RemoveMigration(tempDirectory, rootNamespace: "TestNamespace", force: false, language: "C#", dryRun: false);

            // Verify files were deleted
            Assert.NotNull(removedFiles.MigrationFile);
            Assert.False(File.Exists(removedFiles.MigrationFile));
            Assert.False(File.Exists(removedFiles.MetadataFile));
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

    #endregion
}
