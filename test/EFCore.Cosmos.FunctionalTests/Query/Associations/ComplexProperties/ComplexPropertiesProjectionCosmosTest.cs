// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesProjectionCosmosTest : ComplexPropertiesProjectionTestBase<ComplexPropertiesCosmosFixture>
{
    public ComplexPropertiesProjectionCosmosTest(ComplexPropertiesCosmosFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(outputHelper);
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

    #region Scalar properties

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["String"]
FROM root c
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.String),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalAssociate != null).Select(x => x.OptionalAssociate!.String),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["String"]
FROM root c
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.Int),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalAssociate != null).Select(x => x.OptionalAssociate!.Int),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["Int"]
FROM root c
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => (int?)x.OptionalAssociate!.Int),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalAssociate != null).Select(x => (int?)x.OptionalAssociate!.Int),
            queryTrackingBehavior: queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["Int"]
FROM root c
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        // We don't support (inter-document) navigations with Cosmos.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_required_associate_via_optional_navigation(queryTrackingBehavior));

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["Int"]
FROM root c
""");
    }

    #endregion Structural properties

    #region Structural collection properties

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE a
FROM root c
JOIN a IN c["AssociateCollection"]
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE n
FROM root c
JOIN n IN c["RequiredAssociate"]["NestedCollection"]
""");
    }

    [ConditionalTheory(Skip = "TODO: Query projection")]
    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE n
FROM root c
JOIN n IN c["OptionalAssociate"]["NestedCollection"]
""");
    }

    #endregion Structural collection properties

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

    public override Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertTranslationFailed(() => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));

    public override Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertTranslationFailed(() => base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior));

    #endregion Subquery

    #region Value types
    public override async Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_with_value_types(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    #endregion Value types


    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
