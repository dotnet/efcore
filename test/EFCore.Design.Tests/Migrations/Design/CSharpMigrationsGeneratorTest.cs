// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public partial class CSharpMigrationsGeneratorTest
{
    private static readonly string _nl = Environment.NewLine;
    private static readonly string _toTable = _nl + @"entityTypeBuilder.ToTable(""WithAnnotations"")";
    private static readonly string _toNullTable = _nl + @"entityTypeBuilder.ToTable((string)null)";

    [ConditionalFact]
    public void Test_new_annotations_handled_for_entity_types()
    {
        // Only add the annotation here if it will never be present on IEntityType
        var notForEntityType = new HashSet<string>
        {
            CoreAnnotationNames.MaxLength,
            CoreAnnotationNames.Precision,
            CoreAnnotationNames.Scale,
            CoreAnnotationNames.Unicode,
            CoreAnnotationNames.ProductVersion,
            CoreAnnotationNames.ValueGeneratorFactory,
            CoreAnnotationNames.ValueGeneratorFactoryType,
            CoreAnnotationNames.ValueConverter,
            CoreAnnotationNames.ValueConverterType,
            CoreAnnotationNames.ValueComparer,
            CoreAnnotationNames.ValueComparerType,
            CoreAnnotationNames.BeforeSaveBehavior,
            CoreAnnotationNames.AfterSaveBehavior,
            CoreAnnotationNames.ProviderClrType,
            CoreAnnotationNames.EagerLoaded,
            CoreAnnotationNames.LazyLoadingEnabled,
            CoreAnnotationNames.DuplicateServiceProperties,
            CoreAnnotationNames.AdHocModel,
            RelationalAnnotationNames.ColumnName,
            RelationalAnnotationNames.ColumnOrder,
            RelationalAnnotationNames.ColumnType,
            RelationalAnnotationNames.TableColumnMappings,
            RelationalAnnotationNames.ViewColumnMappings,
            RelationalAnnotationNames.SqlQueryColumnMappings,
            RelationalAnnotationNames.FunctionColumnMappings,
            RelationalAnnotationNames.InsertStoredProcedureParameterMappings,
            RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings,
            RelationalAnnotationNames.DeleteStoredProcedureParameterMappings,
            RelationalAnnotationNames.UpdateStoredProcedureParameterMappings,
            RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings,
            RelationalAnnotationNames.DefaultColumnMappings,
            RelationalAnnotationNames.TableMappings,
            RelationalAnnotationNames.ViewMappings,
            RelationalAnnotationNames.FunctionMappings,
            RelationalAnnotationNames.InsertStoredProcedureMappings,
            RelationalAnnotationNames.DeleteStoredProcedureMappings,
            RelationalAnnotationNames.UpdateStoredProcedureMappings,
            RelationalAnnotationNames.SqlQueryMappings,
            RelationalAnnotationNames.DefaultMappings,
            RelationalAnnotationNames.ForeignKeyMappings,
            RelationalAnnotationNames.TableIndexMappings,
            RelationalAnnotationNames.UniqueConstraintMappings,
            RelationalAnnotationNames.RelationalOverrides,
            RelationalAnnotationNames.DefaultValueSql,
            RelationalAnnotationNames.ComputedColumnSql,
            RelationalAnnotationNames.DefaultValue,
            RelationalAnnotationNames.Name,
#pragma warning disable CS0618 // Type or member is obsolete
            RelationalAnnotationNames.SequencePrefix,
#pragma warning restore CS0618 // Type or member is obsolete
            RelationalAnnotationNames.Sequences,
            RelationalAnnotationNames.CheckConstraints,
            RelationalAnnotationNames.DefaultSchema,
            RelationalAnnotationNames.Filter,
            RelationalAnnotationNames.DbFunctions,
            RelationalAnnotationNames.MaxIdentifierLength,
            RelationalAnnotationNames.IsFixedLength,
            RelationalAnnotationNames.Collation,
            RelationalAnnotationNames.IsStored,
            RelationalAnnotationNames.TpcMappingStrategy,
            RelationalAnnotationNames.TphMappingStrategy,
            RelationalAnnotationNames.TptMappingStrategy,
            RelationalAnnotationNames.RelationalModel,
            RelationalAnnotationNames.ModelDependencies,
            RelationalAnnotationNames.FieldValueGetter,
            RelationalAnnotationNames.JsonPropertyName,
            // Appears on entity type but requires specific model (i.e. owned types that can map to json, otherwise validation throws)
            RelationalAnnotationNames.ContainerColumnName,
#pragma warning disable CS0618
            RelationalAnnotationNames.ContainerColumnTypeMapping,
#pragma warning restore CS0618
            RelationalAnnotationNames.StoreType
        };

        // Add a line here if the code generator is supposed to handle this annotation
        // Note that other tests should be added to check code is generated correctly
        var forEntityType = new Dictionary<string, (object, string)>
        {
            {
                RelationalAnnotationNames.TableName,
                ("MyTable", _nl + "entityTypeBuilder." + nameof(RelationalEntityTypeBuilderExtensions.ToTable) + @"(""MyTable"")")
            },
            {
                RelationalAnnotationNames.Schema, ("MySchema",
                    _nl
                    + "entityTypeBuilder."
                    + nameof(RelationalEntityTypeBuilderExtensions.ToTable)
                    + @"(""WithAnnotations"", ""MySchema"")")
            },
            {
                RelationalAnnotationNames.MappingStrategy, (RelationalAnnotationNames.TphMappingStrategy,
                    _toTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder.UseTphMappingStrategy()")
            },
            {
                CoreAnnotationNames.DiscriminatorProperty, ("Id",
                    _toTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder.HasDiscriminator"
                    + @"<int>(""Id"")")
            },
            {
                CoreAnnotationNames.DiscriminatorValue, ("MyDiscriminatorValue",
                    _toTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder.HasDiscriminator"
                    + "()."
                    + nameof(DiscriminatorBuilder.HasValue)
                    + @"(""MyDiscriminatorValue"")")
            },
            {
                RelationalAnnotationNames.Comment, ("My Comment",
                    _nl
                    + @"entityTypeBuilder.ToTable(""WithAnnotations"", t =>"
                    + _nl
                    + "    {"
                    + _nl
                    + @"        t.HasComment(""My Comment"");"
                    + _nl
                    + "    })")
            },
            {
#pragma warning disable CS0612 // Type or member is obsolete
                CoreAnnotationNames.DefiningQuery,
#pragma warning restore CS0612 // Type or member is obsolete
                (Expression.Lambda(Expression.Constant(null)), _toNullTable)
            },
            {
                RelationalAnnotationNames.ViewName, ("MyView", _toNullTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder."
                    + nameof(RelationalEntityTypeBuilderExtensions.ToView)
                    + @"(""MyView"")")
            },
            {
                RelationalAnnotationNames.FunctionName, (null, _toNullTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder."
                    + nameof(RelationalEntityTypeBuilderExtensions.ToFunction)
                    + @"(null)")
            },
            {
                RelationalAnnotationNames.SqlQuery, (null, _toNullTable
                    + ";"
                    + _nl
                    + _nl
                    + "entityTypeBuilder."
                    + nameof(RelationalEntityTypeBuilderExtensions.ToSqlQuery)
                    + @"(null)")
            }
        };

        MissingAnnotationCheck(
            b => b.Entity<WithAnnotations>().Metadata,
            notForEntityType, forEntityType,
            a => _toTable,
            (g, m, b) => g.TestGenerateEntityTypeAnnotations("entityTypeBuilder", (IEntityType)m, b));
    }

    [ConditionalFact]
    public void Test_new_annotations_handled_for_properties()
    {
        // Only add the annotation here if it will never be present on IProperty
        var notForProperty = new HashSet<string>
        {
            CoreAnnotationNames.ProductVersion,
            CoreAnnotationNames.NavigationAccessMode,
            CoreAnnotationNames.EagerLoaded,
            CoreAnnotationNames.LazyLoadingEnabled,
            CoreAnnotationNames.QueryFilter,
#pragma warning disable CS0612 // Type or member is obsolete
            CoreAnnotationNames.DefiningQuery,
#pragma warning restore CS0612 // Type or member is obsolete
            CoreAnnotationNames.DiscriminatorProperty,
            CoreAnnotationNames.DiscriminatorValue,
            CoreAnnotationNames.InverseNavigations,
            CoreAnnotationNames.InverseNavigationsNoAttribute,
            CoreAnnotationNames.NavigationCandidates,
            CoreAnnotationNames.NavigationCandidatesNoAttribute,
            CoreAnnotationNames.AmbiguousNavigations,
            CoreAnnotationNames.DuplicateServiceProperties,
            CoreAnnotationNames.AdHocModel,
            RelationalAnnotationNames.TableName,
            RelationalAnnotationNames.IsTableExcludedFromMigrations,
            RelationalAnnotationNames.ViewName,
            RelationalAnnotationNames.Schema,
            RelationalAnnotationNames.ViewSchema,
            RelationalAnnotationNames.ViewDefinitionSql,
            RelationalAnnotationNames.FunctionName,
            RelationalAnnotationNames.SqlQuery,
            RelationalAnnotationNames.DefaultSchema,
            RelationalAnnotationNames.DefaultMappings,
            RelationalAnnotationNames.TableColumnMappings,
            RelationalAnnotationNames.ViewColumnMappings,
            RelationalAnnotationNames.SqlQueryColumnMappings,
            RelationalAnnotationNames.FunctionColumnMappings,
            RelationalAnnotationNames.InsertStoredProcedureParameterMappings,
            RelationalAnnotationNames.InsertStoredProcedureResultColumnMappings,
            RelationalAnnotationNames.DeleteStoredProcedureParameterMappings,
            RelationalAnnotationNames.UpdateStoredProcedureParameterMappings,
            RelationalAnnotationNames.UpdateStoredProcedureResultColumnMappings,
            RelationalAnnotationNames.DefaultColumnMappings,
            RelationalAnnotationNames.TableMappings,
            RelationalAnnotationNames.ViewMappings,
            RelationalAnnotationNames.FunctionMappings,
            RelationalAnnotationNames.InsertStoredProcedureMappings,
            RelationalAnnotationNames.DeleteStoredProcedureMappings,
            RelationalAnnotationNames.UpdateStoredProcedureMappings,
            RelationalAnnotationNames.SqlQueryMappings,
            RelationalAnnotationNames.ForeignKeyMappings,
            RelationalAnnotationNames.TableIndexMappings,
            RelationalAnnotationNames.UniqueConstraintMappings,
            RelationalAnnotationNames.MappingFragments,
            RelationalAnnotationNames.Name,
            RelationalAnnotationNames.Sequences,
#pragma warning disable CS0618 // Type or member is obsolete
            RelationalAnnotationNames.SequencePrefix,
#pragma warning restore CS0618 // Type or member is obsolete
            RelationalAnnotationNames.CheckConstraints,
            RelationalAnnotationNames.Filter,
            RelationalAnnotationNames.DbFunctions,
            RelationalAnnotationNames.MaxIdentifierLength,
            RelationalAnnotationNames.MappingStrategy,
            RelationalAnnotationNames.TpcMappingStrategy,
            RelationalAnnotationNames.TphMappingStrategy,
            RelationalAnnotationNames.TptMappingStrategy,
            RelationalAnnotationNames.RelationalModel,
            RelationalAnnotationNames.ModelDependencies,
            RelationalAnnotationNames.FieldValueGetter,
            RelationalAnnotationNames.ContainerColumnName,
#pragma warning disable CS0618
            RelationalAnnotationNames.ContainerColumnTypeMapping,
#pragma warning restore CS0618
            RelationalAnnotationNames.JsonPropertyName,
            RelationalAnnotationNames.StoreType,
        };

        var columnMapping = $@"{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}(""default_int_mapping"")";

        // Add a line here if the code generator is supposed to handle this annotation
        // Note that other tests should be added to check code is generated correctly
        var forProperty = new Dictionary<string, (object, string)>
        {
            { CoreAnnotationNames.MaxLength, (256, $@"{_nl}.{nameof(PropertyBuilder.HasMaxLength)}(256){columnMapping}") },
            { CoreAnnotationNames.Precision, (4, $@"{_nl}.{nameof(PropertyBuilder.HasPrecision)}(4){columnMapping}") },
            { CoreAnnotationNames.Scale, (null, $@"{columnMapping}") },
            { CoreAnnotationNames.Unicode, (false, $@"{_nl}.{nameof(PropertyBuilder.IsUnicode)}(false){columnMapping}") },
            {
                CoreAnnotationNames.ValueConverter, (new ValueConverter<int, long>(v => v, v => (int)v),
                    $@"{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}(""default_long_mapping"")")
            },
            {
                CoreAnnotationNames.ProviderClrType,
                (typeof(long), $@"{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}(""default_long_mapping"")")
            },
            {
                RelationalAnnotationNames.ColumnName,
                ("MyColumn", $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnName)}(""MyColumn"")")
            },
            {
                RelationalAnnotationNames.ColumnOrder,
                (1, $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnOrder)}(1)")
            },
            {
                RelationalAnnotationNames.ColumnType,
                ("int", $@"{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasColumnType)}(""int"")")
            },
            {
                RelationalAnnotationNames.DefaultValueSql,
                ("some SQL", $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql)}(""some SQL"")")
            },
            {
                RelationalAnnotationNames.ComputedColumnSql,
                ("some SQL", $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql)}(""some SQL"")")
            },
            {
                RelationalAnnotationNames.DefaultValue,
                ("1", $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasDefaultValue)}(""1"")")
            },
            {
                RelationalAnnotationNames.IsFixedLength,
                (true, $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.IsFixedLength)}()")
            },
            {
                RelationalAnnotationNames.Comment,
                ("My Comment", $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.HasComment)}(""My Comment"")")
            },
            {
                RelationalAnnotationNames.Collation, ("Some Collation",
                    $@"{columnMapping}{_nl}.{nameof(RelationalPropertyBuilderExtensions.UseCollation)}(""Some Collation"")")
            },
            {
                RelationalAnnotationNames.IsStored,
                (null, $@"{columnMapping}{_nl}.HasAnnotation(""{RelationalAnnotationNames.IsStored}"", null)")
            }
        };

        MissingAnnotationCheck(
            b => b.Entity<WithAnnotations>().Property(e => e.Id).Metadata,
            notForProperty, forProperty,
            a => $"{columnMapping}",
            (g, m, b) => g.TestGeneratePropertyAnnotations("propertyBuilder", (IProperty)m, b));
    }

    private static void MissingAnnotationCheck(
        Func<ModelBuilder, IMutableAnnotatable> createMetadataItem,
        HashSet<string> invalidAnnotations,
        Dictionary<string, (object Value, string Expected)> validAnnotations,
        Func<string, string> generationDefault,
        Action<TestCSharpSnapshotGenerator, IMutableAnnotatable, IndentedStringBuilder> test)
    {
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var sqlServerAnnotationCodeGenerator = new SqlServerAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqlServerTypeMappingSource));

        var codeHelper = new CSharpHelper(sqlServerTypeMappingSource);

        var generator = new TestCSharpSnapshotGenerator(
            new CSharpSnapshotGeneratorDependencies(codeHelper, sqlServerTypeMappingSource, sqlServerAnnotationCodeGenerator));

        var coreAnnotations = typeof(CoreAnnotationNames).GetFields().Where(f => f.FieldType == typeof(string)).ToList();

        foreach (var field in coreAnnotations)
        {
            var annotationName = (string)field.GetValue(null);

            Assert.True(
                CoreAnnotationNames.AllNames.Contains(annotationName),
                nameof(CoreAnnotationNames) + "." + nameof(CoreAnnotationNames.AllNames) + " doesn't contain " + annotationName);
        }

        var relationalAnnotations = typeof(RelationalAnnotationNames).GetFields()
            .Where(f => f.FieldType == typeof(string)
                && f.Name != "Prefix").ToList();

        foreach (var field in relationalAnnotations)
        {
            var annotationName = (string)field.GetValue(null);

            if (field.Name != nameof(RelationalAnnotationNames.TpcMappingStrategy)
                && field.Name != nameof(RelationalAnnotationNames.TptMappingStrategy)
                && field.Name != nameof(RelationalAnnotationNames.TphMappingStrategy))
            {
                Assert.True(
                    RelationalAnnotationNames.AllNames.Contains(annotationName),
                    nameof(RelationalAnnotationNames) + "." + nameof(RelationalAnnotationNames.AllNames) + " doesn't contain " + annotationName);
            }
        }

        foreach (var field in coreAnnotations.Concat(relationalAnnotations))
        {
            var annotationName = (string)field.GetValue(null);

            if (!invalidAnnotations.Contains(annotationName))
            {
                var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
                var metadataItem = createMetadataItem(modelBuilder);
                metadataItem.SetAnnotation(
                    annotationName, validAnnotations.ContainsKey(annotationName)
                        ? validAnnotations[annotationName].Value
                        : null);

                modelBuilder.FinalizeModel(designTime: true, skipValidation: true);

                var sb = new IndentedStringBuilder();

                try
                {
                    // Generator should not throw--either update above, or add to ignored list in generator
                    test(generator, metadataItem, sb);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Annotation '{annotationName}' was not handled by the code generator: {e.Message}");
                }

                try
                {
                    var expected = validAnnotations.ContainsKey(annotationName)
                        ? validAnnotations[annotationName].Expected
                        : generationDefault(annotationName);

                    Assert.Equal(
                        string.IsNullOrEmpty(expected) ? expected : $"{expected};{_nl}",
                        sb.ToString());
                }
                catch (Exception e)
                {
                    throw new Exception(annotationName, e);
                }
            }
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
    private class TestCSharpSnapshotGenerator(CSharpSnapshotGeneratorDependencies dependencies) : CSharpSnapshotGenerator(dependencies)
    {
        public virtual void TestGenerateEntityTypeAnnotations(
            string builderName,
            IEntityType entityType,
            IndentedStringBuilder stringBuilder)
            => GenerateEntityTypeAnnotations(builderName, entityType, stringBuilder);

        public virtual void TestGeneratePropertyAnnotations(
            string builderName,
            IProperty property,
            IndentedStringBuilder stringBuilder)
            => GeneratePropertyAnnotations(builderName, property, stringBuilder);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class WithAnnotations
    {
        public int Id { get; set; }
    }

    private class Derived : WithAnnotations;

    [ConditionalFact]
    public void Snapshot_with_enum_discriminator_uses_converted_values()
    {
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var codeHelper = new CSharpHelper(
            sqlServerTypeMappingSource);

        var sqlServerAnnotationCodeGenerator = new SqlServerAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqlServerTypeMappingSource));

        var generator = new CSharpMigrationsGenerator(
            new MigrationsCodeGeneratorDependencies(
                sqlServerTypeMappingSource,
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
        modelBuilder.Entity<WithAnnotations>(
            eb =>
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

        var snapshotModel = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot").Model;

        Assert.Equal((int)RawEnum.A, snapshotModel.FindEntityType(typeof(WithAnnotations)).GetDiscriminatorValue());
        Assert.Equal((int)RawEnum.B, snapshotModel.FindEntityType(typeof(Derived)).GetDiscriminatorValue());
    }

    private static void AssertConverter(ValueConverter valueConverter, string expected)
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        var property = modelBuilder.Entity<WithAnnotations>().Property(e => e.Id).Metadata;
        property.SetMaxLength(1000);
        property.SetValueConverter(valueConverter);

        modelBuilder.FinalizeModel();

        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var codeHelper = new CSharpHelper(sqlServerTypeMappingSource);

        var sqlServerAnnotationCodeGenerator = new SqlServerAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqlServerTypeMappingSource));

        var generator = new TestCSharpSnapshotGenerator(
            new CSharpSnapshotGeneratorDependencies(
                codeHelper, sqlServerTypeMappingSource, sqlServerAnnotationCodeGenerator));

        var sb = new IndentedStringBuilder();

        generator.TestGeneratePropertyAnnotations("propertyBuilder", (IProperty)property, sb);

        Assert.Equal(expected + _nl + ".HasMaxLength(1000)", sb.ToString());
    }

    [ConditionalFact]
    public void Migrations_compile()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migrationCode = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new MigrationOperation[]
            {
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
            },
            []);
        Assert.Equal(
            """
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

#nullable disable

namespace MyNamespace
{
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

namespace MyNamespace
{
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

    private ModelSnapshot CompileModelSnapshot(string code, string modelSnapshotTypeName)
    {
        var build = new BuildSource { Sources = { { "Snapshot.cs", code } } };

        foreach (var buildReference in GetReferences())
        {
            build.References.Add(buildReference);
        }

        var assembly = build.BuildInMemory();

        var snapshotType = assembly.GetType(modelSnapshotTypeName, throwOnError: true, ignoreCase: false);

        var contextTypeAttribute = snapshotType.GetCustomAttribute<DbContextAttribute>();
        Assert.NotNull(contextTypeAttribute);
        Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

        return (ModelSnapshot)Activator.CreateInstance(snapshotType);
    }

    public class MyContext;

    [ConditionalFact]
    public void Namespaces_imported_for_insert_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new InsertDataOperation
                {
                    Table = "MyTable",
                    Columns = ["Id", "MyColumn"],
                    Values = new object[,] { { 1, null }, { 2, RegexOptions.Multiline } }
                }
            },
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [ConditionalFact]
    public void Namespaces_imported_for_update_data_Values()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new UpdateDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1 } },
                    Columns = ["MyColumn"],
                    Values = new object[,] { { RegexOptions.Multiline } }
                }
            },
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [ConditionalFact]
    public void Namespaces_imported_for_update_data_KeyValues()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new UpdateDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { RegexOptions.Multiline } },
                    Columns = ["MyColumn"],
                    Values = new object[,] { { 1 } }
                }
            },
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [ConditionalFact]
    public void Namespaces_imported_for_delete_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { RegexOptions.Multiline } }
                }
            },
            []);

        Assert.Contains("using System.Text.RegularExpressions;", migration);
    }

    [ConditionalFact]
    public void Multidimensional_array_warning_is_suppressed_for_multidimensional_seed_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1, 2 }, { 3, 4 } }
                }
            },
            []);

        Assert.Contains("#pragma warning disable CA1814", migration);
    }

    [ConditionalFact]
    public void Multidimensional_array_warning_is_not_suppressed_for_unidimensional_seed_data()
    {
        var generator = CreateMigrationsCodeGenerator();

        var migration = generator.GenerateMigration(
            "MyNamespace",
            "MyMigration",
            new[]
            {
                new DeleteDataOperation
                {
                    Table = "MyTable",
                    KeyColumns = ["Id"],
                    KeyValues = new object[,] { { 1, 2 } }
                }
            },
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
