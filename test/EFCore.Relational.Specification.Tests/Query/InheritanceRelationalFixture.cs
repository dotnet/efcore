// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationalFixture : InheritanceFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

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

            modelBuilder.Query<AnimalQuery>().ToView("Animal");
            modelBuilder.Query<KiwiQuery>().HasDiscriminator().HasValue("Kiwi");
            modelBuilder.Query<EagleQuery>().HasDiscriminator().HasValue("Eagle");
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(RelationalEventId.QueryClientEvaluationWarning));
    }
}
