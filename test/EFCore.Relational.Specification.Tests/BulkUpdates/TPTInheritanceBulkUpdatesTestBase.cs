// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class TPTInheritanceBulkUpdatesTestBase<TFixture> : InheritanceBulkUpdatesTestBase<TFixture>
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    protected TPTInheritanceBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
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

    [ConditionalTheory(Skip = "FK constraint issue")]
    public override Task Delete_where_using_hierarchy(bool async)
        => base.Delete_where_using_hierarchy(async);

    [ConditionalTheory(Skip = "FK constraint issue")]
    public override Task Delete_where_using_hierarchy_derived(bool async)
        => base.Delete_where_using_hierarchy_derived(async);

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => Task.CompletedTask;

    public override Task Update_where_hierarchy(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Animal"),
            () => base.Update_where_hierarchy(async));

    public override Task Update_where_hierarchy_derived(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Kiwi"),
            () => base.Update_where_hierarchy_derived(async));

    public override Task Update_with_interface_in_property_expression(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Coke"),
            () => base.Update_with_interface_in_property_expression(async));

    public override Task Update_with_interface_in_EF_Property_in_property_expression(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteUpdate", "Coke"),
            () => base.Update_with_interface_in_EF_Property_in_property_expression(async));
}
