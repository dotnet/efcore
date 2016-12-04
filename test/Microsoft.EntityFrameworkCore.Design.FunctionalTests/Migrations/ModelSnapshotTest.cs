// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.Migrations
{
    public class ModelSnapshotTest
    {
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
        }

        private class EntityWithStringProperty
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public override bool GeneratesTemporaryValues => false;
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
        public void Model_default_schema_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.HasDefaultSchema("DefaultSchema");
                        builder.HasAnnotation("AnnotationName", "AnnotationValue");
                    },
                @"builder
    .HasDefaultSchema(""DefaultSchema"")
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"");
",
                o =>
                    {
                        Assert.Equal(2, o.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", o["AnnotationName"]);
                        Assert.Equal("DefaultSchema", o[RelationalFullAnnotationNames.Instance.DefaultSchema]);
                    });
        }

        [Fact]
        public void Entities_are_stored_in_model_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().Ignore(e => e.EntityWithTwoProperties);
                        builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.GetEntityTypes().Count());
                        Assert.Collection(
                            o.GetEntityTypes(),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties", t.Name));
                    });
        }

        #endregion

        #region EntityType

        [Fact]
        public void EntityType_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithTwoProperties>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");

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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Discriminator"");

        b.HasKey(""Id"");

        b.ToTable(""BaseEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+AnotherDerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Title"");

        b.ToTable(""AnotherDerivedEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+DerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Name"");

        b.ToTable(""DerivedEntity"");
    });
",
                o =>
                    {
                        Assert.Equal(3, o.GetEntityTypes().Count());
                        Assert.Collection(
                            o.GetEntityTypes(),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+AnotherDerivedEntity", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+DerivedEntity", t.Name)
                        );
                    });
        }

        [Fact]
        public void Discriminator_annotations_are_stored_in_snapshot()
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
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Discriminator"")
            .IsRequired();

        b.HasKey(""Id"");

        b.ToTable(""BaseEntity"");

        b.HasDiscriminator<string>(""Discriminator"").HasValue(""BaseEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+AnotherDerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Title"");

        b.ToTable(""AnotherDerivedEntity"");

        b.HasDiscriminator().HasValue(""AnotherDerivedEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+DerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Name"");

        b.ToTable(""DerivedEntity"");

        b.HasDiscriminator().HasValue(""DerivedEntity"");
    });
",
                o =>
                    {
                        Assert.Equal("Discriminator", o.FindEntityType(typeof(BaseEntity))[RelationalFullAnnotationNames.Instance.DiscriminatorProperty]);
                        Assert.Equal("BaseEntity", o.FindEntityType(typeof(BaseEntity))[RelationalFullAnnotationNames.Instance.DiscriminatorValue]);
                        Assert.Equal("AnotherDerivedEntity", o.FindEntityType(typeof(AnotherDerivedEntity))[RelationalFullAnnotationNames.Instance.DiscriminatorValue]);
                        Assert.Equal("DerivedEntity", o.FindEntityType(typeof(DerivedEntity))[RelationalFullAnnotationNames.Instance.DiscriminatorValue]);
                    });
        }

        [Fact]
        public void Properties_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
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
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasKey(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"");

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"", ""AlternateId"");

        b.ToTable(""EntityWithTwoProperties"");
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
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""Id"", ""AlternateId"");

        b.ToTable(""EntityWithTwoProperties"");
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
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId);
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"");

        b.ToTable(""EntityWithTwoProperties"");
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
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""Id"", ""AlternateId"");

        b.ToTable(""EntityWithTwoProperties"");
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
                builder =>
                    {
                        builder
                            .Entity<EntityWithTwoProperties>()
                            .HasOne(e => e.EntityWithOneProperty)
                            .WithOne(e => e.EntityWithTwoProperties)
                            .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o =>
                    {
                        var foreignKey = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().Single();
                        Assert.Equal("AlternateId", foreignKey.Properties[0].Name);
                        Assert.Equal("EntityWithTwoProperties", foreignKey.PrincipalToDependent.Name);
                        Assert.Equal("EntityWithOneProperty", foreignKey.DependentToPrincipal.Name);
                    });
        }

        [Fact]
        public void Relationship_principal_key_is_stored_in_snapshot()
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
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithOne(""EntityWithOneProperty"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Id"")
            .HasPrincipalKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o =>
                    {
                        Assert.Equal(2, o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Count());
                        Assert.True(o.FindEntityType(typeof(EntityWithTwoProperties)).FindProperty("AlternateId").IsKey());
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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithGenericKey<Guid>"");
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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithGenericKey<Guid>"");
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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<Guid>(""Property"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""Property"");

        b.ToTable(""EntityWithGenericProperty<Guid>"");
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
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().Property<int>("Id").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithTwoProperties>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().FindProperty("Id")["AnnotationName"]); }
            );
        }

        [Fact]
        public void Custom_value_generator_is_ignored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().Property<int>("Id").HasValueGenerator<CustomValueGenerator>();
                        builder.Ignore<EntityWithTwoProperties>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });
