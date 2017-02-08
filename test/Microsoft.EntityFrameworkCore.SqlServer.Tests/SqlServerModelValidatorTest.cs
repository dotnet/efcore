// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class SqlServerModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>().Property(b => b.Id).ForSqlServerHasColumnName("Name");

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "int", "nvarchar(max)"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().Property(c => c.Type);

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "nvarchar(max)", "int"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasMaxLength(30);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).HasMaxLength(15);

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "nvarchar(30)", "nvarchar(15)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_unicode()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).IsUnicode(false);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).IsUnicode();

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "varchar(max)", "nvarchar(max)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).ForSqlServerHasConstraintName("FK_Animal_Person_Name").Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("FK_Animal_Person_Name", fk1.Relational().Name);
            Assert.Equal("FK_Animal_Person_Name", fk1.SqlServer().Name);
            Assert.Equal("FK_Animal_Person_Name", fk2.Relational().Name);
            Assert.Equal("FK_Animal_Person_Name0", fk2.SqlServer().Name);
        }

        [Fact]
        public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().ForSqlServerHasName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.Relational().Name);
            Assert.Equal("IX_Animal_Name", index1.SqlServer().Name);
            Assert.Equal("IX_Animal_Name", index2.Relational().Name);
            Assert.Equal("IX_Animal_Name0", index2.SqlServer().Name);
        }

        [Fact]
        public virtual void Throws_for_unsupported_data_types()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Cheese>().Property(e => e.Name).HasColumnType("nvarchar");

            Assert.Equal(
                SqlServerStrings.UnqualifiedDataType("nvarchar"),
                Assert.Throws<ArgumentException>(() => Validate(modelBuilder.Model)).Message);
        }

        [Fact]
        public virtual void Detects_default_decimal_mapping()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>().Property<decimal>("Price");

            VerifyWarning(SqlServerStrings.DefaultDecimalTypeColumn("Price", nameof(Animal)), modelBuilder.Model);
        }

        [Fact]
        public void Throws_for_non_key_identity()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Dog>().Property(c => c.Type).UseSqlServerIdentityColumn();

            VerifyError(SqlServerStrings.NonKeyValueGeneration(nameof(Dog.Type), nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Throws_for_non_key_SequenceHiLo()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Dog>().Property(c => c.Type).ForSqlServerUseSequenceHiLo();

            VerifyError(SqlServerStrings.NonKeyValueGeneration(nameof(Dog.Type), nameof(Dog)), modelBuilder.Model);
        }

        [Fact]
        public void Passes_for_non_key_identity_on_model()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.ForSqlServerUseIdentityColumns();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [Fact]
        public void Passes_for_non_key_SequenceHiLo_on_model()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.ForSqlServerUseSequenceHiLo();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        private class Cheese
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        protected override ModelValidator CreateModelValidator()
            => new SqlServerModelValidator(
                new Logger<RelationalModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(RelationalModelValidator).FullName)),
                new TestSqlServerAnnotationProvider(),
                new SqlServerTypeMapper());
    }
}
