// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public abstract class OwnedJsonStructuralEqualityRelationalTestBase<TFixture> : OwnedNavigationsStructuralEqualityTestBase<TFixture>
    where TFixture : OwnedJsonRelationalFixtureBase, new()
{
    public OwnedJsonStructuralEqualityRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // #36401
    public override Task Related_with_parameter_null()
        => Assert.ThrowsAsync<EqualException>(() => base.Related_with_parameter_null());

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
