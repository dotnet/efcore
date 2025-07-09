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

    // Optional complex types, #31376
    public override Task Select_optional_related_property_value_type(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_optional_related_property_value_type(async, queryTrackingBehavior));

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<FailException>(() => base.Select_related_collection(async, queryTrackingBehavior));

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task Select_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_required_related_nested_collection(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task Select_required_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_required_related_optional_nested(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior));

    // Complex JSON collections, update pipeline not yet supported so no seeding, #31237
    public override Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_related_collection(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task SelectMany_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_required_related_nested_collection(async, queryTrackingBehavior));

    // Optional complex types, #31376
    public override Task SelectMany_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.SelectMany_optional_related_nested_collection(async, queryTrackingBehavior));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
