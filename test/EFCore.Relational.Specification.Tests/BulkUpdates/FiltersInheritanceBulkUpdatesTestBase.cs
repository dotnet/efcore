// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class FiltersInheritanceBulkUpdatesTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : InheritanceBulkUpdatesFixtureBase, new()
{
    protected FiltersInheritanceBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_hierarchy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    [ConditionalTheory(Skip = "Issue#28524")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_hierarchy_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi").OrderBy(e => e.Name).Skip(0).Take(3),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_hierarchy_derived(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Kiwi>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_using_hierarchy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Country>().Where(e => e.Animals.Where(a => a.CountryId > 0).Count() > 0),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_using_hierarchy_derived(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Country>().Where(e => e.Animals.OfType<Kiwi>().Where(a => a.CountryId > 0).Count() > 0),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator("ExecuteDelete", "EagleQuery"),
            () => AssertDelete(
                async,
                ss => ss.Set<EagleQuery>().Where(e => e.CountryId > 0),
                rowsAffectedCount: 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_hierarchy(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi"),
            e => e,
            s => s.SetProperty(e => e.Name, e => "Animal"),
            rowsAffectedCount: 1,
            (b, a) => a.ForEach(e => Assert.Equal("Animal", e.Name)));

    [ConditionalTheory(Skip = "InnerJoin")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_hierarchy_subquery(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi").OrderBy(e => e.Name).Skip(0).Take(3),
            e => e,
            s => s.SetProperty(e => e.Name, e => "Animal"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_hierarchy_derived(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Kiwi>().Where(e => e.Name == "Great spotted kiwi"),
            e => e,
            s => s.SetProperty(e => e.Name, e => "Kiwi"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_using_hierarchy(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Country>().Where(e => e.Animals.Where(a => a.CountryId > 0).Count() > 0),
            e => e,
            s => s.SetProperty(e => e.Name, e => "Monovia"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_using_hierarchy_derived(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Country>().Where(e => e.Animals.OfType<Kiwi>().Where(a => a.CountryId > 0).Count() > 0),
            e => e,
            s => s.SetProperty(e => e.Name, e => "Monovia"),
            rowsAffectedCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator("ExecuteUpdate", "EagleQuery"),
            () => AssertUpdate(
                async,
                ss => ss.Set<EagleQuery>().Where(e => e.CountryId > 0),
                e => e,
                s => s.SetProperty(e => e.Name, e => "Eagle"),
                rowsAffectedCount: 1));

    protected abstract void ClearLog();
}
