// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceFixtureBase
    {
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Do this with Code First when we can

            var model = modelBuilder.Model;

            var country = model.AddEntityType(typeof(Country));
            var countryKey = country.SetPrimaryKey(country.AddProperty("Id", typeof(int)));
            country.AddProperty("Name", typeof(string));

            var animal = model.AddEntityType(typeof(Animal));
            var animalKey = animal.SetPrimaryKey(animal.AddProperty("Species", typeof(string)));
            animal.AddProperty("Name", typeof(string));
            var countryFk = animal.AddForeignKey(animal.AddProperty("CountryId", typeof(int)), countryKey);

            var bird = model.AddEntityType(typeof(Bird));
            bird.BaseType = animal;
            bird.AddProperty("IsFlightless", typeof(bool));

            var kiwi = model.AddEntityType(typeof(Kiwi));
            kiwi.BaseType = bird;
            kiwi.AddProperty("FoundOn", typeof(Island));

            var eagle = model.AddEntityType(typeof(Eagle));
            eagle.BaseType = bird;
            eagle.AddProperty("Group", typeof(EagleGroup));

            var eagleFk = bird.AddForeignKey(bird.AddProperty("EagleId", typeof(string)), animalKey, eagle);

            country.AddNavigation("Animals", countryFk, false);
            eagle.AddNavigation("Prey", eagleFk, false);
        }

        public abstract AnimalContext CreateContext();

        protected void SeedData(AnimalContext context)
        {
            var kiwi = new Kiwi
                {
                    Species = "Apteryx owenii",
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

            var nz = new Country { Id = 1, Name = "New Zealand" };

            nz.Animals.Add(kiwi);

            var usa = new Country { Id = 2, Name = "USA" };

            usa.Animals.Add(eagle);

            context.Set<Animal>().Add(kiwi);
            context.Set<Bird>().Add(eagle);
            context.Set<Country>().Add(nz);
            context.Set<Country>().Add(usa);

            context.SaveChanges();
        }
    }
}
