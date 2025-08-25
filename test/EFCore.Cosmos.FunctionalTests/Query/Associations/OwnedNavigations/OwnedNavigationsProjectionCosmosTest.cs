// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsProjectionCosmosTest : OwnedNavigationsProjectionTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsProjectionCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    #region Simple properties

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["RequiredRelated"]["String"]
FROM root c
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalRelated is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalRelated!.String),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalRelated != null).Select(x => x.OptionalRelated!.String),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalRelated"]["String"]
FROM root c
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalRelated is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalRelated!.Int),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalRelated != null).Select(x => x.OptionalRelated!.Int),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalRelated"]["Int"]
FROM root c
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalRelated is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => (int?)x.OptionalRelated!.Int),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalRelated != null).Select(x => (int?)x.OptionalRelated!.Int),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalRelated"]["Int"]
FROM root c
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT VALUE c
FROM root c
""");
        }
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // When OptionalRelated is null, the property access on it evaluates to undefined in Cosmos, causing the
            // result to be filtered out entirely.
            await Assert.ThrowsAsync<NullReferenceException>(() => // #36403
                AssertQuery(
                    ss => ss.Set<RootEntity>().Select(x => x.OptionalRelated!.OptionalNested),
                    ss => ss.Set<RootEntity>().Where(x => x.OptionalRelated != null).Select(x => x.OptionalRelated!.OptionalNested),
                    queryTrackingBehavior: queryTrackingBehavior));

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

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // When OptionalRelated is null, the property access on it evaluates to undefined in Cosmos, causing the
            // result to be filtered out entirely.
            await Assert.ThrowsAsync<NullReferenceException>(() => // #36403
                AssertQuery(
                    ss => ss.Set<RootEntity>().Select(x => x.OptionalRelated!.RequiredNested),
                    ss => ss.Set<RootEntity>().Where(x => x.OptionalRelated != null).Select(x => x.OptionalRelated!.RequiredNested),
                    queryTrackingBehavior: queryTrackingBehavior));

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

    public override Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        // We don't support (inter-document) navigations with Cosmos.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_required_related_via_optional_navigation(queryTrackingBehavior));

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

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

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await Assert.ThrowsAsync<NullReferenceException>(()
                => base.Select_nested_collection_on_required_related(queryTrackingBehavior));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await Assert.ThrowsAsync<NullReferenceException>(()
                => base.Select_nested_collection_on_optional_related(queryTrackingBehavior));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(() => base.SelectMany_related_collection(queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(()
                => base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior));

            AssertSql();
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            // The given key 'n' was not present in the dictionary
            await Assert.ThrowsAsync<KeyNotFoundException>(()
                => base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior));

            AssertSql();
        }
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(() => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));
        }
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            await AssertTranslationFailed(() => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));
        }
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
