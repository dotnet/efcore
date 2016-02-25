// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Commands.TestUtilities;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Commands.Migrations
{
    public class ModelSnapshotTest
    {
        private class EntityWithOneProperty
        {
            public int Id { get; set; }
        }

        private class EntityWithTwoProperties
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
        }

        private class EntityWithStringProperty
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class EntityWithStringKey
        {
            public string Id { get; set; }
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

        #region Model

        [Fact]
        public void Model_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"builder
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"");
",
                o =>
                    {
                        Assert.Equal(1, o.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", o["AnnotationName"]);
                    });
        }

        [Fact]
        public void Entities_are_stored_in_model_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>();
                        builder.Entity<EntityWithTwoProperties>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.GetEntityTypes().Count());
                        Assert.Collection(
                            o.GetEntityTypes(),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties", t.Name));
                    });
        }

        #endregion

        #region EntityType

        [Fact]
        public void EntityType_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithOneProperty>().HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.HasAnnotation(""AnnotationName"", ""AnnotationValue"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.GetEntityTypes().First().GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", o.GetEntityTypes().First()["AnnotationName"]);
                    });
        }

        [Fact]
        public void BaseType_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                        builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseEntity"", b =>
    {
        b.ToTable(""BaseEntity"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+AnotherDerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseEntity"");

        b.ToTable(""AnotherDerivedEntity"");

        b.Property<string>(""Title"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+DerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseEntity"");

        b.ToTable(""DerivedEntity"");

        b.Property<string>(""Name"");
    });
",
                o =>
                    {
                        Assert.Equal(3, o.GetEntityTypes().Count());
                        Assert.Collection(
                            o.GetEntityTypes(),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+AnotherDerivedEntity", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseEntity", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+DerivedEntity", t.Name)
                            );
                    });
        }

        [Fact]
        public void Properties_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });
",
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
        public void Primary_key_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasKey(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"");

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"", ""AlternateId"");
    });
",
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
        public void Alternate_keys_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""Id"", ""AlternateId"");
    });
",
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
        public void Indexes_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(1, o.GetEntityTypes().First().GetIndexes().Count());
                        Assert.Equal("AlternateId", o.GetEntityTypes().First().GetIndexes().First().Properties[0].Name);
                    });
        }

        [Fact]
        public void Indexes_are_stored_in_snapshot_including_composite_index()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasIndex(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""Id"", ""AlternateId"");
    });
",
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
        public void Foreign_keys_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasOne<EntityWithOneProperty>().WithOne().HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o =>
                    {
                        Assert.Equal(1, o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().Count());
                        Assert.Equal("AlternateId", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First().Properties[0].Name);
                    });
        }

        [Fact]
        public void Relationship_principal_key_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().HasOne<EntityWithTwoProperties>().WithOne()
                            .HasForeignKey<EntityWithOneProperty>(e => e.Id).
                            HasPrincipalKey<EntityWithTwoProperties>(e => e.AlternateId);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.HasIndex(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Id"")
            .HasPrincipalKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o =>
                    {
                        Assert.Equal(2, o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Count());
                        Assert.True(o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Any(k => k.Properties.Any(p => p.Name == "AlternateId")));
                    });
        }

        [Fact]
        public void TableName_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>();

                    originalModel = builder.Model;
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.ToTable(""EntityWithGenericKey<Guid>"");

        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });
",
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);

                    Assert.NotNull(entity);
                    Assert.Equal(originalEntity.SqlServer().TableName, entity.SqlServer().TableName);
                });
        }

        [Fact]
        public void PrimaryKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>();

                    originalModel = builder.Model;
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.ToTable(""EntityWithGenericKey<Guid>"");

        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });
",
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericKey<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);
                    Assert.NotNull(entity);

                    var originalPrimaryKey = originalEntity.FindPrimaryKey();
                    var primaryKey = entity.FindPrimaryKey();

                    Assert.Equal(originalPrimaryKey.SqlServer().Name, primaryKey.SqlServer().Name);
                });
        }

        [Fact]
        public void AlternateKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericProperty<Guid>>().HasAlternateKey(e => e.Property);

                    originalModel = builder.Model;
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.ToTable(""EntityWithGenericProperty<Guid>"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<Guid>(""Property"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""Property"");
    });
