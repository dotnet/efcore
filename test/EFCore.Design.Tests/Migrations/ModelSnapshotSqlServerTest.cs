﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedTypeParameter
namespace Microsoft.EntityFrameworkCore.Migrations;

public class ModelSnapshotSqlServerTest
{
    private class EntityWithManyProperties
    {
        public int Id { get; set; }
        public string String { get; set; }
        public byte[] Bytes { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public float Single { get; set; }
        public bool Boolean { get; set; }
        public byte Byte { get; set; }
        public ushort UnsignedInt16 { get; set; }
        public uint UnsignedInt32 { get; set; }
        public ulong UnsignedInt64 { get; set; }
        public char Character { get; set; }
        public sbyte SignedByte { get; set; }
        public Enum64 Enum64 { get; set; }
        public Enum32 Enum32 { get; set; }
        public Enum16 Enum16 { get; set; }
        public Enum8 Enum8 { get; set; }
        public EnumU64 EnumU64 { get; set; }
        public EnumU32 EnumU32 { get; set; }
        public EnumU16 EnumU16 { get; set; }
        public EnumS8 EnumS8 { get; set; }
        public Geometry SpatialBGeometryCollection { get; set; }
        public Geometry SpatialBLineString { get; set; }
        public Geometry SpatialBMultiLineString { get; set; }
        public Geometry SpatialBMultiPoint { get; set; }
        public Geometry SpatialBMultiPolygon { get; set; }
        public Geometry SpatialBPoint { get; set; }
        public Geometry SpatialBPolygon { get; set; }
        public GeometryCollection SpatialCGeometryCollection { get; set; }
        public LineString SpatialCLineString { get; set; }
        public MultiLineString SpatialCMultiLineString { get; set; }
        public MultiPoint SpatialCMultiPoint { get; set; }
        public MultiPolygon SpatialCMultiPolygon { get; set; }
        public Point SpatialCPoint { get; set; }
        public Polygon SpatialCPolygon { get; set; }
    }

    private enum Enum64 : long
    {
        SomeValue = 1
    }

    private enum Enum32
    {
        SomeValue = 1
    }

    private enum Enum16 : short
    {
        SomeValue = 1
    }

    private enum Enum8 : byte
    {
        SomeValue = 1
    }

    private enum EnumU64 : ulong
    {
        SomeValue = 1234567890123456789UL
    }

    private enum EnumU32 : uint
    {
        SomeValue = uint.MaxValue
    }

    private enum EnumU16 : ushort
    {
        SomeValue = ushort.MaxValue
    }

    private enum EnumS8 : sbyte
    {
        SomeValue = sbyte.MinValue
    }

    private class EntityWithOneProperty
    {
        public int Id { get; set; }
        public EntityWithTwoProperties EntityWithTwoProperties { get; set; }
    }

    private class EntityWithTwoProperties
    {
        [Key]
        public int Id { get; set; }

        public int AlternateId { get; set; }
        public EntityWithOneProperty EntityWithOneProperty { get; set; }

        [NotMapped]
        public EntityWithStringKey EntityWithStringKey { get; set; }
    }

    private class EntityWithStringProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public EntityWithOneProperty EntityWithOneProperty { get; set; }
    }

    private class EntityWithDecimalProperty
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }

    private class EntityWithStringKey
    {
        public string Id { get; set; }
        public ICollection<EntityWithStringProperty> Properties { get; set; }
    }

    private class EntityWithStringAlternateKey
    {
        public int Id { get; set; }
        public string AlternateId { get; set; }
        public ICollection<EntityWithStringProperty> Properties { get; set; }
    }

    private class EntityWithGenericKey<TKey>
    {
        public Guid Id { get; set; }
    }

    private class EntityWithGenericProperty<TProperty>
    {
        public int Id { get; set; }
        public TProperty Property { get; set; }
    }

    private class EntityWithThreeProperties
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    [Index(nameof(FirstName), nameof(LastName))]
    private class EntityWithIndexAttribute
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Index(nameof(FirstName), nameof(LastName), Name = "NamedIndex")]
    private class EntityWithNamedIndexAttribute
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Index(nameof(FirstName), nameof(LastName), IsUnique = true)]
    private class EntityWithUniqueIndexAttribute
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class TestOwner
    {
        public int Id { get; set; }
        public ICollection<TestOwnee> OwnedEntities { get; set; }
    }

    public class TestOwnee
    {
        public int Id { get; set; }
        public TestEnum TestEnum { get; set; }
    }

    public enum TestEnum
    {
        Value0 = 0,
        Value1,
        Value2
    }

    private abstract class AbstractBase
    {
        public int Id { get; set; }
    }

    private class BaseEntity : AbstractBase
    {
        public string Discriminator { get; set; }
    }

    private class DerivedEntity : BaseEntity
    {
        public string Name { get; set; }
    }

    private class DuplicateDerivedEntity : BaseEntity
    {
        public string Name { get; set; }
    }

    private class AnotherDerivedEntity : BaseEntity
    {
        public string Title { get; set; }
    }

    private readonly struct StructDiscriminator
    {
        public string Value { get; init; }
    }

    private class BaseEntityWithStructDiscriminator
    {
        public int Id { get; set; }

        public StructDiscriminator Discriminator { get; set; }
    }

    private class DerivedEntityWithStructDiscriminator : BaseEntityWithStructDiscriminator
    {
        public string Name { get; set; }
    }

    private class AnotherDerivedEntityWithStructDiscriminator : BaseEntityWithStructDiscriminator
    {
        public string Title { get; set; }
    }

    private class BaseType
    {
        public int Id { get; set; }

        public EntityWithOneProperty Navigation { get; set; }
    }

    private class DerivedType : BaseType
    {
    }

    private enum Days : long
    {
        Sun,
        Mon,
        Tue,
        Wed,
        Thu,
        Fri,
        Sat
    }

    private class EntityWithEnumType
    {
        public int Id { get; set; }
        public Days Day { get; set; }
    }

    private class EntityWithNullableEnumType
    {
        public int Id { get; set; }
        public Days? Day { get; set; }
    }

    private class ManyToManyLeft
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ManyToManyRight> Rights { get; set; }
    }

    private class ManyToManyRight
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public List<ManyToManyLeft> Lefts { get; set; }
    }

    private class CustomValueGenerator : ValueGenerator<int>
    {
        public override int Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }

    private abstract class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private abstract class Pet : Animal
    {
        public string Vet { get; set; }
        public ICollection<Human> Humans { get; } = new List<Human>();
    }

    private class Cat : Pet
    {
        public string EducationLevel { get; set; }
    }

    private class Dog : Pet
    {
        public string FavoriteToy { get; set; }
    }

    private class Human : Animal
    {
        public Animal FavoriteAnimal { get; set; }
        public ICollection<Pet> Pets { get; } = new List<Pet>();
    }

    public abstract class BarBase
    {
        public int Id { get; set; }
    }

    public class BarA : BarBase
    {
    }

    public class FooExtension<T>
        where T : BarBase
    {
        public int Id { get; set; }

        public T Bar { get; set; }
    }

    #region Model

    [ConditionalFact]
    public virtual void Model_annotations_are_stored_in_snapshot()
        => Test(
            builder => builder.HasAnnotation("AnnotationName", "AnnotationValue")
                .HasDatabaseMaxSize("100 MB")
                .HasServiceTier("basic")
                .HasPerformanceLevel("S0"),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("AnnotationName", "AnnotationValue")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
            SqlServerModelBuilderExtensions.HasDatabaseMaxSize(modelBuilder, "100 MB");
            SqlServerModelBuilderExtensions.HasServiceTierSql(modelBuilder, "'basic'");
            SqlServerModelBuilderExtensions.HasPerformanceLevelSql(modelBuilder, "'S0'");
"""),
            o =>
            {
                Assert.Equal(9, o.GetAnnotations().Count());
                Assert.Equal("AnnotationValue", o["AnnotationName"]);
            });

    [ConditionalFact]
    public virtual void Model_Fluent_APIs_are_properly_generated()
        => Test(
            builder =>
            {
                builder.UseHiLo();
                builder.Entity<EntityWithOneProperty>();
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseHiLo(modelBuilder, "EntityFrameworkHiLoSequence");

            modelBuilder.HasSequence("EntityFrameworkHiLoSequence")
                .IncrementsBy(10);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, o.GetValueGenerationStrategy());
                Assert.Equal(
                    SqlServerValueGenerationStrategy.SequenceHiLo,
                    o.GetEntityTypes().Single().GetProperty("Id").GetValueGenerationStrategy());
            });

    [ConditionalFact]
    public virtual void Model_fluent_APIs_for_sequence_key_are_properly_generated()
        => Test(
            builder =>
            {
                builder.UseKeySequences();
                builder.Entity<EntityWithOneProperty>();
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseKeySequences(modelBuilder, "Sequence");

            modelBuilder.HasSequence("EntityWithOnePropertySequence");

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR [DefaultSchema].[EntityWithOnePropertySequence]");

                    SqlServerPropertyBuilderExtensions.UseSequence(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(SqlServerValueGenerationStrategy.Sequence, o.GetValueGenerationStrategy());
                Assert.Equal(
                    SqlServerValueGenerationStrategy.Sequence,
                    o.GetEntityTypes().Single().GetProperty("Id").GetValueGenerationStrategy());
            });

    [ConditionalFact]
    public virtual void Model_default_schema_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.HasDefaultSchema("DefaultSchema");
                builder.HasAnnotation("AnnotationName", "AnnotationValue");
            },
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("AnnotationName", "AnnotationValue")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
"""),
            o =>
            {
                Assert.Equal(6, o.GetAnnotations().Count());
                Assert.Equal("AnnotationValue", o["AnnotationName"]);
                Assert.Equal("DefaultSchema", o[RelationalAnnotationNames.DefaultSchema]);
            });

    [ConditionalFact]
    public virtual void Entities_are_stored_in_model_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties);
                builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(2, o.GetEntityTypes().Count());
                Assert.Collection(
                    o.GetEntityTypes(),
                    t => Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", t.Name),
                    t => Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", t.Name));
            });

    [ConditionalFact]
    public virtual void Entities_are_stored_in_model_snapshot_for_TPT()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>()
                    .ToTable("DerivedEntity", "foo");
                builder.Entity<BaseEntity>();
                builder.Entity<AbstractBase>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("AbstractBase", "DefaultSchema");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase");

                    b.ToTable("BaseEntity", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("DerivedEntity", "foo");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", null)
                        .WithOne()
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
"""),
            model =>
            {
                Assert.Equal(5, model.GetAnnotations().Count());
                Assert.Equal(3, model.GetEntityTypes().Count());

                var abstractBase = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase");
                Assert.Equal("AbstractBase", abstractBase.GetTableName());
                Assert.Equal("TPT", abstractBase.GetMappingStrategy());

                var baseType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");
                Assert.Equal("BaseEntity", baseType.GetTableName());
                Assert.Equal("DefaultSchema", baseType.GetSchema());

                var derived = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity");
                Assert.Equal("DerivedEntity", derived.GetTableName());
                Assert.Equal("foo", derived.GetSchema());
            });

    [ConditionalFact]
    public virtual void Entities_are_stored_in_model_snapshot_for_TPT_with_one_excluded()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>()
                    .ToTable("DerivedEntity", "foo", t => t.ExcludeFromMigrations());
                builder.Entity<BaseEntity>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntity", "DefaultSchema");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("DerivedEntity", "foo", t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", null)
                        .WithOne()
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
"""),
            o =>
            {
                Assert.Equal(5, o.GetAnnotations().Count());

                Assert.Equal(
                    "DerivedEntity",
                    o.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity")
                        .GetTableName());
            });

    [ConditionalFact]
    public void Views_are_stored_in_the_model_snapshot()
        => Test(
            builder => builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties).ToView("EntityWithOneProperty"),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.ToView("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o => Assert.Equal("EntityWithOneProperty", o.GetEntityTypes().Single().GetViewName()));

    [ConditionalFact]
    public void Views_with_schemas_are_stored_in_the_model_snapshot()
        => Test(
            builder => builder.Entity<EntityWithOneProperty>()
                .Ignore(e => e.EntityWithTwoProperties)
                .ToView("EntityWithOneProperty", "ViewSchema"),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.ToView("EntityWithOneProperty", "ViewSchema");
                });
"""),
            o =>
            {
                Assert.Equal("EntityWithOneProperty", o.GetEntityTypes().Single().GetViewName());
                Assert.Equal("ViewSchema", o.GetEntityTypes().Single().GetViewSchema());
            });

    [ConditionalFact]
    public virtual void Entities_are_stored_in_model_snapshot_for_TPC()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>()
                    .ToTable("DerivedEntity", "foo")
                    .ToView("DerivedView", "foo");
                builder.Entity<BaseEntity>();
                builder.Entity<AbstractBase>().UseTpcMappingStrategy();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.HasSequence("AbstractBaseSequence");

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR [DefaultSchema].[AbstractBaseSequence]");

                    SqlServerPropertyBuilderExtensions.UseSequence(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.UseTpcMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase");

                    b.ToTable("BaseEntity", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("DerivedEntity", "foo");

                    b.ToView("DerivedView", "foo");
                });
"""),
            model =>
            {
                Assert.Equal(6, model.GetAnnotations().Count());
                Assert.Equal(3, model.GetEntityTypes().Count());

                var abstractBase = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AbstractBase");
                Assert.Null(abstractBase.GetTableName());
                Assert.Null(abstractBase.GetViewName());
                Assert.Equal("TPC", abstractBase.GetMappingStrategy());

                var baseType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");
                Assert.Equal("BaseEntity", baseType.GetTableName());
                Assert.Null(baseType.GetViewName());

                var derived = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity");
                Assert.Equal("DerivedEntity", derived.GetTableName());
                Assert.Equal("DerivedView", derived.GetViewName());
            });

    [ConditionalFact] // Issue #30058
    public virtual void Non_base_abstract_base_class_with_TPC()
        => Test(
            builder =>
            {
                builder.Entity<Animal>().UseTpcMappingStrategy();
                builder.Entity<Pet>();
                builder.Entity<Cat>();
                builder.Entity<Dog>();
                builder.Entity<Human>();
            },
"""
// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence("AnimalSequence");

            modelBuilder.Entity("HumanPet", b =>
                {
                    b.Property<int>("HumansId")
                        .HasColumnType("int");

                    b.Property<int>("PetsId")
                        .HasColumnType("int");

                    b.HasKey("HumansId", "PetsId");

                    b.HasIndex("PetsId");

                    b.ToTable("HumanPet", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Animal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR [DefaultSchema].[AnimalSequence]");

                    SqlServerPropertyBuilderExtensions.UseSequence(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.UseTpcMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Human", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Animal");

                    b.Property<int?>("FavoriteAnimalId")
                        .HasColumnType("int");

                    b.HasIndex("FavoriteAnimalId");

                    b.ToTable("Human", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Pet", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Animal");

                    b.Property<string>("Vet")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable((string)null);
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Cat", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Pet");

                    b.Property<string>("EducationLevel")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Cat", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Dog", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Pet");

                    b.Property<string>("FavoriteToy")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Dog", "DefaultSchema");
                });

            modelBuilder.Entity("HumanPet", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Human", null)
                        .WithMany()
                        .HasForeignKey("HumansId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Pet", null)
                        .WithMany()
                        .HasForeignKey("PetsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Human", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Animal", "FavoriteAnimal")
                        .WithMany()
                        .HasForeignKey("FavoriteAnimalId");

                    b.Navigation("FavoriteAnimal");
                });
#pragma warning restore 612, 618
        }
    }
}

""",
            model =>
            {
                Assert.Equal(6, model.GetAnnotations().Count());
                Assert.Equal(6, model.GetEntityTypes().Count());

                var animalType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Animal");
                Assert.Null(animalType.GetTableName());
                Assert.Null(animalType.GetViewName());
                Assert.Equal("TPC", animalType.GetMappingStrategy());

                var petType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Pet");
                Assert.Null(petType.GetTableName());
                Assert.Null(petType.GetViewName());

                var catType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Cat");
                Assert.Equal("Cat", catType.GetTableName());
                Assert.Null(catType.GetViewName());

                var dogType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Dog");
                Assert.Equal("Dog", dogType.GetTableName());
                Assert.Null(dogType.GetViewName());

                var humanType = model.FindEntityType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Human");
                Assert.Equal("Human", humanType.GetTableName());
                Assert.Null(humanType.GetViewName());

                var humanPetType = model.FindEntityType("HumanPet");
                Assert.Equal("HumanPet", humanPetType.GetTableName());
                Assert.Null(humanPetType.GetViewName());
            });

    [ConditionalFact]
    public virtual void Entity_splitting_is_stored_in_snapshot_with_tables()
        => Test(
            builder =>
            {
                builder.Entity<Order>(
                    b =>
                    {
                        b.Ignore(e => e.OrderInfo);

                        b.Property<int>("Shadow").HasColumnName("Shadow");
                        b.ToTable(
                            "Order", "DefaultSchema", tb =>
                            {
                                tb.Property(e => e.Id).UseIdentityColumn(2, 3).HasAnnotation("fii", "arr");
                                tb.Property("Shadow");
                            });
                        b.SplitToTable(
                            "SplitOrder", "DefaultSchema", sb =>
                            {
                                sb.Property("Shadow");
                                sb.HasTrigger("splitTrigger").HasAnnotation("oof", "rab");
                                sb.HasAnnotation("foo", "bar");
                            });

                        b.OwnsOne(
                            p => p.OrderBillingDetails, od =>
                            {
                                od.OwnsOne(c => c.StreetAddress);

                                od.Property<int>("BillingShadow");
                                od.ToTable(
                                    "SplitOrder", "DefaultSchema", tb =>
                                    {
                                        tb.Property("BillingShadow").HasColumnName("Shadow");
                                    });
                                od.SplitToTable(
                                    "BillingDetails", "DefaultSchema", sb =>
                                    {
                                        sb.Property("BillingShadow").HasColumnName("Shadow");
                                    });
                            });

                        b.OwnsOne(
                            p => p.OrderShippingDetails, od =>
                            {
                                od.OwnsOne(c => c.StreetAddress).ToTable("ShippingDetails");

                                od.Property<int>("ShippingShadow");
                                od.ToTable(
                                    "Order", "DefaultSchema", tb =>
                                    {
                                        tb.Property("ShippingShadow").HasColumnName("Shadow");
                                    });
                                od.SplitToTable(
                                    "ShippingDetails", "DefaultSchema", sb =>
                                    {
                                        sb.Property("ShippingShadow");
                                    });
                            });
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Shadow")
                        .HasColumnType("int")
                        .HasColumnName("Shadow");

                    b.HasKey("Id");

                    b.ToTable("Order", "DefaultSchema", t =>
                        {
                            t.Property("Id")
                                .HasAnnotation("fii", "arr")
                                .HasAnnotation("SqlServer:IdentityIncrement", 3)
                                .HasAnnotation("SqlServer:IdentitySeed", 2L)
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            t.Property("Shadow");
                        });

                    b.SplitToTable("SplitOrder", "DefaultSchema", t =>
                        {
                            t.HasTrigger("splitTrigger")
                                .HasAnnotation("oof", "rab");

                            t.Property("Shadow");

                            t.HasAnnotation("foo", "bar");
                        });

                    b.HasAnnotation("SqlServer:UseSqlOutputClause", false);
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", null)
                        .WithOne()
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderBillingDetails", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.Property<int>("BillingShadow")
                                .HasColumnType("int");

                            b1.HasKey("OrderId");

                            b1.ToTable("SplitOrder", "DefaultSchema", t =>
                                {
                                    t.Property("BillingShadow")
                                        .HasColumnName("Shadow");
                                });

                            b1.SplitToTable("BillingDetails", "DefaultSchema", t =>
                                {
                                    t.Property("BillingShadow")
                                        .HasColumnName("Shadow");
                                });

                            b1.WithOwner()
                                .HasForeignKey("OrderId");

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order.OrderBillingDetails#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", null)
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order.OrderBillingDetails#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress", "StreetAddress", b2 =>
                                {
                                    b2.Property<int>("OrderDetailsOrderId")
                                        .HasColumnType("int");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("OrderDetailsOrderId");

                                    b2.ToTable("SplitOrder", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("OrderDetailsOrderId");
                                });

                            b1.Navigation("StreetAddress");
                        });

                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderShippingDetails", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.Property<int>("ShippingShadow")
                                .HasColumnType("int");

                            b1.HasKey("OrderId");

                            b1.ToTable("Order", "DefaultSchema", t =>
                                {
                                    t.Property("ShippingShadow")
                                        .HasColumnName("Shadow");
                                });

                            b1.SplitToTable("ShippingDetails", "DefaultSchema", t =>
                                {
                                    t.Property("ShippingShadow");
                                });

                            b1.WithOwner()
                                .HasForeignKey("OrderId");

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order.OrderShippingDetails#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", null)
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order.OrderShippingDetails#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress", "StreetAddress", b2 =>
                                {
                                    b2.Property<int>("OrderDetailsOrderId")
                                        .HasColumnType("int");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("OrderDetailsOrderId");

                                    b2.ToTable("ShippingDetails", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("OrderDetailsOrderId");
                                });

                            b1.Navigation("StreetAddress");
                        });

                    b.Navigation("OrderBillingDetails");

                    b.Navigation("OrderShippingDetails");
                });
