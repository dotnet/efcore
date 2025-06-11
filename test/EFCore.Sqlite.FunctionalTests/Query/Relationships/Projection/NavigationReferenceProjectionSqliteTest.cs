// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class NavigationReferenceProjectionSqliteTest
    : NavigationReferenceProjectionRelationalTestBase<NavigationRelationshipsSqliteFixture>
{
    public NavigationReferenceProjectionSqliteTest(NavigationRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async)))
            .Message);

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async)))
            .Message);
}
