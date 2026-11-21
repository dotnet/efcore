// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesMiscellaneousRelationalTestBase<TFixture> : OwnedComplexPropertiesMiscellaneousTestBase<TFixture>
    where TFixture : OwnedComplexPropertiesRelationalFixtureBase, new()
{
    public OwnedComplexPropertiesMiscellaneousRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual Task FromSql_on_root()
        => RelationalAssociationsTests.FromSql_on_root(this, Fixture);

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
