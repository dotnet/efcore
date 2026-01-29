// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonStructuralEqualitySqliteTest(
    OwnedJsonSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonStructuralEqualityRelationalTestBase<OwnedJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalAssociate" IS NULL
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE 0
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredAssociate" ->> 'OptionalNestedAssociate' IS NULL
""");
    }

    public override async Task Optional_associate_nested_associate_with_inline_null()
    {
        await base.Optional_associate_nested_associate_with_inline_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalAssociate" ->> 'OptionalNestedAssociate' IS NULL
""");
    }

    public override async Task Optional_associate_nested_associate_with_inline_not_null()
    {
        await base.Optional_associate_nested_associate_with_inline_not_null();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE "r"."OptionalAssociate" ->> 'OptionalNestedAssociate' IS NOT NULL
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
        );
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
        );
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate", "r"."RequiredAssociate"
FROM "RootEntity" AS "r"
WHERE 0
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
