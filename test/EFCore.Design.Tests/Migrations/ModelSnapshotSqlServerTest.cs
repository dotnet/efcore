// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Logging;
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
            public EntityWithStringProperty EntityWithStringProperty { get; set; }
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
                builder => { builder.HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"builder
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
    .HasAnnotation(""ChangeDetector.SkipDetectChanges"", ""true"")
    .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
",
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
                @"builder
    .HasDefaultSchema(""DefaultSchema"")
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
    .HasAnnotation(""ChangeDetector.SkipDetectChanges"", ""true"")
    .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
",
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty", t.Name),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties", t.Name));
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void BaseType_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<DerivedEntity>().HasBaseType<BaseEntity>();
                        builder.Entity<AnotherDerivedEntity>().HasBaseType<BaseEntity>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

        b.Property<string>(""Title"");

        b.ToTable(""AnotherDerivedEntity"");

        b.HasDiscriminator().HasValue(""AnotherDerivedEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

        b.Property<string>(""Name"");

        b.ToTable(""DerivedEntity"");

        b.HasDiscriminator().HasValue(""DerivedEntity"");
    });
",
                o =>
                    {
                        Assert.Equal(3, o.GetEntityTypes().Count());
                        Assert.Collection(
                            o.GetEntityTypes(),
                            t => Assert.Equal("Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity", t.Name),
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+AnotherDerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

        b.Property<string>(""Title"");

        b.ToTable(""AnotherDerivedEntity"");

        b.HasDiscriminator().HasValue(""AnotherDerivedEntity"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedEntity"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseEntity"");

        b.Property<string>(""Name"");

        b.ToTable(""DerivedEntity"");

        b.HasDiscriminator().HasValue(""DerivedEntity"");
    });
",
                o =>
                    {
                        Assert.Equal("Discriminator", o.FindEntityType(typeof(BaseEntity))[RelationalAnnotationNames.DiscriminatorProperty]);
                        Assert.Equal("BaseEntity", o.FindEntityType(typeof(BaseEntity))[RelationalAnnotationNames.DiscriminatorValue]);
                        Assert.Equal("AnotherDerivedEntity", o.FindEntityType(typeof(AnotherDerivedEntity))[RelationalAnnotationNames.DiscriminatorValue]);
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Primary_key_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasKey(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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
        public virtual void Alternate_keys_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Indexes_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId);
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Indexes_are_stored_in_snapshot_including_composite_index()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => new { t.Id, t.AlternateId });
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
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
        public virtual void Owned_types_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder
                            .Entity<EntityWithOneProperty>()
                            .OwnsOne(
                                eo => eo.EntityWithTwoProperties, eb =>
                                    {
                                        eb.HasForeignKey(e => e.AlternateId);
                                        eb.HasOne(e => e.EntityWithOneProperty)
                                            .WithOne(e => e.EntityWithTwoProperties);
                                        eb.HasIndex(e => e.Id);

                                        eb.OwnsOne(e => e.EntityWithStringProperty);
                                    });
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"", b1 =>
            {
                b1.Property<int>(""AlternateId"")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                b1.Property<int>(""Id"");

                b1.HasIndex(""Id"");

                b1.ToTable(""EntityWithOneProperty"");

                b1.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
                    .WithOne(""EntityWithTwoProperties"")
                    .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
                    .OnDelete(DeleteBehavior.Cascade);

                b1.OwnsOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""EntityWithStringProperty"", b2 =>
                    {
                        b2.Property<int>(""Id"")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

                        b2.Property<string>(""Name"");

                        b2.ToTable(""EntityWithOneProperty"");

                        b2.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"")
                            .WithOne(""EntityWithStringProperty"")
                            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""Id"")
                            .OnDelete(DeleteBehavior.Cascade);
                    });
            });
    });
