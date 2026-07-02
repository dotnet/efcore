// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public partial class CSharpMigrationsGeneratorTest : CSharpMigrationsGeneratorTestBase
{
    private static readonly string _nl = Environment.NewLine;
    private static readonly string _toTable = _nl + @"entityTypeBuilder.ToTable(""WithAnnotations"")";
    private static readonly string _toNullTable = _nl + @"entityTypeBuilder.ToTable((string)null)";

    [Fact]
    public void Snapshot_handles_all_known_annotation_names()
    {
        // Sanity check that catches when a new annotation is added to CoreAnnotationNames or
        // RelationalAnnotationNames but CSharpSnapshotGenerator cannot process it. Setting each
        // annotation as a raw string on the model and asking the generator to produce a snapshot
        // exercises every code path that filters, recognises, or falls back on annotations — a hard
        // cast or unhandled switch on a newly-added annotation will throw, and an annotation that
        // was forgotten in *AnnotationNames.AllNames is also caught by the existing AllNames
        // membership tests elsewhere.
        var generator = CreateMigrationsCodeGenerator();
        var allNames = CoreAnnotationNames.AllNames
            .Concat(RelationalAnnotationNames.AllNames)
            .Distinct()
            .ToList();

        foreach (var annotationName in allNames)
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder(
                configureConventions: c => c.RemoveAllConventions());
            modelBuilder.HasAnnotation(annotationName, value: null);

            // Some annotations are stamped onto every model by convention; surfacing them here is
            // fine because Generate must tolerate null-valued annotations.
            var finalizedModel = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);

            try
            {
                _ = generator.GenerateSnapshot("MyNamespace", typeof(MyContext), "MySnapshot", finalizedModel);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Annotation '{annotationName}' was not handled by CSharpSnapshotGenerator: {ex.Message}");
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class WithAnnotations
    {
        public int Id { get; set; }
    }

    private class Derived : WithAnnotations;

    [Fact]
    public void Snapshot_with_enum_discriminator_uses_converted_values()
    {
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<SqlServerSingletonOptions>());

        var codeHelper = new CSharpHelper(
            sqlServerTypeMappingSource);

        var sqlServerAnnotationCodeGenerator = new SqlServerAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqlServerTypeMappingSource));

        var generator = new CSharpMigrationsGenerator(
            new MigrationsCodeGeneratorDependencies(
                sqlServerAnnotationCodeGenerator),
            new CSharpMigrationsGeneratorDependencies(
                codeHelper,
                new CSharpMigrationOperationGenerator(
                    new CSharpMigrationOperationGeneratorDependencies(
                        codeHelper)),
                new CSharpSnapshotGenerator(
                    new CSharpSnapshotGeneratorDependencies(
                        codeHelper, sqlServerTypeMappingSource, sqlServerAnnotationCodeGenerator))));

        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        modelBuilder.Entity<WithAnnotations>(eb =>
        {
            eb.HasDiscriminator<RawEnum>("EnumDiscriminator")
                .HasValue(RawEnum.A)
                .HasValue<Derived>(RawEnum.B);
            eb.Property<RawEnum>("EnumDiscriminator").HasConversion<int>();
        });

        var finalizedModel = modelBuilder.FinalizeModel(designTime: true);

        var modelSnapshotCode = generator.GenerateSnapshot(
            "MyNamespace",
            typeof(MyContext),
            "MySnapshot",
            finalizedModel);

        var snapshotModel = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot", typeof(MyContext)).Model;

        Assert.Equal((int)RawEnum.A, snapshotModel.FindEntityType(typeof(WithAnnotations)).GetDiscriminatorValue());
        Assert.Equal((int)RawEnum.B, snapshotModel.FindEntityType(typeof(Derived)).GetDiscriminatorValue());
    }

    [Fact]
    public void Migrations_compile()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migrationCode = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new SqlOperation { Sql = "-- TEST", ["Some:EnumValue"] = RegexOptions.Multiline },
                new AlterColumnOperation
                {
                    Name = "C2",
                    Table = "T1",
                    ClrType = typeof(Database),
                    OldColumn = new AddColumnOperation { ClrType = typeof(Property) }
                },
                new AddColumnOperation
                {
                    Name = "C3",
                    Table = "T1",
                    ClrType = typeof(PropertyEntry)
                },
                new InsertDataOperation
                {
                    Table = "T1",
                    Columns = ["Id", "C2", "C3"],
                    Values = new object[,] { { 1, null, -1 } }
                }
            ],
            []);
        Assert.Equal(
            """
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

#nullable disable

namespace MyNamespace;

/// <inheritdoc />
public partial class MyMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("-- TEST")
            .Annotation("Some:EnumValue", RegexOptions.Multiline);

        migrationBuilder.AlterColumn<Database>(
            name: "C2",
            table: "T1",
            nullable: false,
            oldClrType: typeof(Property));

        migrationBuilder.AddColumn<PropertyEntry>(
            name: "C3",
            table: "T1",
            nullable: false);

        migrationBuilder.InsertData(
            table: "T1",
            columns: new[] { "Id", "C2", "C3" },
            values: new object[] { 1, null, -1 });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}

""",
            migrationCode,
            ignoreLineEndingDifferences: true);

        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder(configureConventions: c => c.RemoveAllConventions());
        modelBuilder.HasAnnotation("Some:EnumValue", RegexOptions.Multiline);
        modelBuilder.HasAnnotation(RelationalAnnotationNames.DbFunctions, new Dictionary<string, IDbFunction>());
        modelBuilder.Entity(
            "T1", eb =>
            {
                eb.Property<int>("Id");
                eb.Property<string>("C2").IsRequired();
                eb.Property<int>("C3");
                eb.HasKey("Id");
            });
        modelBuilder.HasAnnotation(CoreAnnotationNames.ProductVersion, null);

        var finalizedModel = modelBuilder.FinalizeModel(designTime: true);

        var migrationMetadataCode = generator.GenerateMetadata(
            "MyNamespace",
            typeof(MyContext),
            "MyMigration",
            "20150511161616_MyMigration",
            finalizedModel);
        Assert.Equal(
            """
// <auto-generated />
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MyNamespace;

[DbContext(typeof(CSharpMigrationsGeneratorTest.MyContext))]
[Migration("20150511161616_MyMigration")]
partial class MyMigration
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("Some:EnumValue", RegexOptions.Multiline);

        modelBuilder.Entity("T1", b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("int");

                b.Property<string>("C2")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<int>("C3")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.ToTable("T1");
            });
#pragma warning restore 612, 618
    }
}

""",
            migrationMetadataCode,
            ignoreLineEndingDifferences: true);

        var build = new BuildSource
        {
            References =
            {
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.Tests"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
            },
            Sources = { { "Migration.cs", migrationCode }, { "MigrationSnapshot.cs", migrationMetadataCode } },
            EmitDocumentationDiagnostics = true
        };

        var assembly = build.BuildInMemory();

        var migrationType = assembly.GetType("MyNamespace.MyMigration", throwOnError: true, ignoreCase: false);

        var contextTypeAttribute = migrationType.GetCustomAttribute<DbContextAttribute>();
        Assert.NotNull(contextTypeAttribute);
        Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

        var migration = (Migration)Activator.CreateInstance(migrationType);

        Assert.Equal("20150511161616_MyMigration", migration.GetId());

        Assert.Equal(4, migration.UpOperations.Count);
        Assert.Empty(migration.DownOperations);
        Assert.Single(migration.TargetModel.GetEntityTypes());
    }

    private enum RawEnum
    {
        A,
        B
    }

    private static int MyDbFunction()
        => throw new NotImplementedException();

    private class EntityWithConstructorBinding(int id)
    {
        public int Id { get; } = id;
    }

    public class MyContext;

    [Fact]
    public void Namespaces_imported_for_insert_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new InsertDataOperation
                {
                    Table = "MyTable",
                    Columns = ["Id", "MyColumn"],
                    Values = new object[,] { { 1, null }, { 2, RegexOptions.Multiline } }
                }
            ],
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [Fact]
    public void No_empty_using_for_global_namespace_column_type()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new InsertDataOperation
                {
                    Table = "MyTable",
                    Columns = ["Id", "MyColumn"],
                    Values = new object[,] { { 1, GlobalNamespaceColumnType.Other } }
                }
            ],
            []);

        Assert.DoesNotContain("using ;", migration);
    }

    [Fact]
    public void Namespaces_imported_for_update_data_Values()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new UpdateDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1 } },
                    Columns = ["MyColumn"],
                    Values = new object[,] { { RegexOptions.Multiline } }
                }
            ],
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [Fact]
    public void Namespaces_imported_for_update_data_KeyValues()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new UpdateDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { RegexOptions.Multiline } },
                    Columns = ["MyColumn"],
                    Values = new object[,] { { 1 } }
                }
            ],
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [Fact]
    public void Namespaces_imported_for_delete_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { RegexOptions.Multiline } }
                }
            ],
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [Fact]
    public void Multidimensional_array_warning_is_suppressed_for_multidimensional_seed_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1, 2 }, { 3, 4 } }
                }
            ],
            []);

        Assert.Contains("#pragma warning disable CA1814", migration);
    }

    [Fact]
    public void Multidimensional_array_warning_is_not_suppressed_for_unidimensional_seed_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            [
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1, 2 } }
                }
            ],
            []);

        Assert.DoesNotContain("#pragma warning disable CA1814", migration);
    }

    public static IMigrationsCodeGenerator CreateMigrationsCodeGenerator()
    {
        var testAssembly = typeof(CSharpMigrationsGeneratorTest).Assembly;
        var reporter = new TestOperationReporter();
        return new DesignTimeServicesBuilder(testAssembly, testAssembly, reporter, [])
            .CreateServiceCollection(SqlServerTestHelpers.Instance.CreateContext())
            .BuildServiceProvider(validateScopes: true)
            .GetRequiredService<IMigrationsCodeGenerator>();
    }
}
