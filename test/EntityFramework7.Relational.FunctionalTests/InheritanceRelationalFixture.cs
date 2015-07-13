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

            // TODO: Code First this

            var animal = modelBuilder.Entity<Animal>().Metadata;

            var animalDiscriminatorProperty
                = animal.AddProperty("Discriminator", typeof(string), shadowProperty: true);

            animalDiscriminatorProperty.IsNullable = false;
            //animalDiscriminatorProperty.IsReadOnlyBeforeSave = true; // #2132
            animalDiscriminatorProperty.IsReadOnlyAfterSave = true;
            animalDiscriminatorProperty.RequiresValueGenerator = true;

            animal.Relational().DiscriminatorProperty = animalDiscriminatorProperty;


            var plant = modelBuilder.Entity<Plant>().Metadata;

            var plantDiscriminatorProperty
                = plant.AddProperty("Genus", typeof(PlantGenus));

            plantDiscriminatorProperty.IsNullable = false;
            //plantDiscriminatorProperty.IsReadOnlyBeforeSave = true; // #2132
            plantDiscriminatorProperty.IsReadOnlyAfterSave = true;
            plantDiscriminatorProperty.RequiresValueGenerator = true;

            plant.Relational().DiscriminatorProperty = plantDiscriminatorProperty;

            var rose = modelBuilder.Entity<Rose>().Metadata;
            rose.Relational().DiscriminatorValue = PlantGenus.Rose;

            var daisy = modelBuilder.Entity<Daisy>().Metadata;
            daisy.Relational().DiscriminatorValue = PlantGenus.Daisy;
        }
    }
}
