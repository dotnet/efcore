// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public class MigrationsScaffolderTest
{
    [ConditionalFact]
    public void ScaffoldMigration_reuses_model_snapshot()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", "WebApplication1");

        Assert.Equal(nameof(ContextWithSnapshotModelSnapshot), migration.SnapshotName);
        Assert.Equal(typeof(ContextWithSnapshotModelSnapshot).Namespace, migration.SnapshotSubnamespace);
    }

    [ConditionalFact]
    public void ScaffoldMigration_handles_generic_contexts()
    {
        var scaffolder = CreateMigrationScaffolder<GenericContext<int>>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", "WebApplication1");

        Assert.Equal("GenericContextModelSnapshot", migration.SnapshotName);
    }

    [ConditionalFact]
    public void ScaffoldMigration_can_override_namespace()
    {
        var scaffolder = CreateMigrationScaffolder<ContextWithSnapshot>();

        var migration = scaffolder.ScaffoldMigration("EmptyMigration", null, "OverrideNamespace.OverrideSubNamespace");

        Assert.Contains("namespace OverrideNamespace.OverrideSubNamespace", migration.MigrationCode);
        Assert.Equal("OverrideNamespace.OverrideSubNamespace", migration.MigrationSubNamespace);

        Assert.Contains("namespace OverrideNamespace.OverrideSubNamespace", migration.SnapshotCode);
        Assert.Equal("OverrideNamespace.OverrideSubNamespace", migration.SnapshotSubnamespace);
    }

    private IMigrationsScaffolder CreateMigrationScaffolder<TContext>()
        where TContext : DbContext, new()
    {
        var currentContext = new CurrentDbContext(new TContext());
        var idGenerator = new MigrationsIdGenerator();
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());
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
                    services.GetRequiredService<CommandBatchPreparerDependencies>()),
                idGenerator,
                new MigrationsCodeGeneratorSelector(
                    new[]
                    {
                        new CSharpMigrationsGenerator(
                            new MigrationsCodeGeneratorDependencies(
                                sqlServerTypeMappingSource,
                                sqlServerAnnotationCodeGenerator),
                            new CSharpMigrationsGeneratorDependencies(
                                code,
                                new CSharpMigrationOperationGenerator(
                                    new CSharpMigrationOperationGeneratorDependencies(
                                        code)),
                                new CSharpSnapshotGenerator(
                                    new CSharpSnapshotGeneratorDependencies(
                                        code, sqlServerTypeMappingSource, sqlServerAnnotationCodeGenerator))))
                    }),
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
                    services.GetRequiredService<IDatabaseProvider>())));
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
        public string GetBeginIfExistsScript(string migrationId)
            => null;

        public string GetBeginIfNotExistsScript(string migrationId)
            => null;

        public string GetCreateScript()
            => null;

        public string GetCreateIfNotExistsScript()
            => null;

        public string GetEndIfScript()
            => null;

        public bool Exists()
            => false;

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
            => Task.FromResult(false);

        public IReadOnlyList<HistoryRow> GetAppliedMigrations()
            => null;

        public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<HistoryRow>>(null);

        public string GetDeleteScript(string migrationId)
            => null;

        public string GetInsertScript(HistoryRow row)
            => null;
    }

    private class MockProvider : IDatabaseProvider
    {
        public string Name
            => "Mock.Provider";

        public bool IsConfigured(IDbContextOptions options)
            => true;
    }
}
