// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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

    public class FindCosmosTestSet : FindCosmosTest
    {
        public FindCosmosTestSet(FindCosmosFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindCosmosTestContext : FindCosmosTest
    {
        public FindCosmosTestContext(FindCosmosFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindCosmosTestNonGeneric : FindCosmosTest
    {
        public FindCosmosTestNonGeneric(FindCosmosFixture fixture)
            : base(fixture)
        {
        }

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
