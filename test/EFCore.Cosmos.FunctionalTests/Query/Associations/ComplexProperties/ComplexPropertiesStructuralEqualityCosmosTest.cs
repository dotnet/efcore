// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesStructuralEqualityCosmosTest : ComplexPropertiesStructuralEqualityTestBase<ComplexPropertiesCosmosFixture>
{
    public ComplexPropertiesStructuralEqualityCosmosTest(ComplexPropertiesCosmosFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        Environment.SetEnvironmentVariable("EF_TEST_REWRITE_BASELINES", "1");

        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(outputHelper);
    }

    public override async Task Two_associates()
    {
        await base.Two_associates();
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"] = c["OptionalAssociate"])
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["RequiredNestedAssociate"] = c["OptionalAssociate"]["RequiredNestedAssociate"])
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"] != c["OptionalAssociate"])
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"] = null)
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
@entity_equality_related='null'

SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"] = @entity_equality_related)
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["OptionalNestedAssociate"] = null)
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["RequiredNestedAssociate"] = {"Id":1000,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_RequiredNestedAssociate","String":"foo"})
""");
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
            """
@entity_equality_nested='{"Id":1000,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_RequiredNestedAssociate","String":"foo"}'

SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["RequiredNestedAssociate"] = @entity_equality_nested)
""");
    }

    [ConditionalFact]
    public async Task Nested_associate_with_parameter_null()
    {
        NestedAssociateType? nested = null;
        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.OptionalNestedAssociate == nested));

        AssertSql(
            """
@entity_equality_nested='null'

SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["OptionalNestedAssociate"] = @entity_equality_nested)
""");
    }

    [ConditionalFact]
    public async Task Nested_associate_with_parameter_not_null()
    {
        NestedAssociateType? nested = null;
        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.OptionalNestedAssociate != nested));

        AssertSql(
            """
@entity_equality_nested='null'

SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["OptionalNestedAssociate"] != @entity_equality_nested)
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """

""");
}

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """

""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """

""");
    }

    [ConditionalFact]
    public override async Task Nullable_value_type_with_null()
    {
        await base.Nullable_value_type_with_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"] = null)
""");
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE (n = {"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"}))
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@entity_equality_nested='{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"}'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE (n = @entity_equality_nested))
""");
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql(
            """
@get_Item_Int='106'
@entity_equality_get_Item='{"Id":3003,"Int":108,"Ints":[8,9,109],"Name":"Root3_RequiredAssociate_NestedCollection_2","String":"foo104"}'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE ((n["Int"] > @get_Item_Int) AND (n = @entity_equality_get_Item)))
""");
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql(
            """
@get_Item_Id='302'
@entity_equality_get_Item='{"Id":303,"Int":130,"Ints":[8,9,131],"Name":"Root3_AssociateCollection_2","String":"foo115","NestedCollection":[{"Id":3014,"Int":136,"Ints":[8,9,137],"Name":"Root3_AssociateCollection_2_NestedCollection_1","String":"foo118"},{"Id":3015,"Int":138,"Ints":[8,9,139],"Name":"Root3_Root1_AssociateCollection_2_NestedCollection_2","String":"foo119"}],"OptionalNestedAssociate":{"Id":3013,"Int":134,"Ints":[8,9,135],"Name":"Root3_AssociateCollection_2_OptionalNestedAssociate","String":"foo117"},"RequiredNestedAssociate":{"Id":3012,"Int":132,"Ints":[8,9,133],"Name":"Root3_AssociateCollection_2_RequiredNestedAssociate","String":"foo116"}}'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM a IN c["AssociateCollection"]
    WHERE ((a["Id"] > @get_Item_Id) AND (a = @entity_equality_get_Item)))
""");
    }

    #endregion Contains

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
