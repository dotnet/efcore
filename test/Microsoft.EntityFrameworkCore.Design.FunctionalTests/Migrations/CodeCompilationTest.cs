// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.Migrations
{
    public class CodeCompilationTest
    {
        [Fact]
        public void Migrations_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationsGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpSnapshotGenerator(codeHelper));

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
                new MigrationOperation[0]);
            Assert.Equal(
                @"using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.RegularExpressions;

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
                migrationCode);

            var migrationMetadataCode = generator.GenerateMetadata(
                "MyNamespace",
                typeof(MyContext),
                "MyMigration",
                "20150511161616_MyMigration",
                new Model { ["Some:EnumValue"] = RegexOptions.Multiline });
            Assert.Equal(
                @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.FunctionalTests.Migrations;
using System.Text.RegularExpressions;

namespace MyNamespace
{
    [DbContext(typeof(CodeCompilationTest.MyContext))]
    [Migration(""20150511161616_MyMigration"")]
    partial class MyMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);
        }
    }
}
",
                migrationMetadataCode);

            var build = new BuildSource
            {
                References =
                {
#if NETCOREAPP1_1
                    BuildReference.ByName("System.Text.RegularExpressions"),
#else
                    BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.FunctionalTests", depContextAssembly: GetType().GetTypeInfo().Assembly),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources = { migrationCode, migrationMetadataCode }
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

        [Fact]
        public void Snapshots_compile()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationsGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpSnapshotGenerator(codeHelper));

            var model = new Model { ["Some:EnumValue"] = RegexOptions.Multiline };
            var entityType = model.AddEntityType("Cheese");
            entityType.AddProperty("Pickle", typeof(StringBuilder));

            var modelSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                typeof(MyContext),
                "MySnapshot",
                model);
            Assert.Equal(@"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.FunctionalTests.Migrations;
using System.Text;
using System.Text.RegularExpressions;

namespace MyNamespace
{
    [DbContext(typeof(CodeCompilationTest.MyContext))]
    partial class MySnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation(""Some:EnumValue"", RegexOptions.Multiline);

            modelBuilder.Entity(""Cheese"", b =>
                {
                    b.Property<StringBuilder>(""Pickle"");

                    b.ToTable(""Cheese"");
                });
        }
    }
}
", modelSnapshotCode);

            var snapshot = CompileModelSnapshot(modelSnapshotCode, "MyNamespace.MySnapshot");
            Assert.Equal(1, snapshot.Model.GetEntityTypes().Count());
        }

        [Fact]
        public void Snapshot_with_default_values_are_round_tripped()
        {
            var codeHelper = new CSharpHelper();
            var generator = new CSharpMigrationsGenerator(
                codeHelper,
                new CSharpMigrationOperationGenerator(codeHelper),
                new CSharpSnapshotGenerator(codeHelper));

            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<EntityWithEveryPrimitive>(eb =>
                {
                    eb.Property(e => e.Boolean).HasDefaultValue(false);
                    eb.Property(e => e.Byte).HasDefaultValue((byte)0);
                    eb.Property(e => e.ByteArray).HasDefaultValue(new byte[] { 0 });
                    eb.Property(e => e.Char).HasDefaultValue('0');
                    eb.Property(e => e.DateTime).HasDefaultValue(new DateTime(1980, 1, 1));
                    eb.Property(e => e.DateTimeOffset).HasDefaultValue(new DateTimeOffset(1980, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0)));
                    eb.Property(e => e.Decimal).HasDefaultValue(0m);
                    eb.Property(e => e.Double).HasDefaultValue(0.0);
                    eb.Property(e => e.Enum).HasDefaultValue(Enum1.Default);
                    eb.Property(e => e.Guid).HasDefaultValue(new Guid());
                    eb.Property(e => e.Int16).HasDefaultValue((short)0);
                    eb.Property(e => e.Int32).HasDefaultValue(0);
                    eb.Property(e => e.Int64).HasDefaultValue(0L);
                    eb.Property(e => e.Single).HasDefaultValue((float)0.0);
                    eb.Property(e => e.SByte).HasDefaultValue((sbyte)0);
                    eb.Property(e => e.String).HasDefaultValue("'\"'");
                    eb.Property(e => e.TimeSpan).HasDefaultValue(new TimeSpan(0, 0, 0));
                    eb.Property(e => e.UInt16).HasDefaultValue((ushort)0);
                    eb.Property(e => e.UInt32).HasDefaultValue(0U);
                    eb.Property(e => e.UInt64).HasDefaultValue(0UL);
                    eb.Property(e => e.NullableBoolean).HasDefaultValue(true);
                    eb.Property(e => e.NullableByte).HasDefaultValue(byte.MaxValue);
                    eb.Property(e => e.NullableChar).HasDefaultValue('\'');
                    eb.Property(e => e.NullableDateTime).HasDefaultValue(new DateTime(1900, 12, 31));
                    eb.Property(e => e.NullableDateTimeOffset).HasDefaultValue(new DateTimeOffset(3000, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0)));
                    eb.Property(e => e.NullableDecimal).HasDefaultValue(2m * long.MaxValue);
                    eb.Property(e => e.NullableDouble).HasDefaultValue(0.6822871999174);
                    eb.Property(e => e.NullableEnum).HasDefaultValue(Enum1.Default);
                    eb.Property(e => e.NullableGuid).HasDefaultValue(new Guid());
                    eb.Property(e => e.NullableInt16).HasDefaultValue(short.MinValue);
                    eb.Property(e => e.NullableInt32).HasDefaultValue(int.MinValue);
                    eb.Property(e => e.NullableInt64).HasDefaultValue(long.MinValue);
                    eb.Property(e => e.NullableSingle).HasDefaultValue(0.3333333f);
                    eb.Property(e => e.NullableSByte).HasDefaultValue(sbyte.MinValue);
                    eb.Property(e => e.NullableTimeSpan).HasDefaultValue(new TimeSpan(-1, 0, 0));
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

        private enum Enum1
        {
            Default
        }

        private ModelSnapshot CompileModelSnapshot(string modelSnapshotCode, string modelSnapshotTypeName)
        {
            var build = new BuildSource
            {
                References =
                {
#if NETCOREAPP1_1
                    BuildReference.ByName("System.Runtime"),
                    BuildReference.ByName("System.Text.RegularExpressions"),
#else
                    BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.FunctionalTests", depContextAssembly: GetType().GetTypeInfo().Assembly),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources = { modelSnapshotCode }
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
    }
}
