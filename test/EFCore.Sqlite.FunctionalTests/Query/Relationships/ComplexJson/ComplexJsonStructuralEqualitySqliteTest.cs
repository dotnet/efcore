// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonStructuralEqualitySqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonStructuralEqualityRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" = "r"."OptionalRelated"
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'RequiredNested' = "r"."OptionalRelated" ->> 'RequiredNested'
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" <> "r"."OptionalRelated" OR "r"."OptionalRelated" IS NULL
""");
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalRelated" IS NULL
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalRelated" IS NULL
""");
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'OptionalNested' IS NULL
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'RequiredNested' = '{"Id":1000,"Int":8,"Name":"Root1_RequiredRelated_RequiredNested","String":"foo"}'
""");
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql(
            """
@entity_equality_nested='?' (Size = 80)

SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'RequiredNested' = @entity_equality_nested
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'NestedCollection' = "r"."OptionalRelated" ->> 'NestedCollection'
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'NestedCollection' = '[{"Id":1002,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Name":"Root1_RequiredRelated_NestedCollection_2","String":"foo"}]'
""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """
@entity_equality_nestedCollection='?' (Size = 171)

SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredRelated" ->> 'NestedCollection' = @entity_equality_nestedCollection
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
