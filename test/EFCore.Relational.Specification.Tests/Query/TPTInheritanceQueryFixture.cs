// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTInheritanceQueryFixture : InheritanceQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Plant>().ToTable("Plants");
            modelBuilder.Entity<Flower>().ToTable("Flowers");
            modelBuilder.Entity<Rose>().ToTable("Roses");
            modelBuilder.Entity<Daisy>().ToTable("Daisies");
            modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Animal>().ToTable("Plants");
            modelBuilder.Entity<Bird>().ToTable("Roses");
            modelBuilder.Entity<Eagle>().ToTable("Daisies");
            modelBuilder.Entity<Kiwi>().ToTable("Daisies");
            modelBuilder.Entity<Animal>().Property(e => e.Species).HasMaxLength(100);
            modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

            modelBuilder.Entity<Drink>().ToTable("Drinks");
            modelBuilder.Entity<Coke>().ToTable("Coke");
            modelBuilder.Entity<Lilt>().ToTable("Lilt");
            modelBuilder.Entity<Tea>().ToTable("Tea");

            modelBuilder.Entity<Coke>().Property(e => e.Carbonation).HasColumnName("CokeCO2");
            modelBuilder.Entity<Coke>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
            modelBuilder.Entity<Coke>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");
            modelBuilder.Entity<Lilt>().Property(e => e.Carbonation).HasColumnName("LiltCO2");
            modelBuilder.Entity<Lilt>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
            modelBuilder.Entity<Tea>().Property(e => e.CaffeineGrams).HasColumnName("CaffeineGrams");

            // Keyless entities are mapped to TPH
            modelBuilder.Entity<AnimalQuery>().HasNoKey().ToQuery(
                () => context.Set<AnimalQuery>().FromSqlRaw("SELECT * FROM Animal"));
            modelBuilder.Entity<KiwiQuery>().HasDiscriminator().HasValue("Kiwi");
            modelBuilder.Entity<EagleQuery>().HasDiscriminator().HasValue("Eagle");
        }
    }
}