",
                o => { Assert.Null(o.GetEntityTypes().First().FindProperty("Id")[CoreAnnotationNames.ValueGeneratorFactoryAnnotation]); }
            );
        }

        [Fact]
        public void Property_isNullable_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsRequired(); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .IsRequired();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.Equal(false, o.GetEntityTypes().First().FindProperty("Name").IsNullable); });
        }

        [Fact]
        public void Property_ValueGenerated_value_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").ValueGeneratedOnAdd();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .HasMaxLength(100);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.Equal(100, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()); });
        }

        [Fact]
        public void Property_unicodeness_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsUnicode(false); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .IsUnicode(false);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsUnicode()); });
        }

        [Fact]
        public void Many_facets_chained_in_snapshot()
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
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o =>
                    {
                        var property = o.GetEntityTypes().First().FindProperty("Name");
                        Assert.Equal(100, property.GetMaxLength());
                        Assert.False(property.IsUnicode());
                        Assert.Equal("AnnotationValue", property["AnnotationName"]);
                    });
        }

        [Fact]
        public void Property_RequiresValueGenerator_is_not_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").Metadata.RequiresValueGenerator = true;
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal(false, o.GetEntityTypes().First().FindProperty("AlternateId").RequiresValueGenerator); });
        }

        [Fact]
        public void Property_concurrencyToken_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").IsConcurrencyToken();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .IsConcurrencyToken();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal(true, o.GetEntityTypes().First().FindProperty("AlternateId").IsConcurrencyToken); });
        }

        [Fact]
        public void Property_column_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .HasColumnName(""CName"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("CName", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnName"]); });
        }

        [Fact]
        public void Property_column_type_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnType("CType");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .HasColumnType(""CType"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("CType", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnType"]); });
        }

        [Fact]
        public void Property_default_value_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValue(1);
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd()
            .HasDefaultValue(1);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal(1, o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValue"]); });
        }

        [Fact]
        public void Property_default_value_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValueSql("SQL");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql(""SQL"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:DefaultValueSql"]); });
        }

        [Fact]
        public void Property_computed_column_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql("SQL");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAddOrUpdate()
            .HasComputedColumnSql(""SQL"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("SQL", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ComputedColumnSql"]); });
        }

        [Fact]
        public void Property_default_value_of_enum_type_is_stored_in_snapshot_without_actual_enum()
        {
            Test(
                builder => { builder.Entity<EntityWithEnumType>().Property(e => e.Day).HasDefaultValue(Days.Wed); },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<long>(""Day"")
            .ValueGeneratedOnAdd()
            .HasDefaultValue(3L);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithEnumType"");
    });
