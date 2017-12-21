﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "int", "nvarchar(max)"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(c => c.Type).HasColumnName("Type").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "nvarchar(max)", "int"), modelBuilder.Model);
        }

        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            GenerateMapping(modelBuilder.Entity<A>().Property(b => b.P0).Metadata);
            GenerateMapping(modelBuilder.Entity<B>().Property(d => d.P0).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "int"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasMaxLength(30).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasMaxLength(15).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "nvarchar(30)", "nvarchar(15)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_unicode()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsUnicode(false).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").IsUnicode().Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "varchar(max)", "nvarchar(max)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_value_generation_strategy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(cb =>
                {
                    cb.Property(c => c.Identity).UseSqlServerIdentityColumn();
                    cb.Property(c => c.Identity).HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(db =>
                {
                    db.Property(d => d.Identity).ValueGeneratedNever();
                    db.Property(c => c.Identity).HasColumnName(nameof(Dog.Identity));
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                    nameof(Cat), nameof(Cat.Identity), nameof(Dog), nameof(Dog.Identity), nameof(Cat.Identity), nameof(Animal)), modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
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
        public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.Relational().Name);
            Assert.Equal("IX_Animal_Name", index1.SqlServer().Name);
            Assert.Equal("IX_Animal_Name1", index2.Relational().Name);
            Assert.Equal("IX_Animal_Name1", index2.SqlServer().Name);
        }

        [Fact]
        public virtual void Detects_incompatible_momory_optimized_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().ToTable("Table").ForSqlServerIsMemoryOptimized();
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch("Table", nameof(A), nameof(B), nameof(A), nameof(B)),
                modelBuilder.Model);
        }

        [Fact]
        public virtual void Throws_for_unsupported_data_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Cheese>().Property(e => e.Name).HasColumnType("nvarchar");

            Assert.Equal(
                SqlServerStrings.UnqualifiedDataType("nvarchar"),
                Assert.Throws<ArgumentException>(() => Validate(modelBuilder.Model)).Message);
        }

        [Fact]
        public virtual void Detects_default_decimal_mapping()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<decimal>("Price");

            VerifyWarning(SqlServerStrings.LogDefaultDecimalTypeColumn.GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_default_nullable_decimal_mapping()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<decimal?>("Price");

            VerifyWarning(SqlServerStrings.LogDefaultDecimalTypeColumn.GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [Fact]
        public void Detects_byte_identity_column()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Dog>().Property<byte>("Bite").UseSqlServerIdentityColumn();

            VerifyWarning(SqlServerStrings.LogByteIdentityColumn.GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Detects_nullable_byte_identity_column()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Dog>().Property<byte?>("Bite").UseSqlServerIdentityColumn();

            VerifyWarning(SqlServerStrings.LogByteIdentityColumn.GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Passes_for_non_key_identity()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type).UseSqlServerIdentityColumn();

            Validate(modelBuilder.Model);
        }

        [Fact]
        public void Throws_for_multiple_identity_properties()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type).UseSqlServerIdentityColumn();
            modelBuilder.Entity<Dog>().Property<int?>("Tag").UseSqlServerIdentityColumn();

            VerifyError(SqlServerStrings.MultipleIdentityColumns("'Dog.Tag', 'Dog.Type'", nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Throws_for_non_key_SequenceHiLo()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ForSqlServerUseSequenceHiLo();

            VerifyError(SqlServerStrings.NonKeyValueGeneration(nameof(Dog.Type), nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Passes_for_non_key_identity_on_model()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.ForSqlServerUseIdentityColumns();
            modelBuilder.Entity<Dog>().Property(c => c.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [Fact]
        public void Passes_for_non_key_SequenceHiLo_on_model()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.ForSqlServerUseSequenceHiLo();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping] = TestServiceFactory.Instance.Create<SqlServerTypeMapper>().GetMapping(property);

        private class Cheese
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        protected override IModelValidator CreateModelValidator()
            => new SqlServerModelValidator(
                new ModelValidatorDependencies(
                    new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                        new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake")),
                    new DiagnosticsLogger<DbLoggerCategory.Model>(
                        new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"))),
                new RelationalModelValidatorDependencies(
                    TestServiceFactory.Instance.Create<SqlServerTypeMapper>()));

        protected override ModelBuilder CreateConventionalModelBuilder()
            => SqlServerTestHelpers.Instance.CreateConventionBuilder();
    }
}
