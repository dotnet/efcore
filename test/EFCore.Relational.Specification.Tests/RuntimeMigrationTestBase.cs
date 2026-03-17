// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Base class for runtime migration tests. These tests validate the ability to
///     scaffold, compile, and apply migrations at runtime without using the CLI.
/// </summary>
public abstract class RuntimeMigrationTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : RuntimeMigrationTestBase<TFixture>.RuntimeMigrationFixtureBase
{
    protected TFixture Fixture { get; } = fixture;

    public virtual async Task InitializeAsync()
    {
        using var context = CreateContext();
        await Fixture.TestStore.CleanAsync(context, createTables: false);
    }

    public virtual Task DisposeAsync()
        => Task.CompletedTask;

    protected abstract Assembly ProviderAssembly { get; }

    #region Model Classes

    public class Blog
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Post> Posts { get; set; } = [];
    }

    public class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; } = null!;
    }

    public class RuntimeMigrationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(b => b.Property(e => e.Name).HasMaxLength(200));

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

    protected RuntimeMigrationDbContext CreateContext()
        => Fixture.CreateContext();

    protected IServiceScope CreateDesignTimeServices(RuntimeMigrationDbContext context)
    {
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>()!.TypeName,
                    throwOnError: true)!)!)
            .ConfigureDesignTimeServices(serviceCollection);
        return serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();
    }

    protected MigrationsOperations CreateMigrationsOperations()
        => new(
            new TestOperationReporter(),
            typeof(RuntimeMigrationDbContext).Assembly,
            typeof(RuntimeMigrationDbContext).Assembly,
            AppContext.BaseDirectory,
            "TestNamespace",
            "C#",
            nullable: false,
            args: []);

    protected (IServiceScope Scope, ScaffoldedMigration Migration)
        CreateScaffoldedMigration(
            RuntimeMigrationDbContext context,
            string migrationName,
            string? outputDir = null,
            string? @namespace = null,
            bool dryRun = true)
    {
        var operations = CreateMigrationsOperations();
        var services = operations.PrepareForMigration(migrationName, context);
        var scope = services.CreateScope();
        var (migration, _) = operations.CreateScaffoldedMigration(
            migrationName,
            outputDir,
            @namespace,
            dryRun,
            scope.ServiceProvider);

        return (scope, migration);
    }

    protected virtual List<string> GetTableNames(DbConnection connection)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public abstract class RuntimeMigrationFixtureBase : SharedStoreFixtureBase<RuntimeMigrationDbContext>
    {
        protected override string StoreName
            => "RuntimeMigration";

        // Disable DbContext pooling because pooled contexts retain runtime migration assemblies
        // from previous tests, causing subsequent tests to fail
        protected override bool UsePooling
            => false;
    }

    #endregion

    #region Tests

    [ConditionalFact]
    public void Can_scaffold_migration()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "TestMigration");
        using (scope)
        {
            Assert.NotNull(migration);
            Assert.Contains("TestMigration", migration.MigrationId);
            Assert.NotEmpty(migration.MigrationCode);
            Assert.NotEmpty(migration.MetadataCode);
            Assert.NotEmpty(migration.SnapshotCode);
        }
    }

    [ConditionalFact]
    public void Can_compile_migration()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "CompiledMigration");
        using (scope)
        {
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());

            Assert.NotNull(compiledAssembly);

            var migrationType = compiledAssembly.GetTypes()
                .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
            Assert.NotNull(migrationType);
        }
    }

    [ConditionalFact]
    public void Can_register_and_apply_compiled_migration()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "AppliedMigration");
        using (scope)
        {
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
            migrationsAssembly.AddMigrations(compiledAssembly);

            Assert.Contains(migration.MigrationId, migrationsAssembly.Migrations.Keys);

            migrator.Migrate(migration.MigrationId);

            var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
            Assert.Contains(migration.MigrationId, appliedMigrations);
        }
    }

    [ConditionalFact]
    public void Compiled_migration_generates_valid_sql()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "SqlGenMigration");
        using (scope)
        {
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            var sqlGenerator = scope.ServiceProvider.GetRequiredService<IMigrationsSqlGenerator>();

            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
            migrationsAssembly.AddMigrations(compiledAssembly);

            var migrationTypeInfo = migrationsAssembly.Migrations[migration.MigrationId];
            var migrationInstance = migrationsAssembly.CreateMigration(
                migrationTypeInfo,
                context.Database.ProviderName!);

            var commands = sqlGenerator.Generate(
                migrationInstance.UpOperations,
                context.Model).ToList();

            Assert.NotEmpty(commands);
        }
    }

    [ConditionalFact]
    public void HasPendingModelChanges_returns_true_for_new_model()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        Assert.True(migrator.HasPendingModelChanges());
    }

    [ConditionalFact]
    public void HasPendingModelChanges_returns_false_after_migration()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "PendingChangesTest");
        using (scope)
        {
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var migrationsAssembly = scope.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();

            Assert.True(migrator.HasPendingModelChanges());

            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
            migrationsAssembly.AddMigrations(compiledAssembly);
            migrator.Migrate(migration.MigrationId);

            Assert.False(migrator.HasPendingModelChanges());
        }
    }

    [ConditionalFact]
    public void Compiled_migration_contains_correct_operations()
    {
        using var context = CreateContext();
        var (scope, migration) = CreateScaffoldedMigration(context, "OperationsTest");
        using (scope)
        {
            var compiler = scope.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());

            var migrationType = compiledAssembly.GetTypes()
                .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
            Assert.NotNull(migrationType);

            var migrationInstance = (Migration)Activator.CreateInstance(migrationType)!;
            var upOperations = migrationInstance.UpOperations.ToList();

            Assert.NotEmpty(upOperations);
            Assert.Contains(upOperations, op => op is CreateTableOperation);
        }
    }

    [ConditionalFact]
    public void Can_scaffold_and_save_migration_to_disk()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "EFCoreMigrationTest_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);

            using var context = CreateContext();
            var (scope, migration) = CreateScaffoldedMigration(
                context,
                "SaveToDisk",
                @namespace: "TestNamespace.Migrations");
            using (scope)
            {
                var scaffolder = scope.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
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
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var migration1 = scaffolder.ScaffoldMigration(
            "FirstSequential",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        var appliedAfterFirst = context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration1.MigrationId, appliedAfterFirst);

        var migration2 = scaffolder.ScaffoldMigration(
            "SecondSequential",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, context.GetType());
        migrationsAssembly.AddMigrations(assembly2);
        migrator.Migrate(migration2.MigrationId);

        var appliedAfterSecond = context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration1.MigrationId, appliedAfterSecond);
        Assert.Contains(migration2.MigrationId, appliedAfterSecond);
    }

    [ConditionalFact]
    public void Migration_down_reverses_up()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var migration = scaffolder.ScaffoldMigration(
            "SymmetryTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        var connection = context.Database.GetDbConnection();
        context.Database.OpenConnection();
        var tablesAfterUp = GetTableNames(connection);
        Assert.Contains("Blogs", tablesAfterUp);
        Assert.Contains("Posts", tablesAfterUp);
        context.Database.CloseConnection();

        migrator.Migrate("0");

        context.Database.OpenConnection();
        var tablesAfterDown = GetTableNames(connection);
        Assert.DoesNotContain("Blogs", tablesAfterDown);
        Assert.DoesNotContain("Posts", tablesAfterDown);
        context.Database.CloseConnection();
    }

    protected DatabaseModel GetDatabaseModel(IServiceScope services, RuntimeMigrationDbContext context)
    {
        var databaseModelFactory = services.ServiceProvider.GetRequiredService<IDatabaseModelFactory>();
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            context.Database.OpenConnection();
        }

        return databaseModelFactory.Create(connection, new DatabaseModelFactoryOptions());
    }

    protected (ScaffoldedMigration Migration, Assembly CompiledAssembly) ScaffoldAndApplyMigration(
        IServiceScope services,
        RuntimeMigrationDbContext context,
        string migrationName)
    {
        var operations = CreateMigrationsOperations();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var (migration, _) = operations.CreateScaffoldedMigration(
            migrationName,
            outputDir: null,
            @namespace: null,
            dryRun: true,
            services.ServiceProvider);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        return (migration, compiledAssembly);
    }

    [ConditionalFact]
    public void Can_revert_migration_using_down_operations()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();
        var sqlGenerator = services.ServiceProvider.GetRequiredService<IMigrationsSqlGenerator>();
        var commandExecutor = services.ServiceProvider.GetRequiredService<IMigrationCommandExecutor>();
        var connection = services.ServiceProvider.GetRequiredService<IRelationalConnection>();

        var migration = scaffolder.ScaffoldMigration(
            "RevertTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        var appliedBefore = context.Database.GetAppliedMigrations().ToList();
        Assert.Contains(migration.MigrationId, appliedBefore);

        var migrationTypeInfo = migrationsAssembly.Migrations[migration.MigrationId];
        var migrationInstance = migrationsAssembly.CreateMigration(
            migrationTypeInfo,
            context.Database.ProviderName!);

        var downCommands = sqlGenerator.Generate(
            migrationInstance.DownOperations,
            context.Model).ToList();

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
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var initialApplied = context.Database.GetAppliedMigrations().ToList();

        var migration = scaffolder.ScaffoldMigration(
            "HistoryTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
        migrationsAssembly.AddMigrations(compiledAssembly);
        migrator.Migrate(migration.MigrationId);

        var afterApplied = context.Database.GetAppliedMigrations().ToList();
        Assert.Equal(initialApplied.Count + 1, afterApplied.Count);
        Assert.Contains(migration.MigrationId, afterApplied);
    }

    [ConditionalFact]
    public void Compiled_migration_has_matching_up_and_down_table_operations()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();

        var migration = scaffolder.ScaffoldMigration(
            "UpDownTest",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var compiledAssembly = compiler.CompileMigration(migration, context.GetType());

        var migrationType = compiledAssembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);

        var migrationInstance = (Migration)Activator.CreateInstance(migrationType)!;

        var upTableCount = migrationInstance.UpOperations.OfType<CreateTableOperation>().Count();
        var downTableCount = migrationInstance.DownOperations.OfType<DropTableOperation>().Count();
        Assert.Equal(upTableCount, downTableCount);
    }

    #endregion

    #region Rigorous Schema Verification Tests

    [ConditionalFact]
    public void Migration_creates_correct_table_structure()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        ScaffoldAndApplyMigration(services, context, "TableStructureTest");

        var dbModel = GetDatabaseModel(services, context);

        Assert.Contains(dbModel.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModel.Tables, t => t.Name == "Posts");

        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.Equal(2, blogsTable.Columns.Count);
        Assert.Single(blogsTable.Columns, c => c.Name == "Id");
        Assert.Single(blogsTable.Columns, c => c.Name == "Name");

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        Assert.Equal(4, postsTable.Columns.Count);
        Assert.Single(postsTable.Columns, c => c.Name == "Id");
        Assert.Single(postsTable.Columns, c => c.Name == "Title");
        Assert.Single(postsTable.Columns, c => c.Name == "Content");
        Assert.Single(postsTable.Columns, c => c.Name == "BlogId");
    }

    [ConditionalFact]
    public void Migration_creates_correct_primary_keys()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        ScaffoldAndApplyMigration(services, context, "PrimaryKeyTest");

        var dbModel = GetDatabaseModel(services, context);

        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.NotNull(blogsTable.PrimaryKey);
        Assert.Single(blogsTable.PrimaryKey.Columns);
        Assert.Equal("Id", blogsTable.PrimaryKey.Columns[0].Name);

        var blogsId = blogsTable.Columns.Single(c => c.Name == "Id");
        Assert.False(blogsId.IsNullable);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        Assert.NotNull(postsTable.PrimaryKey);
        Assert.Single(postsTable.PrimaryKey.Columns);
        Assert.Equal("Id", postsTable.PrimaryKey.Columns[0].Name);

        var postsId = postsTable.Columns.Single(c => c.Name == "Id");
        Assert.False(postsId.IsNullable);
    }

    [ConditionalFact]
    public void Migration_creates_correct_foreign_keys()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        ScaffoldAndApplyMigration(services, context, "ForeignKeyTest");

        var dbModel = GetDatabaseModel(services, context);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");

        var fk = Assert.Single(postsTable.ForeignKeys);

        Assert.Equal("Blogs", fk.PrincipalTable.Name);

        Assert.Single(fk.Columns);
        Assert.Equal("BlogId", fk.Columns[0].Name);

        Assert.Single(fk.PrincipalColumns);
        Assert.Equal("Id", fk.PrincipalColumns[0].Name);

        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
    }

    [ConditionalFact]
    public void Migration_creates_columns_with_correct_constraints()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        ScaffoldAndApplyMigration(services, context, "ColumnConstraintTest");

        var dbModel = GetDatabaseModel(services, context);

        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        var nameColumn = blogsTable.Columns.Single(c => c.Name == "Name");
        Assert.NotNull(nameColumn.StoreType);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");
        var titleColumn = postsTable.Columns.Single(c => c.Name == "Title");
        Assert.NotNull(titleColumn.StoreType);

        var contentColumn = postsTable.Columns.Single(c => c.Name == "Content");
        Assert.True(contentColumn.IsNullable);

        var blogIdColumn = postsTable.Columns.Single(c => c.Name == "BlogId");
        Assert.False(blogIdColumn.IsNullable);
    }

    [ConditionalFact]
    public void Migration_down_removes_schema_completely()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        ScaffoldAndApplyMigration(services, context, "CompleteRemovalTest");

        var dbModelBefore = GetDatabaseModel(services, context);

        Assert.Contains(dbModelBefore.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModelBefore.Tables, t => t.Name == "Posts");

        var postsTableBefore = dbModelBefore.Tables.Single(t => t.Name == "Posts");
        Assert.Single(postsTableBefore.ForeignKeys);

        migrator.Migrate("0");

        var dbModelAfter = GetDatabaseModel(services, context);

        Assert.DoesNotContain(dbModelAfter.Tables, t => t.Name == "Blogs");
        Assert.DoesNotContain(dbModelAfter.Tables, t => t.Name == "Posts");
    }

    [ConditionalFact]
    public void Migration_creates_foreign_key_index()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        ScaffoldAndApplyMigration(services, context, "FKIndexTest");

        var dbModel = GetDatabaseModel(services, context);

        var postsTable = dbModel.Tables.Single(t => t.Name == "Posts");

        var fkIndex = postsTable.Indexes.FirstOrDefault(i =>
            i.Columns.Any(c => c.Name == "BlogId"));

        Assert.NotNull(fkIndex);
        Assert.Single(fkIndex.Columns);
        Assert.Equal("BlogId", fkIndex.Columns[0].Name);
    }

    [ConditionalFact]
    public void Migration_with_no_changes_produces_empty_operations()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var migration1 = scaffolder.ScaffoldMigration(
            "InitialMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        var migration2 = scaffolder.ScaffoldMigration(
            "EmptyMigration",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, context.GetType());

        var migrationType = assembly2.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);

        var migrationInstance = (Migration)Activator.CreateInstance(migrationType)!;

        Assert.Empty(migrationInstance.UpOperations);
        Assert.Empty(migrationInstance.DownOperations);
    }

    [ConditionalFact]
    public void Migration_preserves_existing_data()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
        var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
        var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        var migration1 = scaffolder.ScaffoldMigration(
            "DataPreservationInit",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly1 = compiler.CompileMigration(migration1, context.GetType());
        migrationsAssembly.AddMigrations(assembly1);
        migrator.Migrate(migration1.MigrationId);

        context.Blogs.Add(new Blog { Name = "Test Blog" });
        context.SaveChanges();

        var blogId = context.Blogs.Single().Id;
        context.Posts.Add(new Post
        {
            Title = "Test Post",
            Content = "Test Content",
            BlogId = blogId
        });
        context.SaveChanges();

        Assert.Equal(1, context.Blogs.Count());
        Assert.Equal(1, context.Posts.Count());

        var migration2 = scaffolder.ScaffoldMigration(
            "DataPreservationCheck",
            rootNamespace: "TestNamespace",
            subNamespace: null,
            language: "C#",
            dryRun: true);

        var assembly2 = compiler.CompileMigration(migration2, context.GetType());
        migrationsAssembly.AddMigrations(assembly2);
        migrator.Migrate(migration2.MigrationId);

        Assert.Equal(1, context.Blogs.Count());
        Assert.Equal(1, context.Posts.Count());

        var blog = context.Blogs.Single();
        Assert.Equal("Test Blog", blog.Name);

        var post = context.Posts.Single();
        Assert.Equal("Test Post", post.Title);
        Assert.Equal("Test Content", post.Content);
    }

    [ConditionalFact]
    public void Applied_migration_snapshot_matches_model()
    {
        using var context = CreateContext();
        using var services = CreateDesignTimeServices(context);

        var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

        Assert.True(migrator.HasPendingModelChanges());

        ScaffoldAndApplyMigration(services, context, "SnapshotMatchTest");

        Assert.False(migrator.HasPendingModelChanges());

        var dbModel = GetDatabaseModel(services, context);

        Assert.Contains(dbModel.Tables, t => t.Name == "Blogs");
        Assert.Contains(dbModel.Tables, t => t.Name == "Posts");

        var blogsTable = dbModel.Tables.Single(t => t.Name == "Blogs");
        Assert.Equal(2, blogsTable.Columns.Count);

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

            using var context = CreateContext();
            using var services = CreateDesignTimeServices(context);

            var scaffolder = services.ServiceProvider.GetRequiredService<IMigrationsScaffolder>();
            var compiler = services.ServiceProvider.GetRequiredService<IMigrationCompiler>();
            var migrationsAssembly = services.ServiceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrator = services.ServiceProvider.GetRequiredService<IMigrator>();

            var migration = scaffolder.ScaffoldMigration(
                "RemovableTest",
                rootNamespace: "TestNamespace",
                subNamespace: null,
                language: "C#",
                dryRun: true);

            var files = scaffolder.Save(tempDirectory, migration, outputDir: null, dryRun: false);

            Assert.True(File.Exists(files.MigrationFile));
            Assert.True(File.Exists(files.MetadataFile));
            Assert.True(File.Exists(files.SnapshotFile));

            var compiledAssembly = compiler.CompileMigration(migration, context.GetType());
            migrationsAssembly.AddMigrations(compiledAssembly);

            migrator.Migrate(migration.MigrationId);

            var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
            Assert.Contains(migration.MigrationId, appliedMigrations);

            migrator.Migrate("0");

            var appliedAfterRevert = context.Database.GetAppliedMigrations().ToList();
            Assert.DoesNotContain(migration.MigrationId, appliedAfterRevert);

            var removedFiles = scaffolder.RemoveMigration(tempDirectory, rootNamespace: "TestNamespace", force: false, language: "C#", dryRun: false);

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
