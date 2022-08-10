// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class TPCFiltersInheritanceBulkUpdatesTestBase<TFixture> : FiltersInheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : TPCInheritanceBulkUpdatesFixture, new()
{
    protected TPCFiltersInheritanceBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    // Keyless entities are mapped as TPH only
    public override Task Delete_where_keyless_entity_mapped_to_sql_query(bool async) => Task.CompletedTask;

    public override Task Delete_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy(async));

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query(bool async) => Task.CompletedTask;

    public override Task Update_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Animal"),
            () => base.Update_where_hierarchy(async));
}
