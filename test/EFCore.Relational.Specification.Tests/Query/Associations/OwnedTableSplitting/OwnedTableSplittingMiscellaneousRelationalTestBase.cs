// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public abstract class OwnedTableSplittingMiscellaneousRelationalTestBase<TFixture> : OwnedNavigationsMiscellaneousTestBase<TFixture>
    where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
    public OwnedTableSplittingMiscellaneousRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
