// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class TPHInheritanceTableSplittingQueryRelationalTestBase<TFixture> : InheritanceComplexTypesQueryTestBase<TFixture>
    where TFixture : TPHInheritanceQueryFixture, new()
{
    protected TPHInheritanceTableSplittingQueryRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
