// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public abstract class ComplexJsonMiscellaneousRelationalTestBase<TFixture> : ComplexPropertiesMiscellaneousTestBase<TFixture>
    where TFixture : ComplexJsonRelationalFixtureBase, new()
{
    public ComplexJsonMiscellaneousRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
