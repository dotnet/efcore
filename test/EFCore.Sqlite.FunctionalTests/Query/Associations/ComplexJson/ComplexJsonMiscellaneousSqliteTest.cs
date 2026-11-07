// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousSqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate", "m"."RequiredAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS "m"
""");
    }

    public override async Task FromSql_on_root_composed(bool async)
    {
        await base.FromSql_on_root_composed(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate", "m"."RequiredAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS "m"
WHERE "m"."RequiredAssociate" ->> 'Int' = 8
""");
    }

    #region Left join

    public override async Task Where_on_associate_scalar_property_over_left_join()
    {
        await base.Where_on_associate_scalar_property_over_left_join();

        AssertSql(
            """
SELECT "r"."Id"
FROM "RootEntity" AS "r"
LEFT JOIN "RootEntity" AS "r0" ON "r"."Id" + 1 = "r0"."Id"
WHERE "r0"."RequiredAssociate" ->> 'Int' <> 8 OR "r0"."RequiredAssociate" ->> 'Int' IS NULL
""");
    }

    public override async Task Where_on_nested_associate_scalar_property_over_left_join()
    {
        await base.Where_on_nested_associate_scalar_property_over_left_join();

        AssertSql(
            """
SELECT "r"."Id"
FROM "RootEntity" AS "r"
LEFT JOIN "RootEntity" AS "r0" ON "r"."Id" + 1 = "r0"."Id"
WHERE "r0"."RequiredAssociate" ->> '$.RequiredNestedAssociate.Int' <> 8 OR "r0"."RequiredAssociate" ->> '$.RequiredNestedAssociate.Int' IS NULL
""");
    }

    public override async Task Where_on_associate_scalar_property_over_left_join_subquery()
    {
        await base.Where_on_associate_scalar_property_over_left_join_subquery();

        AssertSql(
            """
@p='5'

SELECT "r"."Id"
FROM "RootEntity" AS "r"
LEFT JOIN (
    SELECT "r0"."Id", "r0"."RequiredAssociate" AS "c1"
    FROM "RootEntity" AS "r0"
    ORDER BY "r0"."Id"
    LIMIT @p
) AS "r1" ON "r"."Id" + 1 = "r1"."Id"
WHERE "r1"."c1" ->> 'Int' <> 8 OR "r1"."c1" ->> 'Int' IS NULL
""");
    }

    #endregion Left join
}
