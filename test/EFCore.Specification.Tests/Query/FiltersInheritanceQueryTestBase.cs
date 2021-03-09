// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FiltersInheritanceQueryTestBase<TFixture> : FilteredQueryTestBase<TFixture>
        where TFixture : InheritanceQueryFixtureBase, new()
    {
        protected FiltersInheritanceQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_animal(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>().Where(a => a is Kiwi),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi_in_projection(bool async)
        {
            return AssertFilteredQueryScalar(
                async,
                ss => ss.Set<Animal>().Select(a => a is Kiwi));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_predicate(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>()
                    .Where(a => a.CountryId == 1)
                    .OfType<Bird>()
                    .OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_with_projection(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>()
                    .OfType<Bird>()
                    .Select(b => new { b.EagleId }),
                elementSorter: e => e.EagleId);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_first(bool async)
        {
            return AssertFirst(
                async,
                ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_kiwi(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Animal>().OfType<Kiwi>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_derived_set(bool async)
        {
            return AssertFilteredQuery(
                async,
                ss => ss.Set<Eagle>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues(bool async)
        {
            using var context = Fixture.CreateContext();
            var eagle = context.Set<Eagle>().IgnoreQueryFilters().Single();

            Assert.Single(context.ChangeTracker.Entries());
            if (async)
            {
                Assert.NotNull(await context.Entry(eagle).GetDatabaseValuesAsync());
            }
            else
            {
                Assert.NotNull(context.Entry(eagle).GetDatabaseValues());
            }
        }
    }
}
