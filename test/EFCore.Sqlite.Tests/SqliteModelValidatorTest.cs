// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder(new CoreConventionSetBuilderDependencies(new CoreTypeMapper(new CoreTypeMapperDependencies()))).CreateConventionSet());
            modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name");

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "INTEGER", "TEXT"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().Property(c => c.Type);

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "TEXT", "INTEGER"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
        }

        [Fact]
        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder(new CoreConventionSetBuilderDependencies(new CoreTypeMapper(new CoreTypeMapperDependencies()))).CreateConventionSet());

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "INTEGER"), modelBuilder.Model);
        }

        [Fact]
        public void Detects_schemas()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.Entity<Animal>().ToTable("Animals", "pet");

            VerifyWarning(SqliteStrings.LogSchemaConfigured.GenerateMessage("Animal", "pet"), modelBuilder.Model);
        }

        [Fact]
        public void Detects_sequences()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            modelBuilder.HasSequence("Fibonacci");

            VerifyWarning(SqliteStrings.LogSequenceConfigured.GenerateMessage("Fibonacci"), modelBuilder.Model);
        }

        protected override ModelValidator CreateModelValidator()
            => new SqliteModelValidator(
                new ModelValidatorDependencies(
                    new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                        new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"))),
                new RelationalModelValidatorDependencies(
                    new SqliteTypeMapper(new RelationalTypeMapperDependencies())));
    }
}
