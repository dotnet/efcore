// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsProjectionCosmosTest : OwnedNavigationsProjectionTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsProjectionCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_root(async, queryTrackingBehavior);

            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                AssertSql();
            }
            else
            {
                await base.Select_trunk_collection(async, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);

                AssertSql();
            }
            else
            {
                //issue #31696
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => base.Select_branch_required_collection(async, queryTrackingBehavior));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);

                AssertSql();
            }
            else
            {
                //issue #31696
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => base.Select_branch_optional_collection(async, queryTrackingBehavior));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    #region Multiple

    public override Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_root_duplicated(a, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override async Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);

                AssertSql();
            }
            else
            {
                //issue #31696
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);

                AssertSql();
            }
            else
            {
                //issue #31696
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            }
        }
    }

    public override async Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                // This particular scenario does not throw an exception in TrackAll mode as it's supposed to
                await Assert.ThrowsAsync<ThrowsException>(() => base.Select_leaf_trunk_root(async, queryTrackingBehavior));
            }
            else
            {
                await base.Select_leaf_trunk_root(async, queryTrackingBehavior);
            }

            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);
            }
            else
            {
                //issue #35702
                await Assert.ThrowsAsync<ArgumentException>(
                    () => base.Select_multiple_branch_leaf(async, queryTrackingBehavior));
            }

            AssertSql();
        }
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        // For TrackAll, the base implementation expects a different exception to be thrown
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(
                () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        // For TrackAll, the base implementation expects a different exception to be thrown
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(
                () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        // For TrackAll, the base implementation expects a different exception to be thrown
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(
                () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        // For TrackAll, the base implementation expects a different exception to be thrown
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(
                () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        // For TrackAll, the base implementation expects a different exception to be thrown
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(
                () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    #endregion Subquery

    #region SelectMany

    public override async Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);
            }
            else
            {
                //issue #34349
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => base.SelectMany_trunk_collection(async, queryTrackingBehavior));
            }

            AssertSql();
        }
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);
            }
            else
            {
                //issue #34349
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior));
            }

            AssertSql();
        }
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
            {
                await base.Select_branch_required_collection(async, queryTrackingBehavior);
            }
            else
            {
                //issue #34349
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior));
            }

            AssertSql();
        }
    }

    #endregion SelectMany

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
