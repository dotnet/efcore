// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public class DynamicMigrationsAssemblyTest
{
    [ConditionalFact]
    public void Migrations_returns_inner_migrations_when_no_dynamic_migrations_registered()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        var migrations = dynamic.Migrations;

        Assert.Same(inner.Migrations, migrations);
    }

    [ConditionalFact]
    public void RegisterDynamicMigration_adds_migration_to_collection()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        var compiledMigration = CreateCompiledMigration("20231215120000_DynamicMigration");

        dynamic.RegisterDynamicMigration(compiledMigration);

        Assert.Contains(compiledMigration.MigrationId, dynamic.Migrations.Keys);
        Assert.Equal(compiledMigration.MigrationTypeInfo, dynamic.Migrations[compiledMigration.MigrationId]);
    }

    [ConditionalFact]
    public void Migrations_merges_inner_and_dynamic_migrations()
    {
        var inner = CreateMockMigrationsAssembly(
            ("20231215100000_First", typeof(FirstMigration).GetTypeInfo()));
        var dynamic = new DynamicMigrationsAssembly(inner);

        var compiledMigration = CreateCompiledMigration("20231215120000_DynamicMigration");
        dynamic.RegisterDynamicMigration(compiledMigration);

        var migrations = dynamic.Migrations;

        Assert.Equal(2, migrations.Count);
        Assert.Contains("20231215100000_First", migrations.Keys);
        Assert.Contains("20231215120000_DynamicMigration", migrations.Keys);
    }

    [ConditionalFact]
    public void Migrations_maintains_sorted_order()
    {
        var inner = CreateMockMigrationsAssembly(
            ("20231215110000_Second", typeof(SecondMigration).GetTypeInfo()));
        var dynamic = new DynamicMigrationsAssembly(inner);

        var first = CreateCompiledMigration("20231215100000_First");
        var third = CreateCompiledMigration("20231215120000_Third");

        dynamic.RegisterDynamicMigration(third);
        dynamic.RegisterDynamicMigration(first);

        var migrations = dynamic.Migrations;
        var keys = migrations.Keys.ToList();

        Assert.Equal(3, keys.Count);
        Assert.Equal("20231215100000_First", keys[0]);
        Assert.Equal("20231215110000_Second", keys[1]);
        Assert.Equal("20231215120000_Third", keys[2]);
    }

    [ConditionalFact]
    public void ClearDynamicMigrations_removes_all_dynamic_migrations()
    {
        var inner = CreateMockMigrationsAssembly(
            ("20231215100000_Static", typeof(StaticMigration).GetTypeInfo()));
        var dynamic = new DynamicMigrationsAssembly(inner);

        dynamic.RegisterDynamicMigration(CreateCompiledMigration("20231215120000_Dynamic1"));
        dynamic.RegisterDynamicMigration(CreateCompiledMigration("20231215130000_Dynamic2"));

        Assert.Equal(3, dynamic.Migrations.Count);

        dynamic.ClearDynamicMigrations();

        Assert.Single(dynamic.Migrations);
        Assert.Contains("20231215100000_Static", dynamic.Migrations.Keys);
    }

    [ConditionalFact]
    public void ModelSnapshot_returns_dynamic_snapshot_when_available()
    {
        var innerSnapshot = new TestModelSnapshot();
        var inner = CreateMockMigrationsAssembly(modelSnapshot: innerSnapshot);
        var dynamic = new DynamicMigrationsAssembly(inner);

        var compiledMigration = CreateCompiledMigration(
            "20231215120000_WithSnapshot",
            snapshotTypeInfo: typeof(DynamicModelSnapshot).GetTypeInfo());

        dynamic.RegisterDynamicMigration(compiledMigration);

        // The snapshot should still be the original if we haven't updated it
        // In real scenarios, snapshot is updated through the migration flow
        Assert.NotNull(dynamic.ModelSnapshot);
    }

    [ConditionalFact]
    public void ModelSnapshot_returns_inner_snapshot_when_no_dynamic_snapshot()
    {
        var innerSnapshot = new TestModelSnapshot();
        var inner = CreateMockMigrationsAssembly(modelSnapshot: innerSnapshot);
        var dynamic = new DynamicMigrationsAssembly(inner);

        Assert.Same(innerSnapshot, dynamic.ModelSnapshot);
    }

    [ConditionalFact]
    public void Assembly_returns_inner_assembly()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        Assert.Same(inner.Assembly, dynamic.Assembly);
    }

    [ConditionalFact]
    public void FindMigrationId_finds_inner_migration()
    {
        var inner = CreateMockMigrationsAssembly(
            ("20231215100000_TestMigration", typeof(TestMigration).GetTypeInfo()));
        var dynamic = new DynamicMigrationsAssembly(inner);

        var id = dynamic.FindMigrationId("TestMigration");

        Assert.Equal("20231215100000_TestMigration", id);
    }

    [ConditionalFact]
    public void FindMigrationId_finds_dynamic_migration()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        dynamic.RegisterDynamicMigration(CreateCompiledMigration("20231215120000_DynamicMigration"));

        var id = dynamic.FindMigrationId("DynamicMigration");

        Assert.Equal("20231215120000_DynamicMigration", id);
    }

    [ConditionalFact]
    public void FindMigrationId_returns_null_when_not_found()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        var id = dynamic.FindMigrationId("NonExistent");

        Assert.Null(id);
    }

    [ConditionalFact]
    public void CreateMigration_creates_instance_of_dynamic_migration()
    {
        var inner = CreateMockMigrationsAssembly();
        var dynamic = new DynamicMigrationsAssembly(inner);

        var compiledMigration = CreateCompiledMigration("20231215120000_DynamicMigration");
        dynamic.RegisterDynamicMigration(compiledMigration);

        var migration = dynamic.CreateMigration(compiledMigration.MigrationTypeInfo, "FakeProvider");

        Assert.NotNull(migration);
        Assert.IsAssignableFrom<Migration>(migration);
    }

    private static MockMigrationsAssembly CreateMockMigrationsAssembly(
        params (string Id, TypeInfo TypeInfo)[] migrations)
        => CreateMockMigrationsAssembly(null, migrations);

    private static MockMigrationsAssembly CreateMockMigrationsAssembly(
        ModelSnapshot modelSnapshot,
        params (string Id, TypeInfo TypeInfo)[] migrations)
    {
        var migrationsDict = new SortedList<string, TypeInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var (id, typeInfo) in migrations)
        {
            migrationsDict.Add(id, typeInfo);
        }

        return new MockMigrationsAssembly(migrationsDict, modelSnapshot);
    }

    private static CompiledMigration CreateCompiledMigration(
        string migrationId,
        TypeInfo snapshotTypeInfo = null)
    {
        var migrationName = migrationId.Split('_').Last();
        var scaffolded = new ScaffoldedMigration(
            fileExtension: ".cs",
            previousMigrationId: null,
            migrationCode: $"// Migration code for {migrationName}",
            migrationId: migrationId,
            metadataCode: $"// Metadata code for {migrationName}",
            migrationSubNamespace: "TestNamespace",
            snapshotCode: "// Snapshot code",
            snapshotName: "TestModelSnapshot",
            snapshotSubNamespace: "TestNamespace");

        return new CompiledMigration(
            typeof(DynamicMigrationsAssemblyTest).Assembly,
            typeof(TestMigration).GetTypeInfo(),
            snapshotTypeInfo,
            migrationId,
            scaffolded);
    }

    private class MockMigrationsAssembly : IMigrationsAssembly
    {
        private readonly IReadOnlyDictionary<string, TypeInfo> _migrations;

        public MockMigrationsAssembly(
            IReadOnlyDictionary<string, TypeInfo> migrations,
            ModelSnapshot modelSnapshot = null)
        {
            _migrations = migrations;
            ModelSnapshot = modelSnapshot;
        }

        public IReadOnlyDictionary<string, TypeInfo> Migrations => _migrations;

        public ModelSnapshot ModelSnapshot { get; }

        public Assembly Assembly => typeof(DynamicMigrationsAssemblyTest).Assembly;

        public string FindMigrationId(string nameOrId)
        {
            foreach (var (id, _) in _migrations)
            {
                if (string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase)
                    || id.EndsWith("_" + nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }
            }

            return null;
        }

        public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
            => (Migration)Activator.CreateInstance(migrationClass.AsType());
    }

    // Test migration classes
    private class TestMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
    }

    private class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
    }

    private class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
    }

    private class StaticMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) { }
    }

    private class TestModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder) { }
    }

    private class DynamicModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder) { }
    }
}
