// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class InheritanceData : ISetSource
{
    public static readonly InheritanceData Instance = new(useGeneratedKeys: false);
    public static readonly InheritanceData GeneratedKeysInstance = new(useGeneratedKeys: true);

    public IReadOnlyList<Animal> Animals { get; }
    public IReadOnlyList<AnimalQuery> AnimalQueries { get; }
    public IReadOnlyList<Country> Countries { get; }
    public IReadOnlyList<Drink> Drinks { get; }
    public IReadOnlyList<Plant> Plants { get; }

    public InheritanceData(bool useGeneratedKeys)
    {
        Animals = CreateAnimals(useGeneratedKeys);
        Countries = CreateCountries();
        Drinks = CreateDrinks(useGeneratedKeys);
        Plants = CreatePlants();

        WireUp(Animals, Countries);

        AnimalQueries = Animals.Select(
            a => a is Eagle
                ? (AnimalQuery)new EagleQuery
                {
                    Name = a.Name,
                    CountryId = a.CountryId,
                    EagleId = ((Bird)a).EagleId,
                    IsFlightless = ((Bird)a).IsFlightless,
                    Group = ((Eagle)a).Group,
                }
                : new KiwiQuery
                {
                    Name = a.Name,
                    CountryId = a.CountryId,
                    EagleId = ((Bird)a).EagleId,
                    IsFlightless = ((Bird)a).IsFlightless,
                    FoundOn = ((Kiwi)a).FoundOn,
                }).ToList();
    }

    public InheritanceData(
        IReadOnlyList<Animal> animals,
        IReadOnlyList<AnimalQuery> animalQueries,
        IReadOnlyList<Country> countries,
        IReadOnlyList<Drink> drinks,
        IReadOnlyList<Plant> plants)
    {
        Animals = animals;
        AnimalQueries = animalQueries;
        Countries = countries;
        Drinks = drinks;
        Plants = plants;
    }

    public virtual IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(Animal))
        {
            return (IQueryable<TEntity>)Animals.AsQueryable();
        }

        if (typeof(TEntity) == typeof(AnimalQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Bird))
        {
            return (IQueryable<TEntity>)Animals.OfType<Bird>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(BirdQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<BirdQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Eagle))
        {
            return (IQueryable<TEntity>)Animals.OfType<Eagle>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(EagleQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<EagleQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Kiwi))
        {
            return (IQueryable<TEntity>)Animals.OfType<Kiwi>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(KiwiQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<KiwiQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Country))
        {
            return (IQueryable<TEntity>)Countries.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Drink))
        {
            return (IQueryable<TEntity>)Drinks.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Coke))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Coke>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Lilt))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Lilt>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Tea))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Tea>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Plant))
        {
            return (IQueryable<TEntity>)Plants.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Flower))
        {
            return (IQueryable<TEntity>)Plants.OfType<Flower>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Daisy))
        {
            return (IQueryable<TEntity>)Plants.OfType<Daisy>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Rose))
        {
            return (IQueryable<TEntity>)Plants.OfType<Rose>().AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<Animal> CreateAnimals(bool useGeneratedKeys)
        => new List<Animal>
        {
            new Kiwi
            {
                Id = useGeneratedKeys ? 0 : 1,
                Species = "Apteryx haastii",
                Name = "Great spotted kiwi",
                IsFlightless = true,
                FoundOn = Island.South
            },
            new Eagle
            {
                Id = useGeneratedKeys ? 0 : 2,
                Species = "Aquila chrysaetos canadensis",
                Name = "American golden eagle",
                Group = EagleGroup.Booted
            },
        };

    public static IReadOnlyList<Country> CreateCountries()
        => new List<Country>
        {
            new() { Id = 1, Name = "New Zealand" }, new() { Id = 2, Name = "USA" },
        };

    public static IReadOnlyList<Drink> CreateDrinks(bool useGeneratedKeys)
        => new List<Drink>
        {
            new Tea
            {
                Id = useGeneratedKeys ? 0 : 1,
                SortIndex = 1,
                HasMilk = true,
                CaffeineGrams = 1
            },
            new Lilt
            {
                Id = useGeneratedKeys ? 0 : 2,
                SortIndex = 2,
                SugarGrams = 4,
                Carbonation = 7
            },
            new Coke
            {
                Id = useGeneratedKeys ? 0 : 3,
                SortIndex = 3,
                SugarGrams = 6,
                CaffeineGrams = 4,
                Carbonation = 5
            },
        };

    public static IReadOnlyList<Plant> CreatePlants()
        => new List<Plant>
        {
            new Rose
            {
                Genus = PlantGenus.Rose,
                Species = "Rosa canina",
                Name = "Dog-rose",
                HasThorns = true
            },
            new Daisy
            {
                Genus = PlantGenus.Daisy,
                Species = "Bellis perennis",
                Name = "Common daisy",
                AdditionalInfo =
                    new AdditionalDaisyInfo
                    {
                        Nickname = "Lawn daisy", LeafStructure = new DaisyLeafStructure { NumLeaves = 5, AreLeavesBig = true }
                    }
            },
            new Daisy
            {
                Genus = PlantGenus.Daisy,
                Species = "Bellis annua",
                Name = "Annual daisy",
                AdditionalInfo = new AdditionalDaisyInfo
                {
                    Nickname = "European daisy", LeafStructure = new DaisyLeafStructure { NumLeaves = 8, AreLeavesBig = false }
                }
            }
        };

    public static void WireUp(
        IReadOnlyList<Animal> animals,
        IReadOnlyList<Country> countries)
    {
        ((Eagle)animals[1]).Prey.Add((Bird)animals[0]);

        countries[0].Animals.Add(animals[0]);
        animals[0].CountryId = countries[0].Id;

        countries[1].Animals.Add(animals[1]);
        animals[1].CountryId = countries[1].Id;
    }
}
