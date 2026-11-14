// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
    public void Save_writes_snapshot_to_output_dir_and_leaves_the_existing_snapshot_alone()
    {
        using var project = new TempDirectory();
        var oldFile = CreateSnapshotFile(project, "Migrations/Old");
        var oldContent = File.ReadAllText(oldFile);
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();
        var migration = scaffolder.ScaffoldMigration("M", "WebApplication1");

        var files = scaffolder.Save(project, migration, "Migrations/New", dryRun: false);

        var newFile = Path.Combine(project, "Migrations", "New", SnapshotFileName);
        Assert.Equal(newFile, files.SnapshotFile);
        Assert.Equal(migration.SnapshotCode, File.ReadAllText(newFile));
        Assert.Equal(oldContent, File.ReadAllText(oldFile));
    }

    [Fact]
    public void Save_overwrites_snapshot_already_in_output_dir()
    {
        using var project = new TempDirectory();
        var existing = CreateSnapshotFile(project, "Migrations/New");
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();
        var migration = scaffolder.ScaffoldMigration("M", "WebApplication1");

        scaffolder.Save(project, migration, "Migrations/New", dryRun: false);

        Assert.Equal(migration.SnapshotCode, File.ReadAllText(existing));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Save_does_not_overwrite_existing_file_when_the_context_has_no_snapshot(bool dryRun)
    {
        using var project = new TempDirectory();
        var otherFile = CreateSnapshotFile(project, "Migrations/New", nameof(GenericContext<int>) + "ModelSnapshot.cs");
        var originalContent = File.ReadAllText(otherFile);
        var scaffolder = CreateMigrationScaffolder<GenericContext<int>>();
        var migration = scaffolder.ScaffoldMigration("M", "WebApplication1");

        var ex = Assert.Throws<OperationException>(
            () => scaffolder.Save(project, migration, "Migrations/New", dryRun));

        Assert.Equal(DesignStrings.SnapshotFileBelongsToAnotherType(otherFile, migration.SnapshotName), ex.Message);
        Assert.Equal(originalContent, File.ReadAllText(otherFile));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(project, "Migrations", "New")));
    }

    [Fact]
    public void Save_dry_run_writes_nothing()
    {
        using var project = new TempDirectory();
        var oldFile = CreateSnapshotFile(project, "Migrations/Old");
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();
        var migration = scaffolder.ScaffoldMigration("M", "WebApplication1");

        var files = scaffolder.Save(project, migration, "Migrations/New", dryRun: true);

        Assert.Equal(Path.Combine(project, "Migrations", "New", SnapshotFileName), files.SnapshotFile);
        Assert.True(File.Exists(oldFile));
        Assert.False(Directory.Exists(Path.Combine(project, "Migrations", "New")));
    }

    private static readonly string SnapshotFileName = nameof(ContextWithSnapshotModelSnapshot) + ".cs";

    private static string CreateSnapshotFile(string projectDir, string relativeDir, string? fileName = null)
    {
        var directory = Path.Combine(projectDir, relativeDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, fileName ?? SnapshotFileName);
        File.WriteAllText(
            file,
            $"// existing snapshot{Environment.NewLine}namespace {typeof(ContextWithSnapshotModelSnapshot).Namespace};"
            + $"{Environment.NewLine}class {Path.GetFileNameWithoutExtension(file)} {{ }}");
        return file;
    }

    private IMigrationsScaffolder CreateMigrationScaffolder<TContext>()
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
        var historyRepository = new MockHistoryRepository();

        var services = FakeRelationalTestHelpers.Instance.CreateContextServices();
        var model = new Model().FinalizeModel();
        model.AddRuntimeAnnotation(RelationalAnnotationNames.RelationalModel, new RelationalModel(model));

        return new MigrationsScaffolder(
            new MigrationsScaffolderDependencies(
                currentContext,
                model,
                migrationAssembly,
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
