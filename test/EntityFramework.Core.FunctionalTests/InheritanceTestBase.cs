// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
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

        protected AnimalContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected TFixture Fixture { get; }

        protected InheritanceTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
