// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Inheritance;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationalFixture<TTestStore> : InheritanceFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plant>().HasDiscriminator(p => p.Genus)
                .HasValue<Rose>(PlantGenus.Rose)
                .HasValue<Daisy>(PlantGenus.Daisy);

            modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

            modelBuilder.Entity<Animal>().Property(e => e.Species).HasMaxLength(100);

            modelBuilder.Entity<Coke>().Property(e => e.Carbination).HasColumnName("CokeCO2");
            modelBuilder.Entity<Coke>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
            modelBuilder.Entity<Coke>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");
            modelBuilder.Entity<Lilt>().Property(e => e.Carbination).HasColumnName("LiltCO2");
            modelBuilder.Entity<Lilt>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
            modelBuilder.Entity<Tea>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");
        }
    }
}
