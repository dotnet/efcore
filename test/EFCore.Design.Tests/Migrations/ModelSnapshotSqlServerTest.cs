// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedTypeParameter
namespace Microsoft.EntityFrameworkCore.Migrations
{
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
            public IGeometry SpatialBGeometryCollection { get; set; }
            public IGeometry SpatialBLineString { get; set; }
            public IGeometry SpatialBMultiLineString { get; set; }
            public IGeometry SpatialBMultiPoint { get; set; }
            public IGeometry SpatialBMultiPolygon { get; set; }
            public IGeometry SpatialBPoint { get; set; }
            public IGeometry SpatialBPolygon { get; set; }
            public GeometryCollection SpatialCGeometryCollection { get; set; }
            public LineString SpatialCLineString { get; set; }
            public MultiLineString SpatialCMultiLineString { get; set; }
            public MultiPoint SpatialCMultiPoint { get; set; }
            public MultiPolygon SpatialCMultiPolygon { get; set; }
            public Point SpatialCPoint { get; set; }
            public Polygon SpatialCPolygon { get; set; }
            public IGeometryCollection SpatialIGeometryCollection { get; set; }
            public ILineString SpatialILineString { get; set; }
            public IMultiLineString SpatialIMultiLineString { get; set; }
            public IMultiPoint SpatialIMultiPoint { get; set; }
            public IMultiPolygon SpatialIMultiPolygon { get; set; }
            public IPoint SpatialIPoint { get; set; }
            public IPolygon SpatialIPolygon { get; set; }
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

        private class EntityWithStringKey
        {
            public string Id { get; set; }
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

        private class BaseEntity
        {
            public int Id { get; set; }

            public string Discriminator { get; set; }
        }

        private class DerivedEntity : BaseEntity
        {
            public string Name { get; set; }
        }

        private class AnotherDerivedEntity : BaseEntity
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

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry) => throw new NotImplementedException();

