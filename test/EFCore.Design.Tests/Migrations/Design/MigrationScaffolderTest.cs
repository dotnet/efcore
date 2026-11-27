// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public class MigrationsScaffolderTest
{
    [Fact]
    public void ScaffoldMigration_reuses_model_snapshot()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", "WebApplication1");

        Assert.Equal(nameof(ContextWithSnapshotModelSnapshot), migration.SnapshotName);
        Assert.Equal(typeof(ContextWithSnapshotModelSnapshot).Namespace, migration.SnapshotSubnamespace);
    }

    [Fact]
    public void ScaffoldMigration_handles_generic_contexts()
    {
        var scaffolder = CreateMigrationScaffolder<GenericContext<int>>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", "WebApplication1");

        Assert.Equal("GenericContextModelSnapshot", migration.SnapshotName);
    }

    [Fact]
    public void ScaffoldMigration_can_override_namespace()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", null, "OverrideNamespace.OverrideSubNamespace");

        Assert.Contains("namespace OverrideNamespace.OverrideSubNamespace", migration.MigrationCode);
        Assert.Equal("OverrideNamespace.OverrideSubNamespace", migration.MigrationSubNamespace);

        Assert.Contains("namespace OverrideNamespace.OverrideSubNamespace", migration.SnapshotCode);
        Assert.Equal("OverrideNamespace.OverrideSubNamespace", migration.SnapshotSubnamespace);
    }

    [Fact]
    public void ScaffoldMigration_uses_migration_id_as_type_name()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();

        var migration = scaffolder.ScaffoldMigration("DateTime", "WebApplication1");

        Assert.Contains($"public partial class _{migration.MigrationId} : Migration", migration.MigrationCode);
        Assert.Contains($"partial class _{migration.MigrationId}", migration.MetadataCode);
        Assert.Contains($"[Migration(\"{migration.MigrationId}\")]", migration.MetadataCode);
    }

    [Fact]
    public void ScaffoldMigration_diffs_against_snapshot_finalized_by_migrator()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithTableSnapshot>(out var migrationsAssembly);

        // Migrator.HasPendingModelChanges() and Migrate() finalize the model of the snapshot instance that the context's
        // IMigrationsAssembly caches, which is the instance the scaffolder reads.
        ((IMutableModel)migrationsAssembly.ModelSnapshot!.Model).FinalizeModel();

        var migration = scaffolder.ScaffoldMigration("DropBlogs", "WebApplication1");

        Assert.Contains("migrationBuilder.DropTable(", migration.MigrationCode);
        Assert.Contains("name: \"Blogs\"", migration.MigrationCode);
    }

    [Fact]
    public void RemoveMigration_works_with_snapshot_finalized_by_migrator()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithMigration>(out var migrationsAssembly);

        ((IMutableModel)migrationsAssembly.ModelSnapshot!.Model).FinalizeModel();

        var projectDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var files = scaffolder.RemoveMigration(projectDir, rootNamespace: null, force: false, language: null, dryRun: true, offline: true);

        Assert.Null(files.MigrationFile);
        Assert.Null(files.SnapshotFile);
    }

    [Fact]
    public void ScaffoldMigration_uses_snapshot_without_parameterless_constructor_when_finalized()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithTableSnapshot>(
            out var migrationsAssembly,
            _ => new FakeMigrationsAssembly(
                new ConstructorStateModelSnapshot(
                    modelBuilder => modelBuilder.Entity(
                        "Blog", b =>
                        {
                            b.Property<int>("Id");
                            b.HasKey("Id");
                            b.ToTable("Blogs");
                        }))));

        ((IMutableModel)migrationsAssembly.ModelSnapshot!.Model).FinalizeModel();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", "WebApplication1");

        Assert.Equal(nameof(ConstructorStateModelSnapshot), migration.SnapshotName);
    }

    [Fact]
    public void ScaffoldMigration_diffs_against_snapshot_with_constructor_state()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithTableSnapshot>(
            out _,
            _ => new FakeMigrationsAssembly(
                new ConstructorStateModelSnapshot(
                    modelBuilder => modelBuilder.Entity(
                        "Blog", b =>
                        {
                            b.Property<int>("Id");
                            b.HasKey("Id");
                            b.ToTable("Blogs");
                        }))));

        var migration = scaffolder.ScaffoldMigration("DropBlogs", "WebApplication1");

        Assert.Contains("migrationBuilder.DropTable(", migration.MigrationCode);
    }

    private IMigrationsScaffolder CreateMigrationScaffolder<TContext>()
        where TContext : DbContext, new()
        =>CreateMigrationScaffolder<TContext>(out _);

    private IMigrationsScaffolder CreateMigrationScaffolder<TContext>(
        out IMigrationsAssembly migrationsAssembly,
        Func<IMigrationsAssembly, IMigrationsAssembly>? decorateMigrationsAssembly = null)
        where TContext : DbContext, new()
    {
        var currentContext = new CurrentDbContext(new TContext());
        var idGenerator = new MigrationsIdGenerator();
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<SqlServerSingletonOptions>());
        var sqlServerAnnotationCodeGenerator = new SqlServerAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqlServerTypeMappingSource));
        var code = new CSharpHelper(sqlServerTypeMappingSource);
        var reporter = new TestOperationReporter();
        var migrationAssembly
            = new MigrationsAssembly(
                currentContext,
                new DbContextOptions<TContext>().WithExtension(new FakeRelationalOptionsExtension()),
                idGenerator,
                new FakeDiagnosticsLogger<DbLoggerCategory.Migrations>());
        migrationsAssembly = decorateMigrationsAssembly?.Invoke(migrationAssembly) ?? migrationAssembly;
        var scaffolderMigrationsAssembly = migrationsAssembly;
        var historyRepository = new MockHistoryRepository();

        var services = FakeRelationalTestHelpers.Instance.CreateContextServices();
        var model = new Model().FinalizeModel();
        model.AddRuntimeAnnotation(RelationalAnnotationNames.RelationalModel, new RelationalModel(model));

        return new MigrationsScaffolder(
            new MigrationsScaffolderDependencies(
                currentContext,
                model,
                scaffolderMigrationsAssembly,
                new MigrationsModelDiffer(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                    new MigrationsAnnotationProvider(
                        new MigrationsAnnotationProviderDependencies()),
                    new RelationalAnnotationProvider(
                        new RelationalAnnotationProviderDependencies()),
                    services.GetRequiredService<IRowIdentityMapFactory>(),
                    services.GetRequiredService<CommandBatchPreparerDependencies>(),
                    new FakeDiagnosticsLogger<DbLoggerCategory.Migrations>()),
                idGenerator,
                new MigrationsCodeGeneratorSelector(
                [
                    new CSharpMigrationsGenerator(
                        new MigrationsCodeGeneratorDependencies(
                            sqlServerAnnotationCodeGenerator),
                        new CSharpMigrationsGeneratorDependencies(
                            code,
                            new CSharpMigrationOperationGenerator(
                                new CSharpMigrationOperationGeneratorDependencies(
                                    code)),
                            new CSharpSnapshotGenerator(
                                new CSharpSnapshotGeneratorDependencies(
                                    code, sqlServerTypeMappingSource, sqlServerAnnotationCodeGenerator))))
                ]),
                historyRepository,
                reporter,
                new MockProvider(),
                new SnapshotModelProcessor(reporter, services.GetRequiredService<IModelRuntimeInitializer>()),
                new Migrator(
                    migrationAssembly,
                    historyRepository,
                    services.GetRequiredService<IDatabaseCreator>(),
                    services.GetRequiredService<IMigrationsSqlGenerator>(),
                    services.GetRequiredService<IRawSqlCommandBuilder>(),
                    services.GetRequiredService<IMigrationCommandExecutor>(),
                    services.GetRequiredService<IRelationalConnection>(),
                    services.GetRequiredService<ISqlGenerationHelper>(),
                    services.GetRequiredService<ICurrentDbContext>(),
                    services.GetRequiredService<IModelRuntimeInitializer>(),
                    services.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Migrations>>(),
                    services.GetRequiredService<IRelationalCommandDiagnosticsLogger>(),
                    services.GetRequiredService<IDatabaseProvider>(),
                    services.GetRequiredService<IMigrationsModelDiffer>(),
                    services.GetRequiredService<IDesignTimeModel>(),
                    services.GetRequiredService<IDbContextOptions>(),
                    services.GetRequiredService<IExecutionStrategy>())));
    }

    // ReSharper disable once UnusedTypeParameter
    private class GenericContext<T> : DbContext;

    private class ContextWithSnapshot : DbContext;

    [DbContext(typeof(ContextWithSnapshot))]
    private class ContextWithSnapshotModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
        }
    }

    private class ContextWithTableSnapshot : DbContext;

    [DbContext(typeof(ContextWithTableSnapshot))]
    private class ContextWithTableSnapshotModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
            => modelBuilder.Entity(
                "Blog", b =>
                {
                    b.Property<int>("Id");
                    b.HasKey("Id");
                    b.ToTable("Blogs");
                });
    }

    private class ContextWithMigration : DbContext;

    [DbContext(typeof(ContextWithMigration))]
    private class ContextWithMigrationModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
            => ContextWithMigrationInitial.BuildBlogs(modelBuilder);
    }

    [DbContext(typeof(ContextWithMigration))]
    [Migration("20200101000000_Initial")]
    private class ContextWithMigrationInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
            => BuildBlogs(modelBuilder);

        public static void BuildBlogs(ModelBuilder modelBuilder)
            => modelBuilder.Entity(
                "Blog", b =>
                {
                    b.Property<int>("Id");
                    b.HasKey("Id");
                    b.ToTable("Blogs");
                });
    }

    private class ConstructorStateModelSnapshot(Action<ModelBuilder> buildModel) : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
            => buildModel(modelBuilder);
    }

    private class FakeMigrationsAssembly(ModelSnapshot modelSnapshot) : IMigrationsAssembly
    {
        public IReadOnlyDictionary<string, TypeInfo> Migrations { get; } = new Dictionary<string, TypeInfo>();

        public ModelSnapshot? ModelSnapshot { get; } = modelSnapshot;

        public Assembly Assembly => typeof(FakeMigrationsAssembly).Assembly;

        public string? FindMigrationId(string nameOrId)
            => null;

        public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
            => throw new NotSupportedException();

        public void AddMigrations(Assembly additionalMigrationsAssembly)
            => throw new NotSupportedException();
    }

    private class MockHistoryRepository : IHistoryRepository
    {
        public virtual LockReleaseBehavior LockReleaseBehavior
            => LockReleaseBehavior.Explicit;

        public string GetBeginIfExistsScript(string migrationId)
            => null!;

        public string GetBeginIfNotExistsScript(string migrationId)
            => null!;

        public string GetCreateScript()
            => null!;

        public string GetCreateIfNotExistsScript()
            => null!;

        public string GetEndIfScript()
            => null!;

        public bool Exists()
            => false;

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
            => Task.FromResult(false);

        public IReadOnlyList<HistoryRow> GetAppliedMigrations()
            => null!;

        public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<HistoryRow>>(null!);

        public string GetDeleteScript(string migrationId)
            => null!;

        public string GetInsertScript(HistoryRow row)
            => null!;

        public void Create()
            => throw new NotImplementedException();

        public Task CreateAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IMigrationsDatabaseLock AcquireDatabaseLock()
            => throw new NotImplementedException();

        public Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private class MockProvider : IDatabaseProvider
    {
        public string Name
            => "Mock.Provider";

        public bool IsConfigured(IDbContextOptions options)
            => true;
    }
}