"""),
            model =>
            {
                Assert.Equal(5, model.GetEntityTypes().Count());

                var orderEntityType = model.FindEntityType(typeof(Order));
                Assert.Equal(nameof(Order), orderEntityType.GetTableName());

                var id = orderEntityType.FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, id.GetValueGenerationStrategy());
                Assert.Equal(1, id.GetIdentitySeed());
                Assert.Equal(1, id.GetIdentityIncrement());

                var overrides = id.FindOverrides(StoreObjectIdentifier.Create(orderEntityType, StoreObjectType.Table).Value)!;
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, overrides.GetValueGenerationStrategy());
                Assert.Equal(2, overrides.GetIdentitySeed());
                Assert.Equal(3, overrides.GetIdentityIncrement());
                Assert.Equal("arr", overrides["fii"]);

                var billingOwnership = orderEntityType.FindNavigation(nameof(Order.OrderBillingDetails))
                    .ForeignKey;
                var billingEntityType = billingOwnership.DeclaringEntityType;
                Assert.Equal("SplitOrder", billingEntityType.GetTableName());

                var billingAddressOwnership = billingEntityType.FindNavigation(nameof(OrderDetails.StreetAddress))
                    .ForeignKey;
                var billingAddress = billingAddressOwnership.DeclaringEntityType;
                Assert.Equal("SplitOrder", billingAddress.GetTableName());

                var shippingOwnership = orderEntityType.FindNavigation(nameof(Order.OrderShippingDetails))
                    .ForeignKey;
                var shippingEntityType = shippingOwnership.DeclaringEntityType;
                Assert.Equal(nameof(Order), shippingEntityType.GetTableName());

                var shippingAddressOwnership = shippingEntityType.FindNavigation(nameof(OrderDetails.StreetAddress))
                    .ForeignKey;
                var shippingAddress = shippingAddressOwnership.DeclaringEntityType;
                Assert.Equal("ShippingDetails", shippingAddress.GetTableName());

                var relationalModel = model.GetRelationalModel();

                Assert.Equal(4, relationalModel.Tables.Count());

                var orderTable = relationalModel.FindTable(orderEntityType.GetTableName()!, orderEntityType.GetSchema());
                Assert.Equal(
                    new[] { orderEntityType, shippingEntityType },
                    orderTable.FindColumn("Shadow").PropertyMappings.Select(m => m.TableMapping.TypeBase));

                var fragment = orderEntityType.GetMappingFragments().Single();
                var splitTable = relationalModel.FindTable(fragment.StoreObject.Name, fragment.StoreObject.Schema);
                Assert.Equal(
                    new[] { billingEntityType, orderEntityType },
                    splitTable.FindColumn("Shadow").PropertyMappings.Select(m => m.TableMapping.TypeBase));
                Assert.Equal("bar", fragment["foo"]);

                var trigger = orderEntityType.GetDeclaredTriggers().Single();
                Assert.Equal(splitTable.Name, trigger.GetTableName());
                Assert.Equal(splitTable.Schema, trigger.GetTableSchema());
                Assert.Equal("rab", trigger["oof"]);

                var billingFragment = billingEntityType.GetMappingFragments().Single();
                var billingTable = relationalModel.FindTable(billingFragment.StoreObject.Name, billingFragment.StoreObject.Schema);
                Assert.Equal(
                    new[] { billingEntityType },
                    billingTable.FindColumn("Shadow").PropertyMappings.Select(m => m.TableMapping.TypeBase));

                var shippingFragment = shippingEntityType.GetMappingFragments().Single();
                var shippingTable = relationalModel.FindTable(shippingFragment.StoreObject.Name, shippingFragment.StoreObject.Schema);
                Assert.Equal(
                    new[] { shippingEntityType },
                    shippingTable.FindColumn("ShippingShadow").PropertyMappings.Select(m => m.TableMapping.TypeBase));

                Assert.Equal(new[] { "Id", "Shadow" }, orderTable.Columns.Select(c => c.Name));
                Assert.Equal(new[] { "Id", "OrderBillingDetails_StreetAddress_City", "Shadow" }, splitTable.Columns.Select(c => c.Name));
                Assert.Equal(new[] { "OrderId", "Shadow" }, billingTable.Columns.Select(c => c.Name));
                Assert.Equal(new[] { "OrderId", "ShippingShadow", "StreetAddress_City" }, shippingTable.Columns.Select(c => c.Name));
            });

    [ConditionalFact]
    public virtual void Entity_splitting_is_stored_in_snapshot_with_views()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.Property<int>("Shadow");
                        b.ToView(
                            "EntityWithOneProperty", tb =>
                            {
                                tb.Property("Shadow");
                            });
                        b.SplitToView(
                            "SplitView", sb =>
                            {
                                sb.Property("Shadow");
                            });

                        b.OwnsOne(
                            eo => eo.EntityWithTwoProperties, eb =>
                            {
                                eb.Ignore(e => e.EntityWithStringKey);

                                eb.ToView(
                                    "EntityWithOneProperty", tb =>
                                    {
                                        tb.Property(e => e.AlternateId).HasColumnName("SomeId");
                                    });
                                eb.SplitToView(
                                    "SplitView", sb =>
                                    {
                                        sb.Property(e => e.AlternateId).HasColumnName("SomeOtherId");
                                    });
                            });
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("Shadow")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.ToView("EntityWithOneProperty", "DefaultSchema", v =>
                        {
                            v.Property("Shadow");
                        });

                    b.SplitToView("SplitView", "DefaultSchema", v =>
                        {
                            v.Property("Shadow");
                        });
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties", b1 =>
                        {
                            b1.Property<int>("Id")
                                .HasColumnType("int");

                            b1.Property<int>("AlternateId")
                                .HasColumnType("int");

                            b1.HasKey("Id");

                            b1.ToTable((string)null);

                            b1.ToView("EntityWithOneProperty", "DefaultSchema", v =>
                                {
                                    v.Property("AlternateId")
                                        .HasColumnName("SomeId");
                                });

                            b1.SplitToView("SplitView", "DefaultSchema", v =>
                                {
                                    v.Property("AlternateId")
                                        .HasColumnName("SomeOtherId");
                                });

                            b1.WithOwner("EntityWithOneProperty")
                                .HasForeignKey("Id");

                            b1.Navigation("EntityWithOneProperty");
                        });

                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            model =>
            {
                var entityWithOneProperty = model.FindEntityType(typeof(EntityWithOneProperty));
                Assert.Equal(nameof(EntityWithOneProperty), entityWithOneProperty.GetViewName());

                var ownership = entityWithOneProperty.FindNavigation(nameof(EntityWithOneProperty.EntityWithTwoProperties))
                    .ForeignKey;
                var ownedType = ownership.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithOneProperty), ownedType.GetViewName());

                var relationalModel = model.GetRelationalModel();

                Assert.Empty(relationalModel.Tables);
                Assert.Equal(2, relationalModel.Views.Count());

                var mainView = relationalModel.FindView(entityWithOneProperty.GetViewName(), "DefaultSchema");

                var fragment = entityWithOneProperty.GetMappingFragments().Single();
                var splitView = relationalModel.FindView(fragment.StoreObject.Name, fragment.StoreObject.Schema);

                Assert.Equal(new[] { "Id", "Shadow", "SomeId" }, mainView.Columns.Select(c => c.Name));
                Assert.Equal(new[] { "Id", "Shadow", "SomeOtherId" }, splitView.Columns.Select(c => c.Name));
            });

    [ConditionalFact]
    public void Unmapped_entity_types_are_stored_in_the_model_snapshot()
        => Test(
            builder =>
            {
                builder.HasDefaultSchema("default");
                builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties).ToTable((string)null)
                    .UpdateUsingStoredProcedure("Update", "sproc", p => p.HasParameter(e => e.Id));
            },
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("default")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable((string)null);
                });
"""),
            o =>
            {
                Assert.Null(o.GetEntityTypes().Single().GetTableName());
                Assert.Null(o.GetEntityTypes().Single().GetSchema());
            });

    private class TestKeylessType
    {
        public string Something { get; set; }
    }

    private static IQueryable<TestKeylessType> GetCountByYear(int id)
        => throw new NotImplementedException();

    [ConditionalFact]
    public void TVF_types_are_stored_in_the_model_snapshot()
        => Test(
            builder =>
            {
                builder.HasDbFunction(
                    typeof(ModelSnapshotSqlServerTest).GetMethod(
                        nameof(GetCountByYear),
                        BindingFlags.NonPublic | BindingFlags.Static));

                builder.Entity<TestKeylessType>().HasNoKey().ToTable((string)null);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestKeylessType", b =>
                {
                    b.Property<string>("Something")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable((string)null);
                });
"""),
            o =>
            {
                var entityType = o.GetEntityTypes().Single();
                Assert.Null(entityType.GetFunctionName());
                Assert.Null(entityType.GetTableName());
            });

    [ConditionalFact]
    public void Entity_types_mapped_to_functions_are_stored_in_the_model_snapshot()
        => Test(
            builder =>
                builder.Entity<TestKeylessType>(
                    kb =>
                    {
                        kb.Property(k => k.Something);
                        kb.HasNoKey().ToFunction("GetCount");
                    }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestKeylessType", b =>
                {
                    b.Property<string>("Something")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable((string)null);

                    b.ToFunction("GetCount");
                });
"""),
            o => Assert.Equal("GetCount", o.GetEntityTypes().Single().GetFunctionName()));

    [ConditionalFact]
    public void Entity_types_mapped_to_queries_are_stored_in_the_model_snapshot()
        => Test(
            builder => builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties).ToSqlQuery("query"),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.ToSqlQuery("query");
                });
"""),
            o => Assert.Equal("query", o.GetEntityTypes().Single().GetSqlQuery()));

    [ConditionalFact]
    public virtual void Sequence_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.HasSequence<int>("Foo", "Bar")
                    .StartsAt(2)
                    .HasMin(1)
                    .HasMax(3)
                    .IncrementsBy(2)
                    .IsCyclic()
                    .HasAnnotation("foo", "bar");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.HasSequence<int>("Foo", "Bar")
                .StartsAt(2L)
                .IncrementsBy(2)
                .HasMin(1L)
                .HasMax(3L)
                .IsCyclic()
                .HasAnnotation("foo", "bar");
"""),
            model =>
            {
                Assert.Equal(6, model.GetAnnotations().Count());

                var sequence = model.GetSequences().Single();
                Assert.Equal(2, sequence.StartValue);
                Assert.Equal(1, sequence.MinValue);
                Assert.Equal(3, sequence.MaxValue);
                Assert.Equal(2, sequence.IncrementBy);
                Assert.True(sequence.IsCyclic);
                Assert.Equal("bar", sequence["foo"]);
            });

    [ConditionalFact]
    public virtual void HiLoSequence_with_default_model_schema()
        => Test(
            modelBuilder => modelBuilder
                .HasDefaultSchema("dbo")
                .Entity("Entity").Property<int>("Id").UseHiLo(schema: "dbo"),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence("EntityFrameworkHiLoSequence", "dbo")
                .IncrementsBy(10);

            modelBuilder.Entity("Entity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseHiLo(b.Property<int>("Id"), "EntityFrameworkHiLoSequence", "dbo");

                    b.HasKey("Id");

                    b.ToTable("Entity", "dbo");
                });
"""),
            model =>
            {
                Assert.Equal("dbo", model.GetDefaultSchema());

                var sequence = Assert.Single(model.GetSequences());
                Assert.Equal("dbo", sequence.Schema);
            });

    [ConditionalFact]
    public virtual void CheckConstraint_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().ToTable(
                    tb =>
                        tb.HasCheckConstraint("AlternateId", "AlternateId > Id")
                            .HasName("CK_Customer_AlternateId")
                            .HasAnnotation("foo", "bar"));
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema", t =>
                        {
                            t.HasCheckConstraint("AlternateId", "AlternateId > Id")
                                .HasName("CK_Customer_AlternateId")
                                .HasAnnotation("foo", "bar");
                        });
                });
"""),
            o =>
            {
                var constraint = o.GetEntityTypes().Single().GetCheckConstraints().Single();
                Assert.Equal("CK_Customer_AlternateId", constraint.Name);
                Assert.Equal("bar", constraint["foo"]);
            });

    [ConditionalFact]
    public virtual void CheckConstraint_is_only_stored_in_snapshot_once_for_TPH()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>()
                    .ToTable(tb => tb.HasCheckConstraint("CK_BaseEntity_AlternateId", "AlternateId > Id"));
                builder.Entity<BaseEntity>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntity", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BaseEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable(t =>
                        {
                            t.HasCheckConstraint("CK_BaseEntity_AlternateId", "AlternateId > Id");
                        });

                    b.HasDiscriminator().HasValue("DerivedEntity");
                });
"""),
            o =>
            {
                var constraint = o.FindEntityType(typeof(DerivedEntity)).GetDeclaredCheckConstraints().Single();
                Assert.Equal("CK_BaseEntity_AlternateId", constraint.Name);
            });

    [ConditionalFact]
    public virtual void Trigger_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .ToTable(tb => tb.HasTrigger("SomeTrigger").HasAnnotation("foo", "bar").HasDatabaseName("SomeTrg"));
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema", t =>
                        {
                            t.HasTrigger("SomeTrigger")
                                .HasDatabaseName("SomeTrg")
                                .HasAnnotation("foo", "bar");
                        });

                    b.HasAnnotation("SqlServer:UseSqlOutputClause", false);
                });
"""),
            o =>
            {
                var trigger = Assert.Single(o.GetEntityTypes().Single().GetDeclaredTriggers());
                Assert.Equal("SomeTrigger", trigger.ModelName);
                Assert.Equal("SomeTrg", trigger.GetDatabaseName());
            });

    [ConditionalFact]
    public virtual void Triggers_and_ExcludeFromMigrations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .ToTable(
                        tb =>
                        {
                            tb.HasTrigger("SomeTrigger1");
                            tb.HasTrigger("SomeTrigger2");
                            tb.ExcludeFromMigrations();
                        });
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema", t =>
                        {
                            t.ExcludeFromMigrations();

                            t.HasTrigger("SomeTrigger1");

                            t.HasTrigger("SomeTrigger2");
                        });

                    b.HasAnnotation("SqlServer:UseSqlOutputClause", false);
                });
