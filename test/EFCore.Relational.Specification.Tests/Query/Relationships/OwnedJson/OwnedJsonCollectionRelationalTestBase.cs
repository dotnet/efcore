// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public abstract class OwnedJsonCollectionRelationalTestBase<TFixture> : OwnedNavigationsCollectionTestBase<TFixture>
    where TFixture : OwnedJsonRelationalFixtureBase, new()
{
    public OwnedJsonCollectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
