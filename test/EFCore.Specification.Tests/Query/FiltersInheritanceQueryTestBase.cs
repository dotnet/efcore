// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query;

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
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Animal>().Where(a => a is Kiwi));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1));
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
            assertOrder: true);
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
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird_with_projection(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Animal>()
                .OfType<Bird>()
                .Select(b => new { b.Name }),
            elementSorter: e => e.Name);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird_first(bool async)
    {
        return AssertFirst(
            async,
            ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_kiwi(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Animal>().OfType<Kiwi>());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_derived_set(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Eagle>(),
            assertEmpty: true);
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
