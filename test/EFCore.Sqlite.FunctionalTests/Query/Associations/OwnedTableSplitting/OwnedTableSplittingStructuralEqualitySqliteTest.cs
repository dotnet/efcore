// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingStructuralEqualitySqliteTest(
    OwnedTableSplittingSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingStructuralEqualityRelationalTestBase<OwnedTableSplittingSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE 0
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE 0
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE 0
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE "r"."OptionalRelated_Id" IS NULL OR "r"."OptionalRelated_Int" IS NULL OR "r"."OptionalRelated_Name" IS NULL OR "r"."OptionalRelated_String" IS NULL
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END IS NULL
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE "r"."RequiredRelated_OptionalNested_Id" IS NULL OR "r"."RequiredRelated_OptionalNested_Int" IS NULL OR "r"."RequiredRelated_OptionalNested_Name" IS NULL OR "r"."RequiredRelated_OptionalNested_String" IS NULL
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql(
        );
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql(
        );
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated_Id", "r"."OptionalRelated_Int", "r"."OptionalRelated_Name", "r"."OptionalRelated_String", "o"."RelatedTypeRootEntityId", "o"."Id", "o"."Int", "o"."Name", "o"."String", "r"."OptionalRelated_OptionalNested_Id", "r"."OptionalRelated_OptionalNested_Int", "r"."OptionalRelated_OptionalNested_Name", "r"."OptionalRelated_OptionalNested_String", "r"."OptionalRelated_RequiredNested_Id", "r"."OptionalRelated_RequiredNested_Int", "r"."OptionalRelated_RequiredNested_Name", "r"."OptionalRelated_RequiredNested_String", "s"."RootEntityId", "s"."Id", "s"."Int", "s"."Name", "s"."String", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "s"."Int0", "s"."Name0", "s"."String0", "s"."OptionalNested_Id", "s"."OptionalNested_Int", "s"."OptionalNested_Name", "s"."OptionalNested_String", "s"."RequiredNested_Id", "s"."RequiredNested_Int", "s"."RequiredNested_Name", "s"."RequiredNested_String", "r"."RequiredRelated_Id", "r"."RequiredRelated_Int", "r"."RequiredRelated_Name", "r"."RequiredRelated_String", "r2"."RelatedTypeRootEntityId", "r2"."Id", "r2"."Int", "r2"."Name", "r2"."String", "r"."RequiredRelated_OptionalNested_Id", "r"."RequiredRelated_OptionalNested_Int", "r"."RequiredRelated_OptionalNested_Name", "r"."RequiredRelated_OptionalNested_String", "r"."RequiredRelated_RequiredNested_Id", "r"."RequiredRelated_RequiredNested_Int", "r"."RequiredRelated_RequiredNested_Name", "r"."RequiredRelated_RequiredNested_String"
FROM "RootEntity" AS "r"
LEFT JOIN "OptionalRelated_NestedCollection" AS "o" ON CASE
    WHEN "r"."OptionalRelated_Id" IS NOT NULL AND "r"."OptionalRelated_Int" IS NOT NULL AND "r"."OptionalRelated_Name" IS NOT NULL AND "r"."OptionalRelated_String" IS NOT NULL THEN "r"."Id"
END = "o"."RelatedTypeRootEntityId"
LEFT JOIN (
    SELECT "r0"."RootEntityId", "r0"."Id", "r0"."Int", "r0"."Name", "r0"."String", "r1"."RelatedTypeRootEntityId", "r1"."RelatedTypeId", "r1"."Id" AS "Id0", "r1"."Int" AS "Int0", "r1"."Name" AS "Name0", "r1"."String" AS "String0", "r0"."OptionalNested_Id", "r0"."OptionalNested_Int", "r0"."OptionalNested_Name", "r0"."OptionalNested_String", "r0"."RequiredNested_Id", "r0"."RequiredNested_Int", "r0"."RequiredNested_Name", "r0"."RequiredNested_String"
    FROM "RelatedCollection" AS "r0"
    LEFT JOIN "RelatedCollection_NestedCollection" AS "r1" ON "r0"."RootEntityId" = "r1"."RelatedTypeRootEntityId" AND "r0"."Id" = "r1"."RelatedTypeId"
) AS "s" ON "r"."Id" = "s"."RootEntityId"
LEFT JOIN "RequiredRelated_NestedCollection" AS "r2" ON "r"."Id" = "r2"."RelatedTypeRootEntityId"
WHERE 0
ORDER BY "r"."Id", "o"."RelatedTypeRootEntityId", "o"."Id", "s"."RootEntityId", "s"."Id", "s"."RelatedTypeRootEntityId", "s"."RelatedTypeId", "s"."Id0", "r2"."RelatedTypeRootEntityId"
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

    #region Contains

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        AssertSql();
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql();
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql();
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql();
    }

    #endregion Contains

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
