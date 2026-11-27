// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

public class ComplexTableSplittingNestedJsonMiscellaneousSqliteTest(ComplexTableSplittingNestedJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingNestedJsonMiscellaneousRelationalTestBase<ComplexTableSplittingNestedJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate_Id", "m"."OptionalAssociate_Int", "m"."OptionalAssociate_Ints", "m"."OptionalAssociate_Name", "m"."OptionalAssociate_String", "m"."OptionalAssociate_NestedCollection", "m"."OptionalAssociate_OptionalNestedAssociate", "m"."OptionalAssociate_RequiredNestedAssociate", "m"."RequiredAssociate_Id", "m"."RequiredAssociate_Int", "m"."RequiredAssociate_Ints", "m"."RequiredAssociate_Name", "m"."RequiredAssociate_String", "m"."RequiredAssociate_NestedCollection", "m"."RequiredAssociate_OptionalNestedAssociate", "m"."RequiredAssociate_RequiredNestedAssociate"
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
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate_Id", "m"."OptionalAssociate_Int", "m"."OptionalAssociate_Ints", "m"."OptionalAssociate_Name", "m"."OptionalAssociate_String", "m"."OptionalAssociate_NestedCollection", "m"."OptionalAssociate_OptionalNestedAssociate", "m"."OptionalAssociate_RequiredNestedAssociate", "m"."RequiredAssociate_Id", "m"."RequiredAssociate_Int", "m"."RequiredAssociate_Ints", "m"."RequiredAssociate_Name", "m"."RequiredAssociate_String", "m"."RequiredAssociate_NestedCollection", "m"."RequiredAssociate_OptionalNestedAssociate", "m"."RequiredAssociate_RequiredNestedAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS "m"
WHERE "m"."RequiredAssociate_Int" = 8
""");
    }

    public override async Task Two_associates_equality()
    {
        await base.Two_associates_equality();

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."AssociateCollection", "r"."OptionalAssociate_Id", "r"."OptionalAssociate_Int", "r"."OptionalAssociate_Ints", "r"."OptionalAssociate_Name", "r"."OptionalAssociate_String", "r"."OptionalAssociate_NestedCollection", "r"."OptionalAssociate_OptionalNestedAssociate", "r"."OptionalAssociate_RequiredNestedAssociate", "r"."RequiredAssociate_Id", "r"."RequiredAssociate_Int", "r"."RequiredAssociate_Ints", "r"."RequiredAssociate_Name", "r"."RequiredAssociate_String", "r"."RequiredAssociate_NestedCollection", "r"."RequiredAssociate_OptionalNestedAssociate", "r"."RequiredAssociate_RequiredNestedAssociate"
FROM "RootEntity" AS "r"
WHERE "r"."RequiredAssociate_Id" = "r"."OptionalAssociate_Id" AND "r"."RequiredAssociate_Int" = "r"."OptionalAssociate_Int" AND "r"."RequiredAssociate_Ints" = "r"."OptionalAssociate_Ints" AND "r"."RequiredAssociate_Name" = "r"."OptionalAssociate_Name" AND "r"."RequiredAssociate_String" = "r"."OptionalAssociate_String" AND "r"."RequiredAssociate_NestedCollection" = "r"."OptionalAssociate_NestedCollection" AND ("r"."RequiredAssociate_OptionalNestedAssociate" = "r"."OptionalAssociate_OptionalNestedAssociate" OR ("r"."RequiredAssociate_OptionalNestedAssociate" IS NULL AND "r"."OptionalAssociate_OptionalNestedAssociate" IS NULL)) AND "r"."RequiredAssociate_RequiredNestedAssociate" = "r"."OptionalAssociate_RequiredNestedAssociate"
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
WHERE "r0"."RequiredAssociate_Int" <> 8 OR "r0"."RequiredAssociate_Int" IS NULL
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
WHERE "r0"."RequiredAssociate_RequiredNestedAssociate" ->> 'Int' <> 8 OR "r0"."RequiredAssociate_RequiredNestedAssociate" ->> 'Int' IS NULL
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
    SELECT "r0"."Id", "r0"."RequiredAssociate_Int"
    FROM "RootEntity" AS "r0"
    ORDER BY "r0"."Id"
    LIMIT @p
) AS "r1" ON "r"."Id" + 1 = "r1"."Id"
WHERE "r1"."RequiredAssociate_Int" <> 8 OR "r1"."RequiredAssociate_Int" IS NULL
""");
    }

    #endregion Left join
}
