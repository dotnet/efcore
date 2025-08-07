// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsStructuralEqualitySqliteTest(
    NavigationsSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsStructuralEqualityRelationalTestBase<NavigationsSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
LEFT JOIN "RelatedType" AS "r1" ON "r"."OptionalRelatedId" = "r1"."Id"
WHERE "r0"."Id" = "r1"."Id"
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
INNER JOIN "NestedType" AS "n" ON "r0"."RequiredNestedId" = "n"."Id"
LEFT JOIN "RelatedType" AS "r1" ON "r"."OptionalRelatedId" = "r1"."Id"
LEFT JOIN "NestedType" AS "n0" ON "r1"."RequiredNestedId" = "n0"."Id"
WHERE "n"."Id" = "n0"."Id"
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
LEFT JOIN "RelatedType" AS "r1" ON "r"."OptionalRelatedId" = "r1"."Id"
WHERE "r0"."Id" <> "r1"."Id" OR "r1"."Id" IS NULL
""");
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
LEFT JOIN "RelatedType" AS "r0" ON "r"."OptionalRelatedId" = "r0"."Id"
WHERE "r0"."Id" IS NULL
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
LEFT JOIN "RelatedType" AS "r0" ON "r"."OptionalRelatedId" = "r0"."Id"
WHERE "r0"."Id" IS NULL
""");
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
LEFT JOIN "NestedType" AS "n" ON "r0"."OptionalNestedId" = "n"."Id"
WHERE "n"."Id" IS NULL
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
INNER JOIN "NestedType" AS "n" ON "r0"."RequiredNestedId" = "n"."Id"
WHERE "n"."Id" = 1000
""");
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='1000' (Nullable = true)

SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
INNER JOIN "NestedType" AS "n" ON "r0"."RequiredNestedId" = "n"."Id"
WHERE "n"."Id" = @entity_equality_nested_Id
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelatedId", "r"."RequiredRelatedId"
FROM "RootEntity" AS "r"
INNER JOIN "RelatedType" AS "r0" ON "r"."RequiredRelatedId" = "r0"."Id"
LEFT JOIN "RelatedType" AS "r1" ON "r"."OptionalRelatedId" = "r1"."Id"
WHERE "r0"."Id" = "r1"."Id"
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
