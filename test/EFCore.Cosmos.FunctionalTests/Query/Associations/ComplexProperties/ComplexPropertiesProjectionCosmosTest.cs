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
SELECT c["RequiredAssociate"]["String"]
FROM root c
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // A single property projection is emitted as an object projection (without VALUE) so that documents where the
        // property access evaluates to undefined (e.g. OptionalAssociate is null) are retained rather than filtered out.
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT c["OptionalAssociate"]["String"]
FROM root c
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        // A single property projection is emitted as an object projection (without VALUE), so a value type property
        // accessed on a null OptionalAssociate surfaces as undefined and throws in the shaper, just like the
        // multi-property case.
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT c["OptionalAssociate"]["Int"]
FROM root c
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // A single property projection is emitted as an object projection (without VALUE) so that documents where the
        // property access evaluates to undefined (e.g. OptionalAssociate is null) are retained rather than filtered out.
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT c["OptionalAssociate"]["Int"]
FROM root c
""");
    }

    [Fact]
    public virtual async Task Select_nested_scalar_guarded_by_navigation_predicate_uses_VALUE()
    {
        // The predicate guarantees the navigation path is defined, so the optimal VALUE projection is preserved.
        await AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(x => x.OptionalAssociate != null && x.OptionalAssociate.OptionalNestedAssociate != null)
                .Select(x => x.OptionalAssociate!.OptionalNestedAssociate!.Int));

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["OptionalNestedAssociate"]["Int"]
FROM root c
WHERE ((c["OptionalAssociate"] != null) AND (c["OptionalAssociate"]["OptionalNestedAssociate"] != null))
""");
    }

    [Fact]
    public virtual async Task Select_nested_scalar_guarded_by_IsDefined_uses_VALUE()
    {
        // The IS_DEFINED guard guarantees the projected path is defined, so the optimal VALUE projection is preserved.
        await AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(x => EF.Functions.IsDefined(x.OptionalAssociate!.OptionalNestedAssociate!.Int))
                .Select(x => x.OptionalAssociate!.OptionalNestedAssociate!.Int),
            ss => ss.Set<RootEntity>()
                .Where(x => x.OptionalAssociate != null && x.OptionalAssociate.OptionalNestedAssociate != null)
                .Select(x => x.OptionalAssociate!.OptionalNestedAssociate!.Int));

        AssertSql(
            """
SELECT VALUE c["OptionalAssociate"]["OptionalNestedAssociate"]["Int"]
FROM root c
WHERE IS_DEFINED(c["OptionalAssociate"]["OptionalNestedAssociate"]["Int"])
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    [Fact]
    public Task Select_distinct_associate()
        => AssertTranslationFailed(() => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate).Distinct(),
            queryTrackingBehavior: QueryTrackingBehavior.NoTracking));

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
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

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT c["RequiredAssociate"]["Int"]
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
SELECT VALUE c
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
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        // TODO: Don't retrieve the entire document. Issue #34067
        AssertSql(
            """
SELECT VALUE c
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
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior));

        AssertSql(
            """
SELECT c["Id"], c
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

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
