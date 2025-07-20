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

    public override Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_root(a, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    #region Simple properties

    public override Task Select_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_related_property(a, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c["RequiredRelated"]["String"]
FROM root c
""");
            });

    public override Task Select_optional_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_optional_related_property(a, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c["OptionalRelated"]["String"]
FROM root c
""");
            });

    public override Task Select_optional_related_property_value_type(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_optional_related_property_value_type(a, queryTrackingBehavior);

                AssertSql(
                    """
SELECT VALUE c["OptionalRelated"]["Int"]
FROM root c
""");
            });

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_related(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    public override async Task Select_optional_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_optional_related(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    public override async Task Select_required_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_required_related_required_nested(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    public override async Task Select_required_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_required_related_optional_nested(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    public override async Task Select_optional_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_optional_related_required_nested(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    public override async Task Select_optional_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_optional_related_optional_nested(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
            {
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            }
        }
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async)
        {
            await base.Select_related_collection(async, queryTrackingBehavior);

            if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
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

    public override async Task Select_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async && queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => base.Select_required_related_nested_collection(async, queryTrackingBehavior));

            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async && queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => base.Select_optional_related_nested_collection(async, queryTrackingBehavior));

            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async && queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(() => base.SelectMany_related_collection(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task SelectMany_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async && queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(() => base.SelectMany_required_related_nested_collection(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task SelectMany_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (async && queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(() => base.SelectMany_optional_related_nested_collection(async, queryTrackingBehavior));

            AssertSql();
        }
    }

    #endregion Collection

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

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(() => base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior));
        }
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(() => base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior));
        }
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
