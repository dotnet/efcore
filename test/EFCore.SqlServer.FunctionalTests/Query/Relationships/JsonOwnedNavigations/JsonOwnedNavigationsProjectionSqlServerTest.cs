// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.JsonOwnedNavigations;

public class JsonOwnedNavigationsProjectionSqlServerTest
    : JsonOwnedNavigationsProjectionRelationalTestBase<JsonOwnedNavigationsSqlServerFixture>
{
    public JsonOwnedNavigationsProjectionSqlServerTest(JsonOwnedNavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[OptionalReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[RequiredReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[CollectionTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_multiple_branch_leaf(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.OptionalReferenceLeaf'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.CollectionLeaf'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), JSON_VALUE([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.OptionalReferenceLeaf.Name')
FROM [RootEntities] AS [r]
""");
        }
    }

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[OptionalReferenceTrunk], [r].[Id], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch'), [r].[OptionalReferenceTrunk], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[RequiredReferenceTrunk], [r].[Id], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf'), [r].[RequiredReferenceTrunk], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    #endregion Subquery

    #region SelectMany

    public override async Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
        }
    }

    #endregion SelectMany

    private async Task AssertCantTrackJson(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
    {
        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

            Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
            AssertSql();

            return;
        }

        await test();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
