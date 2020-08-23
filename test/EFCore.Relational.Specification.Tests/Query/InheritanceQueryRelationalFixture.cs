// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceQueryRelationalFixture : InheritanceQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

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
}