",
                o =>
                    {
                        var ownership1 = o.FindEntityType(typeof(EntityWithOneProperty)).GetReferencingForeignKeys().Single();
                        Assert.Equal("AlternateId", ownership1.Properties[0].Name);
                        Assert.Equal("EntityWithTwoProperties", ownership1.PrincipalToDependent.Name);
                        Assert.Equal("EntityWithOneProperty", ownership1.DependentToPrincipal.Name);
                        Assert.True(ownership1.IsRequired);
                        var ownedType1 = ownership1.DeclaringEntityType;
                        Assert.Equal("AlternateId", ownedType1.FindPrimaryKey().Properties[0].Name);
                        Assert.Equal(1, ownedType1.GetKeys().Count());
                        Assert.Equal("Id", ownedType1.GetIndexes().Single().Properties[0].Name);
                        var ownership2 = ownedType1.GetReferencingForeignKeys().Single();
                        Assert.Equal("Id", ownership2.Properties[0].Name);
                        Assert.Equal("EntityWithStringProperty", ownership2.PrincipalToDependent.Name);
                        Assert.Null(ownership2.DependentToPrincipal);
                        Assert.True(ownership2.IsRequired);
                        var ownedType2 = ownership2.DeclaringEntityType;
                        Assert.Equal("Id", ownedType2.FindPrimaryKey().Properties[0].Name);
                        Assert.Equal(1, ownedType2.GetKeys().Count());
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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

                        Assert.Equal(originalAlternateKey.Relational().Name, alternateKey.Relational().Name);
                    });
        }

        [Fact]
        public virtual void Discriminator_of_enum()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>().HasDiscriminator(e => e.Day),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<long>(""Day"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithEnumType"");

        b.HasDiscriminator<long>(""Day"");
    });
",
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Day"")
            .IsRequired();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithEnumType"");

        b.HasDiscriminator<string>(""Day"");
    });
",
                model =>
                {
                    var discriminatorProperty = model.GetEntityTypes().First().Relational().DiscriminatorProperty;
                    Assert.Equal(typeof(string), discriminatorProperty.ClrType);
                    Assert.False(discriminatorProperty.IsNullable);
                });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.GetEntityTypes().First().FindProperty("Id")["AnnotationName"]); }
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });
",
                o => { Assert.Null(o.GetEntityTypes().First().FindProperty("Id")[CoreAnnotationNames.ValueGeneratorFactoryAnnotation]); }
            );
        }

        [Fact]
        public virtual void Property_isNullable_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsRequired(); },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"")
            .IsRequired();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsNullable); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd()
            .HasDefaultValue(null);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal(ValueGenerated.OnAdd, o.GetEntityTypes().First().FindProperty("AlternateId").ValueGenerated); });
        }

        [Fact]
        public virtual void Property_maxLength_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").HasMaxLength(100); },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"")
            .HasMaxLength(100);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.Equal(100, o.GetEntityTypes().First().FindProperty("Name").GetMaxLength()); });
        }

        [Fact]
        public virtual void Property_unicodeness_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsUnicode(false); },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"")
            .IsUnicode(false);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.False(o.GetEntityTypes().First().FindProperty("Name").IsUnicode()); });
        }

        [Fact]
        public virtual void Property_fixedlengthness_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").IsFixedLength(true); },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"")
            .IsFixedLength(true);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
                o => { Assert.True(o.GetEntityTypes().First().FindProperty("Name").Relational().IsFixedLength); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
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
        public virtual void Property_concurrencyToken_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").IsConcurrencyToken();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"")
            .IsConcurrencyToken();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.True(o.GetEntityTypes().First().FindProperty("AlternateId").IsConcurrencyToken); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"")
            .HasColumnName(""CName"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("CName", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnName"]); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"")
            .HasColumnType(""CType"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
                o => { Assert.Equal("CType", o.GetEntityTypes().First().FindProperty("AlternateId")["Relational:ColumnType"]); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Property_default_value_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasDefaultValueSql("SQL");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Property_computed_column_sql_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasComputedColumnSql("SQL");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Property_default_value_of_enum_type_is_stored_in_snapshot_without_actual_enum()
        {
            Test(
                builder => { builder.Entity<EntityWithEnumType>().Property(e => e.Day).HasDefaultValue(Days.Wed); },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Property_enum_type_is_stored_in_snapshot_with_custom_conversion_and_seed_data()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>(eb =>
                {
                    eb.Property(e => e.Day).HasDefaultValue(Days.Wed)
                        .HasConversion(v => v.ToString(), v => (Days)Enum.Parse(typeof(Days), v));
                    eb.HasData(new
                    {
                        Id = 1,
                        Day = Days.Fri
                    });
                }),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
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
            new { Id = 1, Day = ""Fri"" }
        );
    });
",
                o =>
                {
                    var property = o.GetEntityTypes().First().FindProperty("Day");
                    Assert.Equal(typeof(string), property.ClrType);
                    Assert.Equal(Days.Wed.ToString(), property["Relational:DefaultValue"]);
                    Assert.False(property.IsNullable);
                });
        }

        [Fact]
        public virtual void Property_of_nullable_enum()
        {
            Test(
                builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<long?>(""Day"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithNullableEnumType"");
    });
",
                o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_of_enum_to_nullable()
        {
            Test(
                builder => builder.Entity<EntityWithEnumType>().Property(e => e.Day)
                    .HasConversion(m => (long?)m, p => p.HasValue ? (Days)p.Value : default),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<long>(""Day"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithEnumType"");
    });
",
                o => Assert.False(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_of_nullable_enum_to_string()
        {
            Test(
                builder => builder.Entity<EntityWithNullableEnumType>().Property(e => e.Day).HasConversion<string>(),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithNullableEnumType"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Day"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithNullableEnumType"");
    });
",
                o => Assert.True(o.GetEntityTypes().First().FindProperty("Day").IsNullable));
        }

        [Fact]
        public virtual void Property_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").HasColumnName("CName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Key_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId)
                            .HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Key_name_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("KeyName");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Key_multiple_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasAlternateKey(t => t.AlternateId).HasName("IndexName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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
        public virtual void Index_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                            .HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Index_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).IsUnique();
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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
",
                o => { Assert.True(o.GetEntityTypes().First().GetIndexes().First().IsUnique); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

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
        public virtual void Index_filter_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId)
                            .HasFilter("AlternateId <> 0");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.HasIndex(""AlternateId"")
            .HasFilter(""AlternateId <> 0"");

        b.ToTable(""EntityWithTwoProperties"");
    });
