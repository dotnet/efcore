// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class SqliteModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name");
            modelBuilder.Entity<Animal>().Property(d => d.Name).IsRequired().HasColumnName("Name");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "INTEGER", "TEXT"),
                modelBuilder);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            modelBuilder.Entity<Cat>().Property(c => c.Type).IsRequired().HasColumnName("Type");
            modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "TEXT", "INTEGER"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_srid()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasSrid(30);
            modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasSrid(15);

            VerifyError(
                SqliteStrings.DuplicateColumnNameSridMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder);
        }

        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            modelBuilder.Entity<A>().Property(b => b.P0);
            modelBuilder.Entity<B>().Property(d => d.P0);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "INTEGER"), modelBuilder);
        }

        [ConditionalFact]
        public void Detects_schemas()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().ToTable("Animals", "pet").Ignore(a => a.FavoritePerson);

            VerifyWarning(
                SqliteResources.LogSchemaConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Animal", "pet"),
                modelBuilder);
        }

        [ConditionalFact]
        public void Detects_sequences()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.HasSequence("Fibonacci");

            VerifyWarning(
                SqliteResources.LogSequenceConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Fibonacci"),
                modelBuilder);
        }

        protected override TestHelpers TestHelpers
            => SqliteTestHelpers.Instance;
    }
}
