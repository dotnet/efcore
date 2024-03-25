// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FindCosmosTest : FindTestBase<FindCosmosTest.FindCosmosFixture>
{
    protected FindCosmosTest(FindCosmosFixture fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact(Skip = "#25886")]
    public override void Find_base_type_using_derived_set_tracked() { }

    [ConditionalTheory(Skip = "#25886")]
    public override Task Find_base_type_using_derived_set_tracked_async(CancellationType cancellationType)
        => Task.CompletedTask;

    [ConditionalFact(Skip = "#25886")]
    public override void Find_derived_using_base_set_type_from_store() { }

    [ConditionalTheory(Skip = "#25886")]
    public override Task Find_derived_using_base_set_type_from_store_async(CancellationType cancellationType)
        => Task.CompletedTask;

    public override void Find_int_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_int_key_from_store());

    public override void Returns_null_for_int_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_int_key_not_in_store());

    public override void Find_nullable_int_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_nullable_int_key_from_store());

    public override void Returns_null_for_nullable_int_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_nullable_int_key_not_in_store());

    public override void Find_string_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_string_key_from_store());

    public override void Returns_null_for_string_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_string_key_not_in_store());

    public override void Find_composite_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_composite_key_from_store());

    public override void Returns_null_for_composite_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_composite_key_not_in_store());

    public override void Find_base_type_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_base_type_from_store());

    public override void Returns_null_for_base_type_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_base_type_not_in_store());

    public override void Find_derived_type_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_derived_type_from_store());

    public override void Returns_null_for_derived_type_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_derived_type_not_in_store());

    public override void Find_base_type_using_derived_set_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_base_type_using_derived_set_from_store());

    public override void Find_shadow_key_from_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Find_shadow_key_from_store());

    public override void Returns_null_for_shadow_key_not_in_store()
        => CosmosTestHelpers.Instance.NoSyncTest(() => base.Returns_null_for_shadow_key_not_in_store());

    public class FindCosmosTestSet(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindCosmosTestContext(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindCosmosTestNonGeneric(FindCosmosFixture fixture) : FindCosmosTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindCosmosFixture : FindFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
