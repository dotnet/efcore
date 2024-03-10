// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class TPTFiltersInheritanceBulkUpdatesTestBase<TFixture> : FiltersInheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    protected TPTFiltersInheritanceBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Keyless entities are mapped as TPH only
    public override Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
        => Task.CompletedTask;

    public override Task Delete_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy(async));

    public override Task Delete_where_hierarchy_subquery(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy_subquery(async));

    public override Task Delete_where_hierarchy_derived(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Kiwi"),
            () => base.Delete_where_hierarchy_derived(async));

    public override Task Delete_GroupBy_Where_Select_First_3(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            () => base.Delete_GroupBy_Where_Select_First_3(async));

    public override Task Update_base_and_derived_types(bool async)
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("e => e.Name", "e => e.FoundOn"),
            () => base.Update_base_and_derived_types(async));

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => Task.CompletedTask;
}
