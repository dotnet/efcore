// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class FiltersInheritanceBulkUpdatesInMemoryTest(FiltersInheritanceBulkUpdatesInMemoryFixture fixture)
    : FiltersInheritanceBulkUpdatesTestBase<FiltersInheritanceBulkUpdatesInMemoryFixture>(fixture)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task Delete_where_hierarchy(bool async)
        => AssertTranslationFailed(() => base.Delete_where_hierarchy(async));

    public override Task Delete_where_hierarchy_subquery(bool async)
        => AssertTranslationFailed(() => base.Delete_where_hierarchy_subquery(async));

    public override Task Delete_where_hierarchy_derived(bool async)
        => AssertTranslationFailed(() => base.Delete_where_hierarchy_derived(async));

    public override Task Delete_where_using_hierarchy(bool async)
        => AssertTranslationFailed(() => base.Delete_where_using_hierarchy(async));

    public override Task Delete_where_using_hierarchy_derived(bool async)
        => AssertTranslationFailed(() => base.Delete_where_using_hierarchy_derived(async));

    public override Task Delete_GroupBy_Where_Select_First(bool async)
        => AssertTranslationFailed(() => base.Delete_GroupBy_Where_Select_First(async));

    public override Task Delete_GroupBy_Where_Select_First_2(bool async)
        => AssertTranslationFailed(() => base.Delete_GroupBy_Where_Select_First_2(async));

    public override Task Delete_GroupBy_Where_Select_First_3(bool async)
        => AssertTranslationFailed(() => base.Delete_GroupBy_Where_Select_First_3(async));

    public override Task Update_base_type(bool async)
        => AssertTranslationFailed(() => base.Update_base_type(async));

    public override Task Update_base_type_with_OfType(bool async)
        => AssertTranslationFailed(() => base.Update_base_type_with_OfType(async));

    public override Task Update_where_hierarchy_subquery(bool async)
        => AssertTranslationFailed(() => base.Update_where_hierarchy_subquery(async));

    public override Task Update_base_property_on_derived_type(bool async)
        => AssertTranslationFailed(() => base.Update_base_property_on_derived_type(async));

    public override Task Update_derived_property_on_derived_type(bool async)
        => AssertTranslationFailed(() => base.Update_derived_property_on_derived_type(async));

    public override Task Update_base_and_derived_types(bool async)
        => AssertTranslationFailed(() => base.Update_base_and_derived_types(async));

    public override Task Update_where_using_hierarchy(bool async)
        => AssertTranslationFailed(() => base.Update_where_using_hierarchy(async));

    public override Task Update_where_using_hierarchy_derived(bool async)
        => AssertTranslationFailed(() => base.Update_where_using_hierarchy_derived(async));

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