            public override bool GeneratesTemporaryValues => false;
        }

        #region Model

        [Fact]
        public virtual void Model_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => builder.HasAnnotation("AnnotationName", "AnnotationValue"),
                AddBoilerPlate(
                    @"
            modelBuilder
                .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
                .HasAnnotation(""ChangeDetector.SkipDetectChanges"", ""true"")
                .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);"),
                o =>
                {
                    Assert.Equal(4, o.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", o["AnnotationName"]);
                });
        }

        [Fact]
        public virtual void Model_default_schema_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.HasDefaultSchema("DefaultSchema");
                    builder.HasAnnotation("AnnotationName", "AnnotationValue");
                },
                AddBoilerPlate(
                    @"
            modelBuilder
                .HasDefaultSchema(""DefaultSchema"")
                .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
                .HasAnnotation(""ChangeDetector.SkipDetectChanges"", ""true"")
                .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);"),
                o =>
                {
                    Assert.Equal(5, o.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", o["AnnotationName"]);
                    Assert.Equal("DefaultSchema", o[RelationalAnnotationNames.DefaultSchema]);
                });
        }

        [Fact]
        public virtual void Entities_are_stored_in_model_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties);
                    builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
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
        }

        #endregion

        #region EntityType

        [Fact]
        public virtual void EntityType_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>().HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithTwoProperties>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");

                    b.HasAnnotation(""AnnotationName"", ""AnnotationValue"");
                });"),
                o =>
                {
                    Assert.Equal(2, o.GetEntityTypes().First().GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", o.GetEntityTypes().First()["AnnotationName"]);
                });
        }

        [Fact]
        public virtual void BaseType_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                    builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Discriminator"")
                        .IsRequired();

                    b.HasKey(""Id"");

                    b.ToTable(""BaseEntity"");

                    b.HasDiscriminator<string>(""Discriminator"").HasValue(""BaseEntity"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity"", b =>
                {
                    b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

                    b.Property<string>(""Title"");

                    b.HasDiscriminator().HasValue(""AnotherDerivedEntity"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity"", b =>
                {
                    b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

                    b.Property<string>(""Name"");

                    b.HasDiscriminator().HasValue(""DerivedEntity"");
                });"),
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
        }

        [Fact]
        public virtual void Discriminator_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                    builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
                    builder.Entity<BaseEntity>()
                        .HasDiscriminator(e => e.Discriminator)
                        .HasValue(typeof(BaseEntity), typeof(BaseEntity).Name)
                        .HasValue(typeof(DerivedEntity), typeof(DerivedEntity).Name)
                        .HasValue(typeof(AnotherDerivedEntity), typeof(AnotherDerivedEntity).Name);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Discriminator"")
                        .IsRequired();

                    b.HasKey(""Id"");

                    b.ToTable(""BaseEntity"");

                    b.HasDiscriminator<string>(""Discriminator"").HasValue(""BaseEntity"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity"", b =>
                {
                    b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

                    b.Property<string>(""Title"");

                    b.HasDiscriminator().HasValue(""AnotherDerivedEntity"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity"", b =>
                {
                    b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

                    b.Property<string>(""Name"");

                    b.HasDiscriminator().HasValue(""DerivedEntity"");
                });"),
                o =>
                {
                    Assert.Equal("Discriminator", o.FindEntityType(typeof(BaseEntity))[RelationalAnnotationNames.DiscriminatorProperty]);
                    Assert.Equal("BaseEntity", o.FindEntityType(typeof(BaseEntity))[RelationalAnnotationNames.DiscriminatorValue]);
                    Assert.Equal(
                        "AnotherDerivedEntity",
                        o.FindEntityType(typeof(AnotherDerivedEntity))[RelationalAnnotationNames.DiscriminatorValue]);
                    Assert.Equal("DerivedEntity", o.FindEntityType(typeof(DerivedEntity))[RelationalAnnotationNames.DiscriminatorValue]);
                });
        }

        [Fact]
        public virtual void Properties_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>();
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    Assert.Equal(2, o.GetEntityTypes().First().GetProperties().Count());
                    Assert.Collection(
                        o.GetEntityTypes().First().GetProperties(),
                        t => Assert.Equal("Id", t.Name),
                        t => Assert.Equal("AlternateId", t.Name)
                    );
                });
        }

        [Fact]
        public virtual void Primary_key_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasKey(
                        t => new
                        {
                            t.Id,
                            t.AlternateId
                        });
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"");

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"", ""AlternateId"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    Assert.Equal(2, o.GetEntityTypes().First().FindPrimaryKey().Properties.Count);
                    Assert.Collection(
                        o.GetEntityTypes().First().FindPrimaryKey().Properties,
                        t => Assert.Equal("Id", t.Name),
                        t => Assert.Equal("AlternateId", t.Name)
                    );
                });
        }

        [Fact]
        public virtual void Alternate_keys_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasAlternateKey(
                        t => new
                        {
                            t.Id,
                            t.AlternateId
                        });
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""Id"", ""AlternateId"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    Assert.Collection(
                        o.GetEntityTypes().First().GetDeclaredKeys().First(k => k.Properties.Count == 2).Properties,
                        t => Assert.Equal("Id", t.Name),
                        t => Assert.Equal("AlternateId", t.Name)
                    );
                });
        }

        [Fact]
        public virtual void Indexes_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId);
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    Assert.Equal(1, o.GetEntityTypes().First().GetIndexes().Count());
                    Assert.Equal("AlternateId", o.GetEntityTypes().First().GetIndexes().First().Properties[0].Name);
                });
        }

        [Fact]
        public virtual void Indexes_are_stored_in_snapshot_including_composite_index()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(
                        t => new
                        {
                            t.Id,
                            t.AlternateId
                        });
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""Id"", ""AlternateId"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    Assert.Equal(1, o.GetEntityTypes().First().GetIndexes().Count());
                    Assert.Collection(
                        o.GetEntityTypes().First().GetIndexes().First().Properties,
                        t => Assert.Equal("Id", t.Name),
                        t => Assert.Equal("AlternateId", t.Name));
                });
        }

        [Fact]
        public virtual void Foreign_keys_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder
                        .Entity<EntityWithTwoProperties>()
                        .HasOne(e => e.EntityWithOneProperty)
                        .WithOne(e => e.EntityWithTwoProperties)
                        .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .IsUnique();

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                        .WithOne(""EntityWithTwoProperties"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o =>
                {
                    var foreignKey = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().Single();
                    Assert.Equal("AlternateId", foreignKey.Properties[0].Name);
                    Assert.Equal("EntityWithTwoProperties", foreignKey.PrincipalToDependent.Name);
                    Assert.Equal("EntityWithOneProperty", foreignKey.DependentToPrincipal.Name);
                });
        }

        [Fact]
        public virtual void TableName_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>();

                    originalModel = builder.Model;
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
                {
                    b.Property<Guid>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithGenericKey<Guid>"");
                });", usingSystem: true),
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);

                    Assert.NotNull(entity);
                    Assert.Equal(originalEntity.Relational().TableName, entity.Relational().TableName);
                });
        }

        [Fact]
        public virtual void PrimaryKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>();

                    originalModel = builder.Model;
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
                {
                    b.Property<Guid>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithGenericKey<Guid>"");
                });", usingSystem: true),
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);
                    Assert.NotNull(entity);

                    var originalPrimaryKey = originalEntity.FindPrimaryKey();
                    var primaryKey = entity.FindPrimaryKey();

                    Assert.Equal(originalPrimaryKey.Relational().Name, primaryKey.Relational().Name);
                });
        }

        [Fact]
        public virtual void AlternateKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericProperty<Guid>>().HasAlternateKey(e => e.Property);

                    originalModel = builder.Model;
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid>(""Property"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""Property"");

                    b.ToTable(""EntityWithGenericProperty<Guid>"");
                });", usingSystem: true),
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericProperty<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);
                    Assert.NotNull(entity);

                    var originalAlternateKey = originalEntity.FindKey(originalEntity.FindProperty("Property"));
                    var alternateKey = entity.FindKey(entity.FindProperty("Property"));

                    Assert.Equal(originalAlternateKey.Relational().Name, alternateKey.Relational().Name);
                });
        }

        [Fact]
        public virtual void Discriminator_of_enum()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>().HasDiscriminator(e => e.Day),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>(""Day"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithEnumType"");

                    b.HasDiscriminator<long>(""Day"");
                });"),
                model => Assert.Equal(typeof(long), model.GetEntityTypes().First().Relational().DiscriminatorProperty.ClrType));
        }

        [Fact]
        public virtual void Discriminator_of_enum_to_string()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>(
                    x =>
                    {
                        x.Property(e => e.Day).HasConversion<string>();
                        x.HasDiscriminator(e => e.Day);
                    }),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Day"")
                        .IsRequired();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithEnumType"");

                    b.HasDiscriminator<string>(""Day"");
                });"),
                model =>
                {
                    var discriminatorProperty = model.GetEntityTypes().First().Relational().DiscriminatorProperty;
                    Assert.Equal(typeof(string), discriminatorProperty.ClrType);
                    Assert.False(discriminatorProperty.IsNullable);
                });
        }

        #endregion

        #region Owned types

        [Fact]
        public virtual void Owned_types_are_stored_in_snapshot()
        {
            Test(
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
                                        new EntityWithTwoProperties
                                        {
                                            AlternateId = 1,
                                            Id = -1
                                        });
                                });

                            b.HasData(
                                new EntityWithOneProperty
                                {
                                    Id = 1
                                });
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
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"")
                        .HasName(""PK_Custom"");

                    b.ToTable(""EntityWithOneProperty"");

                    b.HasData(
                        new
                        {
                            Id = 1
                        });
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
                {
                    b.Property<string>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringKey"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"", b1 =>
                        {
                            b1.Property<int>(""AlternateId"")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<string>(""EntityWithStringKeyId"");

                            b1.Property<int>(""Id"");

                            b1.HasKey(""AlternateId"")
                                .HasName(""PK_Custom"");

                            b1.HasIndex(""EntityWithStringKeyId"")
                                .IsUnique()
                                .HasFilter(""[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL"");

                            b1.HasIndex(""Id"");

                            b1.ToTable(""EntityWithOneProperty"");

                            b1.WithOwner(""EntityWithOneProperty"")
                                .HasForeignKey(""AlternateId"")
                                .HasConstraintName(""FK_Custom"");

                            b1.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", ""EntityWithStringKey"")
                                .WithOne()
                                .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithStringKeyId"");

                            b1.HasData(
                                new
                                {
                                    AlternateId = 1,
                                    Id = -1
                                });
                        });
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
                {
                    b.OwnsMany(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""Properties"", b1 =>
                        {
                            b1.Property<int>(""Id"")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<int?>(""EntityWithOnePropertyId"");

                            b1.Property<string>(""EntityWithStringKeyId"")
                                .IsRequired();

                            b1.Property<string>(""Name"");

                            b1.HasKey(""Id"");

                            b1.HasIndex(""EntityWithOnePropertyId"")
                                .IsUnique()
                                .HasFilter(""[EntityWithOnePropertyId] IS NOT NULL"");

                            b1.HasIndex(""EntityWithStringKeyId"");

                            b1.ToTable(""EntityWithStringProperty"");

                            b1.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                                .WithOne()
                                .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""EntityWithOnePropertyId"");

                            b1.WithOwner()
                                .HasForeignKey(""EntityWithStringKeyId"");
                        });
                });", usingSystem: true),
                o =>
                {
                    var entityWithOneProperty = o.FindEntityType(typeof(EntityWithOneProperty));
                    Assert.Equal("PK_Custom", entityWithOneProperty.GetKeys().Single().Relational().Name);
                    Assert.Equal(new object[] { 1 }, entityWithOneProperty.GetData().Single().Values);

                    var ownership1 = entityWithOneProperty.FindNavigation(nameof(EntityWithOneProperty.EntityWithTwoProperties))
                        .ForeignKey;
                    Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownership1.Properties[0].Name);
                    Assert.Equal(nameof(EntityWithTwoProperties.EntityWithOneProperty), ownership1.DependentToPrincipal.Name);
                    Assert.True(ownership1.IsRequired);
                    Assert.Equal("FK_Custom", ownership1.Relational().Name);
                    var ownedType1 = ownership1.DeclaringEntityType;
                    Assert.Equal(nameof(EntityWithTwoProperties.AlternateId), ownedType1.FindPrimaryKey().Properties[0].Name);
                    Assert.Equal("PK_Custom", ownedType1.GetKeys().Single().Relational().Name);
                    Assert.Equal(2, ownedType1.GetIndexes().Count());
                    var owned1index1 = ownedType1.GetIndexes().First();
                    Assert.Equal("EntityWithStringKeyId", owned1index1.Properties[0].Name);
                    Assert.True(owned1index1.IsUnique);
                    Assert.Equal("[EntityWithTwoProperties_EntityWithStringKeyId] IS NOT NULL", owned1index1.Relational().Filter);
                    var owned1index2 = ownedType1.GetIndexes().Last();
                    Assert.Equal("Id", owned1index2.Properties[0].Name);
                    Assert.False(owned1index2.IsUnique);
                    Assert.Null(owned1index2.Relational().Filter);
                    Assert.Equal(new object[] { 1, -1 }, ownedType1.GetData().Single().Values);
                    Assert.Equal(nameof(EntityWithOneProperty), ownedType1.Relational().TableName);

                    var entityWithStringKey = o.FindEntityType(typeof(EntityWithStringKey));
                    Assert.Same(
                        entityWithStringKey,
                        ownedType1.FindNavigation(nameof(EntityWithTwoProperties.EntityWithStringKey)).GetTargetType());
                    Assert.Equal(nameof(EntityWithStringKey), entityWithStringKey.Relational().TableName);

                    var ownership2 = entityWithStringKey.FindNavigation(nameof(EntityWithStringKey.Properties)).ForeignKey;
                    Assert.Equal("EntityWithStringKeyId", ownership2.Properties[0].Name);
                    Assert.Null(ownership2.DependentToPrincipal);
                    Assert.True(ownership2.IsRequired);
                    var ownedType2 = ownership2.DeclaringEntityType;
                    Assert.Equal(nameof(EntityWithStringProperty.Id), ownedType2.FindPrimaryKey().Properties[0].Name);
                    Assert.Equal(1, ownedType2.GetKeys().Count());
                    Assert.Equal(2, ownedType2.GetIndexes().Count());
                    var owned2index1 = ownedType2.GetIndexes().First();
                    Assert.Equal("EntityWithOnePropertyId", owned2index1.Properties[0].Name);
                    Assert.True(owned2index1.IsUnique);
                    Assert.Equal("[EntityWithOnePropertyId] IS NOT NULL", owned2index1.Relational().Filter);
                    var owned2index2 = ownedType2.GetIndexes().Last();
                    Assert.Equal("EntityWithStringKeyId", owned2index2.Properties[0].Name);
                    Assert.False(owned2index2.IsUnique);
                    Assert.Null(owned2index2.Relational().Filter);
                    Assert.Equal(nameof(EntityWithStringProperty), ownedType2.Relational().TableName);

                    Assert.Same(entityWithOneProperty, ownedType2.GetNavigations().Single().GetTargetType());
                });
        }

        [Fact]
        public virtual void Weak_owned_types_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<Order>().OwnsOne(p => p.OrderBillingDetails, od => od.OwnsOne(c => c.StreetAddress));
                    builder.Entity<Order>().OwnsOne(p => p.OrderShippingDetails, od => od.OwnsOne(c => c.StreetAddress));
                    builder.Entity<Order>().OwnsOne(p => p.OrderInfo, od => od.OwnsOne(c => c.StreetAddress));
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""Order"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+Order"", b =>
                {
                    b.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderInfo"", ""OrderInfo"", b1 =>
                        {
                            b1.Property<int>(""OrderId"")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.HasKey(""OrderId"");

                            b1.ToTable(""Order"");

                            b1.WithOwner()
                                .HasForeignKey(""OrderId"");

                            b1.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress"", ""StreetAddress"", b2 =>
                                {
                                    b2.Property<int>(""OrderInfoOrderId"")
                                        .ValueGeneratedOnAdd()
                                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                                    b2.Property<string>(""City"");

                                    b2.HasKey(""OrderInfoOrderId"");

                                    b2.ToTable(""Order"");

                                    b2.WithOwner()
                                        .HasForeignKey(""OrderInfoOrderId"");
                                });
                        });

                    b.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails"", ""OrderBillingDetails"", b1 =>
                        {
                            b1.Property<int>(""OrderId"")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.HasKey(""OrderId"");

                            b1.ToTable(""Order"");

                            b1.WithOwner()
                                .HasForeignKey(""OrderId"");

                            b1.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress"", ""StreetAddress"", b2 =>
                                {
                                    b2.Property<int>(""OrderDetailsOrderId"")
                                        .ValueGeneratedOnAdd()
                                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                                    b2.Property<string>(""City"");

                                    b2.HasKey(""OrderDetailsOrderId"");

                                    b2.ToTable(""Order"");

                                    b2.WithOwner()
                                        .HasForeignKey(""OrderDetailsOrderId"");
                                });
                        });

                    b.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+OrderDetails"", ""OrderShippingDetails"", b1 =>
                        {
                            b1.Property<int>(""OrderId"")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.HasKey(""OrderId"");

                            b1.ToTable(""Order"");

                            b1.WithOwner()
                                .HasForeignKey(""OrderId"");

                            b1.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+StreetAddress"", ""StreetAddress"", b2 =>
                                {
                                    b2.Property<int>(""OrderDetailsOrderId"")
                                        .ValueGeneratedOnAdd()
                                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                                    b2.Property<string>(""City"");

                                    b2.HasKey(""OrderDetailsOrderId"");

                                    b2.ToTable(""Order"");

                                    b2.WithOwner()
                                        .HasForeignKey(""OrderDetailsOrderId"");
                                });
                        });
                });"),
                o =>
                {
                    Assert.Equal(7, o.GetEntityTypes().Count());

                    var order = o.FindEntityType(typeof(Order).FullName);
                    Assert.Equal(1, order.PropertyCount());

                    var orderInfo = order.FindNavigation(nameof(Order.OrderInfo)).GetTargetType();
                    Assert.Equal(1, orderInfo.PropertyCount());

                    var orderInfoAddress = orderInfo.FindNavigation(nameof(OrderInfo.StreetAddress)).GetTargetType();
                    Assert.Equal(2, orderInfoAddress.PropertyCount());

                    var orderBillingDetails = order.FindNavigation(nameof(Order.OrderBillingDetails)).GetTargetType();
                    Assert.Equal(1, orderBillingDetails.PropertyCount());

                    var orderBillingDetailsAddress = orderBillingDetails.FindNavigation(nameof(OrderDetails.StreetAddress)).GetTargetType();
                    Assert.Equal(2, orderBillingDetailsAddress.PropertyCount());

                    var orderShippingDetails = order.FindNavigation(nameof(Order.OrderShippingDetails)).GetTargetType();
                    Assert.Equal(1, orderShippingDetails.PropertyCount());

                    var orderShippingDetailsAddress =
                        orderShippingDetails.FindNavigation(nameof(OrderDetails.StreetAddress)).GetTargetType();
                    Assert.Equal(2, orderShippingDetailsAddress.PropertyCount());
                });
        }

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

        [Fact]
        public virtual void Property_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .Property<int>("Id")
                        .HasAnnotation("AnnotationName", "AnnotationValue")
                        .HasAnnotation(CoreAnnotationNames.TypeMapping, new IntTypeMapping("int"));

                    builder.Ignore<EntityWithTwoProperties>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });"),
                o => Assert.Equal("AnnotationValue", o.GetEntityTypes().First().FindProperty("Id")["AnnotationName"])
            );
        }

        [Fact]
        public virtual void Custom_value_generator_is_ignored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>().Property<int>("Id").HasValueGenerator<CustomValueGenerator>();
                    builder.Ignore<EntityWithTwoProperties>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });"),
                o => Assert.Null(o.GetEntityTypes().First().FindProperty("Id")[CoreAnnotationNames.ValueGeneratorFactoryAnnotation])
            );
        }

        [Fact]
        public virtual void Property_isNullable_is_stored_in_snapshot()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsRequired(),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .IsRequired();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                o => Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsNullable));
        }

        [Fact]
        public virtual void Property_ValueGenerated_value_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue();
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(null);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });", usingSystem: true),
                o => Assert.Equal(ValueGenerated.OnAdd, o.GetEntityTypes().First().FindProperty("AlternateId").ValueGenerated));
        }

        [Fact]
        public virtual void Property_maxLength_is_stored_in_snapshot()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").HasMaxLength(100),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .HasMaxLength(100);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                o => Assert.Equal(100, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()));
        }

        [Fact]
        public virtual void Property_unicodeness_is_stored_in_snapshot()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsUnicode(false),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .IsUnicode(false);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                o => Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsUnicode()));
        }

        [Fact]
        public virtual void Property_fixedlengthness_is_stored_in_snapshot()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsFixedLength(true),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .IsFixedLength(true);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                o => Assert.True(o.GetEntityTypes().First().FindProperty("Name").Relational().IsFixedLength));
        }

        [Fact]
        public virtual void Many_facets_chained_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithStringProperty>()
                        .Property<string>("Name")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                o =>
                {
                    var property = o.GetEntityTypes().First().FindProperty("Name");
                    Assert.Equal(100, property.GetMaxLength());
                    Assert.False(property.IsUnicode());
                    Assert.Equal("AnnotationValue", property["AnnotationName"]);
                });
        }

        [Fact]
        public virtual void Property_concurrencyToken_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").IsConcurrencyToken();
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .IsConcurrencyToken();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.True(o.GetEntityTypes().First().FindProperty("AlternateId").IsConcurrencyToken));
        }

        [Fact]
        public virtual void Property_column_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .HasColumnName(""CName"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("CName", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnName"]));
        }

        [Fact]
        public virtual void Property_column_type_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnType("CType");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .HasColumnType(""CType"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("CType", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnType"]));
        }

        [Fact]
        public virtual void Property_default_value_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue(1);
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(1);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal(1, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValue"]));
        }

        [Fact]
        public virtual void Property_default_value_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValueSql("SQL");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql(""SQL"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValueSql"]));
        }

        [Fact]
        public virtual void Property_computed_column_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql("SQL");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql(""SQL"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ComputedColumnSql"]));
        }

        [Fact]
        public virtual void Property_default_value_of_enum_type_is_stored_in_snapshot_without_actual_enum()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>().Property(e => e.Day).HasDefaultValue(Days.Wed),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>(""Day"")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(3L);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithEnumType"");
                });"),
                o => Assert.Equal(3L, o.GetEntityTypes().First().FindProperty("Day")["Relational:DefaultValue"]));
        }

        [Fact]
        public virtual void Property_enum_type_is_stored_in_snapshot_with_custom_conversion_and_seed_data()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>(
                    eb =>
                    {
                        eb.Property(e => e.Day).HasDefaultValue(Days.Wed)
                            .HasConversion(v => v.ToString(), v => (Days)Enum.Parse(typeof(Days), v));
                        eb.HasData(
                            new
                            {
                                Id = 1,
                                Day = Days.Fri
                            });
                    }),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Day"")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(""Wed"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithEnumType"");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Day = ""Fri""
                        });
                });"),
                o =>
                {
                    var property = o.GetEntityTypes().First().FindProperty("Day");
                    Assert.Equal(typeof(string), property.ClrType);
                    Assert.Equal(nameof(Days.Wed), property["Relational:DefaultValue"]);
                    Assert.False(property.IsNullable);
                });
        }

        [Fact]
        public virtual void Property_of_nullable_enum()
        {
            Test(
                builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>(""Day"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithNullableEnumType"");
                });"),
                o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_of_enum_to_nullable()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>().Property(e => e.Day)
                    .HasConversion(m => (long?)m, p => p.HasValue ? (Days)p.Value : default),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>(""Day"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithEnumType"");
                });", usingSystem: true),
                o => Assert.False(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_of_nullable_enum_to_string()
        {
            Test(
                builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day).HasConversion<string>(),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Day"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithNullableEnumType"");
                });"),
                o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"")
                        .HasColumnName(""CName"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    var property = o.GetEntityTypes().First().FindProperty("AlternateId");
                    Assert.Equal(2, property.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", property["AnnotationName"]);
                    Assert.Equal("CName", property["Relational:ColumnName"]);
                });
        }

        #endregion

        #region HasKey

        [Fact]
        public virtual void Key_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId)
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""AlternateId"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal(
                    "AnnotationValue", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["AnnotationName"]));
        }

        [Fact]
        public virtual void Key_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("KeyName");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""AlternateId"")
                        .HasName(""KeyName"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal(
                    "KeyName", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["Relational:Name"]));
        }

        [Fact]
        public virtual void Key_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("IndexName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""AlternateId"")
                        .HasName(""IndexName"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    var key = o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First();
                    Assert.Equal(2, key.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", key["AnnotationName"]);
                    Assert.Equal("IndexName", key["Relational:Name"]);
                });
        }

        #endregion

        #region HasIndex

        [Fact]
        public virtual void Index_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("AnnotationValue", o.GetEntityTypes().First().GetIndexes().First()["AnnotationName"]));
        }

        [Fact]
        public virtual void Index_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsUnique();
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .IsUnique();

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.True(o.GetEntityTypes().First().GetIndexes().First().IsUnique));
        }

        [Fact]
        public virtual void Index_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasName("IndexName");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .HasName(""IndexName"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal("IndexName", o.GetEntityTypes().First().GetIndexes().First()["Relational:Name"]));
        }

        [Fact]
        public virtual void Index_filter_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                        .HasFilter("AlternateId <> 0");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .HasFilter(""AlternateId <> 0"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o => Assert.Equal(
                    "AlternateId <> 0",
                    o.GetEntityTypes().First().GetIndexes().First().Relational().Filter));
        }

        [Fact]
        public virtual void Index_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasName("IndexName")
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                    builder.Ignore<EntityWithOneProperty>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .HasName(""IndexName"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                    b.ToTable(""EntityWithTwoProperties"");
                });"),
                o =>
                {
                    var index = o.GetEntityTypes().First().GetIndexes().First();
                    Assert.Equal(2, index.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", index["AnnotationName"]);
                    Assert.Equal("IndexName", index["Relational:Name"]);
                });
        }

        [Fact]
        public virtual void Index_with_default_constraint_name_exceeding_max()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>(
                    x =>
                    {
                        const string propertyName =
                            "SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit";
                        x.Property<string>(propertyName);
                        x.HasIndex(propertyName);
                    }),
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"");

                    b.Property<string>(""SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit"");

                    b.HasKey(""Id"");

                    b.HasIndex(""SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit"");

                    b.ToTable(""EntityWithStringProperty"");
                });"),
                model => Assert.Equal(128, model.GetEntityTypes().First().GetIndexes().First().Relational().Name.Length));
        }

        #endregion

        #region ForeignKey

        [Fact]
        public virtual void ForeignKey_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>()
                        .HasOne(e => e.EntityWithOneProperty)
                        .WithOne(e => e.EntityWithTwoProperties)
                        .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                        .HasAnnotation("AnnotationName", "AnnotationValue");
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .IsUnique();

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                        .WithOne(""EntityWithTwoProperties"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o => Assert.Equal(
                    "AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["AnnotationName"]));
        }

        [Fact]
        public virtual void ForeignKey_isRequired_is_stored_in_snapshot()
        {
            Test(
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
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
                {
                    b.Property<string>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringKey"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"")
                        .IsRequired();

                    b.HasKey(""Id"");

                    b.HasIndex(""Name"")
                        .IsUnique();

                    b.ToTable(""EntityWithStringProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"")
                        .WithOne()
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""Name"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o => Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).FindProperty("Name").IsNullable));
        }

        [Fact]
        public virtual void ForeignKey_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithStringProperty>()
                        .HasOne<EntityWithStringKey>()
                        .WithMany(e => e.Properties)
                        .HasForeignKey(e => e.Name);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
                {
                    b.Property<string>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithStringKey"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Name"");

                    b.HasKey(""Id"");

                    b.HasIndex(""Name"");

                    b.ToTable(""EntityWithStringProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"")
                        .WithMany(""Properties"")
                        .HasForeignKey(""Name"");
                });"),
                o => Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique));
        }

        [Fact]
        public virtual void ForeignKey_deleteBehavior_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .HasOne(e => e.EntityWithTwoProperties)
                        .WithMany()
                        .HasForeignKey(e => e.Id);
                    builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
                        .WithMany()
                        .HasForeignKey(""Id"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o => Assert.Equal(
                    DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior));
        }

        [Fact]
        public virtual void ForeignKey_deleteBehavior_is_stored_in_snapshot_for_one_to_one()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .HasOne(e => e.EntityWithTwoProperties)
                        .WithOne(e => e.EntityWithOneProperty)
                        .HasForeignKey<EntityWithOneProperty>(e => e.Id);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
                        .WithOne(""EntityWithOneProperty"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o => Assert.Equal(
                    DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior));
        }

        [Fact]
        public virtual void ForeignKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>().HasMany<EntityWithGenericProperty<Guid>>().WithOne()
                        .HasForeignKey(e => e.Property);

                    originalModel = builder.Model;
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
                {
                    b.Property<Guid>(""Id"")
                        .ValueGeneratedOnAdd();

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithGenericKey<Guid>"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid>(""Property"");

                    b.HasKey(""Id"");

                    b.HasIndex(""Property"");

                    b.ToTable(""EntityWithGenericProperty<Guid>"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"")
                        .WithMany()
                        .HasForeignKey(""Property"")
                        .OnDelete(DeleteBehavior.Cascade);
                });", usingSystem: true),
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

                    Assert.Equal(originalForeignKey.Relational().Name, foreignKey.Relational().Name);

                    var originalIndex = originalChild.FindIndex(originalChild.FindProperty("Property"));
                    var index = child.FindIndex(child.FindProperty("Property"));

                    Assert.Equal(originalIndex.Relational().Name, index.Relational().Name);
                });
        }

        [Fact]
        public virtual void ForeignKey_constraint_name_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithTwoProperties>()
                        .HasOne(e => e.EntityWithOneProperty)
                        .WithOne(e => e.EntityWithTwoProperties)
                        .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                        .HasConstraintName("Constraint");
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .IsUnique();

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                        .WithOne(""EntityWithTwoProperties"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .HasConstraintName(""Constraint"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o => Assert.Equal(
                    "Constraint", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["Relational:Name"]));
        }

        [Fact]
        public virtual void ForeignKey_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
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
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""AlternateId"")
                        .IsUnique();

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                        .WithOne(""EntityWithTwoProperties"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .HasConstraintName(""Constraint"")
                        .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o =>
                {
                    var fk = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First();
                    Assert.Equal(2, fk.GetAnnotations().Count());
                    Assert.Equal("AnnotationValue", fk["AnnotationName"]);
                    Assert.Equal("Constraint", fk["Relational:Name"]);
                });
        }

        [Fact]
        public virtual void Do_not_generate_entity_type_builder_again_if_no_foreign_key_is_defined_on_it()
        {
            Test(
                builder =>
                {
                    builder.Entity<BaseType>();
                    builder.Ignore<EntityWithTwoProperties>();
                    builder.Entity<DerivedType>();
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>(""Discriminator"")
                        .IsRequired();

                    b.Property<int?>(""NavigationId"");

                    b.HasKey(""Id"");

                    b.HasIndex(""NavigationId"");

                    b.ToTable(""BaseType"");

                    b.HasDiscriminator<string>(""Discriminator"").HasValue(""BaseType"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedType"", b =>
                {
                    b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"");

                    b.HasDiscriminator().HasValue(""DerivedType"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Navigation"")
                        .WithMany()
                        .HasForeignKey(""NavigationId"");
                });", usingSystem: true),
                o => { });
        }

        [Fact]
        public virtual void Relationship_principal_key_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .HasOne(e => e.EntityWithTwoProperties)
                        .WithOne(e => e.EntityWithOneProperty)
                        .HasForeignKey<EntityWithOneProperty>(e => e.Id)
                        .HasPrincipalKey<EntityWithTwoProperties>(e => e.AlternateId);
                },
                AddBoilerPlate(
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
                        .WithOne(""EntityWithOneProperty"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
                        .HasPrincipalKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o =>
                {
                    Assert.Equal(2, o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Count());
                    Assert.True(o.FindEntityType(typeof(EntityWithTwoProperties)).FindProperty("AlternateId").IsKey());
                });
        }

        [Fact]
        public virtual void Relationship_principal_key_with_non_default_name_is_stored_in_snapshot()
        {
            Test(
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
                    GetHeading() + @"
            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.Property<int>(""Id"");

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithOneProperty"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>(""AlternateId"");

                    b.HasKey(""Id"");

                    b.HasAlternateKey(""AlternateId"")
                        .HasAnnotation(""Name"", ""Value"");

                    b.ToTable(""EntityWithTwoProperties"");
                });

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
                {
                    b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
                        .WithOne(""EntityWithOneProperty"")
                        .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
                        .HasPrincipalKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                        .OnDelete(DeleteBehavior.Cascade);
                });"),
                o =>
                {
                    var entityType = o.FindEntityType(typeof(EntityWithTwoProperties));

                    Assert.Equal(2, entityType.GetKeys().Count());
                    Assert.Equal("Value", entityType.FindKey(entityType.FindProperty("AlternateId"))["Name"]);
                });
        }

        #endregion

        #region SeedData

        [Fact]
        public virtual void SeedData_annotations_are_stored_in_snapshot()
        {
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
                new IPoint[] { new Point(1.1, 2.2), new Point(2.2, 2.2), new Point(2.2, 1.1) })
            {
                SRID = 4326
            };

            var polygon1 = new Polygon(
                new LinearRing(
                    new[] { new Coordinate(1.1, 2.2), new Coordinate(2.2, 2.2), new Coordinate(2.2, 1.1), new Coordinate(1.1, 2.2) }))
            {
                SRID = 4326
            };

            var polygon2 = new Polygon(
                new LinearRing(
                    new[]
                    {
                        new Coordinate(10.1, 20.2), new Coordinate(20.2, 20.2), new Coordinate(20.2, 10.1), new Coordinate(10.1, 20.2)
                    }))
            {
                SRID = 4326
            };

            var point1 = new Point(1.1, 2.2, 3.3)
            {
                SRID = 4326
            };

            var multiLineString = new MultiLineString(
                new ILineString[] { lineString1, lineString2 })
            {
                SRID = 4326
            };

            var multiPolygon = new MultiPolygon(
                new IPolygon[] { polygon2, polygon1 })
            {
                SRID = 4326
            };

            var geometryCollection = new GeometryCollection(
                new IGeometry[] { lineString1, lineString2, multiPoint, polygon1, polygon2, point1, multiLineString, multiPolygon })
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
                                    SpatialCPolygon = polygon1,
                                    SpatialIGeometryCollection = geometryCollection,
                                    SpatialILineString = lineString1,
                                    SpatialIMultiLineString = multiLineString,
                                    SpatialIMultiPoint = multiPoint,
                                    SpatialIMultiPolygon = multiPolygon,
                                    SpatialIPoint = point1,
                                    SpatialIPolygon = polygon1
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
                @"// <auto-generated />
