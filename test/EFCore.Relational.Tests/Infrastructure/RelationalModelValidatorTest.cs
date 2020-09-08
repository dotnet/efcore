// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

            Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
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

            VerifyWarning(
                RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                    .GenerateMessage("ImBool", "E"), model);
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

            VerifyWarning(
                RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                    .GenerateMessage("ImBool", "E"), model);
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

            VerifyWarning(
                RelationalResources.LogKeyHasDefaultValue(
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

            VerifyWarning(
                RelationalResources.LogKeyHasDefaultValue(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("P0", "A"),
                model);
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
        public virtual void Detects_duplicate_view_names_without_identifying_relationship()
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

            entityA.SetViewName("Table");
            entityA.SetViewSchema("Schema");
            entityB.SetViewName("Table");
            entityB.SetViewSchema("Schema");

            VerifyError(
                RelationalStrings.IncompatibleViewNoRelationship(
                    "Schema.Table", entityB.DisplayName(), entityA.DisplayName()),
                model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_view_names_when_no_key()
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

            entityA.SetViewName("Table");
            entityB.SetViewName("Table");

            VerifyError(
                RelationalStrings.IncompatibleViewNoRelationship(
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

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
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

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
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

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().ToTable("Table").HasComment("My comment");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_primary_key_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().Property(a => a.Id).ValueGeneratedNever().HasColumnName("Key");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.Id).ValueGeneratedNever().HasColumnName(nameof(B.Id));
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateKeyColumnMismatch(
                    "{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table", "{'Id'}", "{'Key'}"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_on_not_configured_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<B>().ToTable("Table");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("default_int_mapping");
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

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();
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

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<A>(b => b.Id).IsRequired();
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                CoreStrings.IdentifyingRelationshipCycle(nameof(A)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

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
        public virtual void Passes_for_compatible_excluded_shared_table_inverted()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<B>().ToTable("Table", t => t.ExcludeFromMigrations());

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_excluded_shared_table_owned()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<B>().OwnsOne(b => b.A);
            modelBuilder.Entity<B>().ToTable("Table", t => t.ExcludeFromMigrations());

            var model = Validate(modelBuilder.Model);

            var b = model.FindEntityType(typeof(B));
            Assert.Equal("Table", b.GetTableName());
            Assert.True(b.IsTableExcludedFromMigrations());
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_excluded_table_derived()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<C>();

            var model = Validate(modelBuilder.Model);

            var c = model.FindEntityType(typeof(C));
            Assert.Equal("Table", c.GetTableName());
            Assert.True(c.IsTableExcludedFromMigrations());
        }

        [ConditionalFact]
        public virtual void Detect_partially_excluded_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.IncompatibleTableExcludedMismatch(
                    nameof(Table), nameof(A), nameof(B)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").IsRequired().Metadata);

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

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").IsRequired().Metadata);
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
                RelationalStrings.DuplicateColumnNameMaxLengthMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "30",
                    "15"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsUnicode()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsUnicode().Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameUnicodenessMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsFixedLength()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsFixedLength().Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsConcurrencyToken()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsConcurrencyToken().Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameConcurrencyTokenMismatch(
                    nameof(Cat), nameof(Cat.Breed),
                    nameof(Dog), nameof(Dog.Breed),
                    nameof(Cat.Breed), nameof(Animal)),
                modelBuilder.Model);
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
            modelBuilder.Entity<Dog>().Property<int?>("OtherId").HasColumnName("Id");

            VerifyError(
                RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                    nameof(Animal), nameof(Animal.Id), nameof(Dog), "OtherId", nameof(Animal.Id), nameof(Animal)),
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
                    eb.Property<string>("Selected").IsRequired().HasDefaultValue("false").HasConversion<bool>();
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_shared_columns()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property(a => a.Id).HasMaxLength(20).HasPrecision(15, 10).IsUnicode();
            modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson);
            modelBuilder.Entity<Dog>();

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
                p => new { p.Id, p.Name }).HasConstraintName("FK");
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
                            c => new { c.Name, c.Breed })
                        .HasPrincipalKey(
                            p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK");
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasOne<Person>().WithMany()
                        .HasForeignKey(
                            d => new { d.Breed, d.Name })
                        .HasPrincipalKey(
                            p => new { p.FavoriteBreed, p.Name })
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
                c => new { c.Name, c.Breed }).HasPrincipalKey(
                p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(
                d => new { d.Name, d.Breed }).HasPrincipalKey(
                p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
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
            Assert.NotEqual(index1.GetDatabaseName(), index2.GetDatabaseName());
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
            Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
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
            Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
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
                            c => new { c.Name, c.Breed })
                        .HasPrincipalKey(
                            p => new { p.Name, p.FavoriteBreed })
                        .Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk2 = et
                        .HasOne(a => (Employee)a.FavoritePerson)
                        .WithMany()
                        .HasForeignKey(
                            c => new { c.Name, c.Breed })
                        .HasPrincipalKey(
                            p => new { p.Name, p.FavoriteBreed })
                        .Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(fk1, fk2);
            Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
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
                            c => new { c.Name, c.Breed })
                        .HasPrincipalKey(
                            p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK")
                        .Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    fk2 = et
                        .HasOne<Employee>()
                        .WithMany()
                        .HasForeignKey(
                            c => new { c.Name, c.Breed })
                        .HasPrincipalKey(
                            p => new { p.Name, p.FavoriteBreed })
                        .HasConstraintName("FK")
                        .Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(fk1, fk2);
            Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

            var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
            var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_count()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<int>("Shadow");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Cat.Name), "Shadow").HasDatabaseName("IX");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX");

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
                        c => new { c.Name, c.Breed }).HasDatabaseName("IX");
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    et.HasIndex(
                        d => new { d.Breed, d.Name }).HasDatabaseName("IX");
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
                c => new { c.Name, c.Breed }).HasDatabaseName("IX");
            modelBuilder.Entity<Dog>().HasIndex(
                d => new { d.Name, d.Breed }).HasDatabaseName("IX");
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
            modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasDatabaseName("IX_Animal_Name");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).HasDatabaseName("IX_Animal_Name");

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
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasDatabaseName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.GetDatabaseName());
            Assert.Equal("IX_Animal_Name1", index2.GetDatabaseName());
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
                    index1 = et.HasIndex(c => c.Breed, "IX_Animal_Breed").Metadata;
                });
            modelBuilder.Entity<Dog>(
                et =>
                {
                    et.Property(c => c.Breed).HasColumnName("Breed");
                    index2 = et.HasIndex(c => c.Breed, "IX_Animal_Breed").Metadata;
                });

            Validate(modelBuilder.Model);

            Assert.NotSame(index1, index2);
            Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
        }

        [ConditionalFact]
        public virtual void Detects_missing_concurrency_token_on_the_base_type_without_convention()
        {
            var modelBuilder = CreateModelBuilderWithoutConvention<TableSharingConcurrencyTokenConvention>();
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
        public virtual void Detects_missing_concurrency_token_on_the_sharing_type_without_convention()
        {
            var modelBuilder = CreateModelBuilderWithoutConvention<TableSharingConcurrencyTokenConvention>();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal));
            modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            VerifyError(
                RelationalStrings.MissingConcurrencyColumn(nameof(Person), "Version", nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_with_missing_concurrency_token_property_on_the_base_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal))
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Cat>()
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            var model = Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_with_missing_concurrency_token_property_on_the_base_type_when_derived_is_sharing()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal))
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            modelBuilder.Entity<Animal>().Ignore(p => p.FavoritePerson);
            modelBuilder.Entity<Cat>().HasOne<Person>().WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Cat>()
                .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            var model = Validate(modelBuilder.Model);

            var animalType = model.FindEntityType(typeof(Animal));
            Assert.Empty(animalType.GetProperties().Where(p => p.IsConcurrencyToken));
        }

        [ConditionalFact]
        public virtual void Passes_with_missing_concurrency_token_property_on_the_sharing_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal));
            modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_explicitly_mapped_concurrency_tokens_with_table_sharing()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Person>().ToTable(nameof(Animal))
                .Property<byte[]>("Version").IsRowVersion();
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
            modelBuilder.Entity<Animal>()
                .Property<byte[]>("Version").IsRowVersion();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_missing_concurrency_token_on_owner()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().OwnsOne(
                a => a.FavoritePerson,
                pb => pb.Property<byte[]>("Version").IsRowVersion().HasColumnName("Version"));
            modelBuilder.Entity<Dog>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_explicitly_mapped_concurrency_tokens_with_owned()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion();
            modelBuilder.Entity<Cat>().OwnsOne(
                a => a.FavoritePerson,
                pb => pb.Property<byte[]>("Version").IsRowVersion());
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
        public virtual void Passes_for_missing_discriminator_value_for_abstract_class()
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
        public virtual void Passes_for_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_unconfigured_entity_type_in_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().ToTable("Cat");
            modelBuilder.Entity<Dog>();

            VerifyError(
                RelationalStrings.NonTPHTableClash(nameof(Dog), nameof(Animal), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_clashing_entity_types_in_view_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat");
            modelBuilder.Entity<Dog>().ToTable("Dog").ToView("Cat");

            VerifyError(
                RelationalStrings.NonTPHViewClash(nameof(Dog), nameof(Cat), "Cat"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_table_and_view_TPT_mismatch()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().ToTable("Animal").ToView("Animal");
            modelBuilder.Entity<Cat>().ToTable("Animal").ToView("Cat");

            VerifyError(
                RelationalStrings.NonTPHTableClash(nameof(Cat), nameof(Animal), "Animal"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_TPT_with_discriminator()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().HasDiscriminator<int>("Discriminator");
            modelBuilder.Entity<Cat>().ToTable("Cat");

            VerifyError(
                RelationalStrings.TPHTableMismatch(nameof(Cat), nameof(Cat), nameof(Animal), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_view_TPT_with_discriminator()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().ToView("Animal").HasDiscriminator<int>("Discriminator");
            modelBuilder.Entity<Cat>().ToView("Cat");

            VerifyError(
                RelationalStrings.TPHViewMismatch(nameof(Cat), nameof(Cat), nameof(Animal), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_table_sharing_with_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>()
                .Ignore(a => a.FavoritePerson);

            modelBuilder.Entity<Cat>(
                x =>
                {
                    x.ToTable("Cat");
                    x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Person>(c => c.Id);
                });

            modelBuilder.Entity<Person>().ToTable("Cat");

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_linking_relationship_on_derived_type_in_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>()
                .Ignore(a => a.FavoritePerson);

            modelBuilder.Entity<Cat>(
                x =>
                {
                    x.ToTable("Cat");
                    x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
                });

            modelBuilder.Entity<Person>().ToTable("Cat");

            VerifyError(
                RelationalStrings.IncompatibleTableDerivedRelationship(
                    "Cat", "Cat", "Person"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_linking_relationship_on_derived_type_in_TPT_views()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>()
                .Ignore(a => a.FavoritePerson)
                .ToView("Animal");

            modelBuilder.Entity<Cat>(
                x =>
                {
                    x.ToView("Cat");
                    x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
                });

            modelBuilder.Entity<Person>().ToView("Cat");

            VerifyError(
                RelationalStrings.IncompatibleViewDerivedRelationship(
                    "Cat", "Cat", "Person"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_unmapped_foreign_keys_in_TPT()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>()
                .Ignore(a => a.FavoritePerson)
                .Property<int>("FavoritePersonId");
            modelBuilder.Entity<Cat>().ToTable("Cat")
                .HasOne<Person>().WithMany()
                .HasForeignKey("FavoritePersonId");

            var definition = RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(definition.GenerateMessage(l => l.Log(
                        definition.Level,
                        definition.EventId,
                        definition.MessageFormat,
                        "{'FavoritePersonId'}", nameof(Cat), nameof(Person), "{'FavoritePersonId'}", nameof(Cat), "{'Id'}", nameof(Person))),
                modelBuilder.Model,
                LogLevel.Error);
        }

        [ConditionalFact]
        public virtual void Passes_for_valid_table_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
            modelBuilder.Entity<Dog>().ToTable("Dog");
            property.HasColumnName("DogName", StoreObjectIdentifier.Table("Dog", null));

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_table_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
            property.HasColumnName("DogName", StoreObjectIdentifier.Table("Dog", null));

            VerifyError(
                RelationalStrings.TableOverrideMismatch("Animal.Name", "Dog"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_valid_view_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().ToView("Animal").Property(a => a.Name).GetInfrastructure();
            modelBuilder.Entity<Dog>().ToView("Dog");
            property.HasColumnName("DogName", StoreObjectIdentifier.View("Dog", null));

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_view_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
            property.HasColumnName("DogName", StoreObjectIdentifier.View("Dog", null));

            VerifyError(
                RelationalStrings.ViewOverrideMismatch("Animal.Name", "Dog"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_sql_query_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
            property.HasColumnName("DogName", StoreObjectIdentifier.SqlQuery("Dog"));

            VerifyError(
                RelationalStrings.SqlQueryOverrideMismatch("Animal.Name", "Dog"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_function_overrides()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
            property.HasColumnName("DogName", StoreObjectIdentifier.DbFunction("Dog"));

            VerifyError(
                RelationalStrings.FunctionOverrideMismatch("Animal.Name", "Dog"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_function_with_invalid_return_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.HasDbFunction(TestMethods.MethodCMi);

            VerifyError(
                RelationalStrings.DbFunctionInvalidReturnType(
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodC()",
                    typeof(TestMethods).ShortDisplayName()),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_function_with_unmapped_return_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();

            var methodInfo
                = typeof(TestMethods)
                    .GetRuntimeMethod(nameof(TestMethods.MethodA), Array.Empty<Type>());

            modelBuilder.HasDbFunction(methodInfo);

            VerifyError(
                RelationalStrings.DbFunctionInvalidReturnEntityType(
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                    typeof(IQueryable<TestMethods>).ShortDisplayName(),
                    typeof(TestMethods).ShortDisplayName()),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_function_with_invalid_parameter_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.HasDbFunction(TestMethods.MethodDMi);

            VerifyError(
                RelationalStrings.DbFunctionInvalidParameterType(
                    "methods",
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodD(Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods)",
                    typeof(TestMethods).ShortDisplayName()),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Passes_for_valid_entity_type_mapped_to_function()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var methodInfo
                = typeof(TestMethods)
                    .GetRuntimeMethod(nameof(TestMethods.MethodA), Array.Empty<Type>());

            var function = modelBuilder.HasDbFunction(methodInfo).Metadata;

            modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction(function.ModelName);

            var model = Validate(modelBuilder.Model);

            Assert.Single(model.GetEntityTypes());
            Assert.Single(model.GetDbFunctions());
        }

        [ConditionalFact]
        public void Detects_entity_type_mapped_to_non_existent_function()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction("NonExistent");

            modelBuilder.Model.RemoveDbFunction("NonExistent");

            VerifyError(
                RelationalStrings.MappedFunctionNotFound(nameof(TestMethods), "NonExistent"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_entity_type_mapped_to_a_scalar_function()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var function = modelBuilder.HasDbFunction(TestMethods.MethodEMi).Metadata;

            modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction(function.ModelName);

            VerifyError(
                RelationalStrings.InvalidMappedFunctionUnmatchedReturn(
                    nameof(TestMethods),
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodE()",
                    "int",
                    nameof(TestMethods)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_entity_type_mapped_to_a_different_type()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var function = modelBuilder.HasDbFunction(TestMethods.MethodAMi).Metadata;

            modelBuilder.Entity<Animal>().HasNoKey().ToFunction(function.ModelName);
            modelBuilder.Entity<TestMethods>().HasNoKey();

            VerifyError(
                RelationalStrings.InvalidMappedFunctionUnmatchedReturn(
                    nameof(Animal),
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                    typeof(IQueryable<TestMethods>).ShortDisplayName(),
                    nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_entity_type_mapped_to_a_function_with_parameters()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var function = modelBuilder.HasDbFunction(TestMethods.MethodBMi).Metadata;

            modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction(function.ModelName);

            VerifyError(
                RelationalStrings.InvalidMappedFunctionWithParameters(
                    nameof(TestMethods),
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodB(System.Int32)",
                    "{'id'}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_multiple_entity_types_mapped_to_the_same_function()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var function = modelBuilder.HasDbFunction(TestMethods.MethodAMi).Metadata;

            modelBuilder.Entity<DerivedTestMethods>(
                db =>
                {
                    db.HasBaseType((string)null);
                    db.OwnsOne(d => d.SomeTestMethods).ToFunction(function.ModelName);
                    db.OwnsOne(d => d.OtherTestMethods).ToFunction(function.ModelName);
                });

            VerifyError(
                RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                    nameof(TestMethods)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_derived_entity_type_mapped_to_a_function()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var function = modelBuilder.HasDbFunction(TestMethods.MethodAMi).Metadata;

            modelBuilder.Entity<BaseTestMethods>().HasNoKey();
            modelBuilder.Entity<TestMethods>().ToFunction(function.ModelName);

            VerifyError(
                RelationalStrings.InvalidMappedFunctionDerivedType(
                    nameof(TestMethods),
                    "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                    nameof(BaseTestMethods)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Passes_for_unnamed_index_with_all_properties_not_mapped_to_any_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable(null);
            modelBuilder.Entity<Animal>().HasIndex(nameof(Animal.Id), nameof(Animal.Name));

            var definition = RelationalResources
                .LogUnnamedIndexAllPropertiesNotToMappedToAnyTable(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    nameof(Animal),
                    "{'Id', 'Name'}"),
                modelBuilder.Model,
                LogLevel.Information);
        }

        [ConditionalFact]
        public void Passes_for_named_index_with_all_properties_not_mapped_to_any_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable(null);
            modelBuilder.Entity<Animal>()
                .HasIndex(
                    new[] { nameof(Animal.Id), nameof(Animal.Name) },
                    "IX_AllPropertiesNotMapped");

            var definition = RelationalResources
                .LogNamedIndexAllPropertiesNotToMappedToAnyTable(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    "IX_AllPropertiesNotMapped",
                    nameof(Animal),
                    "{'Id', 'Name'}"),
                modelBuilder.Model,
                LogLevel.Information);
        }

        [ConditionalFact]
        public void Detects_mix_of_index_property_mapped_and_not_mapped_to_any_table_unmapped_first()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable(null);
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Name), nameof(Cat.Identity));

            var definition = RelationalResources
                .LogUnnamedIndexPropertiesBothMappedAndNotMappedToTable(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    nameof(Cat),
                    "{'Name', 'Identity'}",
                    "Name"),
                modelBuilder.Model,
                LogLevel.Error);
        }

        [ConditionalFact]
        public void Detects_mix_of_index_property_mapped_and_not_mapped_to_any_table_mapped_first()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable(null);
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>()
                .HasIndex(
                    new[] { nameof(Cat.Identity), nameof(Animal.Name) },
                    "IX_MixOfMappedAndUnmappedProperties");

            var definition = RelationalResources
                .LogNamedIndexPropertiesBothMappedAndNotMappedToTable(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    "IX_MixOfMappedAndUnmappedProperties",
                    nameof(Cat),
                    "{'Identity', 'Name'}",
                    "Name"),
                modelBuilder.Model,
                LogLevel.Error);
        }

        [ConditionalFact]
        public void Passes_for_index_properties_mapped_to_same_table_in_TPT_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable("Animals");
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Id), nameof(Cat.Identity));

            Validate(modelBuilder.Model);

            Assert.Empty(
                LoggerFactory.Log
                    .Where(l => l.Level != LogLevel.Trace && l.Level != LogLevel.Debug));
        }

        [ConditionalFact]
        public void Detects_unnamed_index_properties_mapped_to_different_tables_in_TPT_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable("Animals");
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Name), nameof(Cat.Identity));

            var definition = RelationalResources
                .LogUnnamedIndexPropertiesMappedToNonOverlappingTables(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    nameof(Cat),
                    "{'Name', 'Identity'}",
                    nameof(Animal.Name),
                    "{'Animals'}",
                    nameof(Cat.Identity),
                    "{'Cats'}"),
                modelBuilder.Model,
                LogLevel.Error);
        }

        [ConditionalFact]
        public void Detects_named_index_properties_mapped_to_different_tables_in_TPT_hierarchy()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().ToTable("Animals");
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>()
                .HasIndex(
                    new[] { nameof(Animal.Name), nameof(Cat.Identity) },
                    "IX_MappedToDifferentTables");

            var definition = RelationalResources
                .LogNamedIndexPropertiesMappedToNonOverlappingTables(
                    new TestLogger<TestRelationalLoggingDefinitions>());
            VerifyWarning(
                definition.GenerateMessage(
                    l => l.Log(
                        definition.Level,
                        definition.EventId,
                        definition.MessageFormat,
                        "IX_MappedToDifferentTables",
                        nameof(Cat),
                        "{'Name', 'Identity'}",
                        nameof(Animal.Name),
                        "{'Animals'}",
                        nameof(Cat.Identity),
                        "{'Cats'}")),
                modelBuilder.Model,
                LogLevel.Error);
        }

        [ConditionalFact]
        public virtual void Non_TPH_as_a_result_of_DbFunction_throws()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<A>().ToTable("A").HasNoDiscriminator();
            modelBuilder.Entity<C>().ToTable("C");

            modelBuilder.HasDbFunction(TestMethods.MethodFMi);

            VerifyError(
                RelationalStrings.TableValuedFunctionNonTPH(
                    TestMethods.MethodFMi.DeclaringType.FullName + "." + TestMethods.MethodFMi.Name + "()", "C"), modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property.SetTypeMapping(
                new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
                    .FindMapping(property));

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

        protected class Employee : Person
        {
        }

        public class TestDecimalToLongConverter : ValueConverter<decimal, long>
        {
            private static readonly Expression<Func<decimal, long>> convertToProviderExpression = d => (long)(d * 100);
            private static readonly Expression<Func<long, decimal>> convertFromProviderExpression = l => l / 100m;

            public TestDecimalToLongConverter()
                : base(convertToProviderExpression, convertFromProviderExpression)
            {
            }
        }

        public class TestDecimalToDecimalConverter : ValueConverter<decimal, decimal>
        {
            private static readonly Expression<Func<decimal, decimal>> convertToProviderExpression = d => d * 100m;
            private static readonly Expression<Func<decimal, decimal>> convertFromProviderExpression = l => l / 100m;

            public TestDecimalToDecimalConverter()
                : base(convertToProviderExpression, convertFromProviderExpression)
            {
            }
        }

        private class BaseTestMethods
        {
        }

        private class DerivedTestMethods : TestMethods
        {
            public int Id { get; set; }
            public TestMethods SomeTestMethods { get; set; }
            public TestMethods OtherTestMethods { get; set; }
        }

        private class TestMethods : BaseTestMethods
        {
            public static readonly MethodInfo MethodAMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodA));
            public static readonly MethodInfo MethodBMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodB));
            public static readonly MethodInfo MethodCMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodC));
            public static readonly MethodInfo MethodDMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodD));
            public static readonly MethodInfo MethodEMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodE));
            public static readonly MethodInfo MethodFMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodF));

            public static IQueryable<TestMethods> MethodA()
                => throw new NotImplementedException();

            public static IQueryable<TestMethods> MethodB(int id)
                => throw new NotImplementedException();

            public static TestMethods MethodC()
                => throw new NotImplementedException();

            public static int MethodD(TestMethods methods)
                => throw new NotImplementedException();

            public static int MethodE()
                => throw new NotImplementedException();

            public static IQueryable<C> MethodF()
                => throw new NotImplementedException();
        }

        protected virtual ModelBuilder CreateModelBuilderWithoutConvention<T>(
            bool sensitiveDataLoggingEnabled = false)
        {
            var conventionSet = TestHelpers.CreateConventionalConventionSet(
                CreateModelLogger(sensitiveDataLoggingEnabled), CreateValidationLogger(sensitiveDataLoggingEnabled));

            ConventionSet.Remove(
                conventionSet.ModelFinalizingConventions,
                typeof(T));

            return new ModelBuilder(conventionSet);
        }

        protected override TestHelpers TestHelpers
            => RelationalTestHelpers.Instance;
    }
}
