// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsStructuralEqualityRelationalTestBase<TFixture> : NavigationsStructuralEqualityTestBase<TFixture>
    where TFixture : NavigationsRelationalFixtureBase, new()
{
    public NavigationsStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Traditional relational collections navigations can't be compared reliably.
    // The failure below is because collections on null instances are returned as empty collections rather than null; but
    // even disregarding that, elements in the collection don't preserve ordering and so can't be compared reliably.
    public override Task Two_nested_collections()
        => Assert.ThrowsAsync<EqualException>(() => base.Two_nested_collections());

    public override Task Nested_collection_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline());

    public override Task Nested_collection_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter());

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
