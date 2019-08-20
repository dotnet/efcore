// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        protected InheritanceTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Can_query_when_shared_column()
        {
            using (var context = CreateContext())
            {
                var coke = context.Set<Coke>().Single();
                Assert.Equal(6, coke.SugarGrams);
                Assert.Equal(4, coke.CaffeineGrams);
                Assert.Equal(5, coke.Carbonation);

                var lilt = context.Set<Lilt>().Single();
                Assert.Equal(4, lilt.SugarGrams);
                Assert.Equal(7, lilt.Carbonation);

                var tea = context.Set<Tea>().Single();
                Assert.True(tea.HasMilk);
                Assert.Equal(1, tea.CaffeineGrams);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_all_types_when_shared_column()
        {
            using (var context = CreateContext())
            {
                var drinks = context.Set<Drink>().ToList();
                Assert.Equal(3, drinks.Count);

                var coke = drinks.OfType<Coke>().Single();
                Assert.Equal(6, coke.SugarGrams);
                Assert.Equal(4, coke.CaffeineGrams);
                Assert.Equal(5, coke.Carbonation);

                var lilt = drinks.OfType<Lilt>().Single();
                Assert.Equal(4, lilt.SugarGrams);
                Assert.Equal(7, lilt.Carbonation);

                var tea = drinks.OfType<Tea>().Single();
                Assert.True(tea.HasMilk);
                Assert.Equal(1, tea.CaffeineGrams);
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_animal()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species).ToList();

                Assert.Equal(2, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.IsType<Eagle>(animals[1]);
                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_is_kiwi()
        {
            using (var context = CreateContext())
            {
                var kiwis = context.Set<Animal>().Where(a => a is Kiwi).ToList();

                Assert.Equal(1, kiwis.Count);
            }
        }

        [ConditionalFact]
        public virtual void Can_use_backwards_is_animal()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once IsExpressionAlwaysTrue
                var kiwis = context.Set<Kiwi>().Where(a => a is Animal).ToList();

                Assert.Equal(1, kiwis.Count);
            }
        }

        [ConditionalFact]
        public virtual void Can_use_is_kiwi_with_other_predicate()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1).ToList();

                Assert.Equal(1, animals.Count);
            }
        }

        [ConditionalFact]
        public virtual void Can_use_is_kiwi_in_projection()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().Select(a => a is Kiwi).ToList();

                Assert.Equal(2, animals.Count);
                Assert.Equal(1, animals.Count(a => a));
                Assert.Equal(1, animals.Count(a => !a));
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_bird()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species).ToList();

                Assert.Equal(2, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.IsType<Eagle>(animals[1]);
                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_bird_predicate()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .Where(a => a.CountryId == 1)
                        .OfType<Bird>()
                        .OrderBy(a => a.Species)
                        .ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_bird_with_projection()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .OfType<Bird>()
                        .Select(
                            b => new
                            {
                                b.EagleId
                            })
                        .ToList();

                Assert.Equal(2, animals.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_bird_first()
        {
            using (var context = CreateContext())
            {
                var bird = context.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species).First();

                Assert.NotNull(bird);
                Assert.IsType<Kiwi>(bird);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_kiwi()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OfType<Kiwi>().ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact(Skip = "17364")]
        public virtual void Can_use_backwards_of_type_animal()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Kiwi>().OfType<Animal>().ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_rose()
        {
            using (var context = CreateContext())
            {
                var plants = context.Set<Plant>().OfType<Rose>().ToList();

                Assert.Equal(1, plants.Count);
                Assert.IsType<Rose>(plants[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_query_all_animals()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OrderBy(a => a.Species).ToList();

                Assert.Equal(2, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.IsType<Eagle>(animals[1]);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_all_animal_views()
        {
            using (var context = CreateContext())
            {
                var animalQueries = context.Set<AnimalQuery>().OrderBy(av => av.CountryId).ToList();

                Assert.Equal(2, animalQueries.Count);
                Assert.IsType<KiwiQuery>(animalQueries[0]);
                Assert.IsType<EagleQuery>(animalQueries[1]);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_all_plants()
        {
            using (var context = CreateContext())
            {
                var plants = context.Set<Plant>().OrderBy(a => a.Species).ToList();

                Assert.Equal(2, plants.Count);
                Assert.IsType<Daisy>(plants[0]);
                Assert.IsType<Rose>(plants[1]);
            }
        }

        [ConditionalFact]
        public virtual void Can_filter_all_animals()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .OrderBy(a => a.Species)
                        .Where(a => a.Name == "Great spotted kiwi")
                        .ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_all_birds()
        {
            using (var context = CreateContext())
            {
                var birds = context.Set<Bird>().OrderBy(a => a.Species).ToList();

                Assert.Equal(2, birds.Count);
                Assert.IsType<Kiwi>(birds[0]);
                Assert.IsType<Eagle>(birds[1]);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_just_kiwis()
        {
            using (var context = CreateContext())
            {
                var kiwi = context.Set<Kiwi>().Single();

                Assert.NotNull(kiwi);
            }
        }

        [ConditionalFact]
        public virtual void Can_query_just_roses()
        {
            using (var context = CreateContext())
            {
                var rose = context.Set<Rose>().Single();

                Assert.NotNull(rose);
            }
        }

        [ConditionalFact]
        public virtual void Can_include_animals()
        {
            using (var context = CreateContext())
            {
                var countries
                    = context.Set<Country>()
                        .OrderBy(c => c.Name)
                        .Include(c => c.Animals)
                        .ToList();

                Assert.Equal(2, countries.Count);
                Assert.IsType<Kiwi>(countries[0].Animals[0]);
                Assert.IsType<Eagle>(countries[1].Animals[0]);
            }
        }

        [ConditionalFact]
        public virtual void Can_include_prey()
        {
            using (var context = CreateContext())
            {
                var eagle
                    = context.Set<Eagle>()
                        .Include(e => e.Prey)
                        .Single();

                Assert.NotNull(eagle);
                Assert.Equal(1, eagle.Prey.Count);
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_kiwi_where_south_on_derived_property()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .OfType<Kiwi>()
                        .Where(x => x.FoundOn == Island.South)
                        .ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_use_of_type_kiwi_where_north_on_derived_property()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .OfType<Kiwi>()
                        .Where(x => x.FoundOn == Island.North)
                        .ToList();

                Assert.Equal(0, animals.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Discriminator_used_when_projection_over_derived_type()
        {
            using (var context = CreateContext())
            {
                var kiwis
                    = context.Set<Kiwi>()
                        .Select(k => k.FoundOn)
                        .ToArray();

                Assert.Equal(1, kiwis.Length);
            }
        }

        [ConditionalFact]
        public virtual void Discriminator_used_when_projection_over_derived_type2()
        {
            using (var context = CreateContext())
            {
                var birds
                    = context.Set<Bird>()
                        .Select(
                            b => new
                            {
                                b.IsFlightless,
                                Discriminator = EF.Property<string>(b, "Discriminator")
                            })
                        .ToArray();

                Assert.Equal(2, birds.Length);
            }
        }

        [ConditionalFact]
        public virtual void Discriminator_with_cast_in_shadow_property()
        {
            using (var context = CreateContext())
            {
                var predators
                    = context.Set<Animal>()
                        .Where(b => "Kiwi" == EF.Property<string>(b, "Discriminator"))
                        .Select(
                            k => new
                            {
                                Predator = EF.Property<string>((Bird)k, "EagleId")
                            })
                        .ToArray();

                Assert.Equal(1, predators.Length);
            }
        }

        [ConditionalFact]
        public virtual void Discriminator_used_when_projection_over_of_type()
        {
            using (var context = CreateContext())
            {
                var birds
                    = context.Set<Animal>()
                        .OfType<Kiwi>()
                        .Select(k => k.FoundOn)
                        .ToArray();

                Assert.Equal(1, birds.Length);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_update_delete()
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext,
                UseTransaction,
                context =>
                {
                    var kiwi = new Kiwi
                    {
                        Species = "Apteryx owenii",
                        Name = "Little spotted kiwi",
                        IsFlightless = true,
                        FoundOn = Island.North
                    };

                    var nz = context.Set<Country>().Single(c => c.Id == 1);

                    nz.Animals.Add(kiwi);

                    context.SaveChanges();
                },
                context =>
                {
                    var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                    kiwi.EagleId = "Aquila chrysaetos canadensis";

                    context.SaveChanges();
                },
                context =>
                {
                    var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                    Assert.Equal("Aquila chrysaetos canadensis", kiwi.EagleId);

                    context.Set<Bird>().Remove(kiwi);

                    context.SaveChanges();
                },
                context =>
                {
                    var count = context.Set<Kiwi>().Count(k => k.Species.EndsWith("owenii"));

                    Assert.Equal(0, count);
                });
        }

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        [ConditionalFact(Skip = "#16298")]
        public virtual void Union_siblings_with_duplicate_property_in_subquery()
        {
            // Coke and Tea both have CaffeineGrams, which both need to be projected out on each side and so
            // requiring alias uniquification. They also have a different number of properties.
            using (var context = CreateContext())
            {
                var cokes = context.Set<Coke>();

                var teas = context.Set<Tea>();

                var concat = cokes.Cast<Drink>()
                    .Union(teas)
                    .Where(d => d.Id > 0)
                    .ToList();

                Assert.Equal(2, concat.Count);
            }
        }

        [ConditionalFact(Skip = "#16298")]
        public virtual void OfType_Union_subquery()
        {
            using (var context = CreateContext())
            {
                context.Set<Animal>()
                    .OfType<Kiwi>()
                    .Union(context.Set<Animal>()
                        .OfType<Kiwi>())
                    .Where(o => o.FoundOn == Island.North)
                    .ToList();
            }
        }

        [ConditionalFact(Skip = "#16298")]
        public virtual void OfType_Union_OfType()
        {
            using (var context = CreateContext())
            {
                context.Set<Bird>()
                    .OfType<Kiwi>()
                    .Union(context.Set<Bird>())
                    .OfType<Kiwi>()
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Subquery_OfType()
        {
            using (var context = CreateContext())
            {
                context.Set<Bird>()
                    .Take(5)
                    .Distinct()  // Causes pushdown
                    .OfType<Kiwi>()
                    .ToList();
            }
        }

        [ConditionalFact(Skip = "#16298")]
        public virtual void Union_entity_equality()
        {
            using (var context = CreateContext())
            {
                context.Set<Kiwi>()
                    .Union(context.Set<Eagle>().Cast<Bird>())
                    .Where(b => b == null)
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Setting_foreign_key_to_a_different_type_throws()
        {
            using (var context = CreateContext())
            {
                var kiwi = context.Set<Kiwi>().Single();

                var eagle = new Eagle
                {
                    Species = "Haliaeetus leucocephalus",
                    Name = "Bald eagle",
                    Group = EagleGroup.Booted,
                    EagleId = kiwi.Species
                };

                context.Add(eagle);

                // No fixup, because no principal with this key of the correct type is loaded.
                Assert.Empty(eagle.Prey);

                if (EnforcesFkConstraints)
                {
                    // Relational database throws due to constraint violation
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }
        }

        protected virtual bool EnforcesFkConstraints => true;

        [ConditionalFact]
        public virtual void Byte_enum_value_constant_used_in_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Kiwi>().Select(k => k.IsFlightless ? Island.North : Island.South);
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal(Island.North, result[0]);
            }
        }

        protected InheritanceContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