",
                model =>
                {
                    var originalEntity = originalModel.FindEntityType(typeof(EntityWithGenericProperty<Guid>));
                    var entity = model.FindEntityType(originalEntity.Name);
                    Assert.NotNull(entity);

                    var originalAlternateKey = originalEntity.FindKey(originalEntity.FindProperty("Property"));
                    var alternateKey = entity.FindKey(entity.FindProperty("Property"));

                    Assert.Equal(originalAlternateKey.SqlServer().Name, alternateKey.SqlServer().Name);
                });
        }

        #endregion

        #region Property

        [Fact]
        public void Property_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithOneProperty>().Property<int>("Id").HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().FindProperty("Id")["AnnotationName"]); }
                );
        }

        [Fact]
        public void Property_isNullable_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsRequired(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.ToTable(""EntityWithStringProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .IsRequired();

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(false, o.GetEntityTypes().First().FindProperty("Name").IsNullable); });
        }

        [Fact]
        public void Property_ValueGenerated_value_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").ValueGeneratedOnAdd(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(ValueGenerated.OnAdd, o.GetEntityTypes().First().FindProperty("AlternateId").ValueGenerated); });
        }

        [Fact]
        public void Property_maxLength_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").HasMaxLength(100); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.ToTable(""EntityWithStringProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .HasAnnotation(""MaxLength"", 100);

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(100, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()); });
        }

        [Fact]
        public void Property_RequiresValueGenerator_is_not_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").Metadata.RequiresValueGenerator = true; },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(false, o.GetEntityTypes().First().FindProperty("AlternateId").RequiresValueGenerator); });
        }

        [Fact]
        public void Property_concurrencyToken_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").IsConcurrencyToken(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .IsConcurrencyToken();

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(true, o.GetEntityTypes().First().FindProperty("AlternateId").IsConcurrencyToken); });
        }

        [Fact]
        public void Property_default_value_of_enum_type_is_stored_in_snapshot_without_actual_enum()
        {
            Test(
                builder => { builder.Entity<EntityWithEnumType>().Property(e => e.Day).HasDefaultValue(Days.Wed); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithEnumType"", b =>
    {
        b.ToTable(""EntityWithEnumType"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<long>(""Day"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""Relational:DefaultValue"", 3L);

        b.HasKey(""Id"");
    });
",
                o => { Assert.Equal(3L, o.GetEntityTypes().First().FindProperty("Day")["Relational:DefaultValue"]); });
        }

        #endregion

        #region HasIndex

        [Fact]
        public void Index_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().GetIndexes().First()["AnnotationName"]); });
        }

        [Fact]
        public void Index_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsUnique(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();
    });
