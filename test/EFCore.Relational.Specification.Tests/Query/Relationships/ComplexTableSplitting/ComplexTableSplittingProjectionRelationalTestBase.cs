// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public abstract class ComplexTableSplittingProjectionRelationalTestBase<TFixture>
    : RelationshipsProjectionTestBase<TFixture>
    where TFixture : ComplexTableSplittingRelationalFixtureBase, new()
{
    public ComplexTableSplittingProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Collections are not supported with table splitting, only JSON
    public override Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_nested_collection_on_required_related(queryTrackingBehavior));

    // Collections are not supported with table splitting, only JSON
    public override Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_nested_collection_on_optional_related(queryTrackingBehavior));

    // Collections are not supported with table splitting, only JSON
    public override Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_related_collection(queryTrackingBehavior));

    // Collections are not supported with table splitting, only JSON
    public override Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(()
            => base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior));

    // Collections are not supported with table splitting, only JSON
    public override Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(()
            => base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
