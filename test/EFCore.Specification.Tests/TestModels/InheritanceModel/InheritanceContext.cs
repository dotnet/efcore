// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public class InheritanceContext : PoolableDbContext
    {
        public InheritanceContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalQuery> AnimalQueries { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Plant> Plants { get; set; }

        public static void Seed(InheritanceContext context)
        {
            var animals = InheritanceData.CreateAnimals();
            var countries = InheritanceData.CreateCountries();
            var drinks = InheritanceData.CreateDrinks();
            var plants = InheritanceData.CreatePlants();

            InheritanceData.WireUp(animals, countries);

            context.Animals.AddRange(animals);
            context.Countries.AddRange(countries);
            context.Drinks.AddRange(drinks);
            context.Plants.AddRange(plants);

            context.SaveChanges();
        }
    }
}