"""),
            o =>
            {
                var entityType = Assert.Single(o.GetEntityTypes());

                Assert.True(entityType.IsTableExcludedFromMigrations());

                Assert.Collection(
                    entityType.GetDeclaredTriggers(),
                    t => Assert.Equal("SomeTrigger1", t.GetDatabaseName()),
                    t => Assert.Equal("SomeTrigger2", t.GetDatabaseName()));
            });

    [ConditionalFact]
    public virtual void ProductVersion_is_stored_in_snapshot()
    {
        var modelBuilder = CreateConventionalModelBuilder();
        var generator = CreateMigrationsGenerator();
        var code = generator.GenerateSnapshot("RootNamespace", typeof(DbContext), "Snapshot", (IModel)modelBuilder.Model);
        Assert.Contains(@".HasAnnotation(""ProductVersion"",", code);

        var modelFromSnapshot = BuildModelFromSnapshotSource(code);
        Assert.Equal(ProductInfo.GetVersion(), modelFromSnapshot.GetProductVersion());
    }

    [ConditionalFact]
    public virtual void Model_use_identity_columns()
        => Test(
            builder => builder.UseIdentityColumns(),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
"""),
            o =>
            {
                Assert.Equal(5, o.GetAnnotations().Count());
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, o.GetValueGenerationStrategy());
                Assert.Equal(1, o.GetIdentitySeed());
                Assert.Equal(1, o.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Model_use_identity_columns_custom_seed()
        => Test(
            builder => builder.UseIdentityColumns(5),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 5L);
"""),
            o =>
            {
                Assert.Equal(5, o.GetAnnotations().Count());
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, o.GetValueGenerationStrategy());
                Assert.Equal(5, o.GetIdentitySeed());
                Assert.Equal(1, o.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Model_use_identity_columns_custom_increment()
        => Test(
            builder => builder.UseIdentityColumns(increment: 5),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 5);
"""),
            o =>
            {
                Assert.Equal(5, o.GetAnnotations().Count());
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, o.GetValueGenerationStrategy());
                Assert.Equal(1, o.GetIdentitySeed());
                Assert.Equal(5, o.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Model_use_identity_columns_custom_seed_increment()
        => Test(
            builder =>
            {
                builder.UseIdentityColumns(long.MaxValue, 5);
                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id");

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 9223372036854775807L, 5);

            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 9223372036854775807L, 5);

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(5, o.GetAnnotations().Count());
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, o.GetValueGenerationStrategy());
                Assert.Equal(long.MaxValue, o.GetIdentitySeed());
                Assert.Equal(5, o.GetIdentityIncrement());

                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
                Assert.Equal(long.MaxValue, property.GetIdentitySeed());
                Assert.Equal(5, property.GetIdentityIncrement());
            });

    #endregion

    #region EntityType

    [ConditionalFact]
    public virtual void EntityType_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>().HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");

                    b.HasAnnotation("AnnotationName", "AnnotationValue");
                });
"""),
            o =>
            {
                Assert.Equal(3, o.GetEntityTypes().First().GetAnnotations().Count());
                Assert.Equal("AnnotationValue", o.GetEntityTypes().First()["AnnotationName"]);
            });

    [ConditionalFact]
    public virtual void EntityType_Fluent_APIs_are_properly_generated()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>().ToTable(tb => tb.IsMemoryOptimized());
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");

                    SqlServerEntityTypeBuilderExtensions.IsMemoryOptimized(b);
                });
"""),
            o => Assert.True(o.GetEntityTypes().Single().IsMemoryOptimized()));

    [ConditionalFact]
    public virtual void BaseType_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("nvarchar(21)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntity", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BaseEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("AnotherDerivedEntity");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("DerivedEntity");
                });
"""),
            o =>
            {
                Assert.Equal(3, o.GetEntityTypes().Count());
                Assert.Collection(
                    o.GetEntityTypes(),
                    t => Assert.Equal(
                        "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity", t.Name),
                    t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", t.Name),
                    t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", t.Name)
                );
            });

    [ConditionalFact]
    public virtual void Discriminator_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
                builder.Entity<BaseEntity>()
                    .HasDiscriminator(e => e.Discriminator)
                    .IsComplete()
                    .HasValue(typeof(BaseEntity), typeof(BaseEntity).Name)
                    .HasValue(typeof(DerivedEntity), typeof(DerivedEntity).Name)
                    .HasValue(typeof(AnotherDerivedEntity), typeof(AnotherDerivedEntity).Name);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("nvarchar(21)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntity", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").IsComplete(true).HasValue("BaseEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("AnotherDerivedEntity");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("DerivedEntity");
                });
"""),
            o =>
            {
                Assert.Equal("Discriminator", o.FindEntityType(typeof(BaseEntity))[CoreAnnotationNames.DiscriminatorProperty]);
                Assert.Equal("BaseEntity", o.FindEntityType(typeof(BaseEntity))[CoreAnnotationNames.DiscriminatorValue]);
                Assert.Equal(
                    "AnotherDerivedEntity",
                    o.FindEntityType(typeof(AnotherDerivedEntity))[CoreAnnotationNames.DiscriminatorValue]);
                Assert.Equal("DerivedEntity", o.FindEntityType(typeof(DerivedEntity))[CoreAnnotationNames.DiscriminatorValue]);
            });

    [ConditionalFact]
    public virtual void Converted_discriminator_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntityWithStructDiscriminator>().HasBaseType<BaseEntityWithStructDiscriminator>();
                builder.Entity<AnotherDerivedEntityWithStructDiscriminator>().HasBaseType<BaseEntityWithStructDiscriminator>();
                builder.Entity<BaseEntityWithStructDiscriminator>(
                    b =>
                    {
                        b.Property(e => e.Discriminator)
                            .HasConversion(
                                v => v.Value,
                                v => new StructDiscriminator { Value = v });
                        b.HasDiscriminator(e => e.Discriminator)
                            .IsComplete()
                            .HasValue(typeof(BaseEntityWithStructDiscriminator), new StructDiscriminator { Value = "Base" })
                            .HasValue(typeof(DerivedEntityWithStructDiscriminator), new StructDiscriminator { Value = "Derived" })
                            .HasValue(
                                typeof(AnotherDerivedEntityWithStructDiscriminator), new StructDiscriminator { Value = "Another" });
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntityWithStructDiscriminator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntityWithStructDiscriminator", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").IsComplete(true).HasValue("Base");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntityWithStructDiscriminator", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntityWithStructDiscriminator");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("Another");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntityWithStructDiscriminator", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntityWithStructDiscriminator");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("Derived");
                });
"""),
            o =>
            {
                Assert.Equal(
                    "Discriminator",
                    o.FindEntityType(typeof(BaseEntityWithStructDiscriminator))[CoreAnnotationNames.DiscriminatorProperty]);

                Assert.Equal(
                    "Base",
                    o.FindEntityType(typeof(BaseEntityWithStructDiscriminator))[CoreAnnotationNames.DiscriminatorValue]);

                Assert.Equal(
                    "Another",
                    o.FindEntityType(typeof(AnotherDerivedEntityWithStructDiscriminator))[CoreAnnotationNames.DiscriminatorValue]);

                Assert.Equal(
                    "Derived",
                    o.FindEntityType(typeof(DerivedEntityWithStructDiscriminator))[CoreAnnotationNames.DiscriminatorValue]);
            });

    [ConditionalFact]
    public virtual void Properties_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(2, o.GetEntityTypes().First().GetProperties().Count());
                Assert.Collection(
                    o.GetEntityTypes().First().GetProperties(),
                    t => Assert.Equal("Id", t.Name),
                    t => Assert.Equal("AlternateId", t.Name)
                );
            });

    [ConditionalFact]
    public virtual void Primary_key_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasKey(
                    t => new { t.Id, t.AlternateId });
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id", "AlternateId");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal(2, o.GetEntityTypes().First().FindPrimaryKey().Properties.Count);
                Assert.Collection(
                    o.GetEntityTypes().First().FindPrimaryKey().Properties,
                    t => Assert.Equal("Id", t.Name),
                    t => Assert.Equal("AlternateId", t.Name)
                );
            });

    [ConditionalFact]
    public void HasNoKey_is_handled()
        => Test(
            builder => builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties).HasNoKey(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                var entityType = Assert.Single(o.GetEntityTypes());
                Assert.Equal(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", entityType.Name);
                Assert.Null(entityType.FindPrimaryKey());
            });

    [ConditionalFact]
    public virtual void Alternate_keys_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasAlternateKey(
                    t => new { t.Id, t.AlternateId });
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("Id", "AlternateId");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Collection(
                    o.GetEntityTypes().First().GetDeclaredKeys().First(k => k.Properties.Count == 2).Properties,
                    t => Assert.Equal("Id", t.Name),
                    t => Assert.Equal("AlternateId", t.Name)
                );
            });

    [ConditionalFact]
    public virtual void Indexes_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId);
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Single(o.GetEntityTypes().First().GetIndexes());
                Assert.Equal("AlternateId", o.GetEntityTypes().First().GetIndexes().First().Properties[0].Name);
            });

    [ConditionalFact]
    public virtual void Indexes_are_stored_in_snapshot_including_composite_index()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(
                    t => new { t.Id, t.AlternateId });
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Id", "AlternateId");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Single(o.GetEntityTypes().First().GetIndexes());
                Assert.Collection(
                    o.GetEntityTypes().First().GetIndexes().First().Properties,
                    t => Assert.Equal("Id", t.Name),
                    t => Assert.Equal("AlternateId", t.Name));
            });

    [ConditionalFact]
    public virtual void Foreign_keys_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder
                    .Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o =>
            {
                var foreignKey = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().Single();
                Assert.Equal("AlternateId", foreignKey.Properties[0].Name);
                Assert.Equal("EntityWithTwoProperties", foreignKey.PrincipalToDependent.Name);
                Assert.Equal("EntityWithOneProperty", foreignKey.DependentToPrincipal.Name);
            });

    [ConditionalFact]
    public virtual void Many_to_many_join_table_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder
                    .Entity<ManyToManyLeft>()
                    .ToTable("ManyToManyLeft", "schema")
                    .HasMany(l => l.Rights)
                    .WithMany(r => r.Lefts);

                builder
                    .Entity<ManyToManyRight>()
                    .ToTable("ManyToManyRight", "schema");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("ManyToManyLeftManyToManyRight", b =>
                {
                    b.Property<int>("LeftsId")
                        .HasColumnType("int");

                    b.Property<int>("RightsId")
                        .HasColumnType("int");

                    b.HasKey("LeftsId", "RightsId");

                    b.HasIndex("RightsId");

                    b.ToTable("ManyToManyLeftManyToManyRight", "schema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManyToManyLeft", "schema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManyToManyRight", "schema");
                });

            modelBuilder.Entity("ManyToManyLeftManyToManyRight", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft", null)
                        .WithMany()
                        .HasForeignKey("LeftsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight", null)
                        .WithMany()
                        .HasForeignKey("RightsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
"""),
            model =>
            {
                var joinEntity = model.FindEntityType("ManyToManyLeftManyToManyRight");
                Assert.Equal(typeof(Dictionary<string, object>), joinEntity.ClrType);
                Assert.Collection(
                    joinEntity.GetDeclaredProperties(),
                    p =>
                    {
                        Assert.Equal("LeftsId", p.Name);
                        Assert.False(p.IsShadowProperty());
                    },
                    p =>
                    {
                        Assert.Equal("RightsId", p.Name);
                        Assert.False(p.IsShadowProperty());
                    });
                Assert.Collection(
                    joinEntity.FindDeclaredPrimaryKey().Properties,
                    p =>
                    {
                        Assert.Equal("LeftsId", p.Name);
                    },
                    p =>
                    {
                        Assert.Equal("RightsId", p.Name);
                    });
                Assert.Collection(
                    joinEntity.GetDeclaredForeignKeys(),
                    fk =>
                    {
                        Assert.Equal(
                            "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft",
                            fk.PrincipalEntityType.Name);
                        Assert.Collection(
                            fk.PrincipalKey.Properties,
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                            });
                        Assert.Collection(
                            fk.Properties,
                            p =>
                            {
                                Assert.Equal("LeftsId", p.Name);
                            });
                    },
                    fk =>
                    {
                        Assert.Equal(
                            "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight",
                            fk.PrincipalEntityType.Name);
                        Assert.Collection(
                            fk.PrincipalKey.Properties,
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                            });
                        Assert.Collection(
                            fk.Properties,
                            p =>
                            {
                                Assert.Equal("RightsId", p.Name);
                            });
                    });

                Assert.Equal("ManyToManyLeftManyToManyRight", joinEntity.GetTableName());
                Assert.Equal("schema", joinEntity.GetSchema());
            });

    [ConditionalFact]
    public virtual void Can_override_table_name_for_many_to_many_join_table_stored_in_snapshot()
        => Test(
            builder =>
            {
                var manyToMany = builder
                    .Entity<ManyToManyLeft>()
                    .HasMany(l => l.Rights)
                    .WithMany(r => r.Lefts)
                    .UsingEntity(a => a.ToTable("MyJoinTable"));
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("ManyToManyLeftManyToManyRight", b =>
                {
                    b.Property<int>("LeftsId")
                        .HasColumnType("int");

                    b.Property<int>("RightsId")
                        .HasColumnType("int");

                    b.HasKey("LeftsId", "RightsId");

                    b.HasIndex("RightsId");

                    b.ToTable("MyJoinTable", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManyToManyLeft", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ManyToManyRight", "DefaultSchema");
                });

            modelBuilder.Entity("ManyToManyLeftManyToManyRight", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft", null)
                        .WithMany()
                        .HasForeignKey("LeftsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight", null)
                        .WithMany()
                        .HasForeignKey("RightsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
"""),
            model =>
            {
                var joinEntity = model.FindEntityType("ManyToManyLeftManyToManyRight");
                Assert.Equal(typeof(Dictionary<string, object>), joinEntity.ClrType);
                Assert.Equal("MyJoinTable", joinEntity.GetTableName());
                Assert.Collection(
                    joinEntity.GetDeclaredProperties(),
                    p =>
                    {
                        Assert.Equal("LeftsId", p.Name);
                        Assert.False(p.IsShadowProperty());
                        Assert.True(p.IsIndexerProperty());
                    },
                    p =>
                    {
                        Assert.Equal("RightsId", p.Name);
                        Assert.False(p.IsShadowProperty());
                        Assert.True(p.IsIndexerProperty());
                    });
                Assert.Collection(
                    joinEntity.FindDeclaredPrimaryKey().Properties,
                    p =>
                    {
                        Assert.Equal("LeftsId", p.Name);
                    },
                    p =>
                    {
                        Assert.Equal("RightsId", p.Name);
                    });
                Assert.Collection(
                    joinEntity.GetDeclaredForeignKeys(),
                    fk =>
                    {
                        Assert.Equal(
                            "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyLeft",
                            fk.PrincipalEntityType.Name);
                        Assert.Collection(
                            fk.PrincipalKey.Properties,
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                            });
                        Assert.Collection(
                            fk.Properties,
                            p =>
                            {
                                Assert.Equal("LeftsId", p.Name);
                            });
                    },
                    fk =>
                    {
                        Assert.Equal(
                            "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+ManyToManyRight",
                            fk.PrincipalEntityType.Name);
                        Assert.Collection(
                            fk.PrincipalKey.Properties,
                            p =>
                            {
                                Assert.Equal("Id", p.Name);
                            });
                        Assert.Collection(
                            fk.Properties,
                            p =>
                            {
                                Assert.Equal("RightsId", p.Name);
                            });
                    });
            });

    [ConditionalFact]
    public virtual void TableName_preserved_when_generic()
    {
        IReadOnlyModel originalModel = null;

        Test(
            builder =>
            {
                builder.Entity<EntityWithGenericKey<Guid>>();

                originalModel = builder.Model;
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("EntityWithGenericKey<Guid>", "DefaultSchema");
                });
""", usingSystem: true),
            model =>
            {
                var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                var entity = model.FindEntityType(originalEntity.Name);

                Assert.NotNull(entity);
                Assert.Equal(originalEntity.GetTableName(), entity.GetTableName());
            });
    }

    [ConditionalFact]
    public virtual void Shared_columns_are_stored_in_the_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.ToTable("EntityWithProperties");
                        b.Property<int>("AlternateId").HasColumnName("AlternateId");
                    });
                builder.Entity<EntityWithTwoProperties>(
                    b =>
                    {
                        b.ToTable("EntityWithProperties");
                        b.Property(e => e.AlternateId).HasColumnName("AlternateId");
                        b.HasOne(e => e.EntityWithOneProperty).WithOne(e => e.EntityWithTwoProperties)
                            .HasForeignKey<EntityWithTwoProperties>(e => e.Id);
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("AlternateId");

                    b.HasKey("Id");

                    b.ToTable("EntityWithProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("AlternateId");

                    b.HasKey("Id");

                    b.ToTable("EntityWithProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
""", usingSystem: false),
            model =>
            {
                var entityType = model.FindEntityType(typeof(EntityWithOneProperty));

                Assert.Equal(ValueGenerated.OnUpdateSometimes, entityType.FindProperty("AlternateId").ValueGenerated);
            });

    [ConditionalFact]
    public virtual void PrimaryKey_name_preserved_when_generic()
    {
        IReadOnlyModel originalModel = null;

        Test(
            builder =>
            {
                builder.Entity<EntityWithGenericKey<Guid>>();

                originalModel = builder.Model;
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("EntityWithGenericKey<Guid>", "DefaultSchema");
                });
""", usingSystem: true),
            model =>
            {
                var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                var entity = model.FindEntityType(originalEntity.Name);
                Assert.NotNull(entity);

                var originalPrimaryKey = originalEntity.FindPrimaryKey();
                var primaryKey = entity.FindPrimaryKey();

                Assert.Equal(originalPrimaryKey.GetName(), primaryKey.GetName());
            });
    }

    [ConditionalFact]
    public virtual void AlternateKey_name_preserved_when_generic()
    {
        IReadOnlyModel originalModel = null;

        Test(
            builder =>
            {
                builder.Entity<EntityWithGenericProperty<Guid>>().HasAlternateKey(e => e.Property);

                originalModel = builder.Model;
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("Property")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasAlternateKey("Property");

                    b.ToTable("EntityWithGenericProperty<Guid>", "DefaultSchema");
                });
""", usingSystem: true),
            model =>
            {
                var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericProperty<Guid>));
                var entity = model.FindEntityType(originalEntity.Name);
                Assert.NotNull(entity);

                var originalAlternateKey = originalEntity.FindKey(originalEntity.FindProperty("Property"));
                var alternateKey = entity.FindKey(entity.FindProperty("Property"));

                Assert.Equal(originalAlternateKey.GetName(), alternateKey.GetName());
            });
    }

    [ConditionalFact]
    public virtual void Discriminator_of_enum()
        => Test(
            builder => builder.Entity<EntityWithEnumType>().HasDiscriminator(e => e.Day),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("Day")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");

                    b.HasDiscriminator<long>("Day");
                });
"""),
            model => Assert.Equal(typeof(long), model.GetEntityTypes().First().FindDiscriminatorProperty().ClrType));

    [ConditionalFact]
    public virtual void Discriminator_of_enum_to_string()
        => Test(
            builder => builder.Entity<EntityWithEnumType>(
                x =>
                {
                    x.Property(e => e.Day).HasConversion<string>();
                    x.HasDiscriminator(e => e.Day);
                }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Day")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");

                    b.HasDiscriminator<string>("Day");
                });
