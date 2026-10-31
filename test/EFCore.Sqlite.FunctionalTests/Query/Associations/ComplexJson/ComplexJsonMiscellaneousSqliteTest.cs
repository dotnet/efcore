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
}