",
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
                        builder.Entity<EntityWithTwoProperties>().HasIndex(t => t.AlternateId).HasName("IndexName").HasAnnotation("AnnotationName", "AnnotationValue");
                        builder.Ignore<EntityWithOneProperty>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

        [Fact]
        public virtual void Index_with_default_constraint_name_exceeding_max()
        {
            Test(
                builder => builder.Entity<EntityWithStringProperty>(
                    x =>
                    {
                        const string propertyName = "SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit";
                        x.Property<string>(propertyName);
                        x.HasIndex(propertyName);
                    }),
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"");

        b.Property<string>(""SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit"");

        b.HasKey(""Id"");

        b.HasIndex(""SomePropertyWithAnExceedinglyLongIdentifierThatCausesTheDefaultIndexNameToExceedTheMaximumIdentifierLimit"");

        b.ToTable(""EntityWithStringProperty"");
    });
",
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
            .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal("AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["AnnotationName"]); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringKey"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"")
            .WithOne()
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", ""Name"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).FindProperty("Name").IsNullable); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithStringKey"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<string>(""Name"");

        b.HasKey(""Id"");

        b.HasIndex(""Name"");

        b.ToTable(""EntityWithStringProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithStringKey"")
            .WithMany(""Properties"")
            .HasForeignKey(""Name"");
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithMany()
            .HasForeignKey(""Id"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal(DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithOne(""EntityWithOneProperty"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal(DeleteBehavior.Cascade, o.FindEntityType(typeof(EntityWithOneProperty)).GetForeignKeys().First().DeleteBehavior); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"", b =>
    {
        b.Property<Guid>(""Id"")
            .ValueGeneratedOnAdd();

        b.HasKey(""Id"");

        b.ToTable(""EntityWithGenericKey<Guid>"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<Guid>(""Property"");

        b.HasKey(""Id"");

        b.HasIndex(""Property"");

        b.ToTable(""EntityWithGenericProperty<Guid>"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericProperty<System.Guid>"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithGenericKey<System.Guid>"")
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
            .HasConstraintName(""Constraint"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
                o => { Assert.Equal("Constraint", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["Relational:Name"]); });
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""EntityWithOneProperty"")
            .WithOne(""EntityWithTwoProperties"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
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
        public virtual void Do_not_generate_entity_type_builder_again_if_no_foreign_key_is_defined_on_it()
        {
            Test(
                builder =>
                    {
                        builder.Entity<BaseType>();
                        builder.Ignore<EntityWithTwoProperties>();
                        builder.Entity<DerivedType>();
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+DerivedType"", b =>
    {
        b.HasBaseType(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"");


        b.ToTable(""DerivedType"");

        b.HasDiscriminator().HasValue(""DerivedType"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+BaseType"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Navigation"")
            .WithMany()
            .HasForeignKey(""NavigationId"");
    });
",
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithOne(""EntityWithOneProperty"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
            .HasPrincipalKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
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
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
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

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.HasOne(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""EntityWithTwoProperties"")
            .WithOne(""EntityWithOneProperty"")
            .HasForeignKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", ""Id"")
            .HasPrincipalKey(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", ""AlternateId"")
            .OnDelete(DeleteBehavior.Cascade);
    });
",
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
            Test(
                builder =>
                {
                    builder.Entity<EntityWithOneProperty>(eb =>
                    {

                        eb.Ignore(e => e.EntityWithTwoProperties);
                        eb.Property<decimal?>("OptionalProperty");
                        eb.HasData(
                            new EntityWithOneProperty
                            {
                                Id = 42
                            },
                            new
                            {
                                Id = 43,
                                OptionalProperty = 4.3M
                            });
                    });
                    builder.Ignore<EntityWithTwoProperties>();
                },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<decimal?>(""OptionalProperty"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");

        b.HasData(
            new { Id = 42 },
            new { Id = 43, OptionalProperty = 4.3m }
        );
    });
",
                o => Assert.Collection(
                    o.GetEntityTypes().SelectMany(e => e.GetData()),
                    seed => Assert.Equal(42, seed["Id"]),
                    seed =>
                    {
                        Assert.Equal(43, seed["Id"]);
                        Assert.Equal(4.3m, seed["OptionalProperty"]);
                    }));
        }

        [Fact]
        public virtual void SeedData_for_multiple_entities_are_stored_in_model_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>()
                            .Ignore(e => e.EntityWithTwoProperties)
                            .HasData(
                                new EntityWithOneProperty { Id = 27 });
                        builder.Entity<EntityWithTwoProperties>()
                            .Ignore(e => e.EntityWithOneProperty)
                            .HasData(
                                new EntityWithTwoProperties { Id = 42, AlternateId = 43 });
                    },
                GetHeading() + @"
builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.HasKey(""Id"");

        b.ToTable(""EntityWithOneProperty"");

        b.HasData(
            new { Id = 27 }
        );
    });

