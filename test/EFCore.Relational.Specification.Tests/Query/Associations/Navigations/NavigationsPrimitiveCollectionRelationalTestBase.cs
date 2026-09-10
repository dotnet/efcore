// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsPrimitiveCollectionRelationalTestBase<TFixture> : NavigationsPrimitiveCollectionTestBase<TFixture>
    where TFixture : NavigationsRelationalFixtureBase, new()
{
    public NavigationsPrimitiveCollectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