"""),
            model =>
            {
                var discriminatorProperty = model.GetEntityTypes().First().FindDiscriminatorProperty();
                Assert.Equal(typeof(string), discriminatorProperty.ClrType);
                Assert.False(discriminatorProperty.IsNullable);
            });

    [ConditionalFact]
    public virtual void Temporal_table_information_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().ToTable(
                tb => tb.IsTemporal(
                    ttb =>
                    {
                        ttb.UseHistoryTable("HistoryTable");
                        ttb.HasPeriodStart("Start").HasColumnName("PeriodStart");
                        ttb.HasPeriodEnd("End").HasColumnName("PeriodEnd");
                    })),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("End")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodEnd");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Start")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodStart");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");

                    b.ToTable(tb => tb.IsTemporal(ttb =>
                            {
                                ttb.UseHistoryTable("HistoryTable");
                                ttb
                                    .HasPeriodStart("Start")
                                    .HasColumnName("PeriodStart");
                                ttb
                                    .HasPeriodEnd("End")
                                    .HasColumnName("PeriodEnd");
                            }));
                });
""", usingSystem: true),
            o =>
            {
                var temporalEntity = o.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty");
                var annotations = temporalEntity.GetAnnotations().ToList();

                Assert.Equal(7, annotations.Count);
                Assert.Contains(annotations, a => a.Name == SqlServerAnnotationNames.IsTemporal && a.Value as bool? == true);
                Assert.Contains(
                    annotations,
                    a => a.Name == SqlServerAnnotationNames.TemporalHistoryTableName && a.Value as string == "HistoryTable");
                Assert.Contains(
                    annotations,
                    a => a.Name == SqlServerAnnotationNames.TemporalPeriodStartPropertyName && a.Value as string == "Start");
                Assert.Contains(
                    annotations, a => a.Name == SqlServerAnnotationNames.TemporalPeriodEndPropertyName && a.Value as string == "End");
            });

    [ConditionalFact]
    public virtual void Temporal_table_information_is_stored_in_snapshot_minimal_setup()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().ToTable(tb => tb.IsTemporal()),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PeriodEnd")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodEnd");

                    b.Property<DateTime>("PeriodStart")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasColumnName("PeriodStart");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");

                    b.ToTable(tb => tb.IsTemporal(ttb =>
                            {
                                ttb.UseHistoryTable("EntityWithStringPropertyHistory", "DefaultSchema");
                                ttb
                                    .HasPeriodStart("PeriodStart")
                                    .HasColumnName("PeriodStart");
                                ttb
                                    .HasPeriodEnd("PeriodEnd")
                                    .HasColumnName("PeriodEnd");
                            }));
                });
""", usingSystem: true),
            o =>
            {
                var temporalEntity = o.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty");
                var annotations = temporalEntity.GetAnnotations().ToList();

                Assert.Equal(7, annotations.Count);
                Assert.Contains(annotations, a => a.Name == SqlServerAnnotationNames.IsTemporal && a.Value as bool? == true);
                Assert.Contains(
                    annotations,
                    a => a.Name == SqlServerAnnotationNames.TemporalPeriodStartPropertyName && a.Value as string == "PeriodStart");
                Assert.Contains(
                    annotations,
                    a => a.Name == SqlServerAnnotationNames.TemporalPeriodEndPropertyName && a.Value as string == "PeriodEnd");
            });

    #endregion

    #region Owned types

    [ConditionalFact]
    public virtual void Owned_types_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.HasKey(e => e.Id).HasName("PK_Custom");

                        b.OwnsOne(
                            eo => eo.EntityWithTwoProperties, eb =>
                            {
                                eb.HasKey(e => e.AlternateId).HasName("PK_Custom");
                                eb.WithOwner(e => e.EntityWithOneProperty)
                                    .HasForeignKey(e => e.AlternateId)
                                    .HasConstraintName("FK_Custom");
                                eb.HasIndex(e => e.Id)
                                    .IncludeProperties(e => e.AlternateId);

                                eb.HasOne(e => e.EntityWithStringKey).WithOne();

                                eb.HasData(
                                    new EntityWithTwoProperties { AlternateId = 1, Id = -1 });
                            });

                        b.HasData(
                            new EntityWithOneProperty { Id = 1 });
                    });

                builder.Entity<EntityWithStringKey>(
                    b => b.OwnsMany(
                        es => es.Properties, es =>
                        {
                            es.HasKey(e => e.Id);
                            es.HasOne(e => e.EntityWithOneProperty).WithOne();
                        }));
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id")
                        .HasName("PK_Custom");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");

                    b.HasData(
                        new
                        {
                            Id = 1
                        });
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringKey", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties", b1 =>
                        {
                            b1.Property<int>("AlternateId")
                                .HasColumnType("int");

                            b1.Property<string>("EntityWithStringKeyId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .HasColumnType("int");

                            b1.HasKey("AlternateId")
                                .HasName("PK_Custom");

                            b1.HasIndex("EntityWithStringKeyId")
                                .IsUnique()
                                .HasFilter("[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL");

                            b1.HasIndex("Id");

                            SqlServerIndexBuilderExtensions.IncludeProperties(b1.HasIndex("Id"), new[] { "AlternateId" });

                            b1.ToTable("EntityWithOneProperty", "DefaultSchema");

                            b1.WithOwner("EntityWithOneProperty")
                                .HasForeignKey("AlternateId")
                                .HasConstraintName("FK_Custom");

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", "EntityWithStringKey")
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithStringKeyId");

                            b1.Navigation("EntityWithOneProperty");

                            b1.Navigation("EntityWithStringKey");

                            b1.HasData(
                                new
                                {
                                    AlternateId = 1,
                                    Id = -1
                                });
                        });

                    b.Navigation("EntityWithTwoProperties");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.OwnsMany("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "Properties", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int?>("EntityWithOnePropertyId")
                                .HasColumnType("int");

                            b1.Property<string>("EntityWithStringKeyId")
                                .IsRequired()
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("Name")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("Id");

                            b1.HasIndex("EntityWithOnePropertyId")
                                .IsUnique()
                                .HasFilter("[EntityWithOnePropertyId] IS NOT NULL");

                            b1.HasIndex("EntityWithStringKeyId");

                            b1.ToTable("EntityWithStringProperty", "DefaultSchema");

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey.Properties#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "EntityWithOnePropertyId");

                            b1.WithOwner()
                                .HasForeignKey("EntityWithStringKeyId");

                            b1.Navigation("EntityWithOneProperty");
                        });

                    b.Navigation("Properties");
                });
""", usingSystem: true),
            o =>
            {
                var entityWithOneProperty = o.FindEntityType(typeof(EntityWithOneProperty));
                Assert.Equal("PK_Custom", entityWithOneProperty.GetKeys().Single().GetName());
                Assert.Equal(new object[] { 1 }, entityWithOneProperty.GetSeedData().Single().Values);

                var ownership1 = entityWithOneProperty.FindNavigation(nameof(EntityWithOneProperty.EntityWithTwoProperties))
                    .ForeignKey;
                Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownership1.Properties[0].Name);
                Assert.Equal(nameof(EntityWithTwoProperties.EntityWithOneProperty), ownership1.DependentToPrincipal.Name);
                Assert.True(ownership1.IsRequired);
                Assert.Equal("FK_Custom", ownership1.GetConstraintName());
                var ownedType1 = ownership1.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownedType1.FindPrimaryKey().Properties[0].Name);
                Assert.Equal("PK_Custom", ownedType1.GetKeys().Single().GetName());
                Assert.Equal(2, ownedType1.GetIndexes().Count());
                var owned1index1 = ownedType1.GetIndexes().First();
                Assert.Equal("EntityWithStringKeyId", owned1index1.Properties[0].Name);
                Assert.True(owned1index1.IsUnique);
                Assert.Equal("[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL", owned1index1.GetFilter());
                Assert.Null(owned1index1.GetIncludeProperties());
                var owned1index2 = ownedType1.GetIndexes().Last();
                Assert.Equal("Id", owned1index2.Properties[0].Name);
                Assert.False(owned1index2.IsUnique);
                Assert.Null(owned1index2.GetFilter());
                Assert.Equal(new[] { nameof(EntityWithTwoProperties.AlternateId) }, owned1index2.GetIncludeProperties());
                Assert.Equal(new object[] { 1, -1 }, ownedType1.GetSeedData().Single().Values);
                Assert.Equal(nameof(EntityWithOneProperty), ownedType1.GetTableName());
                Assert.False(ownedType1.IsTableExcludedFromMigrations());

                var entityWithStringKey = o.FindEntityType(typeof(EntityWithStringKey));
                Assert.Same(
                    entityWithStringKey,
                    ownedType1.FindNavigation(nameof(EntityWithTwoProperties.EntityWithStringKey)).TargetEntityType);
                Assert.Equal(nameof(EntityWithStringKey), entityWithStringKey.GetTableName());

                var ownership2 = entityWithStringKey.FindNavigation(nameof(EntityWithStringKey.Properties)).ForeignKey;
                Assert.Equal("EntityWithStringKeyId", ownership2.Properties[0].Name);
                Assert.Null(ownership2.DependentToPrincipal);
                Assert.True(ownership2.IsRequired);
                var ownedType2 = ownership2.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithStringProperty.Id), ownedType2.FindPrimaryKey().Properties[0].Name);
                Assert.Single(ownedType2.GetKeys());
                Assert.Equal(2, ownedType2.GetIndexes().Count());
                var owned2index1 = ownedType2.GetIndexes().First();
                Assert.Equal("EntityWithOnePropertyId", owned2index1.Properties[0].Name);
                Assert.True(owned2index1.IsUnique);
                Assert.Equal("[EntityWithOnePropertyId] IS NOT NULL", owned2index1.GetFilter());
                var owned2index2 = ownedType2.GetIndexes().Last();
                Assert.Equal("EntityWithStringKeyId", owned2index2.Properties[0].Name);
                Assert.False(owned2index2.IsUnique);
                Assert.Null(owned2index2.GetFilter());
                Assert.Equal(nameof(EntityWithStringProperty), ownedType2.GetTableName());
                Assert.False(ownedType2.IsTableExcludedFromMigrations());

                Assert.Same(entityWithOneProperty, ownedType2.GetNavigations().Single().TargetEntityType);
            });

    [ConditionalFact]
    public virtual void Owned_types_are_stored_in_snapshot_when_excluded()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.HasKey(e => e.Id).HasName("PK_Custom");

                        b.OwnsOne(
                            eo => eo.EntityWithTwoProperties, eb =>
                            {
                                eb.HasKey(e => e.AlternateId).HasName("PK_Custom");
                                eb.WithOwner(e => e.EntityWithOneProperty)
                                    .HasForeignKey(e => e.AlternateId)
                                    .HasConstraintName("FK_Custom");
                                eb.HasIndex(e => e.Id);

                                eb.HasOne(e => e.EntityWithStringKey).WithOne();

                                eb.HasData(
                                    new EntityWithTwoProperties { AlternateId = 1, Id = -1 });
                            });

                        b.HasData(
                            new EntityWithOneProperty { Id = 1 });

                        b.ToTable("EntityWithOneProperty", "DefaultSchema", e => e.ExcludeFromMigrations());
                    });

                builder.Entity<EntityWithStringKey>(
                    b =>
                    {
                        b.OwnsMany(
                            es => es.Properties, es =>
                            {
                                es.HasKey(e => e.Id);
                                es.HasOne(e => e.EntityWithOneProperty).WithOne();

                                es.ToTable("EntityWithStringProperty", t => t.ExcludeFromMigrations());
                            });

                        b.ToTable("EntityWithStringKey", e => e.ExcludeFromMigrations());
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id")
                        .HasName("PK_Custom");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema", t =>
                        {
                            t.ExcludeFromMigrations();
                        });

                    b.HasData(
                        new
                        {
                            Id = 1
                        });
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringKey", "DefaultSchema", t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties", b1 =>
                        {
                            b1.Property<int>("AlternateId")
                                .HasColumnType("int");

                            b1.Property<string>("EntityWithStringKeyId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .HasColumnType("int");

                            b1.HasKey("AlternateId")
                                .HasName("PK_Custom");

                            b1.HasIndex("EntityWithStringKeyId")
                                .IsUnique()
                                .HasFilter("[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL");

                            b1.HasIndex("Id");

                            b1.ToTable("EntityWithOneProperty", "DefaultSchema");

                            b1.WithOwner("EntityWithOneProperty")
                                .HasForeignKey("AlternateId")
                                .HasConstraintName("FK_Custom");

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", "EntityWithStringKey")
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithStringKeyId");

                            b1.Navigation("EntityWithOneProperty");

                            b1.Navigation("EntityWithStringKey");

                            b1.HasData(
                                new
                                {
                                    AlternateId = 1,
                                    Id = -1
                                });
                        });

                    b.Navigation("EntityWithTwoProperties");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.OwnsMany("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "Properties", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int?>("EntityWithOnePropertyId")
                                .HasColumnType("int");

                            b1.Property<string>("EntityWithStringKeyId")
                                .IsRequired()
                                .HasColumnType("nvarchar(450)");

                            b1.Property<string>("Name")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("Id");

                            b1.HasIndex("EntityWithOnePropertyId")
                                .IsUnique()
                                .HasFilter("[EntityWithOnePropertyId] IS NOT NULL");

                            b1.HasIndex("EntityWithStringKeyId");

                            b1.ToTable("EntityWithStringProperty", "DefaultSchema", t =>
                                {
                                    t.ExcludeFromMigrations();
                                });

                            b1.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                                .WithOne()
                                .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey.Properties#Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "EntityWithOnePropertyId");

                            b1.WithOwner()
                                .HasForeignKey("EntityWithStringKeyId");

                            b1.Navigation("EntityWithOneProperty");
                        });

                    b.Navigation("Properties");
                });
