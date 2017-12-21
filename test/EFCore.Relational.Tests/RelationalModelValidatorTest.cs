// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class RelationalModelValidatorTest : ModelValidatorTest
    {
        [Fact]
        public virtual void Detects_bool_with_default_value()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(E));
            SetPrimaryKey(entityType);
            entityType.AddProperty("ImBool", typeof(bool)).Relational().DefaultValue = true;
            entityType.AddProperty("ImNot", typeof(bool?)).Relational().DefaultValue = true;

            VerifyWarning(RelationalStrings.LogBoolWithDefaultWarning.GenerateMessage("ImBool", "E"), model);
        }

        [Fact]
        public virtual void Detects_bool_with_default_expression()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(E));
            SetPrimaryKey(entityType);
            entityType.AddProperty("ImBool", typeof(bool)).Relational().DefaultValueSql = "TRUE";
            entityType.AddProperty("ImNot", typeof(bool?)).Relational().DefaultValueSql = "TRUE";

            VerifyWarning(RelationalStrings.LogBoolWithDefaultWarning.GenerateMessage("ImBool", "E"), model);
        }

        [Fact]
        public virtual void Detects_primary_key_with_default_value()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            entityA.FindProperty("Id").Relational().DefaultValue = 1;

            VerifyWarning(RelationalStrings.LogKeyHasDefaultValue.GenerateMessage("Id", "A"), model);
        }

        [Fact]
        public virtual void Detects_alternate_key_with_default_value()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);

            var property = entityA.AddProperty("P0", typeof(int?));
            property.IsNullable = false;
            entityA.AddKey(new[] { property });
            property.Relational().DefaultValue = 1;

            VerifyWarning(RelationalStrings.LogKeyHasDefaultValue.GenerateMessage("P0", "A"), model);
        }

        [Fact]
        public virtual void Detects_duplicate_table_names_without_identifying_relationship()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "Schema";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "Schema";

            VerifyError(
                RelationalStrings.IncompatibleTableNoRelationship(
                    "Schema.Table", entityB.DisplayName(), entityA.DisplayName(), "{'Id'}", "{'Id'}"),
                model);
        }

        [Fact]
        public virtual void Does_not_detect_duplicate_table_names_in_different_schema()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "SchemaA";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "SchemaB";

            Validate(model);
        }

        [Fact]
        public virtual void Does_not_detect_duplicate_table_names_for_inherited_entities()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            SetBaseType(entityC, entityA);

            Validate(model);
        }

        [Fact]
        public virtual void Detects_incompatible_primary_keys_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().HasKey(a => a.Id).HasName("Key");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableKeyNameMismatch(
                    "Table", nameof(A), nameof(B), "Key", "{'Id'}", "PK_Table", "{'Id'}"),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_incompatible_primary_key_columns_with_shared_table()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.Id).HasColumnName("Key");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateKeyColumnMismatch(
                    "{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table", "{'Id'}", "{'Key'}"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            GenerateMapping(modelBuilder.Entity<A>().Property(b => b.P0).Metadata);
            GenerateMapping(modelBuilder.Entity<B>().Property(d => d.P0).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "default_int_mapping"), modelBuilder.Model);
        }

        [Fact] // #8973
        public virtual void Detects_derived_principal_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<D>().HasOne<B>().WithOne().IsRequired().HasForeignKey<B>(a => a.Id).HasPrincipalKey<D>(b => b.Id);
            modelBuilder.Entity<D>().HasBaseType<A>().ToTable("Table");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableDerivedPrincipal(
                    "Table", nameof(B), nameof(D), nameof(A)), modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_compatible_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_compatible_shared_table_inverted()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "default_int_mapping", "just_string(2000)"),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Type), nameof(Dog), nameof(Dog.Type), nameof(Cat.Type), nameof(Animal), "just_string(2000)", "default_int_mapping"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasMaxLength(30).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasMaxLength(15).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "just_string(30)", "just_string(15)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_ComputedColumnSql()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComputedColumnSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

            VerifyError(
                RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValue()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValue("1");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "NULL", "1"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValueSql()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_nullability()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Id");

            VerifyError(
                RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                    nameof(Animal), nameof(Animal.Id), nameof(Dog), nameof(Dog.Type), nameof(Animal.Id), nameof(Animal)), modelBuilder.Model);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_count()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<int>("FriendId");
            modelBuilder.Entity<Animal>().Property<string>("Shadow");
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId", "Shadow").HasPrincipalKey(p => new { p.Id, p.Name }).HasConstraintName("FK");
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

        [Fact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_order()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasOne<Person>().WithMany()
                        .HasForeignKey(c => new { c.Name, c.Breed })
                        .HasPrincipalKey(p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK");
                });
            modelBuilder.Entity<Dog>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasOne<Person>().WithMany()
                        .HasForeignKey(d => new { d.Breed, d.Name })
                        .HasPrincipalKey(p => new { p.FavoriteBreed, p.Name })
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

        [Fact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_mapped_to_different_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => new { c.Name, c.Breed }).HasPrincipalKey(p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => new { d.Name, d.Breed }).HasPrincipalKey(p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
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

        [Fact]
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

        [Fact]
        public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_uniqueness()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithOne().HasForeignKey<Dog>(d => d.Name).HasPrincipalKey<Person>(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name");

            VerifyError(
                RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "FK_Animal_Person_Name"),
                modelBuilder.Model);
        }

        [Fact]
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

        [Fact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name").Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("FK_Animal_Person_Name", fk1.Relational().Name);
            Assert.Equal("FK_Animal_Person_Name1", fk2.Relational().Name);
        }

        [Fact]
        public virtual void Passes_for_compatible_duplicate_foreignKey_names_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            IForeignKey fk1 = null;
            IForeignKey fk2 = null;

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk1 = et
                        .HasOne<Person>()
                        .WithMany()
                        .HasForeignKey(c => new { c.Name, c.Breed })
                        .HasPrincipalKey(p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK")
                        .Metadata;
                });
            modelBuilder.Entity<Dog>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk2 = et
                        .HasOne<Customer>()
                        .WithMany()
                        .HasForeignKey(c => new { c.Name, c.Breed })
                        .HasPrincipalKey(p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK")
                        .Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(fk1, fk2);
            Assert.Equal(fk1.Relational().Name, fk2.Relational().Name);
        }

        [Fact]
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

        [Fact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_order()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasIndex(c => new { c.Name, c.Breed }).HasName("IX");
                });
            modelBuilder.Entity<Dog>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasIndex(d => new { d.Breed, d.Name }).HasName("IX");
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

        [Fact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_mapped_to_different_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(c => new { c.Name, c.Breed }).HasName("IX");
            modelBuilder.Entity<Dog>().HasIndex(d => new { d.Name, d.Breed }).HasName("IX");
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

        [Fact]
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

        [Fact]
        public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.Relational().Name);
            Assert.Equal("IX_Animal_Name1", index2.Relational().Name);
        }

        [Fact]
        public virtual void Passes_for_compatible_duplicate_index_names_within_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            IMutableIndex index1 = null;
            IMutableIndex index2 = null;
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    index1 = et.HasIndex(c => c.Breed).HasName("IX_Animal_Breed").Metadata;
                });
            modelBuilder.Entity<Dog>(et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    index2 = et.HasIndex(c => c.Breed).HasName("IX_Animal_Breed").Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(index1, index2);
            Assert.Equal(index1.Relational().Name, index2.Relational().Name);
        }

        [Fact]
        public virtual void Passes_for_non_hierarchical_model()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);

            Validate(model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            entityC.HasBaseType(entityA);

            VerifyError(RelationalStrings.NoDiscriminatorProperty(entityA.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_value_on_base()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            entityC.HasBaseType(entityA);

            var discriminatorProperty = entityA.AddProperty("D", typeof(int));
            entityA.Relational().DiscriminatorProperty = discriminatorProperty;
            entityC.Relational().DiscriminatorValue = 1;

            VerifyError(RelationalStrings.NoDiscriminatorValue(entityA.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_value_on_leaf()
        {
            var model = new Model();
            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            var entityGeneric = model.AddEntityType(typeof(Generic<string>));
            entityGeneric.HasBaseType(entityAbstract);

            var discriminatorProperty = entityAbstract.AddProperty("D", typeof(int));
            entityAbstract.Relational().DiscriminatorProperty = discriminatorProperty;
            entityAbstract.Relational().DiscriminatorValue = 0;

            VerifyError(RelationalStrings.NoDiscriminatorValue(entityGeneric.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_non_string_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<C>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<D>(1);

            var model = modelBuilder.Model;
            VerifyError(RelationalStrings.NoDiscriminatorValue(typeof(C).Name), model);
        }

        [Fact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(1)
                .HasValue<C>(1)
                .HasValue<D>(2);

            var model = modelBuilder.Model;
            VerifyError(RelationalStrings.DuplicateDiscriminatorValue(typeof(C).Name, 1, typeof(A).Name), model);
        }

        [Fact]
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

        [Fact]
        public void Detects_function_with_invalid_return_type_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var methodInfo
                = typeof(DbFunctionMetadataTests.TestMethods)
                    .GetRuntimeMethod(nameof(DbFunctionMetadataTests.TestMethods.MethodD), new Type[] { });

            modelBuilder.HasDbFunction(methodInfo);

            VerifyError(
                RelationalStrings.DbFunctionInvalidReturnType(
                    methodInfo.DisplayName(),
                    typeof(DbFunctionMetadataTests.TestMethods).ShortDisplayName()),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_function_with_invalid_parameter_type_but_translate_callback_does_not_throw()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var methodInfo
                = typeof(DbFunctionMetadataTests.TestMethods)
                    .GetRuntimeMethod(
                        nameof(DbFunctionMetadataTests.TestMethods.MethodF),
                        new[] { typeof(DbFunctionMetadataTests.MyBaseContext) });

            var dbFuncBuilder = modelBuilder.HasDbFunction(methodInfo);

            dbFuncBuilder.HasTranslation(parameters => null);

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_function_with_invalid_parameter_type_but_no_translate_callback_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var methodInfo
                = typeof(DbFunctionMetadataTests.TestMethods)
                    .GetRuntimeMethod(
                        nameof(DbFunctionMetadataTests.TestMethods.MethodF),
                        new[] { typeof(DbFunctionMetadataTests.MyBaseContext) });

            modelBuilder.HasDbFunction(methodInfo);

            VerifyError(
                RelationalStrings.DbFunctionInvalidParameterType(
                    "context", methodInfo.DisplayName(), typeof(DbFunctionMetadataTests.MyBaseContext).ShortDisplayName()),
                modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping]
                = TestServiceFactory.Instance.Create<TestRelationalTypeMapper>().GetMapping(property);

        protected override void SetBaseType(EntityType entityType, EntityType baseEntityType)
        {
            base.SetBaseType(entityType, baseEntityType);

            var discriminatorProperty = baseEntityType.GetOrAddProperty("Discriminator", typeof(string));
            baseEntityType.Relational().DiscriminatorProperty = discriminatorProperty;
            baseEntityType.Relational().DiscriminatorValue = baseEntityType.Name;
            entityType.Relational().DiscriminatorValue = entityType.Name;
        }

        protected class C : A
        {
        }

        protected class Animal
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

        protected override IModelValidator CreateModelValidator()
            => new RelationalModelValidator(
                new ModelValidatorDependencies(
                    new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                        new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake")),
                    new DiagnosticsLogger<DbLoggerCategory.Model>(
                        new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"))),
                new RelationalModelValidatorDependencies(
                    TestServiceFactory.Instance.Create<TestRelationalTypeMapper>()));

        protected virtual ModelBuilder CreateConventionalModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();
    }
}
