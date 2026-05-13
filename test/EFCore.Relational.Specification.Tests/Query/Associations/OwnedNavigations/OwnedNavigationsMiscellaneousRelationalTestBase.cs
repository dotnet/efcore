// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsMiscellaneousRelationalTestBase<TFixture> : OwnedNavigationsMiscellaneousTestBase<TFixture>
    where TFixture : OwnedNavigationsRelationalFixtureBase, new()
{
    public OwnedNavigationsMiscellaneousRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual Task FromSql_on_root()
        => RelationalAssociationsTests.FromSql_on_root(this, Fixture);

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
