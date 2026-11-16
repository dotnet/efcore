// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesStructuralEqualityCosmosTest : OwnedComplexPropertiesStructuralEqualityTestBase<OwnedComplexPropertiesCosmosFixture>
{
    public OwnedComplexPropertiesStructuralEqualityCosmosTest(OwnedComplexPropertiesCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
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
WHERE false
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

    public override Task Associate_with_parameter_null()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Associate_with_parameter_null);

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

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["NestedCollection"] = c["OptionalAssociate"]["NestedCollection"])
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["NestedCollection"] = [{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}])
""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """
@entity_equality_nestedCollection='[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}]'

SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["NestedCollection"] = @entity_equality_nestedCollection)
""");
    }

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
        await AssertTranslationFailed(base.Contains_with_nested_and_composed_operators);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
