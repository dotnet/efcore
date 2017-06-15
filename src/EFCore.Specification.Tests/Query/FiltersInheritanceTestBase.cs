// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FiltersInheritanceTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : InheritanceFixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void Can_use_of_type_animal()
        {
            var animals = _context.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species).ToList();

            Assert.Equal(1, animals.Count);
            Assert.IsType<Kiwi>(animals[0]);
            Assert.Equal(1, _context.ChangeTracker.Entries().Count());
        }

        [Fact]
        public virtual void Can_use_is_kiwi()
        {
            var kiwis = _context.Set<Animal>().Where(a => a is Kiwi).ToList();

            Assert.Equal(1, kiwis.Count);
        }

        [Fact]
        public virtual void Can_use_is_kiwi_with_other_predicate()
        {
            var animals = _context.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1).ToList();

            Assert.Equal(1, animals.Count);
        }

        [Fact]
        public virtual void Can_use_is_kiwi_in_projection()
        {
            var animals = _context.Set<Animal>().Select(a => a is Kiwi).ToList();

            Assert.Equal(1, animals.Count);
            Assert.Equal(1, animals.Count(a => a));
            Assert.Equal(0, animals.Count(a => !a));
        }

        [Fact]
        public virtual void Can_use_of_type_bird()
        {
            var animals = _context.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species).ToList();

            Assert.Equal(1, animals.Count);
            Assert.IsType<Kiwi>(animals[0]);
            Assert.Equal(1, _context.ChangeTracker.Entries().Count());
        }

        [Fact]
        public virtual void Can_use_of_type_bird_predicate()
        {
            var animals
                = _context.Set<Animal>()
                    .Where(a => a.CountryId == 1)
                    .OfType<Bird>()
                    .OrderBy(a => a.Species)
                    .ToList();

            Assert.Equal(1, animals.Count);
            Assert.IsType<Kiwi>(animals[0]);
            Assert.Equal(1, _context.ChangeTracker.Entries().Count());
        }

        [Fact]
        public virtual void Can_use_of_type_bird_with_projection()
        {
            var animals
                = _context.Set<Animal>()
                    .OfType<Bird>()
                    .Select(b => new { b.EagleId })
                    .ToList();

            Assert.Equal(1, animals.Count);
            Assert.Equal(0, _context.ChangeTracker.Entries().Count());
        }

        [Fact]
        public virtual void Can_use_of_type_bird_first()
        {
            var bird = _context.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species).First();

            Assert.NotNull(bird);
            Assert.IsType<Kiwi>(bird);
            Assert.Equal(1, _context.ChangeTracker.Entries().Count());
        }

        [Fact]
        public virtual void Can_use_of_type_kiwi()
        {
            var animals = _context.Set<Animal>().OfType<Kiwi>().ToList();

            Assert.Equal(1, animals.Count);
            Assert.IsType<Kiwi>(animals[0]);
            Assert.Equal(1, _context.ChangeTracker.Entries().Count());
        }

        protected TFixture Fixture { get; }

        private readonly InheritanceContext _context;

        protected FiltersInheritanceTestBase(TFixture fixture)
        {
            Fixture = fixture;
            _context = fixture.CreateContext();
        }

        public void Dispose() => _context.Dispose();
    }
}
