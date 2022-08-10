// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class TPTFiltersInheritanceBulkUpdatesTestBase<TFixture> : FiltersInheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    protected TPTFiltersInheritanceBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    // Keyless entities are mapped as TPH only
    public override Task Delete_where_keyless_entity_mapped_to_sql_query(bool async) => Task.CompletedTask;

    public override Task Delete_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy(async));

    public override Task Delete_where_hierarchy_derived(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Kiwi"),
            () => base.Delete_where_hierarchy_derived(async));

    [ConditionalTheory(Skip = "Issue#28532")]
    public override Task Delete_where_using_hierarchy(bool async)
    {
        return base.Delete_where_using_hierarchy(async);
    }

    [ConditionalTheory(Skip = "Issue#28532")]
    public override Task Delete_where_using_hierarchy_derived(bool async)
    {
        return base.Delete_where_using_hierarchy_derived(async);
    }

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query(bool async) => Task.CompletedTask;

    public override Task Update_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Animal"),
            () => base.Update_where_hierarchy(async));

    public override Task Update_where_hierarchy_derived(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Kiwi"),
            () => base.Update_where_hierarchy_derived(async));
}
