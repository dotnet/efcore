// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests;
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
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>().Property(b => b.Id).ForSqlServerHasColumnName("Name");

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "int", "nvarchar(max)"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().Property(c => c.Type);

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "nvarchar(max)", "int"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasMaxLength(30);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).HasMaxLength(15);

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "nvarchar(30)", "nvarchar(15)"), modelBuilder.Model);
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
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_unicode()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).IsUnicode(false);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).IsUnicode();

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "varchar(max)", "nvarchar(max)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_default_decimal_mapping()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>().Property<decimal>("Price");

            VerifyWarning(SqlServerStrings.DefaultDecimalTypeColumn("Price", nameof(Animal)), modelBuilder.Model);
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

    public class TestSqlServerAnnotationProvider : TestAnnotationProvider
    {
        public override IRelationalPropertyAnnotations For(IProperty property) => new SqlServerPropertyAnnotations(property);
    }
}
