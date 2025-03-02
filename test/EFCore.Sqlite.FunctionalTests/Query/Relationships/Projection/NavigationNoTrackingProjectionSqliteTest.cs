// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class NavigationNoTrackingProjectionSqliteTest
    : NavigationNoTrackingProjectionRelationalTestBase<NavigationRelationshipsSqliteFixture>
{
    public NavigationNoTrackingProjectionSqliteTest(NavigationRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async)))
            .Message);

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async)))
            .Message);

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async)))
            .Message);
}
