// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class SqliteModelValidatorTest : RelationalModelValidatorTest
{
    public override void Detects_duplicate_column_names()
    {
        var modelBuilder = CreateConventionModelBuilder();

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
        var modelBuilder = CreateConventionModelBuilder();
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
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasSrid(30);
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasSrid(15);

        VerifyError(
            SqliteStrings.DuplicateColumnNameSridMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_schemas()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable("Animals", "pet").Ignore(a => a.FavoritePerson);

        VerifyWarning(
            SqliteResources.LogSchemaConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Animal", "pet"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_sequences()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.HasSequence("Fibonacci");

        VerifyWarning(
            SqliteResources.LogSequenceConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Fibonacci"),
            modelBuilder);
    }

    public override void Store_generated_in_composite_key()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<CarbonComposite>(
            b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.Property(e => e.Id2).ValueGeneratedOnAdd();
            });

        VerifyWarning(
            SqliteResources.LogCompositeKeyWithValueGeneration(
                new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage(nameof(CarbonComposite), "{'Id1', 'Id2'}"),
            modelBuilder);
    }

    protected override TestHelpers TestHelpers
        => SqliteTestHelpers.Instance;
}