using System;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace RootNamespace
{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithManyProperties"", b =>
                {
                    b.Property<int>(""Id"")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>(""Boolean"");

                    b.Property<byte>(""Byte"");

                    b.Property<byte[]>(""Bytes"");

                    b.Property<string>(""Character"")
                        .IsRequired()
                        .HasConversion(new ValueConverter<string, string>(v => default(string), v => default(string), new ConverterMappingHints(size: 1)));

                    b.Property<DateTime>(""DateTime"");

                    b.Property<DateTimeOffset>(""DateTimeOffset"");

                    b.Property<decimal>(""Decimal"");

                    b.Property<double>(""Double"");

                    b.Property<short>(""Enum16"");

                    b.Property<int>(""Enum32"");

                    b.Property<long>(""Enum64"");

                    b.Property<byte>(""Enum8"");

                    b.Property<short>(""EnumS8"");

                    b.Property<int>(""EnumU16"");

                    b.Property<long>(""EnumU32"");

                    b.Property<decimal>(""EnumU64"")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<short>(""Int16"");

                    b.Property<int>(""Int32"");

                    b.Property<long>(""Int64"");

                    b.Property<decimal?>(""OptionalProperty"");

                    b.Property<short>(""SignedByte"");

                    b.Property<float>(""Single"");

                    b.Property<IGeometry>(""SpatialBGeometryCollection"");

                    b.Property<IGeometry>(""SpatialBLineString"");

                    b.Property<IGeometry>(""SpatialBMultiLineString"");

                    b.Property<IGeometry>(""SpatialBMultiPoint"");

                    b.Property<IGeometry>(""SpatialBMultiPolygon"");

                    b.Property<IGeometry>(""SpatialBPoint"");

                    b.Property<IGeometry>(""SpatialBPolygon"");

                    b.Property<GeometryCollection>(""SpatialCGeometryCollection"");

                    b.Property<LineString>(""SpatialCLineString"");

                    b.Property<MultiLineString>(""SpatialCMultiLineString"");

                    b.Property<MultiPoint>(""SpatialCMultiPoint"");

                    b.Property<MultiPolygon>(""SpatialCMultiPolygon"");

                    b.Property<Point>(""SpatialCPoint"");

                    b.Property<Polygon>(""SpatialCPolygon"");

                    b.Property<IGeometryCollection>(""SpatialIGeometryCollection"");

                    b.Property<ILineString>(""SpatialILineString"");

                    b.Property<IMultiLineString>(""SpatialIMultiLineString"");

                    b.Property<IMultiPoint>(""SpatialIMultiPoint"");

                    b.Property<IMultiPolygon>(""SpatialIMultiPolygon"");

                    b.Property<IPoint>(""SpatialIPoint"");

                    b.Property<IPolygon>(""SpatialIPolygon"");

                    b.Property<string>(""String"");

                    b.Property<TimeSpan>(""TimeSpan"");

                    b.Property<int>(""UnsignedInt16"");

                    b.Property<long>(""UnsignedInt32"");

                    b.Property<decimal>(""UnsignedInt64"")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey(""Id"");

                    b.ToTable(""EntityWithManyProperties"");

                    b.HasData(
                        new
                        {
                            Id = 42,
                            Boolean = true,
                            Byte = (byte)55,
                            Bytes = new byte[] { 44, 45 },
                            Character = ""9"",
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
                            SpatialBGeometryCollection = (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;GEOMETRYCOLLECTION (LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), LINESTRING (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2), MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1)), POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)), POLYGON ((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), POINT (1.1 2.2 3.3), MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2)), MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))))""),
                            SpatialBLineString = (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)""),
                            SpatialBMultiLineString = (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))""),
                            SpatialBMultiPoint = (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))""),
                            SpatialBMultiPolygon = (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))""),
                            SpatialBPoint = (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POINT (1.1 2.2 3.3)""),
                            SpatialBPolygon = (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))""),
                            SpatialCGeometryCollection = (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;GEOMETRYCOLLECTION (LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), LINESTRING (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2), MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1)), POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)), POLYGON ((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), POINT (1.1 2.2 3.3), MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2)), MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))))""),
                            SpatialCLineString = (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)""),
                            SpatialCMultiLineString = (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))""),
                            SpatialCMultiPoint = (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))""),
                            SpatialCMultiPolygon = (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))""),
                            SpatialCPoint = (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POINT (1.1 2.2 3.3)""),
                            SpatialCPolygon = (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))""),
                            SpatialIGeometryCollection = (NetTopologySuite.Geometries.GeometryCollection)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;GEOMETRYCOLLECTION (LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), LINESTRING (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2), MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1)), POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)), POLYGON ((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), POINT (1.1 2.2 3.3), MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2)), MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))))""),
                            SpatialILineString = (NetTopologySuite.Geometries.LineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;LINESTRING (1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2)""),
                            SpatialIMultiLineString = (NetTopologySuite.Geometries.MultiLineString)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTILINESTRING ((1.1 2.2, 2.2 2.2, 2.2 1.1, 7.1 7.2), (7.1 7.2, 20.2 20.2, 20.2 1.1, 70.1 70.2))""),
                            SpatialIMultiPoint = (NetTopologySuite.Geometries.MultiPoint)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOINT ((1.1 2.2), (2.2 2.2), (2.2 1.1))""),
                            SpatialIMultiPolygon = (NetTopologySuite.Geometries.MultiPolygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;MULTIPOLYGON (((10.1 20.2, 20.2 20.2, 20.2 10.1, 10.1 20.2)), ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2)))""),
                            SpatialIPoint = (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POINT (1.1 2.2 3.3)""),
                            SpatialIPolygon = (NetTopologySuite.Geometries.Polygon)new NetTopologySuite.IO.WKTReader().Read(""SRID=4326;POLYGON ((1.1 2.2, 2.2 2.2, 2.2 1.1, 1.1 2.2))""),
                            String = ""FortyThree"",
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
                            Character = ""9"",
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
                            String = ""FortyThree"",
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
",
                o => Assert.Collection(
                    o.GetEntityTypes().SelectMany(e => e.GetData()),
                    seed =>
                    {
                        Assert.Equal(42, seed["Id"]);
                        Assert.Equal("FortyThree", seed["String"]);
                        Assert.Equal(new byte[] { 44, 45 }, seed["Bytes"]);
                        Assert.Equal((short)46, seed["Int16"]);
                        Assert.Equal(47, seed["Int32"]);
                        Assert.Equal((long)48, seed["Int64"]);
                        Assert.Equal(49.0, seed["Double"]);
                        Assert.Equal(50.0m, seed["Decimal"]);
                        Assert.Equal(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc), seed["DateTime"]);
                        Assert.Equal(
                            new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(1, 0, 0)), seed["DateTimeOffset"]);
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
                        Assert.Equal(geometryCollection, seed["SpatialIGeometryCollection"]);
                        Assert.Equal(lineString1, seed["SpatialILineString"]);
                        Assert.Equal(multiLineString, seed["SpatialIMultiLineString"]);
                        Assert.Equal(multiPoint, seed["SpatialIMultiPoint"]);
                        Assert.Equal(multiPolygon, seed["SpatialIMultiPolygon"]);
                        Assert.Equal(point1, seed["SpatialIPoint"]);
                        Assert.Equal(polygon1, seed["SpatialIPolygon"]);

                        Assert.Equal(4326, ((IGeometry)seed["SpatialBGeometryCollection"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBLineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBMultiLineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBMultiPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBMultiPolygon"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialBPolygon"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCGeometryCollection"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCLineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCMultiLineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCMultiPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCMultiPolygon"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialCPolygon"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIGeometryCollection"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialILineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIMultiLineString"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIMultiPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIMultiPolygon"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIPoint"]).SRID);
                        Assert.Equal(4326, ((IGeometry)seed["SpatialIPolygon"]).SRID);
                    },
                    seed =>
                    {
                        Assert.Equal(43, seed["Id"]);
                        Assert.Equal("FortyThree", seed["String"]);
                        Assert.Equal(new byte[] { 44, 45 }, seed["Bytes"]);
                        Assert.Equal((short)-46, seed["Int16"]);
                        Assert.Equal(-47, seed["Int32"]);
                        Assert.Equal((long)-48, seed["Int64"]);
                        Assert.Equal(-49.0, seed["Double"]);
                        Assert.Equal(-50.0m, seed["Decimal"]);
                        Assert.Equal(new DateTime(1973, 9, 3, 12, 10, 42, 344, DateTimeKind.Utc), seed["DateTime"]);
                        Assert.Equal(
                            new DateTimeOffset(new DateTime(1973, 9, 3, 12, 10, 42, 344), new TimeSpan(-1, 0, 0)), seed["DateTimeOffset"]);
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
                    }));
        }

        #endregion

        protected virtual string GetHeading() => @"
            modelBuilder
                .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
                .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
