// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPHInheritanceQueryFixture : InheritanceQueryFixtureBase, ITestSqlLoggerFactory
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    // #31378
    public override bool EnableComplexTypes
        => false;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Plant>().HasDiscriminator(p => p.Genus)
            .HasValue<Rose>(PlantGenus.Rose)
            .HasValue<Daisy>(PlantGenus.Daisy)
            .IsComplete(IsDiscriminatorMappingComplete);

        modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

        modelBuilder.Entity<Animal>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);
        modelBuilder.Entity<Animal>().Property(e => e.Species).HasMaxLength(100);

        modelBuilder.Entity<Coke>().Property(e => e.Carbonation).HasColumnName("CokeCO2");
        modelBuilder.Entity<Coke>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
        modelBuilder.Entity<Coke>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");
        modelBuilder.Entity<Lilt>().Property(e => e.Carbonation).HasColumnName("LiltCO2");
        modelBuilder.Entity<Lilt>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
        modelBuilder.Entity<Tea>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");

        modelBuilder.Entity<AnimalQuery>().HasNoKey().ToSqlQuery("SELECT * FROM Animals");
        modelBuilder.Entity<KiwiQuery>().HasDiscriminator().HasValue("Kiwi");
        modelBuilder.Entity<EagleQuery>().HasDiscriminator().HasValue("Eagle");
    }
}
