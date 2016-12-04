// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Inheritance;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class InheritanceTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        [Fact]
        public virtual void Can_query_when_shared_column()
        {
            using (var context = CreateContext())
            {
                var coke = context.Set<Coke>().Single();
                Assert.Equal(6, coke.SugarGrams);
                Assert.Equal(4, coke.CaffeineGrams);
                Assert.Equal(5, coke.Carbination);

                var lilt = context.Set<Lilt>().Single();
                Assert.Equal(4, lilt.SugarGrams);
                Assert.Equal(7, lilt.Carbination);

                var tea = context.Set<Tea>().Single();
                Assert.True(tea.HasMilk);
                Assert.Equal(1, tea.CaffeineGrams);
            }
        }

        [Fact]
        public virtual void Can_query_all_types_when_shared_column()
        {
            using (var context = CreateContext())
            {
                var drinks = context.Set<Drink>().ToList();
                Assert.Equal(3, drinks.Count);

                var coke = drinks.OfType<Coke>().Single();
                Assert.Equal(6, coke.SugarGrams);
                Assert.Equal(4, coke.CaffeineGrams);
                Assert.Equal(5, coke.Carbination);

                var lilt = drinks.OfType<Lilt>().Single();
                Assert.Equal(4, lilt.SugarGrams);
                Assert.Equal(7, lilt.Carbination);

                var tea = drinks.OfType<Tea>().Single();
                Assert.True(tea.HasMilk);
                Assert.Equal(1, tea.CaffeineGrams);
            }
        }

        [Fact]
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

        [Fact]
        public virtual void Can_use_is_kiwi()
        {
            using (var context = CreateContext())
            {
                var kiwis = context.Set<Animal>().Where(a => a is Kiwi).ToList();

                Assert.Equal(1, kiwis.Count);
            }
        }

        [Fact]
        public virtual void Can_use_is_kiwi_with_other_predicate()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1).ToList();

                Assert.Equal(1, animals.Count);
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Can_use_of_type_bird_with_projection()
        {
            using (var context = CreateContext())
            {
                var animals
                    = context.Set<Animal>()
                        .OfType<Bird>()
                        .Select(b => new { b.EagleId })
                        .ToList();

                Assert.Equal(2, animals.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Can_query_just_kiwis()
        {
            using (var context = CreateContext())
            {
                var kiwi = context.Set<Kiwi>().Single();

                Assert.NotNull(kiwi);
            }
        }

        [Fact]
        public virtual void Can_query_just_roses()
        {
            using (var context = CreateContext())
            {
                var rose = context.Set<Rose>().Single();

                Assert.NotNull(rose);
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Discriminator_used_when_projection_over_derived_type2()
        {
            using (var context = CreateContext())
            {
                var birds
                    = context.Set<Bird>()
                        .Select(b => new { b.IsFlightless, Discriminator = EF.Property<string>(b, "Discriminator") })
                        .ToArray();

                Assert.Equal(2, birds.Length);
            }
        }

        [Fact]
        public virtual void Discriminator_with_cast_in_shadow_property()
        {
            using (var context = CreateContext())
            {
                var preditors
                    = context.Set<Animal>()
                        .Where(b => "Kiwi" == EF.Property<string>(b, "Discriminator"))
                        .Select(k => new { Preditor = EF.Property<string>((Bird)k, "EagleId") })
                        .ToArray();

                Assert.Equal(1, preditors.Length);
            }
        }

        [Fact]
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

        [Fact]
        public virtual void Can_insert_update_delete()
        {
            using (var context = CreateContext())
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
            }

            using (var context = CreateContext())
            {
                var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                kiwi.EagleId = "Aquila chrysaetos canadensis";

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                Assert.Equal("Aquila chrysaetos canadensis", kiwi.EagleId);

                context.Set<Bird>().Remove(kiwi);

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var count = context.Set<Kiwi>().Count(k => k.Species.EndsWith("owenii"));

                Assert.Equal(0, count);
            }
        }

        protected InheritanceContext CreateContext() => Fixture.CreateContext();

        protected TFixture Fixture { get; }

        protected InheritanceTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