""", usingSystem: true),
            o =>
            {
                var entityWithOneProperty = o.FindEntityType(typeof(EntityWithOneProperty));
                Assert.Equal("PK_Custom", entityWithOneProperty.GetKeys().Single().GetName());
                Assert.Equal(new object[] { 1 }, entityWithOneProperty.GetSeedData().Single().Values);

                var ownership1 = entityWithOneProperty.FindNavigation(nameof(EntityWithOneProperty.EntityWithTwoProperties))
                    .ForeignKey;
                Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownership1.Properties[0].Name);
                Assert.Equal(nameof(EntityWithTwoProperties.EntityWithOneProperty), ownership1.DependentToPrincipal.Name);
                Assert.True(ownership1.IsRequired);
                Assert.Equal("FK_Custom", ownership1.GetConstraintName());
                var ownedType1 = ownership1.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownedType1.FindPrimaryKey().Properties[0].Name);
                Assert.Equal("PK_Custom", ownedType1.GetKeys().Single().GetName());
                Assert.Equal(2, ownedType1.GetIndexes().Count());
                var owned1index1 = ownedType1.GetIndexes().First();
                Assert.Equal("EntityWithStringKeyId", owned1index1.Properties[0].Name);
                Assert.True(owned1index1.IsUnique);
                Assert.Equal("[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL", owned1index1.GetFilter());
                var owned1index2 = ownedType1.GetIndexes().Last();
                Assert.Equal("Id", owned1index2.Properties[0].Name);
                Assert.False(owned1index2.IsUnique);
                Assert.Null(owned1index2.GetFilter());
                Assert.Equal(new object[] { 1, -1 }, ownedType1.GetSeedData().Single().Values);
                Assert.Equal(nameof(EntityWithOneProperty), ownedType1.GetTableName());
                Assert.True(ownedType1.IsTableExcludedFromMigrations());

                var entityWithStringKey = o.FindEntityType(typeof(EntityWithStringKey));
                Assert.Same(
                    entityWithStringKey,
                    ownedType1.FindNavigation(nameof(EntityWithTwoProperties.EntityWithStringKey)).TargetEntityType);
                Assert.Equal(nameof(EntityWithStringKey), entityWithStringKey.GetTableName());
                Assert.True(entityWithStringKey.IsTableExcludedFromMigrations());

                var ownership2 = entityWithStringKey.FindNavigation(nameof(EntityWithStringKey.Properties)).ForeignKey;
                Assert.Equal("EntityWithStringKeyId", ownership2.Properties[0].Name);
                Assert.Null(ownership2.DependentToPrincipal);
                Assert.True(ownership2.IsRequired);
                var ownedType2 = ownership2.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithStringProperty.Id), ownedType2.FindPrimaryKey().Properties[0].Name);
                Assert.Single(ownedType2.GetKeys());
                Assert.Equal(2, ownedType2.GetIndexes().Count());
                var owned2index1 = ownedType2.GetIndexes().First();
                Assert.Equal("EntityWithOnePropertyId", owned2index1.Properties[0].Name);
                Assert.True(owned2index1.IsUnique);
                Assert.Equal("[EntityWithOnePropertyId] IS NOT NULL", owned2index1.GetFilter());
                var owned2index2 = ownedType2.GetIndexes().Last();
                Assert.Equal("EntityWithStringKeyId", owned2index2.Properties[0].Name);
                Assert.False(owned2index2.IsUnique);
                Assert.Null(owned2index2.GetFilter());
                Assert.Equal(nameof(EntityWithStringProperty), ownedType2.GetTableName());
                Assert.True(ownedType2.IsTableExcludedFromMigrations());

                Assert.Same(entityWithOneProperty, ownedType2.GetNavigations().Single().TargetEntityType);
            });

    [ConditionalFact]
    public virtual void Shared_owned_types_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<Order>().OwnsOne(p => p.OrderBillingDetails, od => od.OwnsOne(c => c.StreetAddress));
                builder.Entity<Order>().OwnsOne(p => p.OrderShippingDetails, od => od.OwnsOne(c => c.StreetAddress));
                builder.Entity<Order>().OwnsOne(p => p.OrderInfo, od => od.OwnsOne(c => c.StreetAddress));
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("Order", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order", b =>
                {
                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderBillingDetails", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.HasKey("OrderId");

                            b1.ToTable("Order", "DefaultSchema");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress", "StreetAddress", b2 =>
                                {
                                    b2.Property<int>("OrderDetailsOrderId")
                                        .HasColumnType("int");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("OrderDetailsOrderId");

                                    b2.ToTable("Order", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("OrderDetailsOrderId");
                                });

                            b1.Navigation("StreetAddress");
                        });

                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails", "OrderShippingDetails", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.HasKey("OrderId");

                            b1.ToTable("Order", "DefaultSchema");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress", "StreetAddress", b2 =>
                                {
                                    b2.Property<int>("OrderDetailsOrderId")
                                        .HasColumnType("int");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("OrderDetailsOrderId");

                                    b2.ToTable("Order", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("OrderDetailsOrderId");
                                });

                            b1.Navigation("StreetAddress");
                        });

                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderInfo", "OrderInfo", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.HasKey("OrderId");

                            b1.ToTable("Order", "DefaultSchema");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress", "StreetAddress", b2 =>
                                {
                                    b2.Property<int>("OrderInfoOrderId")
                                        .HasColumnType("int");

                                    b2.Property<string>("City")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("OrderInfoOrderId");

                                    b2.ToTable("Order", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("OrderInfoOrderId");
                                });

                            b1.Navigation("StreetAddress");
                        });

                    b.Navigation("OrderBillingDetails");

                    b.Navigation("OrderInfo");

                    b.Navigation("OrderShippingDetails");
                });
"""),
            o =>
            {
                Assert.Equal(7, o.GetEntityTypes().Count());

                var order = (IRuntimeEntityType)o.FindEntityType(typeof(Order).FullName);
                Assert.Equal(1, order.PropertyCount);

                var orderInfo = (IRuntimeEntityType)order.FindNavigation(nameof(Order.OrderInfo)).TargetEntityType;
                Assert.Equal(1, orderInfo.PropertyCount);

                var orderInfoAddress = (IRuntimeEntityType)orderInfo.FindNavigation(nameof(OrderInfo.StreetAddress)).TargetEntityType;
                Assert.Equal(2, orderInfoAddress.PropertyCount);

                var orderBillingDetails = (IRuntimeEntityType)order.FindNavigation(nameof(Order.OrderBillingDetails)).TargetEntityType;
                Assert.Equal(1, orderBillingDetails.PropertyCount);

                var orderBillingDetailsAddress =
                    (IRuntimeEntityType)orderBillingDetails.FindNavigation(nameof(OrderDetails.StreetAddress)).TargetEntityType;
                Assert.Equal(2, orderBillingDetailsAddress.PropertyCount);

                var orderShippingDetails = (IRuntimeEntityType)order.FindNavigation(nameof(Order.OrderShippingDetails)).TargetEntityType;
                Assert.Equal(1, orderShippingDetails.PropertyCount);

                var orderShippingDetailsAddress =
                    (IRuntimeEntityType)orderShippingDetails.FindNavigation(nameof(OrderDetails.StreetAddress)).TargetEntityType;
                Assert.Equal(2, orderShippingDetailsAddress.PropertyCount);
            });

    [ConditionalFact]
    public virtual void Owned_types_can_be_mapped_to_view()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Entity<TestOwner>()
                    .OwnsMany(
                        o => o.OwnedEntities,
                        ownee => ownee.ToView("OwnedView"));
            },
"""
// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("TestOwner", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner", b =>
                {
                    b.OwnsMany("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwnee", "OwnedEntities", b1 =>
                        {
                            b1.Property<int>("TestOwnerId")
                                .HasColumnType("int");

                            b1.Property<int>("Id")
                                .HasColumnType("int");

                            b1.Property<int>("TestEnum")
                                .HasColumnType("int");

                            b1.HasKey("TestOwnerId", "Id");

                            b1.ToTable((string)null);

                            b1.ToView("OwnedView", "DefaultSchema");

                            b1.WithOwner()
                                .HasForeignKey("TestOwnerId");
                        });

                    b.Navigation("OwnedEntities");
                });
#pragma warning restore 612, 618
        }
    }
}

""",
            model =>
            {
                Assert.Equal(2, model.GetEntityTypes().Count());
                var testOwner = model.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner");
                var testOwnee = model.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwnee", "OwnedEntities", testOwner);
                Assert.Equal("OwnedView", testOwnee.GetViewName());
            });

    [ConditionalFact]
    public virtual void Snapshot_with_OwnedNavigationBuilder_HasCheckConstraint_compiles()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Entity<TestOwner>()
                    .OwnsMany(
                        o => o.OwnedEntities,
                        ownee => ownee.ToTable(
                            tb => tb.HasCheckConstraint("CK_TestOwnee_TestEnum_Enum_Constraint", "[TestEnum] IN (0, 1, 2)")));
            },
"""
// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("TestOwner", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner", b =>
                {
                    b.OwnsMany("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwnee", "OwnedEntities", b1 =>
                        {
                            b1.Property<int>("TestOwnerId")
                                .HasColumnType("int");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<int>("TestEnum")
                                .HasColumnType("int");

                            b1.HasKey("TestOwnerId", "Id");

                            b1.ToTable("TestOwnee", "DefaultSchema", t =>
                                {
                                    t.HasCheckConstraint("CK_TestOwnee_TestEnum_Enum_Constraint", "[TestEnum] IN (0, 1, 2)");
                                });

                            b1.WithOwner()
                                .HasForeignKey("TestOwnerId");
                        });

                    b.Navigation("OwnedEntities");
                });
#pragma warning restore 612, 618
        }
    }
}

""",
            model =>
            {
                Assert.Equal(2, model.GetEntityTypes().Count());
                var testOwner = model.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwner");
                var testOwnee = model.FindEntityType(
                    "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+TestOwnee", "OwnedEntities", testOwner);
                Assert.NotNull(testOwnee.FindCheckConstraint("CK_TestOwnee_TestEnum_Enum_Constraint"));
            });

    [ConditionalFact]
    public virtual void Owned_types_mapped_to_json_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.HasKey(x => x.Id).HasName("PK_Custom");

                        b.OwnsOne(
                            x => x.EntityWithTwoProperties, bb =>
                            {
                                bb.ToJson();
                                bb.Ignore(x => x.Id);
                                bb.Property(x => x.AlternateId).HasJsonPropertyName("NotKey");
                                bb.WithOwner(e => e.EntityWithOneProperty);
                                bb.OwnsOne(
                                    x => x.EntityWithStringKey, bbb =>
                                    {
                                        bbb.Ignore(x => x.Id);
                                        bbb.OwnsMany(x => x.Properties, bbbb => bbbb.HasJsonPropertyName("JsonProps"));
                                    });
                            });
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id")
                        .HasName("PK_Custom");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties", b1 =>
                        {
                            b1.Property<int>("EntityWithOnePropertyId")
                                .HasColumnType("int");

                            b1.Property<int>("AlternateId")
                                .HasColumnType("int")
                                .HasAnnotation("Relational:JsonPropertyName", "NotKey");

                            b1.HasKey("EntityWithOnePropertyId");

                            b1.ToTable("EntityWithOneProperty", "DefaultSchema");

                            b1.ToJson("EntityWithTwoProperties");

                            b1.WithOwner("EntityWithOneProperty")
                                .HasForeignKey("EntityWithOnePropertyId");

                            b1.OwnsOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", "EntityWithStringKey", b2 =>
                                {
                                    b2.Property<int>("EntityWithTwoPropertiesEntityWithOnePropertyId")
                                        .HasColumnType("int");

                                    b2.HasKey("EntityWithTwoPropertiesEntityWithOnePropertyId");

                                    b2.ToTable("EntityWithOneProperty", "DefaultSchema");

                                    b2.WithOwner()
                                        .HasForeignKey("EntityWithTwoPropertiesEntityWithOnePropertyId");

                                    b2.OwnsMany("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "Properties", b3 =>
                                        {
                                            b3.Property<int>("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId")
                                                .HasColumnType("int");

                                            b3.Property<int>("Id")
                                                .ValueGeneratedOnAdd()
                                                .HasColumnType("int");

                                            b3.Property<string>("Name")
                                                .HasColumnType("nvarchar(max)");

                                            b3.HasKey("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId", "Id");

                                            b3.ToTable("EntityWithOneProperty", "DefaultSchema");

                                            b3.HasAnnotation("Relational:JsonPropertyName", "JsonProps");

                                            b3.WithOwner()
                                                .HasForeignKey("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId");
                                        });

                                    b2.Navigation("Properties");
                                });

                            b1.Navigation("EntityWithOneProperty");

                            b1.Navigation("EntityWithStringKey");
                        });

                    b.Navigation("EntityWithTwoProperties");
                });
""", usingSystem: false),
            o =>
            {
                var entityWithOneProperty = o.FindEntityType(typeof(EntityWithOneProperty));
                Assert.Equal("PK_Custom", entityWithOneProperty.GetKeys().Single().GetName());

                var ownership1 = entityWithOneProperty.FindNavigation(nameof(EntityWithOneProperty.EntityWithTwoProperties))
                    .ForeignKey;
                Assert.Equal("EntityWithOnePropertyId", ownership1.Properties[0].Name);

                Assert.Equal(nameof(EntityWithTwoProperties.EntityWithOneProperty), ownership1.DependentToPrincipal.Name);
                Assert.True(ownership1.IsRequired);
                Assert.Equal("FK_EntityWithOneProperty_EntityWithOneProperty_EntityWithOnePropertyId", ownership1.GetConstraintName());
                var ownedType1 = ownership1.DeclaringEntityType;
                Assert.Equal("EntityWithOnePropertyId", ownedType1.FindPrimaryKey().Properties[0].Name);

                var ownedProperties1 = ownedType1.GetProperties().ToList();
                Assert.Equal("EntityWithOnePropertyId", ownedProperties1[0].Name);
                Assert.Equal("AlternateId", ownedProperties1[1].Name);
                Assert.Equal("NotKey", RelationalPropertyExtensions.GetJsonPropertyName(ownedProperties1[1]));

                Assert.Equal(nameof(EntityWithOneProperty), ownedType1.GetTableName());
                Assert.Equal("EntityWithTwoProperties", ownedType1.GetContainerColumnName());

                var ownership2 = ownedType1.FindNavigation(nameof(EntityWithStringKey)).ForeignKey;
                Assert.Equal("EntityWithTwoPropertiesEntityWithOnePropertyId", ownership2.Properties[0].Name);
                Assert.Equal(nameof(EntityWithTwoProperties.EntityWithStringKey), ownership2.PrincipalToDependent.Name);
                Assert.True(ownership2.IsRequired);

                var ownedType2 = ownership2.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithStringKey), ownedType2.DisplayName());
                Assert.Equal("EntityWithTwoPropertiesEntityWithOnePropertyId", ownedType2.FindPrimaryKey().Properties[0].Name);

                var ownedProperties2 = ownedType2.GetProperties().ToList();
                Assert.Equal("EntityWithTwoPropertiesEntityWithOnePropertyId", ownedProperties2[0].Name);

                var navigation3 = ownedType2.FindNavigation(nameof(EntityWithStringKey.Properties));
                Assert.Equal("JsonProps", navigation3.TargetEntityType.GetJsonPropertyName());
                var ownership3 = navigation3.ForeignKey;
                Assert.Equal("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId", ownership3.Properties[0].Name);
                Assert.Equal(nameof(EntityWithStringKey.Properties), ownership3.PrincipalToDependent.Name);
                Assert.True(ownership3.IsRequired);
                Assert.False(ownership3.IsUnique);

                var ownedType3 = ownership3.DeclaringEntityType;
                Assert.Equal(nameof(EntityWithStringProperty), ownedType3.DisplayName());
                var pkProperties3 = ownedType3.FindPrimaryKey().Properties;
                Assert.Equal("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId", pkProperties3[0].Name);
                Assert.Equal("Id", pkProperties3[1].Name);

                var ownedProperties3 = ownedType3.GetProperties().ToList();
                Assert.Equal(3, ownedProperties3.Count);

                Assert.Equal("EntityWithStringKeyEntityWithTwoPropertiesEntityWithOnePropertyId", ownedProperties3[0].Name);
                Assert.Equal("Id", ownedProperties3[1].Name);
                Assert.Equal("Name", ownedProperties3[2].Name);
            });

    private class Order
    {
        public int Id { get; set; }
        public OrderDetails OrderBillingDetails { get; set; }
        public OrderDetails OrderShippingDetails { get; set; }
        public OrderInfo OrderInfo { get; set; }
    }

    private class OrderDetails
    {
        public StreetAddress StreetAddress { get; set; }
    }

    private class OrderInfo
    {
        public StreetAddress StreetAddress { get; set; }
    }

    private class StreetAddress
    {
        public string City { get; set; }
    }

    #endregion

    #region Property

    [ConditionalFact]
    public virtual void Property_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .Property<int>("Id")
                    .HasAnnotation("AnnotationName", "AnnotationValue");

                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o => Assert.Equal("AnnotationValue", o.GetEntityTypes().First().FindProperty("Id")["AnnotationName"])
        );

    [ConditionalFact]
    public virtual void Custom_value_generator_is_ignored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>().Property<int>("Id").HasValueGenerator<CustomValueGenerator>();
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o => Assert.Null(o.GetEntityTypes().First().FindProperty("Id")[CoreAnnotationNames.ValueGeneratorFactory])
        );

    [ConditionalFact]
    public virtual void Property_isNullable_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsRequired(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o => Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsNullable));

    [ConditionalFact]
    public virtual void Property_ValueGenerated_value_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue();

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
""", usingSystem: true),
            o => Assert.Equal(ValueGenerated.OnAdd, o.GetEntityTypes().First().FindProperty("AlternateId").ValueGenerated));

    [ConditionalFact]
    public virtual void Property_ValueGenerated_non_identity()
        => Test(
            modelBuilder => modelBuilder.Entity<EntityWithEnumType>(
                x =>
                {
                    x.Property(e => e.Id).Metadata.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.None);
                    x.Property(e => e.Day).ValueGeneratedOnAdd()
                        .Metadata.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.None);
                }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

                    b.Property<long>("Day")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");
                });
