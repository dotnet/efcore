// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NonSharedModelBulkUpdatesInMemoryTest : NonSharedModelBulkUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
        // TODO: Fake transactions needed for real tests.
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task Delete_aggregate_root_when_eager_loaded_owned_collection(bool async)
        => AssertTranslationFailed(() => base.Delete_aggregate_root_when_eager_loaded_owned_collection(async));

    public override Task Delete_with_owned_collection_and_non_natively_translatable_query(bool async)
        => AssertTranslationFailed(() => base.Delete_with_owned_collection_and_non_natively_translatable_query(async));

    public override Task Delete_aggregate_root_when_table_sharing_with_owned(bool async)
        => AssertTranslationFailed(() => base.Delete_aggregate_root_when_table_sharing_with_owned(async));

    public override Task Replace_ColumnExpression_in_column_setter(bool async)
        => AssertTranslationFailed(() => base.Replace_ColumnExpression_in_column_setter(async));

    public override Task Update_non_owned_property_on_entity_with_owned(bool async)
        => AssertTranslationFailed(() => base.Update_non_owned_property_on_entity_with_owned(async));

    public override Task Update_non_owned_property_on_entity_with_owned2(bool async)
        => AssertTranslationFailed(() => base.Update_non_owned_property_on_entity_with_owned2(async));

    public override Task Update_non_owned_property_on_entity_with_owned_in_join(bool async)
        => AssertTranslationFailed(() => base.Update_non_owned_property_on_entity_with_owned_in_join(async));

    public override Task Update_owned_and_non_owned_properties_with_table_sharing(bool async)
        => AssertTranslationFailed(() => base.Update_owned_and_non_owned_properties_with_table_sharing(async));

    public override Task Delete_entity_with_auto_include(bool async)
        => AssertTranslationFailed(() => base.Delete_entity_with_auto_include(async));

    public override Task Delete_predicate_based_on_optional_navigation(bool async)
        => AssertTranslationFailed(() => base.Delete_predicate_based_on_optional_navigation(async));

    public override Task Update_with_alias_uniquification_in_setter_subquery(bool async)
        => AssertTranslationFailed(() => base.Update_with_alias_uniquification_in_setter_subquery(async));

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
