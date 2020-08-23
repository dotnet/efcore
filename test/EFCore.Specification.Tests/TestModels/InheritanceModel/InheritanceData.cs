// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public class InheritanceData : ISetSource
    {
        public IReadOnlyList<Animal> Animals { get; }
        public IReadOnlyList<AnimalQuery> AnimalQueries { get; }
        public IReadOnlyList<Country> Countries { get; }
        public IReadOnlyList<Drink> Drinks { get; }
        public IReadOnlyList<Plant> Plants { get; }

        public InheritanceData()
        {
            Animals = CreateAnimals();
            Countries = CreateCountries();
            Drinks = CreateDrinks();
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
                    }).ToList().AsReadOnly();
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

        public static IReadOnlyList<Animal> CreateAnimals()
            => new List<Animal>
            {
                new Kiwi
                {
                    Species = "Apteryx haastii",
                    Name = "Great spotted kiwi",
                    IsFlightless = true,
                    FoundOn = Island.South
                },
                new Eagle
                {
                    Species = "Aquila chrysaetos canadensis",
                    Name = "American golden eagle",
                    Group = EagleGroup.Booted
                },
            };

        public static IReadOnlyList<Country> CreateCountries()
            => new List<Country>
            {
                new Country { Id = 1, Name = "New Zealand" }, new Country { Id = 2, Name = "USA" },
            };

        public static IReadOnlyList<Drink> CreateDrinks()
            => new List<Drink>
            {
                new Tea
                {
                    Id = 1,
                    HasMilk = true,
                    CaffeineGrams = 1
                },
                new Lilt
                {
                    Id = 2,
                    SugarGrams = 4,
                    Carbonation = 7
                },
                new Coke
                {
                    Id = 3,
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
                    Name = "Common daisy"
                },
            };

        public static void WireUp(
            IReadOnlyList<Animal> animals,
            IReadOnlyList<Country> countries)
        {
            ((Eagle)animals[1]).Prey.Add((Bird)animals[0]);
            ((Bird)animals[0]).EagleId = animals[1].Species;

            countries[0].Animals.Add(animals[0]);
            animals[0].CountryId = countries[0].Id;

            countries[1].Animals.Add(animals[1]);
            animals[1].CountryId = countries[1].Id;
        }
    }
}
