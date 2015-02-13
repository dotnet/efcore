// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceFixtureBase
    {
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>().Key(a => a.Species);

            // TODO: Remove when Code First does this
            var animalEntityType = modelBuilder.Entity<Animal>().Metadata;
            var birdEntityType = modelBuilder.Entity<Bird>().Metadata;
            var kiwiEntityType = modelBuilder.Entity<Kiwi>().Metadata;
            var eagleEntityType = modelBuilder.Entity<Eagle>().Metadata;

            birdEntityType.BaseType = animalEntityType;
            kiwiEntityType.BaseType = birdEntityType;
            eagleEntityType.BaseType = birdEntityType;
        }

        public abstract AnimalContext CreateContext();

        protected void SeedData(AnimalContext context)
        {
            context.Animals.Add(
                new Kiwi
                    {
                        Species = "Apteryx owenii",
                        Name = "Great spotted kiwi",
                        IsFlightless = true
                    });

            context.Animals.Add(
                new Eagle
                    {
                        Species = "Aquila chrysaetos canadensis",
                        Name = "American golden eagle"
                    });

            context.SaveChanges();
        }
    }
}
