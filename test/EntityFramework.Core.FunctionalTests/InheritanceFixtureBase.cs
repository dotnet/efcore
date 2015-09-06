// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceFixtureBase
    {
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kiwi>().BaseType<Bird>();
            modelBuilder.Entity<Eagle>().BaseType<Bird>();
            modelBuilder.Entity<Bird>().BaseType<Animal>();
            modelBuilder.Entity<Animal>().Key(e => e.Species);
            modelBuilder.Entity<Rose>().BaseType<Flower>();
            modelBuilder.Entity<Daisy>().BaseType<Flower>();
            modelBuilder.Entity<Flower>().BaseType<Plant>();
            modelBuilder.Entity<Plant>().Key(e => e.Species);
            modelBuilder.Entity<Country>();
        }

        public abstract InheritanceContext CreateContext();

        protected void SeedData(InheritanceContext context)
        {
            var kiwi = new Kiwi
                {
                    Species = "Apteryx haastii",
                    Name = "Great spotted kiwi",
                    IsFlightless = true,
                    FoundOn = Island.South
                };

            var eagle = new Eagle
                {
                    Species = "Aquila chrysaetos canadensis",
                    Name = "American golden eagle",
                    Group = EagleGroup.Booted
                };

            eagle.Prey.Add(kiwi);

            var rose = new Rose
                {
                    Species = "Rosa canina",
                    Name = "Dog-rose",
                    HasThorns = true
                };

            var daisy = new Daisy
                {
                    Species = "Bellis perennis",
                    Name = "Common daisy"
                };

            var nz = new Country { Id = 1, Name = "New Zealand" };

            nz.Animals.Add(kiwi);

            var usa = new Country { Id = 2, Name = "USA" };

            usa.Animals.Add(eagle);

            context.Set<Animal>().Add(kiwi);
            context.Set<Bird>().Add(eagle);
            context.Set<Country>().Add(nz);
            context.Set<Country>().Add(usa);
            context.Set<Rose>().Add(rose);
            context.Set<Daisy>().Add(daisy);

            context.SaveChanges();
        }
    }
}