"""),
            model =>
            {
                var id = model.GetEntityTypes().Single().GetProperty(nameof(EntityWithEnumType.Id));
                Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
                Assert.Equal(SqlServerValueGenerationStrategy.None, id.GetValueGenerationStrategy());
                var day = model.GetEntityTypes().Single().GetProperty(nameof(EntityWithEnumType.Day));
                Assert.Equal(ValueGenerated.OnAdd, day.ValueGenerated);
                Assert.Equal(SqlServerValueGenerationStrategy.None, day.GetValueGenerationStrategy());
            });

    [ConditionalFact]
    public virtual void Property_maxLength_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").HasMaxLength(100),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o => Assert.Equal(100, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()));


    [ConditionalFact]
    public virtual void Property_maximum_maxLength_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").HasMaxLength(-1),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(-1)
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o => Assert.Equal(-1, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()));

    [ConditionalFact]
    public virtual void Property_unicodeness_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsUnicode(false),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o => Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsUnicode()));

    [ConditionalFact]
    public virtual void Property_fixedlengthness_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsFixedLength().HasMaxLength(100),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nchar(100)")
                        .IsFixedLength();

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().FindProperty("Name").IsFixedLength()));

    [ConditionalFact]
    public virtual void Property_precision_is_stored_in_snapshot()
        => Test(
            builder => builder
                .Entity<EntityWithDecimalProperty>()
                .Property<decimal>(nameof(EntityWithDecimalProperty.Price))
                .HasPrecision(7),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithDecimalProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Price")
                        .HasPrecision(7)
                        .HasColumnType("decimal(7,2)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithDecimalProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.GetEntityTypes().First().FindProperty(nameof(EntityWithDecimalProperty.Price));
                Assert.Equal(7, property.GetPrecision());
                Assert.Null(property.GetScale());
            });

    [ConditionalFact]
    public virtual void Property_precision_and_scale_is_stored_in_snapshot()
        => Test(
            builder => builder
                .Entity<EntityWithDecimalProperty>()
                .Property<decimal>(nameof(EntityWithDecimalProperty.Price))
                .HasPrecision(7, 3),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithDecimalProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Price")
                        .HasPrecision(7, 3)
                        .HasColumnType("decimal(7,3)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithDecimalProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.GetEntityTypes().First().FindProperty(nameof(EntityWithDecimalProperty.Price));
                Assert.Equal(7, property.GetPrecision());
                Assert.Equal(3, property.GetScale());
            });

    [ConditionalFact]
    public virtual void Many_facets_chained_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithStringProperty>()
                    .Property<string>("Name")
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasAnnotation("AnnotationName", "AnnotationValue");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.GetEntityTypes().First().FindProperty("Name");
                Assert.Equal(100, property.GetMaxLength());
                Assert.False(property.IsUnicode());
                Assert.Equal("AnnotationValue", property["AnnotationName"]);
            });

    [ConditionalFact]
    public virtual void Property_concurrencyToken_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").IsConcurrencyToken();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .IsConcurrencyToken()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().FindProperty("AlternateId").IsConcurrencyToken));

    [ConditionalFact]
    public virtual void Property_column_name_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int")
                        .HasColumnName("CName");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal("CName", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnName"]));

    [ConditionalFact]
    public virtual void Property_column_name_is_stored_in_snapshot_when_DefaultColumnName_uses_clr_type()
        => Test(
            modelBuilder => modelBuilder
                .Entity<BarA>(b => b.HasBaseType<BarBase>())
                .Entity<FooExtension<BarA>>(b => b.HasOne(x => x.Bar).WithOne().HasForeignKey<BarA>()),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarBase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.HasKey("Id");

                    b.ToTable("BarBase", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BarBase");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+FooExtension<Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("FooExtension<BarA>", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarBase");

                    b.Property<int?>("FooExtensionId")
                        .HasColumnType("int")
                        .HasColumnName("FooExtension<BarA>Id");

                    b.HasIndex("FooExtensionId")
                        .IsUnique()
                        .HasFilter("[FooExtension<BarA>Id] IS NOT NULL");

                    b.HasDiscriminator().HasValue("BarA");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+FooExtension<Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA>", null)
                        .WithOne("Bar")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA", "FooExtensionId");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+FooExtension<Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BarA>", b =>
                {
                    b.Navigation("Bar");
                });
""", usingSystem: true),
            model =>
            {
                var entityType = model.FindEntityType(typeof(BarA).FullName);
                Assert.NotNull(entityType);

                var property = entityType.FindProperty("FooExtensionId");
                Assert.NotNull(property);
                Assert.Equal("FooExtension<BarA>Id", property.GetColumnName());
            });

    [ConditionalFact]
    public virtual void Property_column_name_on_specific_table_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                builder.Entity<DuplicateDerivedEntity>().HasBaseType<BaseEntity>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(34)
                        .HasColumnType("nvarchar(34)");

                    b.HasKey("Id");

                    b.ToTable("BaseEntity", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BaseEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("DerivedEntity");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DuplicateDerivedEntity", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("BaseEntity", "DefaultSchema", t =>
                        {
                            t.Property("Name")
                                .HasColumnName("DuplicateDerivedEntity_Name");
                        });

                    b.HasDiscriminator().HasValue("DuplicateDerivedEntity");
                });
"""),
            o =>
            {
                Assert.Equal(3, o.GetEntityTypes().Count());
                Assert.Collection(
                    o.GetEntityTypes(),
                    t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity", t.Name),
                    t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity", t.Name),
                    t =>
                    {
                        Assert.Equal(
                            "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DuplicateDerivedEntity", t.Name);
                        Assert.Equal(
                            "DuplicateDerivedEntity_Name",
                            t.FindProperty(nameof(DuplicateDerivedEntity.Name))
                                .GetColumnName(StoreObjectIdentifier.Table(nameof(BaseEntity), "DefaultSchema")));
                    }
                );
            });

    [ConditionalFact]
    public virtual void Property_column_type_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnType("CType");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("CType");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal("CType", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnType"]));

    [ConditionalFact]
    public virtual void Property_default_value_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue(1);
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(1, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValue"]));

    [ConditionalFact]
    public virtual void Property_default_value_annotation_is_stored_in_snapshot_as_fluent_api_unspecified()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue();

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
""",
                usingSystem: true),
            o => Assert.Equal(DBNull.Value, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValue"]));

    [ConditionalFact]
    public virtual void Property_default_value_sql_annotation_is_stored_in_snapshot_as_fluent_api_unspecified()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValueSql();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql();

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(string.Empty, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValueSql"]));

    [ConditionalFact]
    public virtual void Property_default_value_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValueSql("SQL");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("SQL");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValueSql"]));

    [ConditionalFact]
    public virtual void Property_computed_column_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql("SQL");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int")
                        .HasComputedColumnSql("SQL");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ComputedColumnSql"]));

    [ConditionalFact]
    public virtual void Property_computed_column_sql_stored_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql("SQL", true);
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int")
                        .HasComputedColumnSql("SQL", true);

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ComputedColumnSql"]);
                Assert.Equal(true, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:IsStored"]);
            });

    [ConditionalFact]
    public virtual void Property_computed_column_sql_annotation_is_stored_in_snapshot_as_fluent_api_unspecified()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int")
                        .HasComputedColumnSql();

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(string.Empty, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ComputedColumnSql"]));

    [ConditionalFact]
    public virtual void Property_default_value_of_enum_type_is_stored_in_snapshot_without_actual_enum()
        => Test(
            builder => builder.Entity<EntityWithEnumType>().Property(e => e.Day).HasDefaultValue(Days.Wed),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("Day")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(3L);

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");
                });
"""),
            o => Assert.Equal(3L, o.GetEntityTypes().First().FindProperty("Day")["Relational:DefaultValue"]));

    [ConditionalFact]
    public virtual void Property_enum_type_is_stored_in_snapshot_with_custom_conversion_and_seed_data()
        => Test(
            builder => builder.Entity<EntityWithEnumType>(
                eb =>
                {
                    eb.Property(e => e.Day).HasDefaultValue(Days.Wed)
                        .HasConversion(v => v.ToString(), v => (Days)Enum.Parse(typeof(Days), v));
                    eb.HasData(
                        new { Id = 1, Day = Days.Fri });
                }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Day")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(max)")
                        .HasDefaultValue("Wed");

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Day = "Fri"
                        });
                });
"""),
            o =>
            {
                var property = o.GetEntityTypes().First().FindProperty("Day");
                Assert.Equal(typeof(string), property.ClrType);
                Assert.Equal(nameof(Days.Wed), property["Relational:DefaultValue"]);
                Assert.False(property.IsNullable);
            });

    [ConditionalFact]
    public virtual void Property_of_nullable_enum()
        => Test(
            builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("Day")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("EntityWithNullableEnumType", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));

    [ConditionalFact]
    public virtual void Property_of_enum_to_nullable()
        => Test(
            builder => builder.Entity<EntityWithEnumType>().Property(e => e.Day)
                .HasConversion(m => (long?)m, p => p.HasValue ? (Days)p.Value : default),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("Day")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("EntityWithEnumType", "DefaultSchema");
                });
""", usingSystem: true),
            o => Assert.False(o.GetEntityTypes().First().FindProperty("Day").IsNullable));

    [ConditionalFact]
    public virtual void Property_of_nullable_enum_to_string()
        => Test(
            builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day).HasConversion<string>(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Day")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithNullableEnumType", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));

    [ConditionalFact]
    public virtual void Property_multiple_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName")
                    .HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int")
                        .HasColumnName("CName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.GetEntityTypes().First().FindProperty("AlternateId");
                Assert.Equal(3, property.GetAnnotations().Count());
                Assert.Equal("AnnotationValue", property["AnnotationName"]);
                Assert.Equal("CName", property["Relational:ColumnName"]);
                Assert.Equal("int", property["Relational:ColumnType"]);
            });

    [ConditionalFact]
    public virtual void Property_without_column_type()
        => Test(
            builder =>
            {
                builder
                    .HasAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, SqlServerValueGenerationStrategy.IdentityColumn);

                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation(
                                SqlServerAnnotationNames.ValueGenerationStrategy, SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal("int", property.GetColumnType());
            });

    [ConditionalFact]
    public virtual void Property_with_identity_column()
        => Test(
            builder =>
            {
                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id").UseIdentityColumn();

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
                Assert.Equal(1, property.GetIdentitySeed());
                Assert.Equal(1, property.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Property_with_identity_column_custom_seed()
        => Test(
            builder =>
            {
                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id").UseIdentityColumn(seed: 5);

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 5L);

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
                Assert.Equal(5, property.GetIdentitySeed());
                Assert.Equal(1, property.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Property_with_identity_column_custom_increment()
        => Test(
            builder =>
            {
                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id").UseIdentityColumn(increment: 5);

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 5);

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
                Assert.Equal(1, property.GetIdentitySeed());
                Assert.Equal(5, property.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Property_with_identity_column_custom_seed_increment()
        => Test(
            builder =>
            {
                builder.Entity(
                    "Building", b =>
                    {
                        b.Property<int>("Id").UseIdentityColumn(5, 5);

                        b.HasKey("Id");

                        b.ToTable("Buildings", "DefaultSchema");
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 5L, 5);

                    b.HasKey("Id");

                    b.ToTable("Buildings", "DefaultSchema");
                });
"""),
            o =>
            {
                var property = o.FindEntityType("Building").FindProperty("Id");
                Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
                Assert.Equal(5, property.GetIdentitySeed());
                Assert.Equal(5, property.GetIdentityIncrement());
            });

    [ConditionalFact]
    public virtual void Property_column_order_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnOrder(1);
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(1, o.GetEntityTypes().First().FindProperty("AlternateId").GetColumnOrder()));

    [ConditionalFact]
    public virtual void SQLServer_model_legacy_identity_seed_int_annotation()
        => Test(
            builder => builder.HasAnnotation(SqlServerAnnotationNames.IdentitySeed, 8),
            AddBoilerPlate(
"""
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 8L);
"""),
            o => Assert.Equal(8L, o.GetIdentitySeed()));

    [ConditionalFact]
    public virtual void SQLServer_property_legacy_identity_seed_int_annotation()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().Property(e => e.Id)
                    .HasAnnotation(SqlServerAnnotationNames.IdentitySeed, 8);
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 8L);

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(8L, o.GetEntityTypes().First().FindProperty("Id").GetIdentitySeed()));

    #endregion

    #region Complex types

    [ConditionalFact]
    public virtual void Complex_properties_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>(
                    b =>
                    {
                        b.ComplexProperty(eo => eo.EntityWithTwoProperties, eb =>
                        {
                            eb.Property(e => e.AlternateId).HasColumnOrder(1);
                            eb.ComplexProperty(e => e.EntityWithStringKey);
                        });
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.ComplexProperty<Dictionary<string, object>>("EntityWithTwoProperties", "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties", b1 =>
                        {
                            b1.Property<int>("AlternateId")
                                .HasColumnType("int")
                                .HasColumnOrder(1);

                            b1.Property<int>("Id")
                                .HasColumnType("int");

                            b1.ComplexProperty<Dictionary<string, object>>("EntityWithStringKey", "Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties.EntityWithStringKey#EntityWithStringKey", b2 =>
                                {
                                    b2.Property<string>("Id")
                                        .HasColumnType("nvarchar(max)");
                                });
                        });

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
""", usingCollections: true),
            o =>
            {
                var entityWithOneProperty = o.FindEntityType(typeof(EntityWithOneProperty));
                Assert.Equal(nameof(EntityWithOneProperty), entityWithOneProperty.GetTableName());

                var complexProperty = entityWithOneProperty.FindComplexProperty(nameof(EntityWithOneProperty.EntityWithTwoProperties));
                Assert.False(complexProperty.IsCollection);
                Assert.True(complexProperty.IsNullable);
                var complexType = complexProperty.ComplexType;
                Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties", complexType.Name);
                Assert.Equal("EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties", complexType.DisplayName());
                Assert.Equal(nameof(EntityWithOneProperty), complexType.GetTableName());
                var alternateIdProperty = complexType.FindProperty(nameof(EntityWithTwoProperties.AlternateId));
                Assert.Equal(1, alternateIdProperty.GetColumnOrder());

                var nestedComplexProperty = complexType.FindComplexProperty(nameof(EntityWithTwoProperties.EntityWithStringKey));
                Assert.False(nestedComplexProperty.IsCollection);
                Assert.True(nestedComplexProperty.IsNullable);
                var nestedComplexType = nestedComplexProperty.ComplexType;
                Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties.EntityWithStringKey#EntityWithStringKey", nestedComplexType.Name);
                Assert.Equal("EntityWithOneProperty.EntityWithTwoProperties#EntityWithTwoProperties.EntityWithStringKey#EntityWithStringKey", nestedComplexType.DisplayName());
                Assert.Equal(nameof(EntityWithOneProperty), nestedComplexType.GetTableName());
                var nestedIdProperty = nestedComplexType.FindProperty(nameof(EntityWithStringKey.Id));
                Assert.True(nestedIdProperty.IsNullable);
            });

    #endregion

    #region HasKey

    [ConditionalFact]
    public virtual void Key_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId)
                    .HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("AlternateId")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(
                "AnnotationValue", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["AnnotationName"]));

    [ConditionalFact]
    public virtual void Key_Fluent_APIs_are_properly_generated()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>().HasKey(t => t.Id).IsClustered();
                builder.Ignore<EntityWithTwoProperties>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().GetKeys().Single(k => k.IsPrimaryKey()).IsClustered()));

    [ConditionalFact]
    public virtual void Key_name_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("KeyName");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("AlternateId")
                        .HasName("KeyName");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(
                "KeyName", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["Relational:Name"]));

    [ConditionalFact]
    public virtual void Key_multiple_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("IndexName")
                    .HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("AlternateId")
                        .HasName("IndexName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                var key = o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First();
                Assert.Equal(2, key.GetAnnotations().Count());
                Assert.Equal("AnnotationValue", key["AnnotationName"]);
                Assert.Equal("IndexName", key["Relational:Name"]);
            });

    #endregion

    #region Index

    [ConditionalFact]
    public virtual void Index_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                    .HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal("AnnotationValue", o.GetEntityTypes().First().GetIndexes().First()["AnnotationName"]));

    [ConditionalFact]
    public virtual void Index_Fluent_APIs_are_properly_generated()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsClustered();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("AlternateId"));

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().Single().GetIndexes().Single().IsClustered()));

    [ConditionalFact]
    public virtual void Index_IsUnique_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsUnique();
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.True(o.GetEntityTypes().First().GetIndexes().First().IsUnique));

    [ConditionalFact]
    public virtual void Index_IsDescending_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithThreeProperties>(
                    e =>
                    {
                        e.HasIndex(
                            t => new
                            {
                                t.X,
                                t.Y,
                                t.Z
                            }, "IX_unspecified");
                        e.HasIndex(
                                t => new
                                {
                                    t.X,
                                    t.Y,
                                    t.Z
                                }, "IX_empty")
                            .IsDescending();
                        e.HasIndex(
                                t => new
                                {
                                    t.X,
                                    t.Y,
                                    t.Z
                                }, "IX_all_ascending")
                            .IsDescending(false, false, false);
                        e.HasIndex(
                                t => new
                                {
                                    t.X,
                                    t.Y,
                                    t.Z
                                }, "IX_all_descending")
                            .IsDescending(true, true, true);
                        e.HasIndex(
                                t => new
                                {
                                    t.X,
                                    t.Y,
                                    t.Z
                                }, "IX_mixed")
                            .IsDescending(false, true, false);
                    });
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithThreeProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("X")
                        .HasColumnType("int");

                    b.Property<int>("Y")
                        .HasColumnType("int");

                    b.Property<int>("Z")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "X", "Y", "Z" }, "IX_all_ascending");

                    b.HasIndex(new[] { "X", "Y", "Z" }, "IX_all_descending")
                        .IsDescending();

                    b.HasIndex(new[] { "X", "Y", "Z" }, "IX_empty")
                        .IsDescending();

                    b.HasIndex(new[] { "X", "Y", "Z" }, "IX_mixed")
                        .IsDescending(false, true, false);

                    b.HasIndex(new[] { "X", "Y", "Z" }, "IX_unspecified");

                    b.ToTable("EntityWithThreeProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                var entityType = o.GetEntityTypes().Single();
                Assert.Equal(5, entityType.GetIndexes().Count());

                var unspecifiedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_unspecified");
                Assert.Null(unspecifiedIndex.IsDescending);

                var emptyIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_empty");
                Assert.Equal(Array.Empty<bool>(), emptyIndex.IsDescending);

                var allAscendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_ascending");
                Assert.Null(allAscendingIndex.IsDescending);

                var allDescendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_descending");
                Assert.Equal(Array.Empty<bool>(), allDescendingIndex.IsDescending);

                var mixedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_mixed");
                Assert.Equal(new[] { false, true, false }, mixedIndex.IsDescending);
            });

    [ConditionalFact]
    public virtual void Index_database_name_annotation_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasIndex(t => t.AlternateId)
                    .HasDatabaseName("IndexName");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .HasDatabaseName("IndexName");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                var index = o.GetEntityTypes().First().GetIndexes().First();
                Assert.Null(index.Name);
                Assert.Equal("IndexName", index.GetDatabaseName());
            });

    [ConditionalFact]
    public virtual void Index_filter_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                    .HasFilter("AlternateId <> 0");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .HasFilter("AlternateId <> 0");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o => Assert.Equal(
                "AlternateId <> 0",
                o.GetEntityTypes().First().GetIndexes().First().GetFilter()));

    [ConditionalFact]
    public virtual void Index_multiple_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId, "IndexName")
                    .HasAnnotation("AnnotationName", "AnnotationValue");
                builder.Ignore<EntityWithOneProperty>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "AlternateId" }, "IndexName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });
"""),
            o =>
            {
                var index = o.GetEntityTypes().First().GetIndexes().First();
                Assert.Equal("IndexName", index.Name);
                Assert.Single(index.GetAnnotations());
                Assert.Equal("AnnotationValue", index["AnnotationName"]);
                Assert.Null(index["RelationalAnnotationNames.Name"]);
            });

    [ConditionalFact]
    public virtual void Index_with_default_constraint_name_exceeding_max()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>(
                x =>
                {
                    const string propertyName =
                        "SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit";
                    x.Property<string>(propertyName);
                    x.HasIndex(propertyName);
                }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            model => Assert.Equal(128, model.GetEntityTypes().First().GetIndexes().First().GetDatabaseName().Length));

    [ConditionalFact]
    public virtual void IndexAttribute_causes_column_to_have_key_or_index_column_length()
        => Test(
            builder => builder.Entity<EntityWithIndexAttribute>(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithIndexAttribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("FirstName", "LastName");

                    b.ToTable("EntityWithIndexAttribute", "DefaultSchema");
                });
