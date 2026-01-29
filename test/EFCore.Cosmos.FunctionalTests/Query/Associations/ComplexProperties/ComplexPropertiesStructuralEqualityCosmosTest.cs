// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesStructuralEqualityCosmosTest : ComplexPropertiesStructuralEqualityTestBase<ComplexPropertiesCosmosFixture>
{
    public ComplexPropertiesStructuralEqualityCosmosTest(ComplexPropertiesCosmosFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(outputHelper);
    }

    public override Task Two_associates()
        => AssertTranslationFailed(base.Two_associates);

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["RequiredAssociate"]["RequiredNestedAssociate"] = null) AND (c["OptionalAssociate"]["RequiredNestedAssociate"] = null)) OR ((c["OptionalAssociate"]["RequiredNestedAssociate"] != null) AND (((((c["RequiredAssociate"]["RequiredNestedAssociate"]["Id"] = c["OptionalAssociate"]["RequiredNestedAssociate"]["Id"]) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Int"] = c["OptionalAssociate"]["RequiredNestedAssociate"]["Int"])) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Ints"] = c["OptionalAssociate"]["RequiredNestedAssociate"]["Ints"])) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Name"] = c["OptionalAssociate"]["RequiredNestedAssociate"]["Name"])) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["String"] = c["OptionalAssociate"]["RequiredNestedAssociate"]["String"]))))
""");
    }

    public override Task Not_equals()
        => AssertTranslationFailed(base.Not_equals); // Complex collection equality... Need ALL support

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

    public override Task Associate_with_parameter_null()
        => AssertTranslationFailed(base.Associate_with_parameter_null); // Complex collection equality... Need ALL support

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
WHERE (((((c["RequiredAssociate"]["RequiredNestedAssociate"]["Id"] = 1000) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Int"] = 8)) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Ints"] = [1,2,3])) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Name"] = "Root1_RequiredAssociate_RequiredNestedAssociate")) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["String"] = "foo"))
""");
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
            """
@entity_equality_nested='{}'
@entity_equality_nested_Id='1000'
@entity_equality_nested_Int='8'
@entity_equality_nested_Ints='[1,2,3]'
@entity_equality_nested_Name='Root1_RequiredAssociate_RequiredNestedAssociate'
@entity_equality_nested_String='foo'

SELECT VALUE c
FROM root c
WHERE (((c["RequiredAssociate"]["RequiredNestedAssociate"] = null) AND (@entity_equality_nested = null)) OR ((@entity_equality_nested != null) AND (((((c["RequiredAssociate"]["RequiredNestedAssociate"]["Id"] = @entity_equality_nested_Id) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Int"] = @entity_equality_nested_Int)) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Ints"] = @entity_equality_nested_Ints)) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["Name"] = @entity_equality_nested_Name)) AND (c["RequiredAssociate"]["RequiredNestedAssociate"]["String"] = @entity_equality_nested_String))))
""");
    }

    [ConditionalFact]
    public async Task Nested_associate_with_parameter_null()
    {
        NestedAssociateType? nested = null;
        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.OptionalNestedAssociate == nested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.OptionalNestedAssociate == nested));

        AssertSql(
            """
@entity_equality_nested=null
@entity_equality_nested_Id=null
@entity_equality_nested_Int=null
@entity_equality_nested_Ints=null
@entity_equality_nested_Name=null
@entity_equality_nested_String=null

SELECT VALUE c
FROM root c
WHERE (((c["RequiredAssociate"]["OptionalNestedAssociate"] = null) AND (@entity_equality_nested = null)) OR ((@entity_equality_nested != null) AND (((((c["RequiredAssociate"]["OptionalNestedAssociate"]["Id"] = @entity_equality_nested_Id) AND (c["RequiredAssociate"]["OptionalNestedAssociate"]["Int"] = @entity_equality_nested_Int)) AND (c["RequiredAssociate"]["OptionalNestedAssociate"]["Ints"] = @entity_equality_nested_Ints)) AND (c["RequiredAssociate"]["OptionalNestedAssociate"]["Name"] = @entity_equality_nested_Name)) AND (c["RequiredAssociate"]["OptionalNestedAssociate"]["String"] = @entity_equality_nested_String))))
""");
    }

    public override Task Two_nested_collections()
        => AssertTranslationFailed(base.Two_nested_collections); // Complex collection equality... Need ALL support

    public override Task Nested_collection_with_inline()
       => AssertTranslationFailed(base.Nested_collection_with_inline); // Complex collection equality... Need ALL support

    public override Task Nested_collection_with_parameter()
       => AssertTranslationFailed(base.Nested_collection_with_parameter); // Complex collection equality... Need ALL support

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
    WHERE (((((n["Id"] = 1002) AND (n["Int"] = 8)) AND (n["Ints"] = [1,2,3])) AND (n["Name"] = "Root1_RequiredAssociate_NestedCollection_1")) AND (n["String"] = "foo")))
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@entity_equality_nested='{}'
@entity_equality_nested_Id='1002'
@entity_equality_nested_Int='8'
@entity_equality_nested_Ints='[1,2,3]'
@entity_equality_nested_Name='Root1_RequiredAssociate_NestedCollection_1'
@entity_equality_nested_String='foo'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE (((n = null) AND (@entity_equality_nested = null)) OR ((@entity_equality_nested != null) AND (((((n["Id"] = @entity_equality_nested_Id) AND (n["Int"] = @entity_equality_nested_Int)) AND (n["Ints"] = @entity_equality_nested_Ints)) AND (n["Name"] = @entity_equality_nested_Name)) AND (n["String"] = @entity_equality_nested_String)))))
""");
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql(
            """
@get_Item_Int='106'
@entity_equality_get_Item='{}'
@entity_equality_get_Item_Id='3003'
@entity_equality_get_Item_Int='108'
@entity_equality_get_Item_Ints='[8,9,109]'
@entity_equality_get_Item_Name='Root3_RequiredAssociate_NestedCollection_2'
@entity_equality_get_Item_String='foo104'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE ((n["Int"] > @get_Item_Int) AND (((n = null) AND (@entity_equality_get_Item = null)) OR ((@entity_equality_get_Item != null) AND (((((n["Id"] = @entity_equality_get_Item_Id) AND (n["Int"] = @entity_equality_get_Item_Int)) AND (n["Ints"] = @entity_equality_get_Item_Ints)) AND (n["Name"] = @entity_equality_get_Item_Name)) AND (n["String"] = @entity_equality_get_Item_String))))))
""");
    }

    public override async Task Contains_with_nested_and_composed_operators()
        => await AssertTranslationFailed(base.Contains_with_nested_and_composed_operators); // Complex collection equality... Need ALL support

    #endregion Contains

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
