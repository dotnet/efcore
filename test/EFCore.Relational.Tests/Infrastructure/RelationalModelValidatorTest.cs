// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class RelationalModelValidatorTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Ignores_bool_with_default_value_false()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(E));
            SetPrimaryKey(entityType);
            entityType.AddProperty("ImNot", typeof(bool?)).SetDefaultValue(false);
            entityType.AddProperty("ImNotUsed", typeof(bool)).SetDefaultValue(false);

            var property = entityType.AddProperty("ImBool", typeof(bool));
            property.SetDefaultValue(false);
            property.ValueGenerated = ValueGenerated.OnAdd;

            Validate(model);

            Assert.False(LoggerFactory.Log.Any(l => l.Level == LogLevel.Warning));
        }

        [ConditionalFact]
        public virtual void Detects_bool_with_default_value_not_false()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(E));
            SetPrimaryKey(entityType);
            entityType.AddProperty("ImNot", typeof(bool?)).SetDefaultValue(true);
            entityType.AddProperty("ImNotUsed", typeof(bool)).SetDefaultValue(true);

            var property = entityType.AddProperty("ImBool", typeof(bool));
            property.SetDefaultValue(true);
            property.ValueGenerated = ValueGenerated.OnAdd;

            VerifyWarning(RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("ImBool", "E"), model);
        }

        [ConditionalFact]
        public virtual void Detects_bool_with_default_expression()
        {
            var model = CreateConventionlessModelBuilder().Model;
            var entityType = model.AddEntityType(typeof(E));
            SetPrimaryKey(entityType);
            entityType.AddProperty("ImNot", typeof(bool?)).SetDefaultValueSql("TRUE");
            entityType.AddProperty("ImNotUsed", typeof(bool)).SetDefaultValueSql("TRUE");

            var property = entityType.AddProperty("ImBool", typeof(bool));
            property.SetDefaultValueSql("TRUE");
            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;

            VerifyWarning(RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("ImBool", "E"), model);
        }

        [ConditionalFact]
        public virtual void Detects_primary_key_with_default_value()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            entityA.FindProperty("Id").SetDefaultValue(1);
            AddProperties(entityA);

            entityA.FindProperty("Id").SetDefaultValue(1);

            VerifyWarning(RelationalResources.LogKeyHasDefaultValue(
                new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("Id", "A"), model);
        }

        [ConditionalFact]
        public virtual void Detects_alternate_key_with_default_value()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            entityA.AddProperty(nameof(A.P1), typeof(int?));
            entityA.AddProperty(nameof(A.P2), typeof(int?));
            entityA.AddProperty(nameof(A.P3), typeof(int?));

            var property = entityA.AddProperty("P0", typeof(int?));
            property.IsNullable = false;
            entityA.AddKey(new[] { property });
            property.SetDefaultValue(1);

            VerifyWarning(RelationalResources.LogKeyHasDefaultValue(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("P0", "A"), model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_table_names_without_identifying_relationship()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            AddProperties(entityB);
            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            entityA.SetTableName("Table");
            entityA.SetSchema("Schema");
            entityB.SetTableName("Table");
            entityB.SetSchema("Schema");

            VerifyError(
                RelationalStrings.IncompatibleTableNoRelationship(
                    "Schema.Table", entityB.DisplayName(), entityA.DisplayName()),
                model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_table_names_when_no_key()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            entityA.AddProperty("Id", typeof(int));
            entityA.IsKeyless = true;
            AddProperties(entityA);

            var entityB = model.AddEntityType(typeof(B));
            entityB.AddProperty("Id", typeof(int));
            entityB.IsKeyless = true;
            AddProperties(entityB);
            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));

            entityA.SetTableName("Table");
            entityB.SetTableName("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableNoRelationship(
                    "Table", entityB.DisplayName(), entityA.DisplayName()),
                model);
        }

        [ConditionalFact]
        public virtual void Passes_for_duplicate_table_names_in_different_schema()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            AddProperties(entityB);
            entityB.AddIgnored(nameof(B.A));
            entityB.AddIgnored(nameof(B.AnotherA));
            entityB.AddIgnored(nameof(B.ManyAs));
            entityA.SetTableName("Table");
            entityA.SetSchema("SchemaA");
            entityB.SetTableName("Table");
            entityB.SetSchema("SchemaB");

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Passes_for_duplicate_table_names_for_inherited_entities()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            var entityC = model.AddEntityType(typeof(C));
            SetBaseType(entityC, entityA);

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_primary_keys_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().HasKey(a => a.Id).HasName("Key");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableKeyNameMismatch(
                    "Table", nameof(B), nameof(A), "PK_Table", "{'Id'}", "Key", "{'Id'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_comments_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table").HasComment("My comment");
            modelBuilder.Entity<B>().ToTable("Table").HasComment("my comment");

            VerifyError(
                RelationalStrings.IncompatibleTableCommentMismatch(
                    "Table", nameof(A), nameof(B), "My comment", "my comment"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_on_null_comments()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table").HasComment("My comment");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_primary_key_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.Id).ValueGeneratedNever().HasColumnName("Key");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.Id).ValueGeneratedNever().HasColumnName(nameof(B.Id));
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateKeyColumnMismatch(
                    "{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table", "{'Id'}", "{'Key'}"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "default_int_mapping"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_multiple_shared_table_roots()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<A>().HasOne<C>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<C>(b => b.Id);
            modelBuilder.Entity<C>().HasBaseType((string)null).ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableNoRelationship("Table", nameof(C), nameof(B)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_shared_table_root_cycle()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<B>(a => a.Id).HasPrincipalKey<A>(b => b.Id);
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                CoreStrings.IdentifyingRelationshipCycle(nameof(A)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);

            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>(
                b =>
                {
                    b.ToTable("Table");
                    b.Property(bb => bb.Id)
                        .HasColumnName("Key")
                        .HasColumnType("someInt")
                        .HasDefaultValueSql("NEXT value");

                    b.HasKey(bb => bb.Id)
                        .HasName("Key");
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_shared_table_inverted()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "default_int_mapping", "just_string(max)"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Type), nameof(Dog), nameof(Dog.Type), nameof(Cat.Type), nameof(Animal), "just_string(max)",
                    "default_int_mapping"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasMaxLength(30).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasMaxLength(15).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "just_string(30)",
                    "just_string(15)"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_ComputedColumnSql()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComputedColumnSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

            VerifyError(
                RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValue()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValue("1");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "NULL", "1"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValueSql()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_nullability()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Id");

            VerifyError(
                RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                    nameof(Animal), nameof(Animal.Id), nameof(Dog), nameof(Dog.Type), nameof(Animal.Id), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_comments()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComment("My comment");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

            VerifyError(
                RelationalStrings.DuplicateColumnNameCommentMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "My comment", ""),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_duplicate_column_names_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                eb =>
                {
                    eb.Ignore(e => e.Type);
                    eb.Property(c => c.Breed).HasMaxLength(25);
                    eb.Property(c => c.Breed).HasColumnName("BreedName");
                    eb.Property(c => c.Breed).HasDefaultValue("None");
                    eb.Property<bool>("Selected").HasDefaultValue(false);
                });
            modelBuilder.Entity<Dog>(
                eb =>
                {
                    eb.Ignore(e => e.Type);
                    eb.Property(c => c.Breed).HasMaxLength(25);
                    eb.Property(c => c.Breed).HasColumnName("BreedName");
                    eb.Property(c => c.Breed).HasDefaultValue("None");
                    eb.Property<bool>("Selected").HasDefaultValue(false);
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_principal_tables()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");
            modelBuilder.Entity<Dog>().HasOne<Animal>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                    "{'FriendId'}", nameof(Dog),
                    "{'FriendId'}", nameof(Cat),
                    nameof(Animal), "FK",
                    nameof(Animal),
                    nameof(Person)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_count()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<int>("FriendId");
            modelBuilder.Entity<Animal>().Property<string>("Shadow");
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId", "Shadow").HasPrincipalKey(
                p => new
                {
                    p.Id,
                    p.Name
                }).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyColumnMismatch(
                    "{'FriendId'}", nameof(Dog),
                    "{'FriendId', 'Shadow'}", nameof(Cat),
                    nameof(Animal), "FK",
                    "{'FriendId'}",
                    "{'FriendId', 'Shadow'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_order()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasOne<Person>().WithMany()
                        .HasForeignKey(
                            c => new
                            {
                                c.Name,
                                c.Breed
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.Name,
                                p.FavoriteBreed
                            })
                        .HasConstraintName("FK");
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasOne<Person>().WithMany()
                        .HasForeignKey(
                            d => new
                            {
                                d.Breed,
                                d.Name
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.FavoriteBreed,
                                p.Name
                            })
                        .HasConstraintName("FK");
                });

            VerifyError(
                RelationalStrings.DuplicateForeignKeyColumnMismatch(
                    "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                    nameof(Animal), "FK",
                    "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}",
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_mapped_to_different_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(
                c => new
                {
                    c.Name,
                    c.Breed
                }).HasPrincipalKey(
                p => new
                {
                    p.Name,
                    p.FavoriteBreed
                }).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(
                d => new
                {
                    d.Name,
                    d.Breed
                }).HasPrincipalKey(
                p => new
                {
                    p.Name,
                    p.FavoriteBreed
                }).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyColumnMismatch(
                    "{'" + nameof(Dog.Name) + "', '" + nameof(Dog.Breed) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                    nameof(Animal), "FK",
                    "{'" + nameof(Dog.Name) + "', 'DogBreed'}",
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_referencing_different_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>()
                .HasOne<Person>().WithMany()
                .HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name).HasConstraintName("FK");
            modelBuilder.Entity<Dog>()
                .HasOne<Person>().WithMany()
                .HasForeignKey(d => d.Name).HasPrincipalKey(p => p.FavoriteBreed).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "FK",
                    "{'" + nameof(Person.FavoriteBreed) + "'}",
                    "{'" + nameof(Person.Name) + "'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_uniqueness()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name").Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithOne().HasForeignKey<Dog>(d => d.Name)
                .HasPrincipalKey<Person>(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name").Metadata;

            VerifyError(
                RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "FK_Animal_Person_Name"),
                modelBuilder.Model);

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.NotEqual(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_delete_behavior()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_Animal_Person_Name");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "FK_Animal_Person_Name",
                    DeleteBehavior.SetNull, DeleteBehavior.Cascade),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
            Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.NotEqual(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name").Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
            Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.NotEqual(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_duplicate_foreignKey_names_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            IForeignKey fk1 = null;
            IForeignKey fk2 = null;

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk1 = et
                        .HasOne(a => a.FavoritePerson)
                        .WithMany()
                        .HasForeignKey(
                            c => new
                            {
                                c.Name,
                                c.Breed
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.Name,
                                p.FavoriteBreed
                            })
                        .Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk2 = et
                        .HasOne(a => (Customer)a.FavoritePerson)
                        .WithMany()
                        .HasForeignKey(
                            c => new
                            {
                                c.Name,
                                c.Breed
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.Name,
                                p.FavoriteBreed
                            })
                        .Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(fk1, fk2);
            Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_duplicate_foreignKey_names_within_hierarchy_name_configured_explicitly()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            IForeignKey fk1 = null;
            IForeignKey fk2 = null;

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk1 = et
                        .HasOne<Person>()
                        .WithMany()
                        .HasForeignKey(
                            c => new
                            {
                                c.Name,
                                c.Breed
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.Name,
                                p.FavoriteBreed
                            })
                        .HasConstraintName("FK")
                        .Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk2 = et
                        .HasOne<Customer>()
                        .WithMany()
                        .HasForeignKey(
                            c => new
                            {
                                c.Name,
                                c.Breed
                            })
                        .HasPrincipalKey(
                            p => new
                            {
                                p.Name,
                                p.FavoriteBreed
                            })
                        .HasConstraintName("FK")
                        .Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(fk1, fk2);
            Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_count()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<int>("Shadow");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Cat.Name), "Shadow").HasName("IX");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasName("IX");

            VerifyError(
                RelationalStrings.DuplicateIndexColumnMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "', 'Shadow'}", nameof(Cat),
                    nameof(Animal), "IX",
                    "{'" + nameof(Dog.Name) + "'}",
                    "{'" + nameof(Cat.Name) + "', 'Shadow'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_order()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasIndex(
                        c => new
                        {
                            c.Name,
                            c.Breed
                        }).HasName("IX");
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasIndex(
                        d => new
                        {
                            d.Breed,
                            d.Name
                        }).HasName("IX");
                });

            VerifyError(
                RelationalStrings.DuplicateIndexColumnMismatch(
                    "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                    nameof(Animal), "IX",
                    "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}",
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_mapped_to_different_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(
                c => new
                {
                    c.Name,
                    c.Breed
                }).HasName("IX");
            modelBuilder.Entity<Dog>().HasIndex(
                d => new
                {
                    d.Name,
                    d.Breed
                }).HasName("IX");
            modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

            VerifyError(
                RelationalStrings.DuplicateIndexColumnMismatch(
                    "{'" + nameof(Dog.Name) + "', '" + nameof(Dog.Breed) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                    nameof(Animal), "IX",
                    "{'" + nameof(Dog.Name) + "', 'DogBreed'}",
                    "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_uniqueness()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasName("IX_Animal_Name");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).HasName("IX_Animal_Name");

            VerifyError(
                RelationalStrings.DuplicateIndexUniquenessMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "IX_Animal_Name"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.GetName());
            Assert.Equal("IX_Animal_Name1", index2.GetName());
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_duplicate_index_names_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            IMutableIndex index1 = null;
            IMutableIndex index2 = null;
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    index1 = et.HasIndex(c => c.Breed).HasName("IX_Animal_Breed").Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    index2 = et.HasIndex(c => c.Breed).HasName("IX_Animal_Breed").Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetName(), index2.GetName());
        }

        [ConditionalFact]
        public virtual void Detects_missing_concurrency_token_on_the_base_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal))
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Cat>()
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            VerifyError(
                RelationalStrings.MissingConcurrencyColumn(nameof(Animal), "Version", nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_missing_concurrency_token_on_the_sharing_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal));
            modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            VerifyError(
                RelationalStrings.MissingConcurrencyColumn(nameof(Person), "Version", nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_correctly_mapped_concurrency_tokens_with_table_sharing()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal))
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Animal>()
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_correctly_mapped_concurrency_tokens_with_owned()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson,
                    pb => pb.Property<byte[]>("Version").IsRowVersion().HasColumnName("Version"));
            modelBuilder.Entity<Dog>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_non_hierarchical_model()
        {
            var model = CreateConventionlessModelBuilder().Model;

            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            AddProperties(entityA);

            Validate(model);
        }

        [ConditionalFact]
        public virtual void Does_not_detect_missing_discriminator_value_for_abstract_class()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Abstract>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<C>(1)
                .HasValue<D>(2)
                .HasValue<Generic<string>>(3);

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_ToTable_on_derived_entity_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().ToTable("Animal");
            modelBuilder.Entity<Cat>().ToTable("Cat");

            VerifyError(
                RelationalStrings.DerivedTypeTable(nameof(Cat), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_function_with_invalid_return_type_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var methodInfo
                = typeof(DbFunctionMetadataTests.TestMethods)
                    .GetRuntimeMethod(nameof(DbFunctionMetadataTests.TestMethods.MethodD), Array.Empty<Type>());

            modelBuilder.HasDbFunction(methodInfo);

            VerifyError(
                RelationalStrings.DbFunctionInvalidReturnType(
                    methodInfo.DisplayName(),
                    typeof(DbFunctionMetadataTests.TestMethods).ShortDisplayName()),
                modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping]
                = new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
                    .FindMapping(property);

        protected override void SetBaseType(IMutableEntityType entityType, IMutableEntityType baseEntityType)
        {
            base.SetBaseType(entityType, baseEntityType);

            baseEntityType.SetDiscriminatorProperty(baseEntityType.AddProperty("Discriminator", typeof(string)));
            baseEntityType.SetDiscriminatorValue(baseEntityType.Name);
            entityType.SetDiscriminatorValue(entityType.Name);
        }

        protected class Animal
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public Person FavoritePerson { get; set; }
        }

        protected class Cat : Animal
        {
            public string Breed { get; set; }

            [NotMapped]
            public string Type { get; set; }

            public int Identity { get; set; }
        }

        protected class Dog : Animal
        {
            public string Breed { get; set; }

            [NotMapped]
            public int Type { get; set; }

            public int Identity { get; set; }
        }

        protected class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FavoriteBreed { get; set; }
        }

        protected class Customer : Person
        {
        }

        protected override TestHelpers TestHelpers => RelationalTestHelpers.Instance;
    }
}
