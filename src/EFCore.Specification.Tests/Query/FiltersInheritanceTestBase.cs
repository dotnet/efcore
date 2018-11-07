// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FiltersInheritanceTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        protected FiltersInheritanceTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Can_use_of_type_animal()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species).ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
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

                Assert.Equal(1, animals.Count);
                Assert.Equal(1, animals.Count(a => a));
                Assert.Equal(0, animals.Count(a => !a));
            }
        }

        [Fact]
        public virtual void Can_use_of_type_bird()
        {
            using (var context = CreateContext())
            {
                var animals = context.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species).ToList();

                Assert.Equal(1, animals.Count);
                Assert.IsType<Kiwi>(animals[0]);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
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
                        .Select(
                            b => new
                            {
                                b.EagleId
                            })
                        .ToList();

                Assert.Equal(1, animals.Count);
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
        public virtual void Can_use_derived_set()
        {
            using (var context = CreateContext())
            {
                var eagles = context.Set<Eagle>().ToList();

                Assert.Equal(0, eagles.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
        {
            using (var context = CreateContext())
            {
                var eagle = context.Set<Eagle>().IgnoreQueryFilters().Single();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.NotNull(context.Entry(eagle).GetDatabaseValues());
            }
        }

        protected InheritanceContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
