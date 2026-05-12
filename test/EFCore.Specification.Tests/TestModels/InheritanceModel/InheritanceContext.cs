// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

#nullable disable

public class InheritanceContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Animal> Animals { get; set; }
    public DbSet<AnimalQuery> AnimalQueries { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Drink> Drinks { get; set; }
    public DbSet<Coke> Coke { get; set; }
    public DbSet<Lilt> Lilt { get; set; }
    public DbSet<Tea> Tea { get; set; }
    public DbSet<Plant> Plants { get; set; }

    public static Task SeedAsync(InheritanceContext context, bool useGeneratedKeys)
    {
        var animals = InheritanceData.CreateAnimals(useGeneratedKeys);
        var countries = InheritanceData.CreateCountries();
        var drinks = InheritanceData.CreateDrinks(useGeneratedKeys);
        var plants = InheritanceData.CreatePlants();

        InheritanceData.WireUp(animals, countries);

        context.Animals.AddRange(animals);
        context.Countries.AddRange(countries);
        context.Drinks.AddRange(drinks);
        context.Plants.AddRange(plants);

        return context.SaveChangesAsync();
    }
}
