// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceRelationalFixture : InheritanceFixtureBase
    {
        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plant>().HasDiscriminator(p => p.Genus)
                .HasValue<Rose>(PlantGenus.Rose)
                .HasValue<Daisy>(PlantGenus.Daisy);

            modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

            // #2455
            modelBuilder.Entity<Animal>().Property(e => e.Species).HasColumnType("nvarchar(100)");
            modelBuilder.Entity<Bird>().Property(e => e.EagleId).HasColumnType("nvarchar(100)");
        }
    }
}
