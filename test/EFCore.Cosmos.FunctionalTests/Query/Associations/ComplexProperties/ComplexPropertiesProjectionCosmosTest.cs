// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["String"]
FROM root c
""");
    }

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

    [Fact]
    public virtual Task Select_scalar_on_distinct_required_associate()
        => AssertTranslationFailed(() => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate).Distinct().Select(x => x.String),
            queryTrackingBehavior: QueryTrackingBehavior.NoTracking));


    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]
FROM root c
""");
    }

    [Fact]
    public async Task Select_distinct_associate()
    {
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate).Distinct(),
            queryTrackingBehavior: QueryTrackingBehavior.NoTracking);

        AssertSql(
            """
SELECT DISTINCT VALUE c["RequiredAssociate"]
FROM root c
""");
    }

    [Fact]
    public async Task Select_distinct_nested_associate()
    {
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate.RequiredNestedAssociate).Distinct(),
            queryTrackingBehavior: QueryTrackingBehavior.NoTracking);

        AssertSql(
            """
SELECT DISTINCT VALUE c["RequiredAssociate"]["RequiredNestedAssociate"]
FROM root c
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]
FROM root c
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["RequiredNestedAssociate"]
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["OptionalNestedAssociate"]
FROM root c
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.OptionalNestedAssociate),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalAssociate != null).Select(x => x.OptionalAssociate!.OptionalNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["OptionalNestedAssociate"]
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.RequiredNestedAssociate),
            ss => ss.Set<RootEntity>().Where(x => x.OptionalAssociate != null).Select(x => x.OptionalAssociate!.RequiredNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["RequiredNestedAssociate"]
FROM root c
""");
    }

    public override Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        // We don't support (inter-document) navigations with Cosmos.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_required_associate_via_optional_navigation(queryTrackingBehavior));

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]
FROM root c
""");
    }

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

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["AssociateCollection"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]["NestedCollection"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // When OptionalAssociate is null, the property access on it evaluates to undefined in Cosmos, causing the
        // result to be filtered out entirely.
        await AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalAssociate!.NestedCollection),
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Where(x => x.OptionalAssociate != null).Select(x => x.OptionalAssociate!.NestedCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["NestedCollection"]
FROM root c
ORDER BY c["Id"]
""");
    }

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

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        AssertSql(
            """
SELECT c["Id"], c["RequiredAssociate"]
FROM root c
""");
    }

    public override async Task Select_required_associate_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["RequiredAssociate"]
FROM root c
""");
    }

    public override async Task Select_required_associate_and_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_and_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE
{
    "First" : c["RequiredAssociate"],
    "Second" : c["OptionalAssociate"]
}
FROM root c
""");
    }

    public override async Task Select_optional_associate_and_ints(QueryTrackingBehavior queryTrackingBehavior)
    {
        // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/335
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Select_optional_associate_and_ints(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE
{
    "First" : c["OptionalAssociate"],
    "Ints" : c["RequiredAssociate"]["Ints"]
}
FROM root c
""");
    }

    #endregion Multiple

    #region Subquery

    public override Task Select_subquery_FirstOrDefault_complex_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertTranslationFailed(() => base.Select_subquery_FirstOrDefault_complex_collection(queryTrackingBehavior));

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
SELECT VALUE c["RequiredAssociate"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_nullable_value_type_with_Value(queryTrackingBehavior));
        Assert.Equal("Nullable object must have a value.", ex.Message);

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]
FROM root c
ORDER BY c["Id"]
""");
    }

    #endregion Value types

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
