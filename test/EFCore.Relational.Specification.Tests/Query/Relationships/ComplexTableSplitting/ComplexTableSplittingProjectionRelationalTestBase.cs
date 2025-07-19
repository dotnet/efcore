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

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<NotNullException>(() => base.Select_related_collection(async, queryTrackingBehavior));

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task Select_nested_collection_on_required_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_nested_collection_on_required_related(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task Select_optional_nested_on_required_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_optional_nested_on_required_related(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior));

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_related_collection(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task SelectMany_nested_collection_on_required_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_nested_collection_on_required_related(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task SelectMany_nested_collection_on_optional_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_nested_collection_on_optional_related(async, queryTrackingBehavior));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