",
                o => { Assert.Equal(3L, o.GetEntityTypes().First().FindProperty("Day")["Relational:DefaultValue"]); });
        }

        [Fact]
        public void Property_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .HasColumnName(""CName"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
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
        public void Key_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""AlternateId"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["AnnotationName"]); });
        }

        [Fact]
        public void Key_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("KeyName");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""AlternateId"")
            .HasName(""KeyName"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("KeyName", o.GetEntityTypes().First().GetKeys().Where(k => !k.IsPrimaryKey()).First()["Relational:Name"]); });
        }

        [Fact]
        public void Key_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("IndexName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasAlternateKey(""AlternateId"")
            .HasName(""IndexName"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
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
        public void Index_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().GetIndexes().First()["AnnotationName"]); });
        }

        [Fact]
        public void Index_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsUnique();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal(true, o.GetEntityTypes().First().GetIndexes().First().IsUnique); });
        }

        [Fact]
        public void Index_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasName("IndexName");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .HasName(""IndexName"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("IndexName", o.GetEntityTypes().First().GetIndexes().First()["Relational:Name"]); });
        }

        [Fact]
        public void Index_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasName("IndexName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .HasName(""IndexName"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o =>
                    {
                        var index = o.GetEntityTypes().First().GetIndexes().First();
                        Assert.Equal(2, index.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", index["AnnotationName"]);
                        Assert.Equal("IndexName", index["Relational:Name"]);
                    });
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
                            .HasOne(e => e.EntityWithOneProperty)
                            .WithOne(e => e.EntityWithTwoProperties)
                            .HasForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                            .HasAnnotation("AnnotationName", "AnnotationValue");
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
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
                        builder.Entity<EntityWithStringKey>().Ignore(e => e.Properties);
                        builder.Entity<EntityWithStringProperty>()
                            .HasOne<EntityWithStringKey>()
                            .WithOne()
                            .HasForeignKey<EntityWithStringProperty>(e => e.Name)
                            .IsRequired();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringKey"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .IsRequired();

        b.HasKey(""Id"");

        b.HasIndex(""Name"")
            .IsUnique();

        b.ToTable(""EntityWithStringProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", ""Name"")
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
                            .WithMany(e => e.Properties)
                            .HasForeignKey(e => e.Name);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringKey"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"");

        b.HasKey(""Id"");

        b.HasIndex(""Name"");

        b.ToTable(""EntityWithStringProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .WithMany(""Properties"")
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
                            .HasOne(e => e.EntityWithTwoProperties)
                            .WithMany()
                            .HasForeignKey(e => e.Id);
                        builder.Entity<EntityWithTwoProperties>().Ignore(e => e.EntityWithOneProperty);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
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
                            .HasOne(e => e.EntityWithTwoProperties)
                            .WithOne(e => e.EntityWithOneProperty)
                            .HasForeignKey<EntityWithOneProperty>(e => e.Id);
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithOne(""EntityWithOneProperty"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Id"")
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
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithGenericKey<Guid>"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<Guid>(""Property"");

        b.HasKey(""Id"");

        b.HasIndex(""Property"");

        b.ToTable(""EntityWithGenericProperty<Guid>"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithGenericKey<System.Guid>"")
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
        public void ForeignKey_constraint_name_is_stored_in_snapshot_as_fluent_api()
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
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .HasConstraintName(""Constraint"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal("Constraint", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["Relational:Name"]); });
        }

        [Fact]
        public void ForeignKey_multiple_annotations_are_stored_in_snapshot()
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
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .IsUnique();

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .HasConstraintName(""Constraint"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o =>
                    {
                        var fk = o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First();
                        Assert.Equal(2, fk.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", fk["AnnotationName"]);
                        Assert.Equal("Constraint", fk["Relational:Name"]);
                    });
        }

        [Fact]
        public void Do_not_generate_entity_type_builder_again_if_no_foreign_key_is_defined_on_it()
        {
            Test(
                builder =>
                    {
                        builder.Entity<BaseType>();
                        builder.Ignore<EntityWithTwoProperties>();
                        builder.Entity<DerivedType>();
                    },
                @"
builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int?>(""NavigationId"");

        b.HasKey(""Id"");

        b.HasIndex(""NavigationId"");

        b.ToTable(""BaseType"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+DerivedType"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseType"");


        b.ToTable(""DerivedType"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+BaseType"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.FunctionalTests.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Navigation"")
            .WithMany()
            .HasForeignKey(""NavigationId"");
    });
",
                o => { });
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
#if NET451
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
