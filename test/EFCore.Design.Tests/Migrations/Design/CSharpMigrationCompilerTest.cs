// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        var compiled = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        Assert.NotNull(compiled);
        Assert.NotNull(compiled.Assembly);
        Assert.NotNull(compiled.MigrationTypeInfo);
        Assert.Equal("20231215120000_TestMigration", compiled.MigrationId);
        Assert.Equal(typeof(Migration), compiled.MigrationTypeInfo.BaseType);
        Assert.Equal("TestMigration", compiled.MigrationTypeInfo.Name);
    }

    [ConditionalFact]
    public void CompileMigration_throws_on_invalid_code()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateScaffoldedMigration(
            migrationId: "20231215120000_InvalidMigration",
            migrationCode: """
                using Microsoft.EntityFrameworkCore.Migrations;

                namespace TestNamespace
                {
                    // Invalid C# - missing class body
                    public partial class InvalidMigration : Migration
                """,
            metadataCode: """
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Infrastructure;
                using Microsoft.EntityFrameworkCore.Migrations;

                namespace TestNamespace
                {
                    [DbContext(typeof(TestContext))]
                    [Migration("20231215120000_InvalidMigration")]
                    partial class InvalidMigration { }
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

        var exception = Assert.Throws<InvalidOperationException>(
            () => compiler.CompileMigration(scaffoldedMigration, typeof(TestContext)));

        Assert.Contains("20231215120000_InvalidMigration", exception.Message);
    }

    [ConditionalFact]
    public void CompileMigration_includes_migration_source_code_in_result()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateValidScaffoldedMigration("20231215130000_SourceCodeTest");

        var compiled = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        Assert.NotNull(compiled.SourceCode);
        Assert.Same(scaffoldedMigration, compiled.SourceCode);
        Assert.Equal(scaffoldedMigration.MigrationCode, compiled.SourceCode.MigrationCode);
    }

    [ConditionalFact]
    public void CompileMigration_finds_snapshot_type()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffoldedMigration = CreateValidScaffoldedMigration("20231215140000_SnapshotTest");

        var compiled = compiler.CompileMigration(scaffoldedMigration, typeof(TestContext));

        Assert.NotNull(compiled.SnapshotTypeInfo);
        Assert.Equal("TestContextModelSnapshot", compiled.SnapshotTypeInfo.Name);
        Assert.Equal(typeof(ModelSnapshot), compiled.SnapshotTypeInfo.BaseType);
    }

    [ConditionalFact]
    public void CompileMigration_creates_unique_assembly_per_compilation()
    {
        var compiler = new CSharpMigrationCompiler();

        var scaffolded1 = CreateValidScaffoldedMigration("20231215150000_First");
        var scaffolded2 = CreateValidScaffoldedMigration("20231215150001_Second");

        var compiled1 = compiler.CompileMigration(scaffolded1, typeof(TestContext));
        var compiled2 = compiler.CompileMigration(scaffolded2, typeof(TestContext));

        Assert.NotSame(compiled1.Assembly, compiled2.Assembly);
        Assert.NotEqual(compiled1.Assembly.FullName, compiled2.Assembly.FullName);
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