"""),
            model =>
                Assert.Collection(
                    model.GetEntityTypes().First().GetIndexes().First().Properties,
                    p0 =>
                    {
                        Assert.Equal("FirstName", p0.Name);
                        Assert.Equal("nvarchar(450)", p0.GetColumnType());
                    },
                    p1 =>
                    {
                        Assert.Equal("LastName", p1.Name);
                        Assert.Equal("nvarchar(450)", p1.GetColumnType());
                    }
                ));

    [ConditionalFact]
    public virtual void IndexAttribute_name_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithNamedIndexAttribute>(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNamedIndexAttribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "FirstName", "LastName" }, "NamedIndex");

                    b.ToTable("EntityWithNamedIndexAttribute", "DefaultSchema");
                });
"""),
            model =>
            {
                var index = model.GetEntityTypes().First().GetIndexes().First();
                Assert.Equal("NamedIndex", index.Name);
                Assert.Collection(
                    index.Properties,
                    p0 =>
                    {
                        Assert.Equal("FirstName", p0.Name);
                        Assert.Equal("nvarchar(450)", p0.GetColumnType());
                    },
                    p1 =>
                    {
                        Assert.Equal("LastName", p1.Name);
                        Assert.Equal("nvarchar(450)", p1.GetColumnType());
                    }
                );
            });

    [ConditionalFact]
    public virtual void IndexAttribute_IsUnique_is_stored_in_snapshot()
        => Test(
            builder => builder.Entity<EntityWithUniqueIndexAttribute>(),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithUniqueIndexAttribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("FirstName", "LastName")
                        .IsUnique()
                        .HasFilter("[FirstName] IS NOT NULL AND [LastName] IS NOT NULL");

                    b.ToTable("EntityWithUniqueIndexAttribute", "DefaultSchema");
                });
"""),
            model =>
            {
                var index = model.GetEntityTypes().First().GetIndexes().First();
                Assert.True(index.IsUnique);
                Assert.Collection(
                    index.Properties,
                    p0 =>
                    {
                        Assert.Equal("FirstName", p0.Name);
                        Assert.Equal("nvarchar(450)", p0.GetColumnType());
                    },
                    p1 =>
                    {
                        Assert.Equal("LastName", p1.Name);
                        Assert.Equal("nvarchar(450)", p1.GetColumnType());
                    }
                );
            });

    [ConditionalFact]
    public virtual void IndexAttribute_IncludeProperties_generated_without_fluent_api()
        => Test(
            builder => builder.Entity<EntityWithStringProperty>(
                x =>
                {
                    x.HasIndex(e => e.Id).IncludeProperties(e => e.Name);
                }),
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Id"), new[] { "Name" });

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });
"""),
            model =>
            {
                var index = model.GetEntityTypes().First().GetIndexes().First();
                Assert.Equal("Name", Assert.Single(index.GetIncludeProperties()));
            });

    #endregion

    #region ForeignKey

    [ConditionalFact]
    public virtual void ForeignKey_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                    .HasAnnotation("AnnotationName", "AnnotationValue");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o => Assert.Equal(
                "AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["AnnotationName"]));

    [ConditionalFact]
    public virtual void ForeignKey_isRequired_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithStringKey>().Ignore(e => e.Properties);
                builder.Entity<EntityWithStringProperty>()
                    .HasOne<EntityWithStringKey>()
                    .WithOne()
                    .HasForeignKey<EntityWithStringProperty>(e => e.Name)
                    .IsRequired();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringKey", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", null)
                        .WithOne()
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", "Name")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
"""),
            o => Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).FindProperty("Name").IsNullable));

    [ConditionalFact]
    public virtual void ForeignKey_isUnique_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithStringProperty>()
                    .HasOne<EntityWithStringKey>()
                    .WithMany(e => e.Properties)
                    .HasForeignKey(e => e.Name);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringKey", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", null)
                        .WithMany("Properties")
                        .HasForeignKey("Name");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey", b =>
                {
                    b.Navigation("Properties");
                });
"""),
            o => Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique));

    [ConditionalFact]
    public virtual void ForeignKey_with_non_primary_principal_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithStringProperty>()
                    .HasOne<EntityWithStringAlternateKey>()
                    .WithMany(e => e.Properties)
                    .HasForeignKey(e => e.Name)
                    .HasPrincipalKey(e => e.AlternateId);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringAlternateKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlternateId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithStringAlternateKey", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("EntityWithStringProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringAlternateKey", null)
                        .WithMany("Properties")
                        .HasForeignKey("Name")
                        .HasPrincipalKey("AlternateId");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringAlternateKey", b =>
                {
                    b.Navigation("Properties");
                });
"""),
            o => Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique));

    [ConditionalFact]
    public virtual void ForeignKey_deleteBehavior_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .HasOne(e => e.EntityWithTwoProperties)
                    .WithMany()
                    .HasForeignKey(e => e.Id);
                builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o => Assert.Equal(
                DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior));

    [ConditionalFact]
    public virtual void ForeignKey_deleteBehavior_is_stored_in_snapshot_for_one_to_one()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .HasOne(e => e.EntityWithTwoProperties)
                    .WithOne(e => e.EntityWithOneProperty)
                    .HasForeignKey<EntityWithOneProperty>(e => e.Id);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties")
                        .WithOne("EntityWithOneProperty")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithTwoProperties");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Navigation("EntityWithOneProperty");
                });
"""),
            o => Assert.Equal(
                DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior));

    [ConditionalFact]
    public virtual void ForeignKey_name_preserved_when_generic()
    {
        IReadOnlyModel originalModel = null;

        Test(
            builder =>
            {
                builder.Entity<EntityWithGenericKey<Guid>>().HasMany<EntityWithGenericProperty<Guid>>().WithOne()
                    .HasForeignKey(e => e.Property);

                originalModel = builder.Model;
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("EntityWithGenericKey<Guid>", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("Property")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Property");

                    b.ToTable("EntityWithGenericProperty<Guid>", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("Property")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
""", usingSystem: true),
            model =>
            {
                var originalParent = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                var parent = model.FindEntityType(originalParent.Name);
                Assert.NotNull(parent);

                var originalChild = originalModel.FindEntityType(typeof(EntityWithGenericProperty<Guid>));
                var child = model.FindEntityType(originalChild.Name);
                Assert.NotNull(child);

                var originalForeignKey = originalChild.FindForeignKey(
                    originalChild.FindProperty("Property"),
                    originalParent.FindPrimaryKey(),
                    originalParent);
                var foreignKey = child.FindForeignKey(
                    child.FindProperty("Property"),
                    parent.FindPrimaryKey(),
                    parent);

                Assert.Equal(originalForeignKey.GetConstraintName(), foreignKey.GetConstraintName());

                var originalIndex = originalChild.FindIndex(originalChild.FindProperty("Property"));
                var index = child.FindIndex(child.FindProperty("Property"));

                Assert.Equal(originalIndex.GetDatabaseName(), index.GetDatabaseName());
            });
    }

    [ConditionalFact]
    public virtual void ForeignKey_constraint_name_is_stored_in_snapshot_as_fluent_api()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                    .HasConstraintName("Constraint");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Constraint");

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o => Assert.Equal(
                "Constraint", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["Relational:Name"]));

    [ConditionalFact]
    public virtual void ForeignKey_multiple_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                    .HasAnnotation("AnnotationName", "AnnotationValue")
                    .HasConstraintName("Constraint");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Constraint")
                        .HasAnnotation("AnnotationName", "AnnotationValue");

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o =>
            {
                var fk = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First();
                Assert.Equal(2, fk.GetAnnotations().Count());
                Assert.Equal("AnnotationValue", fk["AnnotationName"]);
                Assert.Equal("Constraint", fk["Relational:Name"]);
            });

    [ConditionalFact]
    public virtual void Do_not_generate_entity_type_builder_again_if_no_foreign_key_is_defined_on_it()
        => Test(
            builder =>
            {
                builder.Entity<BaseType>();
                builder.Ignore<EntityWithTwoProperties>();
                builder.Entity<DerivedType>();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.Property<int?>("NavigationId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NavigationId");

                    b.ToTable("BaseType", "DefaultSchema");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BaseType");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedType", b =>
                {
                    b.HasBaseType("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType");

                    b.HasDiscriminator().HasValue("DerivedType");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "Navigation")
                        .WithMany()
                        .HasForeignKey("NavigationId");

                    b.Navigation("Navigation");
                });
""", usingSystem: true),
            o => { });

    [ConditionalFact]
    public virtual void ForeignKey_principal_key_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .HasOne(e => e.EntityWithTwoProperties)
                    .WithOne(e => e.EntityWithOneProperty)
                    .HasForeignKey<EntityWithOneProperty>(e => e.Id)
                    .HasPrincipalKey<EntityWithTwoProperties>(e => e.AlternateId);
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties")
                        .WithOne("EntityWithOneProperty")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "Id")
                        .HasPrincipalKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithTwoProperties");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Navigation("EntityWithOneProperty");
                });
"""),
            o =>
            {
                Assert.Equal(2, o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Count());
                Assert.True(o.FindEntityType(typeof(EntityWithTwoProperties)).FindProperty("AlternateId").IsKey());
            });

    [ConditionalFact]
    public virtual void ForeignKey_principal_key_with_non_default_name_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithOneProperty>()
                    .HasOne(e => e.EntityWithTwoProperties)
                    .WithOne(e => e.EntityWithOneProperty)
                    .HasForeignKey<EntityWithOneProperty>(e => e.Id)
                    .HasPrincipalKey<EntityWithTwoProperties>(e => e.AlternateId);

                builder.Entity<EntityWithTwoProperties>().HasAlternateKey(e => e.AlternateId).HasAnnotation("Name", "Value");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("AlternateId")
                        .HasAnnotation("Name", "Value");

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "EntityWithTwoProperties")
                        .WithOne("EntityWithOneProperty")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "Id")
                        .HasPrincipalKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithTwoProperties");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Navigation("EntityWithOneProperty");
                });
"""),
            o =>
            {
                var entityType = o.FindEntityType(typeof(EntityWithTwoProperties));

                Assert.Equal(2, entityType.GetKeys().Count());
                Assert.Equal("Value", entityType.FindKey(entityType.FindProperty("AlternateId"))["Name"]);
            });

    #endregion

    #region Navigation

    [ConditionalFact]
    public virtual void Navigation_annotations_are_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId);

                builder.Entity<EntityWithTwoProperties>().Navigation(e => e.EntityWithOneProperty)
                    .HasAnnotation("AnnotationName", "AnnotationValue");
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithOneProperty")
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties");
                });
"""),
            o => Assert.Equal(
                "AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetNavigations().First()["AnnotationName"]));

    [ConditionalFact]
    public virtual void Navigation_isRequired_is_stored_in_snapshot()
        => Test(
            builder =>
            {
                builder.Entity<EntityWithTwoProperties>()
                    .HasOne(e => e.EntityWithOneProperty)
                    .WithOne(e => e.EntityWithTwoProperties)
                    .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId);

                builder.Entity<EntityWithOneProperty>().Navigation(e => e.EntityWithTwoProperties)
                    .IsRequired();
            },
            AddBoilerPlate(
                GetHeading() +
"""
            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.HasKey("Id");

                    b.ToTable("EntityWithOneProperty", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlternateId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlternateId")
                        .IsUnique();

                    b.ToTable("EntityWithTwoProperties", "DefaultSchema");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", b =>
                {
                    b.HasOne("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", "EntityWithOneProperty")
                        .WithOne("EntityWithTwoProperties")
                        .HasForeignKey("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", "AlternateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EntityWithOneProperty");
                });

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", b =>
                {
                    b.Navigation("EntityWithTwoProperties")
                        .IsRequired();
                });
"""),
            o => Assert.True(o.FindEntityType(typeof(EntityWithOneProperty)).GetNavigations().First().ForeignKey.IsRequiredDependent));

    #endregion

    #region SeedData

    [ConditionalFact]
    public virtual void SeedData_annotations_are_stored_in_snapshot()
    {
        static List<IProperty> getAllProperties(IModel model)
            => model
                .GetEntityTypes()
                .SelectMany(m => m.GetProperties())
                .OrderBy(p => p.DeclaringType.Name)
                .ThenBy(p => p.Name)
                .ToList();

        var lineString1 = new LineString(
            new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(7.1, 7.2) })
        {
            SRID = 4326
        };

        var lineString2 = new LineString(
            new[] { new Coordinate(7.1, 7.2), new Coordinate(20.2, 20.2), new Coordinate(20.20, 1.1), new Coordinate(70.1, 70.2) })
        {
            SRID = 4326
        };

        var multiPoint = new MultiPoint(
            new[] { new Point(1.1, 2.2), new Point(2.2, 2.2), new Point(2.2, 1.1) }) { SRID = 4326 };

        var polygon1 = new Polygon(
            new LinearRing(
                new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2) }))
        {
            SRID = 4326
        };

        var polygon2 = new Polygon(
            new LinearRing(
                new[] { new Coordinate(10.1, 20.2), new Coordinate(20.2, 20.2), new Coordinate(20.2, 10.1), new Coordinate(10.1, 20.2) }))
        {
            SRID = 4326
        };

        var point1 = new Point(1.1, 2.2, 3.3) { SRID = 4326 };

        var multiLineString = new MultiLineString(
            new[] { lineString1, lineString2 }) { SRID = 4326 };

        var multiPolygon = new MultiPolygon(
            new[] { polygon2, polygon1 }) { SRID = 4326 };

        var geometryCollection = new GeometryCollection(
            new Geometry[] { lineString1, lineString2, multiPoint, polygon1, polygon2, point1, multiLineString, multiPolygon })
        {
            SRID = 4326
        };

        Test(
            builder =>
            {
                builder.Entity<EntityWithManyProperties>(
                    eb =>
                    {
                        eb.Property<decimal?>("OptionalProperty");

                        eb.HasData(
                            new EntityWithManyProperties
                            {
                                Id = 42,
                                String = "FortyThree",
                                Bytes = new byte[] { 44, 45 },
                                Int16 = 46,
                                Int32 = 47,
                                Int64 = 48,
                                Double = 49.0,
                                Decimal = 50.0m,
                                DateTime = new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc),
                                DateTimeOffset = new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(1, 0, 0)),
                                TimeSpan = new TimeSpan(51, 52, 53),
                                Single = 54.0f,
                                Boolean = true,
                                Byte = 55,
                                UnsignedInt16 = 56,
                                UnsignedInt32 = 57,
                                UnsignedInt64 = 58,
                                Character = '9',
                                SignedByte = 60,
                                Enum64 = Enum64.SomeValue,
                                Enum32 = Enum32.SomeValue,
                                Enum16 = Enum16.SomeValue,
                                Enum8 = Enum8.SomeValue,
                                EnumU64 = EnumU64.SomeValue,
                                EnumU32 = EnumU32.SomeValue,
                                EnumU16 = EnumU16.SomeValue,
                                EnumS8 = EnumS8.SomeValue,
                                SpatialBGeometryCollection = geometryCollection,
                                SpatialBLineString = lineString1,
                                SpatialBMultiLineString = multiLineString,
                                SpatialBMultiPoint = multiPoint,
                                SpatialBMultiPolygon = multiPolygon,
                                SpatialBPoint = point1,
                                SpatialBPolygon = polygon1,
                                SpatialCGeometryCollection = geometryCollection,
                                SpatialCLineString = lineString1,
                                SpatialCMultiLineString = multiLineString,
                                SpatialCMultiPoint = multiPoint,
                                SpatialCMultiPolygon = multiPolygon,
                                SpatialCPoint = point1,
                                SpatialCPolygon = polygon1
                            },
                            new
                            {
                                Id = 43,
                                String = "FortyThree",
                                Bytes = new byte[] { 44, 45 },
                                Int16 = (short)-46,
                                Int32 = -47,
                                Int64 = (long)-48,
                                Double = -49.0,
                                Decimal = -50.0m,
                                DateTime = new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc),
                                DateTimeOffset = new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(-1, 0, 0)),
                                TimeSpan = new TimeSpan(-51, 52, 53),
                                Single = -54.0f,
                                Boolean = true,
                                Byte = (byte)55,
                                UnsignedInt16 = (ushort)56,
                                UnsignedInt32 = (uint)57,
                                UnsignedInt64 = (ulong)58,
                                Character = '9',
                                SignedByte = (sbyte)-60,
                                Enum64 = Enum64.SomeValue,
                                Enum32 = Enum32.SomeValue,
                                Enum16 = Enum16.SomeValue,
                                Enum8 = Enum8.SomeValue,
                                EnumU64 = EnumU64.SomeValue,
                                EnumU32 = EnumU32.SomeValue,
                                EnumU16 = EnumU16.SomeValue,
                                EnumS8 = EnumS8.SomeValue
                            });
                    });
                builder.Ignore<EntityWithTwoProperties>();
            },
"""
// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

