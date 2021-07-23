// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