builder.Entity(""Microsoft.EntityFrameworkCore.Migrations.ModelSnapshotSqlServerTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);

        b.Property<int>(""AlternateId"");

        b.HasKey(""Id"");

        b.ToTable(""EntityWithTwoProperties"");

        b.HasData(
            new { Id = 42, AlternateId = 43 }
        );
    });
",
                o => Assert.Collection(
                    o.GetEntityTypes().Select(e => e.GetData().Single()),
                    seed => Assert.Equal(27, seed["Id"]),
                    seed =>
                        {
                            Assert.Equal(42, seed["Id"]);
                            Assert.Equal(43, seed["AlternateId"]);
                        }));
        }

        #endregion

        protected virtual string GetHeading() => @"builder
    .HasAnnotation(""Relational:MaxIdentifierLength"", 128)
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
";

        protected virtual ICollection<BuildReference> GetReferences() => new List<BuildReference>
        {
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer")
        };

        protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            buildModel(modelBuilder);

            modelBuilder.GetInfrastructure().Metadata.Validate();
            CreateModelValidator().Validate(modelBuilder.Model);
            var model = modelBuilder.Model;

            var generator = new CSharpSnapshotGenerator(new CSharpSnapshotGeneratorDependencies(new CSharpHelper()));

            var builder = new IndentedStringBuilder();
            generator.Generate("builder", model, builder);
            var code = builder.ToString();

            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);

            var build = new BuildSource
            {
                Sources =
                {
                    @"
                    using System;
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Metadata;
                    using Microsoft.EntityFrameworkCore.Metadata.Conventions;
                    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
                "
                }
            };

            foreach (var buildReference in GetReferences())
            {
                build.References.Add(buildReference);
            }

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("ModelSnapshot");
            var property = factoryType.GetTypeInfo().GetDeclaredProperty("Model");
            var value = (IModel)property.GetValue(null);

            Assert.NotNull(value);
            assert(value);
        }

        protected ModelBuilder CreateConventionalModelBuilder() => new ModelBuilder(SqlServerConventionSetBuilder.Build());

        protected ModelValidator CreateModelValidator()
            => new SqlServerModelValidator(
                new ModelValidatorDependencies(
                    new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                        new ListLoggerFactory(new List<(LogLevel, EventId, string)>(), l => l == DbLoggerCategory.Model.Validation.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake")),
                    new DiagnosticsLogger<DbLoggerCategory.Model>(
                        new ListLoggerFactory(new List<(LogLevel, EventId, string)>(), l => l == DbLoggerCategory.Model.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"))),
                new RelationalModelValidatorDependencies(
#pragma warning disable 618
                    TestServiceFactory.Instance.Create<ObsoleteRelationalTypeMapper>(),
#pragma warning restore 618
                    new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
    }
}
