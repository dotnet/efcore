// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class TPCInheritanceBulkUpdatesTestBase<TFixture> : InheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : TPCInheritanceBulkUpdatesFixture, new()
{
    protected TPCInheritanceBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
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
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy(async));

    public override Task Delete_where_hierarchy_subquery(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            () => base.Delete_where_hierarchy_subquery(async));

    public override Task Delete_GroupBy_Where_Select_First_3(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            () => base.Delete_GroupBy_Where_Select_First_3(async));

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => Task.CompletedTask;

    public override Task Update_base_type(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Animal"),
            () => base.Update_base_type(async));

    public override Task Update_base_type_with_OfType(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Animal"),
            () => base.Update_base_type_with_OfType(async));
}
