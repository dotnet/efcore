// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonReferenceProjectionSqlServerTest
    : OwnedJsonReferenceProjectionRelationalTestBase<OwnedJsonRelationshipsSqlServerFixture>
{
    public OwnedJsonReferenceProjectionSqlServerTest(OwnedJsonRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async)
    {
        await base.Select_root(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        await base.Select_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override Task Select_trunk_optional(bool async)
        => AssertCantTrackJson(() => base.Select_trunk_optional(async));

    public override Task Select_trunk_required(bool async)
        => AssertCantTrackJson(() => base.Select_trunk_required(async));

    public override Task Select_branch_required_required(bool async)
        => AssertCantTrackJson(() => base.Select_branch_required_required(async));

    public override Task Select_branch_required_optional(bool async)
        => AssertCantTrackJson(() => base.Select_branch_required_optional(async));

    public override  Task Select_branch_optional_required(bool async)
        => AssertCantTrackJson(() => base.Select_branch_optional_required(async));

    public override Task Select_branch_optional_optional(bool async)
        => AssertCantTrackJson(() => base.Select_branch_optional_optional(async));

    public override Task Select_trunk_and_branch_duplicated(bool async)
        => AssertCantTrackJson(() => base.Select_trunk_and_branch_duplicated(async));

    public override Task Select_trunk_and_trunk_duplicated(bool async)
        => AssertCantTrackJson(() => base.Select_trunk_and_trunk_duplicated(async));

    public override Task Select_leaf_trunk_root(bool async)
        => AssertCantTrackJson(() => base.Select_leaf_trunk_root(async));

    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackJson(() => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackJson(() => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async));

    private async Task AssertCantTrackJson(Func<Task> test)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery("AsNoTracking"), message);
        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
