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
            // TODO: Do this with Code First when we can

            var model = modelBuilder.Model;

            var country = model.AddEntityType(typeof(Country));
            var countryIdProperty = country.AddProperty("Id", typeof(int));
            countryIdProperty.IsShadowProperty = false;
            countryIdProperty.RequiresValueGenerator = true;
            var countryKey = country.SetPrimaryKey(countryIdProperty);
            var property1 = country.AddProperty("Name", typeof(string));
            property1.IsShadowProperty = false;

            var animal = model.AddEntityType(typeof(Animal));
            var animalSpeciesProperty = animal.AddProperty("Species", typeof(string));
            animalSpeciesProperty.IsShadowProperty = false;
            animalSpeciesProperty.RequiresValueGenerator = true;
            var animalKey = animal.SetPrimaryKey(animalSpeciesProperty);
            var property3 = animal.AddProperty("Name", typeof(string));
            property3.IsShadowProperty = false;
            var property4 = animal.AddProperty("CountryId", typeof(int));
            property4.IsShadowProperty = false;
            var countryFk = animal.AddForeignKey(property4, countryKey, country);

            var bird = model.AddEntityType(typeof(Bird));
            bird.BaseType = animal;
            var property5 = bird.AddProperty("IsFlightless", typeof(bool));
            property5.IsShadowProperty = false;

            var kiwi = model.AddEntityType(typeof(Kiwi));
            kiwi.BaseType = bird;
            var property6 = kiwi.AddProperty("FoundOn", typeof(Island));
            property6.IsShadowProperty = false;

            var eagle = model.AddEntityType(typeof(Eagle));
            eagle.BaseType = bird;
            var property7 = eagle.AddProperty("Group", typeof(EagleGroup));
            property7.IsShadowProperty = false;

            var plant = model.AddEntityType(typeof(Plant));
            var property8 = plant.AddProperty("Species", typeof(string));
            property8.IsShadowProperty = false;
            var plantSpeciesProperty = property8;
            plantSpeciesProperty.RequiresValueGenerator = true;
            plant.SetPrimaryKey(plantSpeciesProperty);
            var property9 = plant.AddProperty("Name", typeof(string));
            property9.IsShadowProperty = false;

            var flower = model.AddEntityType(typeof(Flower));
            flower.BaseType = plant;

            var rose = model.AddEntityType(typeof(Rose));
            rose.BaseType = flower;
            var property10 = rose.AddProperty("HasThorns", typeof(bool));
            property10.IsShadowProperty = false;

            var daisy = model.AddEntityType(typeof(Daisy));
            daisy.BaseType = flower;

            var property11 = bird.AddProperty("EagleId", typeof(string));
            property11.IsShadowProperty = false;
            var eagleFk = bird.AddForeignKey(property11, animalKey, eagle);

            country.AddNavigation("Animals", countryFk, false);
            eagle.AddNavigation("Prey", eagleFk, false);
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
