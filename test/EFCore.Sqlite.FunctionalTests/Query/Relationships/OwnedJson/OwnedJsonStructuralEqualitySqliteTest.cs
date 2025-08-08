// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonStructuralEqualitySqliteTest(
    OwnedJsonSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonStructuralEqualityRelationalTestBase<OwnedJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE 0
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
WHERE 0
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
SELECT "r"."Id", "r"."Name", "r"."OptionalRelated", "r"."RelatedCollection", "r"."RequiredRelated"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
);
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
