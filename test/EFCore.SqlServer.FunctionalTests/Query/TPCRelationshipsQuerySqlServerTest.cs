// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCRelationshipsQuerySqlServerTest
    : TPCRelationshipsQueryTestBase<TPCRelationshipsQuerySqlServerTest.TPCRelationshipsQuerySqlServerFixture>
{
    public TPCRelationshipsQuerySqlServerTest(
        TPCRelationshipsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact(Skip = "Issue#3170")]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalFact(Skip = "Issue#3170")]
    public override void Changes_in_derived_related_entities_are_detected()
    {
        base.Changes_in_derived_related_entities_are_detected();

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance(bool async)
    {
        await base.Include_collection_without_inheritance(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_with_filter(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived1(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived2(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived3(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_with_filter(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance(bool async)
    {
        await base.Include_reference_without_inheritance(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived1(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived2(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_on_derived_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_with_filter(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_without_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_without_inheritance_with_filter_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance(bool async)
    {
        await base.Include_reference_with_inheritance(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived1(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived2(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived4(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter1(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter2(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter4(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_on_derived_with_filter_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_with_filter(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_reference_with_inheritance_with_filter_reverse(bool async)
    {
        await base.Include_reference_with_inheritance_with_filter_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_self_reference_with_inheritance(bool async)
    {
        await base.Include_self_reference_with_inheritance(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_self_reference_with_inheritance_reverse(bool async)
    {
        await base.Include_self_reference_with_inheritance_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_collection_reference_on_non_entity_base(bool async)
    {
        await base.Nested_include_collection_reference_on_non_entity_base(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_collection(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_reference(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_reference(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_on_base(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
    {
        await base.Nested_include_with_inheritance_reference_reference_reverse(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Collection_projection_on_base_type(bool async)
    {
        await base.Collection_projection_on_base_type(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_split(bool async)
    {
        await base.Include_collection_with_inheritance_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_with_filter_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_split(bool async)
    {
        await base.Include_collection_without_inheritance_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_with_filter_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
    {
        await base.Include_collection_without_inheritance_with_filter_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived1_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived1_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived2_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived2_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived3_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived3_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
    {
        await base.Include_collection_with_inheritance_on_derived_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_on_base_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_reference_collection_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_reference_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_reference_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_collection_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
    {
        await base.Nested_include_with_inheritance_collection_collection_reverse_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Nested_include_collection_reference_on_non_entity_base_split(bool async)
    {
        await base.Nested_include_collection_reference_on_non_entity_base_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Collection_projection_on_base_type_split(bool async)
    {
        await base.Collection_projection_on_base_type_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast_split(async);

        AssertSql();
    }

    [ConditionalFact(Skip = "Issue#3170")]
    public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
    {
        base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class TPCRelationshipsQuerySqlServerFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
