// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class FiltersInheritanceBulkUpdatesRelationalTestBase<TFixture> : FiltersInheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : InheritanceBulkUpdatesRelationalFixtureBase, new()
{
    protected FiltersInheritanceBulkUpdatesRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

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
    public virtual Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator("ExecuteUpdate", "EagleQuery"),
            () => AssertUpdate(
                async,
                ss => ss.Set<EagleQuery>().Where(e => e.CountryId > 0),
                e => e,
                s => s.SetProperty(e => e.Name, "Eagle"),
                rowsAffectedCount: 1));

    protected static async Task AssertTranslationFailed(string details, Func<Task> query)
        => Assert.Contains(
            CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);

    protected abstract void ClearLog();
}