#nullable disable

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithManyProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Boolean")
                        .HasColumnType("bit");

                    b.Property<byte>("Byte")
                        .HasColumnType("tinyint");

                    b.Property<byte[]>("Bytes")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Character")
                        .IsRequired()
                        .HasColumnType("nvarchar(1)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTimeOffset>("DateTimeOffset")
                        .HasColumnType("datetimeoffset");

                    b.Property<decimal>("Decimal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<double>("Double")
                        .HasColumnType("float");

                    b.Property<short>("Enum16")
                        .HasColumnType("smallint");

                    b.Property<int>("Enum32")
                        .HasColumnType("int");

                    b.Property<long>("Enum64")
                        .HasColumnType("bigint");

                    b.Property<byte>("Enum8")
                        .HasColumnType("tinyint");

                    b.Property<short>("EnumS8")
                        .HasColumnType("smallint");

                    b.Property<int>("EnumU16")
                        .HasColumnType("int");

                    b.Property<long>("EnumU32")
                        .HasColumnType("bigint");

                    b.Property<decimal>("EnumU64")
                        .HasColumnType("decimal(20,0)");

                    b.Property<short>("Int16")
                        .HasColumnType("smallint");

                    b.Property<int>("Int32")
                        .HasColumnType("int");

                    b.Property<long>("Int64")
                        .HasColumnType("bigint");

                    b.Property<decimal?>("OptionalProperty")
                        .HasColumnType("decimal(18,2)");

                    b.Property<short>("SignedByte")
                        .HasColumnType("smallint");

                    b.Property<float>("Single")
                        .HasColumnType("real");

                    b.Property<Geometry>("SpatialBGeometryCollection")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBLineString")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBMultiLineString")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBMultiPoint")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBMultiPolygon")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBPoint")
                        .HasColumnType("geography");

                    b.Property<Geometry>("SpatialBPolygon")
                        .HasColumnType("geography");

                    b.Property<GeometryCollection>("SpatialCGeometryCollection")
                        .HasColumnType("geography");

                    b.Property<LineString>("SpatialCLineString")
                        .HasColumnType("geography");

                    b.Property<MultiLineString>("SpatialCMultiLineString")
                        .HasColumnType("geography");

                    b.Property<MultiPoint>("SpatialCMultiPoint")
                        .HasColumnType("geography");

                    b.Property<MultiPolygon>("SpatialCMultiPolygon")
                        .HasColumnType("geography");

                    b.Property<Point>("SpatialCPoint")
                        .HasColumnType("geography");

                    b.Property<Polygon>("SpatialCPolygon")
                        .HasColumnType("geography");

                    b.Property<string>("String")
                        .HasColumnType("nvarchar(max)");

                    b.Property<TimeSpan>("TimeSpan")
                        .HasColumnType("time");

                    b.Property<int>("UnsignedInt16")
                        .HasColumnType("int");

                    b.Property<long>("UnsignedInt32")
                        .HasColumnType("bigint");

                    b.Property<decimal>("UnsignedInt64")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("EntityWithManyProperties", "DefaultSchema");

                    b.HasData(
                        new
                        {
                            Id = 42,
                            Boolean = true,
                            Byte = (byte)55,
                            Bytes = new byte[] { 44, 45 },
                            Character = "9",
                            DateTime = new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc),
                            DateTimeOffset = new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Unspecified), new TimeSpan(0, 1, 0, 0, 0)),
                            Decimal = 50.0m,
                            Double = 49.0,
                            Enum16 = (short)1,
                            Enum32 = 1,
                            Enum64 = 1L,
                            Enum8 = (byte)1,
                            EnumS8 = (short)-128,
                            EnumU16 = 65535,
                            EnumU32 = 4294967295L,
                            EnumU64 = 1234567890123456789m,
                            Int16 = (short)46,
                            Int32 = 47,
                            Int64 = 48L,
                            SignedByte = (short)60,
                            Single = 54f,
                            SpatialBGeometryCollection = (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;GEOMETRYCOLLECTION Z(LINESTRING Z(1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), LINESTRING Z(7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN), MULTIPOINT Z((1.1 2.2 NaN), (2.2 2.2 NaN), (2.2 1.1 NaN)), POLYGON Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN)), POLYGON Z((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), POINT Z(1.1 2.2 3.3), MULTILINESTRING Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), (7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN)), MULTIPOLYGON Z(((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), ((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN))))"),
                            SpatialBLineString = (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)"),
                            SpatialBMultiLineString = (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))"),
                            SpatialBMultiPoint = (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))"),
                            SpatialBMultiPolygon = (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))"),
                            SpatialBPoint = (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT Z(1.1 2.2 3.3)"),
                            SpatialBPolygon = (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))"),
                            SpatialCGeometryCollection = (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;GEOMETRYCOLLECTION Z(LINESTRING Z(1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), LINESTRING Z(7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN), MULTIPOINT Z((1.1 2.2 NaN), (2.2 2.2 NaN), (2.2 1.1 NaN)), POLYGON Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN)), POLYGON Z((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), POINT Z(1.1 2.2 3.3), MULTILINESTRING Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), (7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN)), MULTIPOLYGON Z(((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), ((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN))))"),
                            SpatialCLineString = (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)"),
                            SpatialCMultiLineString = (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))"),
                            SpatialCMultiPoint = (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))"),
                            SpatialCMultiPolygon = (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))"),
                            SpatialCPoint = (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT Z(1.1 2.2 3.3)"),
                            SpatialCPolygon = (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))"),
                            String = "FortyThree",
                            TimeSpan = new TimeSpan(2, 3, 52, 53, 0),
                            UnsignedInt16 = 56,
                            UnsignedInt32 = 57L,
                            UnsignedInt64 = 58m
                        },
                        new
                        {
                            Id = 43,
                            Boolean = true,
                            Byte = (byte)55,
                            Bytes = new byte[] { 44, 45 },
                            Character = "9",
                            DateTime = new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc),
                            DateTimeOffset = new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Unspecified), new TimeSpan(0, -1, 0, 0, 0)),
                            Decimal = -50.0m,
                            Double = -49.0,
                            Enum16 = (short)1,
                            Enum32 = 1,
                            Enum64 = 1L,
                            Enum8 = (byte)1,
                            EnumS8 = (short)-128,
                            EnumU16 = 65535,
                            EnumU32 = 4294967295L,
                            EnumU64 = 1234567890123456789m,
                            Int16 = (short)-46,
                            Int32 = -47,
                            Int64 = -48L,
                            SignedByte = (short)-60,
                            Single = -54f,
                            String = "FortyThree",
                            TimeSpan = new TimeSpan(-2, -2, -7, -7, 0),
                            UnsignedInt16 = 56,
                            UnsignedInt32 = 57L,
                            UnsignedInt64 = 58m
                        });
                });
#pragma warning restore 612, 618
        }
    }
}

""",
            (snapshotModel, originalModel) =>
            {
                var originalProperties = getAllProperties(originalModel);
                var snapshotProperties = getAllProperties(snapshotModel);

                Assert.Equal(originalProperties.Count, snapshotProperties.Count);

                for (var i = 0; i < originalProperties.Count; i++)
                {
                    var originalProperty = originalProperties[i];
                    var snapshotProperty = snapshotProperties[i];

                    Assert.Equal(originalProperty.DeclaringType.Name, snapshotProperty.DeclaringType.Name);
                    Assert.Equal(originalProperty.Name, snapshotProperty.Name);

                    Assert.Equal(originalProperty.GetColumnType(), snapshotProperty.GetColumnType());
                    Assert.Equal(originalProperty.GetMaxLength(), snapshotProperty.GetMaxLength());
                    Assert.Equal(originalProperty.IsUnicode(), snapshotProperty.IsUnicode());
                    Assert.Equal(originalProperty.IsConcurrencyToken, snapshotProperty.IsConcurrencyToken);
                    Assert.Equal(originalProperty.IsFixedLength(), snapshotProperty.IsFixedLength());
                }

                Assert.Collection(
                    snapshotModel.GetEntityTypes().SelectMany(e => e.GetSeedData()),
                    seed =>
                    {
                        Assert.Equal(42, seed["Id"]);
                        Assert.Equal("FortyThree", seed["String"]);
                        Assert.Equal(
                            new byte[] { 44, 45 }, seed["Bytes"]);
                        Assert.Equal((short)46, seed["Int16"]);
                        Assert.Equal(47, seed["Int32"]);
                        Assert.Equal((long)48, seed["Int64"]);
                        Assert.Equal(49.0, seed["Double"]);
                        Assert.Equal(50.0m, seed["Decimal"]);
                        Assert.Equal(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc), seed["DateTime"]);
                        Assert.Equal(
                            new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(1, 0, 0)),
                            seed["DateTimeOffset"]);
                        Assert.Equal(new TimeSpan(51, 52, 53), seed["TimeSpan"]);
                        Assert.Equal(54.0f, seed["Single"]);
                        Assert.Equal(true, seed["Boolean"]);
                        Assert.Equal((byte)55, seed["Byte"]);
                        Assert.Equal(56, seed["UnsignedInt16"]);
                        Assert.Equal((long)57, seed["UnsignedInt32"]);
                        Assert.Equal((decimal)58, seed["UnsignedInt64"]);
                        Assert.Equal("9", seed["Character"]);
                        Assert.Equal((short)60, seed["SignedByte"]);
                        Assert.Equal(1L, seed["Enum64"]);
                        Assert.Equal(1, seed["Enum32"]);
                        Assert.Equal((short)1, seed["Enum16"]);
                        Assert.Equal((byte)1, seed["Enum8"]);
                        Assert.Equal(1234567890123456789m, seed["EnumU64"]);
                        Assert.Equal(4294967295L, seed["EnumU32"]);
                        Assert.Equal(65535, seed["EnumU16"]);
                        Assert.Equal((short)-128, seed["EnumS8"]);
                        Assert.Equal(geometryCollection, seed["SpatialBGeometryCollection"]);
                        Assert.Equal(lineString1, seed["SpatialBLineString"]);
                        Assert.Equal(multiLineString, seed["SpatialBMultiLineString"]);
                        Assert.Equal(multiPoint, seed["SpatialBMultiPoint"]);
                        Assert.Equal(multiPolygon, seed["SpatialBMultiPolygon"]);
                        Assert.Equal(point1, seed["SpatialBPoint"]);
                        Assert.Equal(polygon1, seed["SpatialBPolygon"]);
                        Assert.Equal(geometryCollection, seed["SpatialCGeometryCollection"]);
                        Assert.Equal(lineString1, seed["SpatialCLineString"]);
                        Assert.Equal(multiLineString, seed["SpatialCMultiLineString"]);
                        Assert.Equal(multiPoint, seed["SpatialCMultiPoint"]);
                        Assert.Equal(multiPolygon, seed["SpatialCMultiPolygon"]);
                        Assert.Equal(point1, seed["SpatialCPoint"]);
                        Assert.Equal(polygon1, seed["SpatialCPolygon"]);

                        Assert.Equal(4326, ((Geometry)seed["SpatialBGeometryCollection"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBLineString"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBMultiLineString"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBMultiPoint"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBMultiPolygon"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBPoint"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialBPolygon"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCGeometryCollection"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCLineString"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCMultiLineString"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCMultiPoint"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCMultiPolygon"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCPoint"]).SRID);
                        Assert.Equal(4326, ((Geometry)seed["SpatialCPolygon"]).SRID);
                    },
                    seed =>
                    {
                        Assert.Equal(43, seed["Id"]);
                        Assert.Equal("FortyThree", seed["String"]);
                        Assert.Equal(
                            new byte[] { 44, 45 }, seed["Bytes"]);
                        Assert.Equal((short)-46, seed["Int16"]);
                        Assert.Equal(-47, seed["Int32"]);
                        Assert.Equal((long)-48, seed["Int64"]);
                        Assert.Equal(-49.0, seed["Double"]);
                        Assert.Equal(-50.0m, seed["Decimal"]);
                        Assert.Equal(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc), seed["DateTime"]);
                        Assert.Equal(
                            new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(-1, 0, 0)),
                            seed["DateTimeOffset"]);
                        Assert.Equal(new TimeSpan(-51, 52, 53), seed["TimeSpan"]);
                        Assert.Equal(-54.0f, seed["Single"]);
                        Assert.Equal(true, seed["Boolean"]);
                        Assert.Equal((byte)55, seed["Byte"]);
                        Assert.Equal(56, seed["UnsignedInt16"]);
                        Assert.Equal((long)57, seed["UnsignedInt32"]);
                        Assert.Equal((decimal)58, seed["UnsignedInt64"]);
                        Assert.Equal("9", seed["Character"]);
                        Assert.Equal((short)-60, seed["SignedByte"]);
                        Assert.Equal(1L, seed["Enum64"]);
                        Assert.Equal(1, seed["Enum32"]);
                        Assert.Equal((short)1, seed["Enum16"]);
                        Assert.Equal((byte)1, seed["Enum8"]);
                        Assert.Equal(1234567890123456789m, seed["EnumU64"]);
                        Assert.Equal(4294967295L, seed["EnumU32"]);
                        Assert.Equal(65535, seed["EnumU16"]);
                        Assert.Equal((short)-128, seed["EnumS8"]);
                    });
            });
    }

    #endregion

    protected virtual string GetHeading(bool empty = false)
        => """
            modelBuilder
                .HasDefaultSchema("DefaultSchema")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

""" + (empty ? null : Environment.NewLine);

    protected virtual ICollection<BuildReference> GetReferences()
        => new List<BuildReference>
        {
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"),
            BuildReference.ByName("NetTopologySuite")
        };

    protected virtual string AddBoilerPlate(string code, bool usingSystem = false, bool usingCollections = false)
        => $$"""
// <auto-generated />
{{(usingSystem
    ? @"using System;
"
    : "")}}{{(usingCollections
    ? @"using System.Collections.Generic;
"
    : "")}}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
{{code}}
#pragma warning restore 612, 618
        }
    }
}

""";

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        => Test(buildModel, expectedCode, (m, _) => assert(m));

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel, IModel> assert)
    {
        var modelBuilder = CreateConventionalModelBuilder();
        modelBuilder.HasDefaultSchema("DefaultSchema");
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        buildModel(modelBuilder);

        var model = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);

        Test(model, expectedCode, assert);
    }

    protected void Test(IModel model, string expectedCode, Action<IModel, IModel> assert)
    {
        var generator = CreateMigrationsGenerator();
        var code = generator.GenerateSnapshot("RootNamespace", typeof(DbContext), "Snapshot", model);

        var modelFromSnapshot = BuildModelFromSnapshotSource(code);
        assert(modelFromSnapshot, model);

        try
        {
            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);
        }
        catch (EqualException e)
        {
            throw new Exception(e.Message + Environment.NewLine + Environment.NewLine + "-- Actual code:" + Environment.NewLine + code);
        }

        var targetOptionsBuilder = TestHelpers
            .AddProviderOptions(new DbContextOptionsBuilder())
            .UseModel(model)
            .EnableSensitiveDataLogging();

        var modelDiffer = CreateModelDiffer(targetOptionsBuilder.Options);

        var noopOperations = modelDiffer.GetDifferences(modelFromSnapshot.GetRelationalModel(), model.GetRelationalModel());
        Assert.Empty(noopOperations);
    }

    protected IModel BuildModelFromSnapshotSource(string code)
    {
        var build = new BuildSource { Sources = { { "Snapshot.cs", code } } };

        foreach (var buildReference in GetReferences())
        {
            build.References.Add(buildReference);
        }

        var assembly = build.BuildInMemory();
        var factoryType = assembly.GetType("RootNamespace.Snapshot");

        var buildModelMethod = factoryType.GetMethod(
            "BuildModel",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(ModelBuilder) },
            null);

        var builder = new ModelBuilder();
        builder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

        buildModelMethod.Invoke(
            Activator.CreateInstance(factoryType),
            new object[] { builder });

        var services = TestHelpers.CreateContextServices(new ServiceCollection().AddEntityFrameworkSqlServerNetTopologySuite());

        var processor = new SnapshotModelProcessor(new TestOperationReporter(), services.GetService<IModelRuntimeInitializer>());
        return processor.Process(builder.Model);
    }

    protected TestHelpers.TestModelBuilder CreateConventionalModelBuilder()
        => TestHelpers.CreateConventionBuilder(
            customServices: new ServiceCollection()
                .AddEntityFrameworkSqlServerNetTopologySuite());

    protected virtual MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
        => (MigrationsModelDiffer)TestHelpers.CreateContext(options).GetService<IMigrationsModelDiffer>();

    protected TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;

    protected CSharpMigrationsGenerator CreateMigrationsGenerator()
    {
        var sqlServerTypeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            new RelationalTypeMappingSourceDependencies(
                new IRelationalTypeMappingSourcePlugin[]
                {
                    new SqlServerNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices.Instance)
                }));

        var codeHelper = new CSharpHelper(sqlServerTypeMappingSource);

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

        return generator;
    }
}
