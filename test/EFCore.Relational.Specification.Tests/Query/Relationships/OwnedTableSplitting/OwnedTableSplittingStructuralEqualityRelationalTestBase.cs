// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

public abstract class OwnedTableSplittingStructuralEqualityRelationalTestBase<TFixture> : OwnedNavigationsStructuralEqualityTestBase<TFixture>
    where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
    public OwnedTableSplittingStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
