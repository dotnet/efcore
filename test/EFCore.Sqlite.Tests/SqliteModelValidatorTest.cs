// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqliteModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "INTEGER", "TEXT"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "TEXT", "INTEGER"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
        }

        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            GenerateMapping(modelBuilder.Entity<A>().Property(b => b.P0).Metadata);
            GenerateMapping(modelBuilder.Entity<B>().Property(d => d.P0).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "INTEGER"), modelBuilder.Model);
        }

        [Fact]
        public void Detects_schemas()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().ToTable("Animals", "pet");

            VerifyWarning(SqliteStrings.LogSchemaConfigured.GenerateMessage("Animal", "pet"), modelBuilder.Model);
        }

        [Fact]
        public void Detects_sequences()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.HasSequence("Fibonacci");

            VerifyWarning(SqliteStrings.LogSequenceConfigured.GenerateMessage("Fibonacci"), modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping]
                = TestServiceFactory.Instance.Create<SqliteTypeMapper>().GetMapping(property);

        protected override IModelValidator CreateModelValidator()
            => new SqliteModelValidator(
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
                    TestServiceFactory.Instance.Create<SqliteTypeMapper>()));

        protected override ModelBuilder CreateConventionalModelBuilder()
            => SqliteTestHelpers.Instance.CreateConventionBuilder();
    }
}