",
                o => { Assert.Equal(true, o.GetEntityTypes().First().GetIndexes().First().IsUnique); });
        }

        #endregion

        #region ForeignKey

        [Fact]
        public void ForeignKey_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>()
                            .HasOne<EntityWithOneProperty>()
                            .WithOne()
                            .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                            .HasAnnotation("AnnotationName", "AnnotationValue");
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal("AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["AnnotationName"]); });
        }

        [Fact]
        public void ForeignKey_isRequired_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithStringProperty>()
                            .HasOne<EntityWithStringKey>()
                            .WithOne()
                            .HasForeignKey<EntityWithStringProperty>(e => e.Name)
                            .IsRequired();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.ToTable(""EntityWithStringKey"");

        b.Property<string>(""Id"");

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.ToTable(""EntityWithStringProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .IsRequired();

        b.HasKey(""Id"");

        b.HasIndex(""Name"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", ""Name"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).FindProperty("Name").IsNullable); });
        }

        [Fact]
        public void ForeignKey_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithStringProperty>()
                            .HasOne<EntityWithStringKey>()
                            .WithMany()
                            .HasForeignKey(e => e.Name);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.ToTable(""EntityWithStringKey"");

        b.Property<string>(""Id"");

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.ToTable(""EntityWithStringProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"");

        b.HasKey(""Id"");

        b.HasIndex(""Name"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .WithMany()
            .HasForeignKey(""Name"");
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique); });
        }

        [Fact]
        public void ForeignKey_deleteBehavior_is_stored_in_snapshot()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .HasOne<EntityWithTwoProperties>()
                        .WithMany()
                        .HasForeignKey(e => e.Id);
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.HasIndex(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"")
            .WithMany()
            .HasForeignKey(""Id"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal(DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior); });
        }

        [Fact]
        public void ForeignKey_deleteBehavior_is_stored_in_snapshot_for_one_to_one()
        {
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>()
                        .HasOne<EntityWithTwoProperties>()
                        .WithOne()
                        .HasForeignKey<EntityWithOneProperty>(e => e.Id);
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.HasIndex(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.ToTable(""EntityWithTwoProperties"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Id"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal(DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior); });
        }

        [Fact]
        public void ForeignKey_name_preserved_when_generic()
        {
            IModel originalModel = null;

            Test(
                builder =>
                {
                    builder.Entity<EntityWithGenericKey<Guid>>().HasMany<EntityWithGenericProperty<Guid>>().WithOne()
                        .HasForeignKey(e => e.Property);

                    originalModel = builder.Model;
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.ToTable(""EntityWithGenericKey<Guid>"");

        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.ToTable(""EntityWithGenericProperty<Guid>"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<Guid>(""Property"");

        b.HasKey(""Id"");

        b.HasIndex(""Property"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"")
            .WithMany()
            .HasForeignKey(""Property"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
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

                    Assert.Equal(originalForeignKey.SqlServer().Name, foreignKey.SqlServer().Name);

                    var originalIndex = originalChild.FindIndex(originalChild.FindProperty("Property"));
                    var index = child.FindIndex(child.FindProperty("Property"));

                    Assert.Equal(originalIndex.SqlServer().Name, index.SqlServer().Name);
                });
        }

        [Fact]
        public void Do_not_generate_entity_type_builder_again_if_no_foreign_key_is_defined_on_it()
        {
            Test(
                builder =>
                {
                    builder.Entity<BaseType>();
                    builder.Entity<DerivedType>();
                },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseType"", b =>
    {
        b.ToTable(""BaseType"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int?>(""NavigationId"");

        b.HasKey(""Id"");

        b.HasIndex(""NavigationId"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.ToTable(""EntityWithOneProperty"");

        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+DerivedType"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseType"");

        b.ToTable(""DerivedType"");

    });

builder.Entity(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+BaseType"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"")
            .WithMany()
            .HasForeignKey(""NavigationId"");
    });
",
                o => {  });
        }

        #endregion

        private void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        {
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();
            buildModel(modelBuilder);
            var model = modelBuilder.Model;

            var generator = new CSharpSnapshotGenerator(new CSharpHelper());

            var builder = new IndentedStringBuilder();
            generator.Generate("builder", model, builder);
            var code = builder.ToString();

            Assert.Equal(expectedCode, code);

            var build = new BuildSource
            {
                References =
                {
#if (NET451 || DNX451)
                    BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
                },
                Sources = { @"
                    using System;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Metadata;
                    using Microsoft.EntityFrameworkCore.Metadata.Conventions;

                    public static class ModelSnapshot
                    {
                        public static IModel Model
                        {
                            get
                            {
                                var builder = new ModelBuilder(new ConventionSet());
                                " + code + @"

                                return builder.Model;
                            }
                        }
                   }
                " }
            };

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("ModelSnapshot");
            var property = factoryType.GetTypeInfo().GetDeclaredProperty("Model");
            var value = (IModel)property.GetValue(null);

            Assert.NotNull(value);
            assert(value);
        }
    }
}
