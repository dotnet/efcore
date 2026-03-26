// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public class CSharpMigrationCompilerTest
{
    [ConditionalFact]
    public void CompileMigration_compiles_valid_migration_code()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateScaffoldedMigration(
            migrationId: "20231215120000_TestMigration",
            migrationCode: """
                using Microsoft.EntityFrameworkCore.Migrations;

                #nullable disable

                namespace TestNamespace
                {
                    public partial class TestMigration : Migration
                    {
                        protected override void Up(MigrationBuilder migrationBuilder)
                        {
                            migrationBuilder.CreateTable(
                                name: "TestTable",
                                columns: table => new
                                {
                                    Id = table.Column<int>(type: "int", nullable: false)
                                },
                                constraints: table =>
                                {
                                    table.PrimaryKey("PK_TestTable", x => x.Id);
                                });
                        }

                        protected override void Down(MigrationBuilder migrationBuilder)
                        {
                            migrationBuilder.DropTable(name: "TestTable");
                        }
                    }
                }
                """,
            metadataCode: """
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Infrastructure;
                using Microsoft.EntityFrameworkCore.Migrations;

                #nullable disable

                namespace TestNamespace
                {
                    public class TestContext : DbContext { }

                    [DbContext(typeof(TestContext))]
                    [Migration("20231215120000_TestMigration")]
                    partial class TestMigration
                    {
                        protected override void BuildTargetModel(ModelBuilder modelBuilder)
                        {
                            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");
                        }
                    }
                }
                """,
            snapshotCode: """
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Infrastructure;

                #nullable disable

                namespace TestNamespace
                {
                    [DbContext(typeof(TestContext))]
                    partial class TestContextModelSnapshot : ModelSnapshot
                    {
                        protected override void BuildModel(ModelBuilder modelBuilder)
                        {
                            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");
                        }
                    }
                }
                """,
            snapshotName: "TestContextModelSnapshot");

        var assembly = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        Assert.NotNull(assembly);

        // Find the migration type
        var migrationType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);
        Assert.Equal("TestMigration", migrationType.Name);
        Assert.Equal(typeof(Migration), migrationType.BaseType);

        // Verify it has the Migration attribute with correct ID
        var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>();
        Assert.NotNull(migrationAttribute);
        Assert.Equal("20231215120000_TestMigration", migrationAttribute.Id);
    }

    [ConditionalFact]
    public void CompileMigration_finds_snapshot_type()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateValidScaffoldedMigration("20231215140000_SnapshotTest");

        var assembly = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        // Find the snapshot type
        var snapshotType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(ModelSnapshot).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(snapshotType);
        Assert.Equal("TestContextModelSnapshot", snapshotType.Name);
        Assert.Equal(typeof(ModelSnapshot), snapshotType.BaseType);
    }

    [ConditionalFact]
    public void CompileMigration_creates_unique_assembly_per_compilation()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffolded1 = CreateValidScaffoldedMigration("20231215150000_First");
        var scaffolded2 = CreateValidScaffoldedMigration("20231215150001_Second");

        var assembly1 = compiler.CompileMigration(scaffolded1, typeof(TestContext));
        var assembly2 = compiler.CompileMigration(scaffolded2, typeof(TestContext));

        Assert.NotSame(assembly1, assembly2);
        Assert.NotEqual(assembly1.FullName, assembly2.FullName);
    }

    [ConditionalFact]
    public void CompileMigration_throws_on_null_migration()
    {
        var compiler = new CSharpMigrationCompiler();

        Assert.Throws<NullReferenceException>(
            () => compiler.CompileMigration(null!, typeof(TestContext)));
    }

    [ConditionalFact]
    public void CompileMigration_throws_on_null_context_type()
    {
        var compiler = new CSharpMigrationCompiler();
        var scaffoldedMigration = CreateValidScaffoldedMigration("20231215160000_NullContext");

        Assert.Throws<NullReferenceException>(
            () => compiler.CompileMigration(scaffoldedMigration, null!));
    }

    [ConditionalFact]
    public void CompileMigration_throws_on_empty_migration_code()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateScaffoldedMigration(
            migrationId: "20231215170000_EmptyCode",
            migrationCode: "",
            metadataCode: """
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Infrastructure;
                using Microsoft.EntityFrameworkCore.Migrations;

                namespace TestNamespace
                {
                    [DbContext(typeof(TestContext))]
                    [Migration("20231215170000_EmptyCode")]
                    partial class EmptyCode { }
                }
                """,
            snapshotCode: """
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Infrastructure;

                namespace TestNamespace
                {
                    [DbContext(typeof(TestContext))]
                    partial class TestContextModelSnapshot : ModelSnapshot
                    {
                        protected override void BuildModel(ModelBuilder modelBuilder) { }
                    }
                }
                """,
            snapshotName: "TestContextModelSnapshot");

        // Empty migration code results in compilation failure
        var exception = Assert.Throws<InvalidOperationException>(
            () => compiler.CompileMigration(scaffoldedMigration, typeof(TestContext)));

        Assert.Contains("20231215170000_EmptyCode", exception.Message);
    }

    [ConditionalFact]
    public void CompileMigration_handles_unicode_in_migration_name()
    {
        var compiler = new CSharpMigrationCompiler();

        // Unicode characters in migration name should work (e.g., Japanese characters)
        // Note: The class name must be a valid C# identifier, but the migration ID can contain unicode
        var scaffoldedMigration = CreateValidScaffoldedMigration("20231215180000_TestMigration");

        var assembly = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        Assert.NotNull(assembly);
        var migrationType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract);
        Assert.NotNull(migrationType);
    }

    private static ScaffoldedMigration CreateValidScaffoldedMigration(string migrationId)
    {
        var migrationName = migrationId.Split('_').Last();

        var migrationCode = $@"using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestNamespace
{{
    public partial class {migrationName} : Migration
    {{
        protected override void Up(MigrationBuilder migrationBuilder)
        {{
        }}

        protected override void Down(MigrationBuilder migrationBuilder)
        {{
        }}
    }}
}}";

        var metadataCode = $@"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestNamespace
{{
    public class TestContext : DbContext {{ }}

    [DbContext(typeof(TestContext))]
    [Migration(""{migrationId}"")]
    partial class {migrationName}
    {{
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {{
            modelBuilder.HasAnnotation(""ProductVersion"", ""8.0.0"");
        }}
    }}
}}";

        var snapshotCode = @"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace TestNamespace
{
    [DbContext(typeof(TestContext))]
    partial class TestContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation(""ProductVersion"", ""8.0.0"");
        }
    }
}";

        return new ScaffoldedMigration(
            fileExtension: ".cs",
            previousMigrationId: null,
            migrationCode: migrationCode,
            migrationId: migrationId,
            metadataCode: metadataCode,
            migrationSubNamespace: "TestNamespace",
            snapshotCode: snapshotCode,
            snapshotName: "TestContextModelSnapshot",
            snapshotSubNamespace: "TestNamespace");
    }

    private static ScaffoldedMigration CreateScaffoldedMigration(
        string migrationId,
        string migrationCode,
        string metadataCode,
        string snapshotCode,
        string snapshotName)
    {
        return new ScaffoldedMigration(
            fileExtension: ".cs",
            previousMigrationId: null,
            migrationCode: migrationCode,
            migrationId: migrationId,
            metadataCode: metadataCode,
            migrationSubNamespace: "TestNamespace",
            snapshotCode: snapshotCode,
            snapshotName: snapshotName,
            snapshotSubNamespace: "TestNamespace");
    }

    private class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("Test");
    }
}
