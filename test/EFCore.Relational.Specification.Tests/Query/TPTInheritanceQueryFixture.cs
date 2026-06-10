// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTInheritanceQueryFixture : InheritanceQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "TPTInheritanceTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override bool HasDiscriminator
        => false;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Flower>().ToTable("Flowers");
        modelBuilder.Entity<Rose>().ToTable("Roses");
        modelBuilder.Entity<Daisy>().ToTable("Daisies");
        modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<Bird>().ToTable("Birds");
        modelBuilder.Entity<Kiwi>().ToTable("Kiwi");
        modelBuilder.Entity<Animal>().Property(e => e.Species).HasMaxLength(100);
        modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

        modelBuilder.Entity<Coke>().ToTable("Coke");
        modelBuilder.Entity<Lilt>().ToTable("Lilt");
        modelBuilder.Entity<Tea>().ToTable("Tea");

        modelBuilder.Entity<Coke>().Property(e => e.Carbonation).HasColumnName("CokeCO2");
        modelBuilder.Entity<Coke>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
        modelBuilder.Entity<Coke>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");
        modelBuilder.Entity<Lilt>().Property(e => e.Carbonation).HasColumnName("LiltCO2");
        modelBuilder.Entity<Lilt>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
        modelBuilder.Entity<Tea>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");

        // Keyless entities are mapped to TPH so ignoring them
        modelBuilder.Ignore<AnimalQuery>();
        modelBuilder.Ignore<BirdQuery>();
        modelBuilder.Ignore<KiwiQuery>();
        modelBuilder.Ignore<EagleQuery>();
    }
}
