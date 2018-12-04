// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class CSharpMigrationsGeneratorTest
    {
        private static readonly string _nl = Environment.NewLine;
        private static readonly string _toTable = _nl + @"modelBuilder.ToTable(""WithAnnotations"");" + _nl;

        [Fact]
        public void Test_new_annotations_handled_for_entity_types()
        {
            var model = RelationalTestHelpers.Instance.CreateConventionBuilder();
            var entityType = model.Entity<WithAnnotations>().Metadata;

            // Only add the annotation here if it will never be present on IEntityType
            var notForEntityType = new HashSet<string>
            {
                CoreAnnotationNames.MaxLengthAnnotation,
                CoreAnnotationNames.UnicodeAnnotation,
                CoreAnnotationNames.ProductVersionAnnotation,
                CoreAnnotationNames.ValueGeneratorFactoryAnnotation,
                CoreAnnotationNames.OwnedTypesAnnotation,
                CoreAnnotationNames.TypeMapping,
                CoreAnnotationNames.ValueConverter,
                CoreAnnotationNames.ValueComparer,
                CoreAnnotationNames.KeyValueComparer,
                CoreAnnotationNames.StructuralValueComparer,
                CoreAnnotationNames.ProviderClrType,
                RelationalAnnotationNames.ColumnName,
                RelationalAnnotationNames.ColumnType,
                RelationalAnnotationNames.DefaultValueSql,
                RelationalAnnotationNames.ComputedColumnSql,
                RelationalAnnotationNames.DefaultValue,
                RelationalAnnotationNames.Name,
                RelationalAnnotationNames.SequencePrefix,
                RelationalAnnotationNames.DefaultSchema,
                RelationalAnnotationNames.Filter,
                RelationalAnnotationNames.DbFunction,
                RelationalAnnotationNames.MaxIdentifierLength,
                RelationalAnnotationNames.IsFixedLength
            };

            // Add a line here if the code generator is supposed to handle this annotation
            // Note that other tests should be added to check code is generated correctly
            var forEntityType = new Dictionary<string, (object, string)>
            {
                {
                    RelationalAnnotationNames.TableName,
                    ("MyTable", _nl + "modelBuilder." + nameof(RelationalEntityTypeBuilderExtensions.ToTable) + @"(""MyTable"");" + _nl)
                },
                {
                    RelationalAnnotationNames.Schema,
                    ("MySchema", _nl + "modelBuilder." + nameof(RelationalEntityTypeBuilderExtensions.ToTable) + @"(""WithAnnotations"",""MySchema"");" + _nl)
                },
                {
                    RelationalAnnotationNames.DiscriminatorProperty,
                    ("Id", _toTable + _nl + "modelBuilder." + nameof(RelationalEntityTypeBuilderExtensions.HasDiscriminator) + @"<int>(""Id"");" + _nl)
                },
                {
                    RelationalAnnotationNames.DiscriminatorValue,
                    ("MyDiscriminatorValue",
                        _toTable + _nl + "modelBuilder." + nameof(RelationalEntityTypeBuilderExtensions.HasDiscriminator)
                        + "()." + nameof(DiscriminatorBuilder.HasValue) + @"(""MyDiscriminatorValue"");" + _nl)
                }
            };

            MissingAnnotationCheck(
                entityType, notForEntityType, forEntityType,
                _toTable,
                (g, m, b) => g.TestGenerateEntityTypeAnnotations("modelBuilder", (IEntityType)m, b));
        }

        [Fact]
        public void Test_new_annotations_handled_for_properties()
        {
            var model = RelationalTestHelpers.Instance.CreateConventionBuilder();
            var property = model.Entity<WithAnnotations>().Property(e => e.Id).Metadata;

            // Only add the annotation here if it will never be present on IProperty
            var notForProperty = new HashSet<string>
            {
                CoreAnnotationNames.ProductVersionAnnotation,
                CoreAnnotationNames.OwnedTypesAnnotation,
                CoreAnnotationNames.ConstructorBinding,
                CoreAnnotationNames.NavigationAccessModeAnnotation,
                RelationalAnnotationNames.TableName,
                RelationalAnnotationNames.Schema,
                RelationalAnnotationNames.DefaultSchema,
                RelationalAnnotationNames.Name,
                RelationalAnnotationNames.SequencePrefix,
                RelationalAnnotationNames.DiscriminatorProperty,
                RelationalAnnotationNames.DiscriminatorValue,
                RelationalAnnotationNames.Filter,
                RelationalAnnotationNames.DbFunction,
                RelationalAnnotationNames.MaxIdentifierLength
            };

            // Add a line here if the code generator is supposed to handle this annotation
            // Note that other tests should be added to check code is generated correctly
            var forProperty = new Dictionary<string, (object, string)>
            {
                {
                    CoreAnnotationNames.MaxLengthAnnotation,
                    (256, _nl + "." + nameof(PropertyBuilder.HasMaxLength) + "(256)")
                },
                {
                    CoreAnnotationNames.UnicodeAnnotation,
                    (false, _nl + "." + nameof(PropertyBuilder.IsUnicode) + "(false)")
                },
                {
                    CoreAnnotationNames.ValueConverter,
                    (new ValueConverter<int, long>(v => v, v => (int)v), "")
                },
                {
                    CoreAnnotationNames.ProviderClrType,
                    (typeof(long), "")
                },
                {
                    RelationalAnnotationNames.ColumnName,
                    ("MyColumn", _nl + "." + nameof(RelationalPropertyBuilderExtensions.HasColumnName) + @"(""MyColumn"")")
                },
                {
                    RelationalAnnotationNames.ColumnType,
                    ("int", _nl + "." + nameof(RelationalPropertyBuilderExtensions.HasColumnType) + @"(""int"")")
                },
                {
                    RelationalAnnotationNames.DefaultValueSql,
                    ("some SQL", _nl + "." + nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql) + @"(""some SQL"")")
                },
                {
                    RelationalAnnotationNames.ComputedColumnSql,
                    ("some SQL", _nl + "." + nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql) + @"(""some SQL"")")
                },
                {
                    RelationalAnnotationNames.DefaultValue,
                    ("1", _nl + "." + nameof(RelationalPropertyBuilderExtensions.HasDefaultValue) + @"(""1"")")
                },
                {
                    CoreAnnotationNames.TypeMapping,
                    (new LongTypeMapping("bigint"), "")
                },
                {
                    RelationalAnnotationNames.IsFixedLength,
                    (true, _nl + "." + nameof(RelationalPropertyBuilderExtensions.IsFixedLength) + "(true)")
                }
            };

            MissingAnnotationCheck(
                property, notForProperty, forProperty,
                "",
                (g, m, b) => g.TestGeneratePropertyAnnotations((IProperty)m, b));
        }

        private static void MissingAnnotationCheck(
            IMutableAnnotatable metadataItem,
            HashSet<string> invalidAnnotations,
            Dictionary<string, (object Value, string Expected)> validAnnotations,
            string generationDefault,
            Action<TestCSharpSnapshotGenerator, IMutableAnnotatable, IndentedStringBuilder> test)
        {
            var codeHelper = new CSharpHelper(new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            var generator = new TestCSharpSnapshotGenerator(
                new CSharpSnapshotGeneratorDependencies(codeHelper));

            foreach (var field in typeof(CoreAnnotationNames).GetFields().Concat(
                typeof(RelationalAnnotationNames).GetFields().Where(f => f.Name != "Prefix")))
            {
                var annotationName = (string)field.GetValue(null);

                if (!invalidAnnotations.Contains(annotationName))
                {
                    metadataItem[annotationName] = validAnnotations.ContainsKey(annotationName)
                        ? validAnnotations[annotationName].Value
                        : new Random(); // Something that cannot be scaffolded by default

                    var sb = new IndentedStringBuilder();

                    try
                    {
                        // Generator should not throw--either update above, or add to ignored list in generator
                        test(generator, metadataItem, sb);
                    }
                    catch (Exception e)
                    {
                        Assert.False(true, $"Annotation '{annotationName}' was not handled by the code generator: {e.Message}");
                    }

                    Assert.Equal(
                        validAnnotations.ContainsKey(annotationName)
                            ? validAnnotations[annotationName].Expected
                            : generationDefault,
                        sb.ToString());

                    metadataItem[annotationName] = null;
                }
            }
        }

        // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
        private class TestCSharpSnapshotGenerator : CSharpSnapshotGenerator
        {
            public TestCSharpSnapshotGenerator(CSharpSnapshotGeneratorDependencies dependencies)
                : base(dependencies)
            {
            }

            public virtual void TestGenerateEntityTypeAnnotations(string builderName, IEntityType entityType, IndentedStringBuilder stringBuilder)
                => GenerateEntityTypeAnnotations(builderName, entityType, stringBuilder);

            public virtual void TestGeneratePropertyAnnotations(IProperty property, IndentedStringBuilder stringBuilder)
                => GeneratePropertyAnnotations(property, stringBuilder);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class WithAnnotations
        {
            public int Id { get; set; }
        }

        private class Derived : WithAnnotations
        {
        }

        [Fact]
        public void Snapshot_with_enum_discriminator_uses_converted_values()
        {
            var codeHelper = new CSharpHelper(new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            var generator = new CSharpMigrationsGenerator(
                new MigrationsCodeGeneratorDependencies(),
                new CSharpMigrationsGeneratorDependencies(
                    codeHelper,
                    new CSharpMigrationOperationGenerator(
                        new CSharpMigrationOperationGeneratorDependencies(
                            codeHelper)),
                    new CSharpSnapshotGenerator(
                        new CSharpSnapshotGeneratorDependencies(
                            codeHelper))));

            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersionAnnotation);
            modelBuilder.Entity<WithAnnotations>(
                eb =>
                {
                    eb.HasDiscriminator<RawEnum>("EnumDiscriminator")
                        .HasValue(RawEnum.A)
                        .HasValue<Derived>(RawEnum.B);
                    eb.Property<RawEnum>("EnumDiscriminator").HasConversion<int>();
                });

            modelBuilder.FinalizeModel();

            var modelSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                typeof(MyContext),
                "MySnapshot",
                modelBuilder.Model);

            var snapshotModel = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot").Model;

            Assert.Equal((int)RawEnum.A, snapshotModel.FindEntityType(typeof(WithAnnotations)).SqlServer().DiscriminatorValue);
            Assert.Equal((int)RawEnum.B, snapshotModel.FindEntityType(typeof(Derived)).SqlServer().DiscriminatorValue);
        }

        [Fact]
        public void Value_converters_with_mapping_hints_are_scaffolded_correctly()
        {
            var commonPrefix
                = _nl + "." + nameof(PropertyBuilder.HasConversion) + "(new " + nameof(ValueConverter) + "<long, long>(v => default(long), v => default(long)";

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v), "");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new ConverterMappingHints(size: 10)),
                commonPrefix + ", new ConverterMappingHints(size: 10)))");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new ConverterMappingHints(precision: 10)),
                commonPrefix + ", new ConverterMappingHints(precision: 10)))");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new ConverterMappingHints(scale: 10)),
                commonPrefix + ", new ConverterMappingHints(scale: 10)))");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new ConverterMappingHints(unicode: true)),
                commonPrefix + ", new ConverterMappingHints(unicode: true)))");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new RelationalConverterMappingHints(fixedLength: false)),
                commonPrefix + ", new ConverterMappingHints(fixedLength: false)))");

            AssertConverter(
                new ValueConverter<int, long>(v => v, v => (int)v, new RelationalConverterMappingHints(fixedLength: false, size: 77, scale: -1)),
                commonPrefix + ", new ConverterMappingHints(size: 77, scale: -1, fixedLength: false)))");
        }

        private static void AssertConverter(ValueConverter valueConverter, string expected)
        {
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            var property = modelBuilder.Entity<WithAnnotations>().Property(e => e.Id).Metadata;
            property.SetMaxLength(1000);

            modelBuilder.FinalizeModel();

            var codeHelper = new CSharpHelper(new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()));

            var generator = new TestCSharpSnapshotGenerator(
                new CSharpSnapshotGeneratorDependencies(codeHelper));

            property.SetValueConverter(valueConverter);

            var sb = new IndentedStringBuilder();

            generator.TestGeneratePropertyAnnotations(property, sb);

            Assert.Equal(expected + _nl + ".HasMaxLength(1000)", sb.ToString());
        }

        [Fact]
        public void Migrations_compile()
        {
            var generator = CreateMigrationsCodeGenerator();

            var migrationCode = generator.GenerateMigration(
                "MyNamespace",
                "MyMigration",
                new MigrationOperation[]
                {
                    new SqlOperation
                    {
                        Sql = "-- TEST",
                        ["Some:EnumValue"] = RegexOptions.Multiline
                    },
                    new AlterColumnOperation
                    {
                        Name = "C2",
                        Table = "T1",
                        ClrType = typeof(Database),
                        OldColumn = new ColumnOperation
                        {
                            ClrType = typeof(Property)
                        }
                    },
                    new AddColumnOperation
                    {
                        Name = "C3",
                        Table = "T1",
                        ClrType = typeof(PropertyEntry)
                    }
                },
                Array.Empty<MigrationOperation>());
            Assert.Equal(
                @"using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(""-- TEST"")
                .Annotation(""Some:EnumValue"", RegexOptions.Multiline);

            migrationBuilder.AlterColumn<Database>(
                name: ""C2"",
                table: ""T1"",
                nullable: false,
                oldClrType: typeof(Property));

            migrationBuilder.AddColumn<PropertyEntry>(
                name: ""C3"",
                table: ""T1"",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
",
                migrationCode,
                ignoreLineEndingDifferences: true);

            var migrationMetadataCode = generator.GenerateMetadata(
                "MyNamespace",
                typeof(MyContext),
                "MyMigration",
                "20150511161616_MyMigration",
                new Model
                {
                    ["Some:EnumValue"] = RegexOptions.Multiline,
                    ["Relational:DbFunction:MyFunc"] = new object()
                });
            Assert.Equal(
                @"// <auto-generated />
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyNamespace
{
    [DbContext(typeof(CSharpMigrationsGeneratorTest.MyContext))]
    [Migration(""20150511161616_MyMigration"")]
    partial class MyMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);
#pragma warning restore 612, 618
        }
    }
}
",
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
                Sources =
                {
                    migrationCode,
                    migrationMetadataCode
                }
            };

            var assembly = build.BuildInMemory();

            var migrationType = assembly.GetType("MyNamespace.MyMigration", throwOnError: true, ignoreCase: false);

            var contextTypeAttribute = migrationType.GetTypeInfo().GetCustomAttribute<DbContextAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            var migration = (Migration)Activator.CreateInstance(migrationType);

            Assert.Equal("20150511161616_MyMigration", migration.GetId());

            Assert.Equal(3, migration.UpOperations.Count);
            Assert.Empty(migration.DownOperations);
            Assert.Empty(migration.TargetModel.GetEntityTypes());
        }

        private enum RawEnum
        {
            A,
            B
        }

        private static int MyDbFunction()
            => throw new NotImplementedException();

        private class EntityWithConstructorBinding
        {
            public EntityWithConstructorBinding(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        [Fact]
        public void Snapshots_compile()
        {
            var generator = CreateMigrationsCodeGenerator();

            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder(skipValidation: true);
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersionAnnotation);
            modelBuilder.Entity<EntityWithConstructorBinding>(
                x =>
                {
                    x.Property(e => e.Id);

                    x.Property<Guid>("PropertyWithValueGenerator").HasValueGenerator<GuidValueGenerator>();
                });
            modelBuilder.HasDbFunction(() => MyDbFunction());

            var model = modelBuilder.Model;
            model["Some:EnumValue"] = RegexOptions.Multiline;

            var entityType = model.AddEntityType("Cheese");
            var property1 = entityType.AddProperty("Pickle", typeof(StringBuilder));
            property1.SetValueConverter(new ValueConverter<StringBuilder, string>(v => v.ToString(), v => new StringBuilder(v), new ConverterMappingHints(size: 10)));

            var property2 = entityType.AddProperty("Ham", typeof(RawEnum));
            property2.SetValueConverter(new ValueConverter<RawEnum, string>(v => v.ToString(), v => (RawEnum)Enum.Parse(typeof(RawEnum), v), new ConverterMappingHints(size: 10)));

            modelBuilder.FinalizeModel();

            var modelSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                typeof(MyContext),
                "MySnapshot",
                model);
            Assert.Equal(
                @"// <auto-generated />
using System;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyNamespace
{
    [DbContext(typeof(CSharpMigrationsGeneratorTest.MyContext))]
    partial class MySnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);

            modelBuilder.Entity(""Cheese"", b =>
                {
                    b.Property<string>(""Ham"")
                        .IsRequired()
                        .HasConversion(new ValueConverter<string, string>(v => default(string), v => default(string), new ConverterMappingHints(size: 10)));

                    b.Property<string>(""Pickle"")
                        .HasConversion(new ValueConverter<string, string>(v => default(string), v => default(string), new ConverterMappingHints(size: 10)));

                    b.ToTable(""Cheese"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.Design.CSharpMigrationsGeneratorTest+EntityWithConstructorBinding"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>(""PropertyWithValueGenerator"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithConstructorBinding"");
                });
#pragma warning restore 612, 618
        }
    }
}
", modelSnapshotCode, ignoreLineEndingDifferences: true);

            var snapshot = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot");
            Assert.Equal(2, snapshot.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Snapshot_with_default_values_are_round_tripped()
        {
            var generator = CreateMigrationsCodeGenerator();

            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<EntityWithEveryPrimitive>(
                eb =>
                {
                    eb.Property(e => e.Boolean).HasDefaultValue(false);
                    eb.Property(e => e.Byte).HasDefaultValue(byte.MinValue);
                    eb.Property(e => e.ByteArray).HasDefaultValue(new byte[] { 0 });
                    eb.Property(e => e.Char).HasDefaultValue('0');
                    eb.Property(e => e.DateTime).HasDefaultValue(DateTime.MinValue);
                    eb.Property(e => e.DateTimeOffset).HasDefaultValue(DateTimeOffset.MinValue);
                    eb.Property(e => e.Decimal).HasDefaultValue(decimal.MinValue);
                    eb.Property(e => e.Double).HasDefaultValue(double.MinValue); //double.NegativeInfinity
                    eb.Property(e => e.Enum).HasDefaultValue(Enum1.Default);
                    eb.Property(e => e.NullableEnum).HasDefaultValue(Enum1.Default).HasConversion<string>();
                    eb.Property(e => e.Guid).HasDefaultValue(Guid.NewGuid());
                    eb.Property(e => e.Int16).HasDefaultValue(short.MaxValue);
                    eb.Property(e => e.Int32).HasDefaultValue(int.MaxValue);
                    eb.Property(e => e.Int64).HasDefaultValue(long.MaxValue);
                    eb.Property(e => e.Single).HasDefaultValue(float.Epsilon);
                    eb.Property(e => e.SByte).HasDefaultValue(sbyte.MinValue);
                    eb.Property(e => e.String).HasDefaultValue("'\"'@\r\\\n");
                    eb.Property(e => e.TimeSpan).HasDefaultValue(TimeSpan.MaxValue);
                    eb.Property(e => e.UInt16).HasDefaultValue(ushort.MinValue);
                    eb.Property(e => e.UInt32).HasDefaultValue(uint.MinValue);
                    eb.Property(e => e.UInt64).HasDefaultValue(ulong.MinValue);
                    eb.Property(e => e.NullableBoolean).HasDefaultValue(true);
                    eb.Property(e => e.NullableByte).HasDefaultValue(byte.MaxValue);
                    eb.Property(e => e.NullableChar).HasDefaultValue('\'');
                    eb.Property(e => e.NullableDateTime).HasDefaultValue(DateTime.MaxValue);
                    eb.Property(e => e.NullableDateTimeOffset).HasDefaultValue(DateTimeOffset.MaxValue);
                    eb.Property(e => e.NullableDecimal).HasDefaultValue(decimal.MaxValue);
                    eb.Property(e => e.NullableDouble).HasDefaultValue(0.6822871999174);
                    eb.Property(e => e.NullableEnum).HasDefaultValue(Enum1.One | Enum1.Two);
                    eb.Property(e => e.NullableStringEnum).HasDefaultValue(Enum1.One).HasConversion<string>();
                    eb.Property(e => e.NullableGuid).HasDefaultValue(new Guid());
                    eb.Property(e => e.NullableInt16).HasDefaultValue(short.MinValue);
                    eb.Property(e => e.NullableInt32).HasDefaultValue(int.MinValue);
                    eb.Property(e => e.NullableInt64).HasDefaultValue(long.MinValue);
                    eb.Property(e => e.NullableSingle).HasDefaultValue(0.3333333f);
                    eb.Property(e => e.NullableSByte).HasDefaultValue(sbyte.MinValue);
                    eb.Property(e => e.NullableTimeSpan).HasDefaultValue(TimeSpan.MinValue.Add(new TimeSpan()));
                    eb.Property(e => e.NullableUInt16).HasDefaultValue(ushort.MaxValue);
                    eb.Property(e => e.NullableUInt32).HasDefaultValue(uint.MaxValue);
                    eb.Property(e => e.NullableUInt64).HasDefaultValue(ulong.MaxValue);
                });

            var modelSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                typeof(MyContext),
                "MySnapshot",
                modelBuilder.Model);

            var snapshot = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot");
            var entityType = snapshot.Model.GetEntityTypes().Single();
            Assert.Equal(typeof(EntityWithEveryPrimitive).FullName, entityType.DisplayName());

            foreach (var property in modelBuilder.Model.GetEntityTypes().Single().GetProperties())
            {
                var snapshotProperty = entityType.FindProperty(property.Name);
                Assert.Equal(property.Relational().DefaultValue, snapshotProperty.Relational().DefaultValue);
            }
        }

        private class EntityWithEveryPrimitive
        {
            public bool Boolean { get; set; }
            public byte Byte { get; set; }
            public byte[] ByteArray { get; set; }
            public char Char { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public Enum1 Enum { get; set; }
            public Enum1 StringEnum { get; set; }
            public Guid Guid { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public bool? NullableBoolean { get; set; }
            public byte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
            public DateTime? NullableDateTime { get; set; }
            public DateTimeOffset? NullableDateTimeOffset { get; set; }
            public decimal? NullableDecimal { get; set; }
            public double? NullableDouble { get; set; }
            public Enum1? NullableEnum { get; set; }
            public Enum1? NullableStringEnum { get; set; }
            public Guid? NullableGuid { get; set; }
            public short? NullableInt16 { get; set; }
            public int? NullableInt32 { get; set; }
            public long? NullableInt64 { get; set; }
            public sbyte? NullableSByte { get; set; }
            public float? NullableSingle { get; set; }
            public TimeSpan? NullableTimeSpan { get; set; }
            public ushort? NullableUInt16 { get; set; }
            public uint? NullableUInt32 { get; set; }
            public ulong? NullableUInt64 { get; set; }
            public int PrivateSetter { get; private set; }
            public sbyte SByte { get; set; }
            public float Single { get; set; }
            public string String { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }
        }

        [Flags]
        public enum Enum1
        {
            Default = 0,
            One = 1,
            Two = 2
        }

        private ModelSnapshot CompileModelSnapshot(string modelSnapshotCode, string modelSnapshotTypeName)
        {
            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.Tests"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources =
                {
                    modelSnapshotCode
                }
            };

            var assembly = build.BuildInMemory();

            var snapshotType = assembly.GetType(modelSnapshotTypeName, throwOnError: true, ignoreCase: false);

            var contextTypeAttribute = snapshotType.GetTypeInfo().GetCustomAttribute<DbContextAttribute>();
            Assert.NotNull(contextTypeAttribute);
            Assert.Equal(typeof(MyContext), contextTypeAttribute.ContextType);

            return (ModelSnapshot)Activator.CreateInstance(snapshotType);
        }

        public class MyContext
        {
        }

        [Fact]
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
                        Columns = new[] { "Id", "MyColumn" },
                        Values = new object[,]
                        {
                            { 1, null },
                            { 2, RegexOptions.Multiline }
                        }
                    }
                },
                Array.Empty<MigrationOperation>());

            Assert.Contains("using System.Text.RegularExpressions;", migration);
        }

        [Fact]
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
                        KeyColumns = new[] { "Id" },
                        KeyValues = new object[,] { { 1 } },
                        Columns = new[] { "MyColumn" },
                        Values = new object[,] { { RegexOptions.Multiline } }
                    }
                },
                Array.Empty<MigrationOperation>());

            Assert.Contains("using System.Text.RegularExpressions;", migration);
        }

        [Fact]
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
                        KeyColumns = new[] { "Id" },
                        KeyValues = new object[,] { { RegexOptions.Multiline } },
                        Columns = new[] { "MyColumn" },
                        Values = new object[,] { { 1 } }
                    }
                },
                Array.Empty<MigrationOperation>());

            Assert.Contains("using System.Text.RegularExpressions;", migration);
        }

        [Fact]
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
                        KeyColumns = new[] { "Id" },
                        KeyValues = new object[,] { { RegexOptions.Multiline } }
                    }
                },
                Array.Empty<MigrationOperation>());

            Assert.Contains("using System.Text.RegularExpressions;", migration);
        }

        private static IMigrationsCodeGenerator CreateMigrationsCodeGenerator()
            => new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddEntityFrameworkDesignTimeServices()
                .BuildServiceProvider()
                .GetRequiredService<IMigrationsCodeGenerator>();
    }
}