";

        protected virtual ICollection<BuildReference> GetReferences() => new List<BuildReference>
        {
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"),
            BuildReference.ByName("GeoAPI"),
            BuildReference.ByName("NetTopologySuite")
        };

        protected virtual string AddBoilerPlate(string code, bool usingSystem = false)
            => $@"// <auto-generated />
{(usingSystem
                ? @"using System;
"
                : "")}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RootNamespace
{{
    [DbContext(typeof(DbContext))]
    partial class Snapshot : ModelSnapshot
    {{
        protected override void BuildModel(ModelBuilder modelBuilder)
        {{
#pragma warning disable 612, 618{code}
#pragma warning restore 612, 618
        }}
    }}
}}
";

        protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersionAnnotation);
            buildModel(modelBuilder);

            var model = modelBuilder.FinalizeModel();

            var codeHelper = new CSharpHelper(
                new SqlServerTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    new RelationalTypeMappingSourceDependencies(
                        new IRelationalTypeMappingSourcePlugin[]
                            { new SqlServerNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices.Instance) })));

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

            var code = generator.GenerateSnapshot("RootNamespace", typeof(DbContext), "Snapshot", model);

            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);

            var build = new BuildSource
            {
                Sources =
                {
                    code
                }
            };

            foreach (var buildReference in GetReferences())
            {
                build.References.Add(buildReference);
            }

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("RootNamespace.Snapshot");

            var buildModelMethod = factoryType.GetTypeInfo().GetMethod(
                "BuildModel",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(ModelBuilder) },
                null);

            var builder = new ModelBuilder(new ConventionSet());
            builder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersionAnnotation);

            buildModelMethod.Invoke(
                Activator.CreateInstance(factoryType),
                new object[] { builder });

            var value = builder.Model;
            Assert.NotNull(value);

            var reporter = new TestOperationReporter();
            assert(new SnapshotModelProcessor(reporter).Process(value));
        }

        protected ModelBuilder CreateConventionalModelBuilder()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddEntityFrameworkSqlServerNetTopologySuite()
                .AddDbContext<DbContext>((p, o) =>
                    o.UseSqlServer("Server=.", b => b.UseNetTopologySuite())
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DbContext>())
                {
                    return new ModelBuilder(ConventionSet.CreateConventionSet(context));
                }
            }
        }
    }
}
