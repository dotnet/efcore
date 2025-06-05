// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocMiscellaneousQuerySqliteTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder SetTranslateParameterizedCollectionsToConstants(DbContextOptionsBuilder optionsBuilder)
    {
        new SqliteDbContextOptionsBuilder(optionsBuilder).TranslateParameterizedCollectionsToConstants();

        return optionsBuilder;
    }

    protected override Task Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRawAsync(
            """
CREATE TABLE ZeroKey (Id int);
INSERT INTO ZeroKey VALUES (NULL)
""");

    public override async Task Average_with_cast()
    {
        await base.Average_with_cast();

        AssertSql(
            """
SELECT "p"."Id", "p"."DecimalColumn", "p"."DoubleColumn", "p"."FloatColumn", "p"."IntColumn", "p"."LongColumn", "p"."NullableDecimalColumn", "p"."NullableDoubleColumn", "p"."NullableFloatColumn", "p"."NullableIntColumn", "p"."NullableLongColumn", "p"."Price"
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."Price")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."IntColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."NullableIntColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."LongColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG(CAST("p"."NullableLongColumn" AS REAL))
FROM "Prices" AS "p"
""",
            //
            """
SELECT CAST(AVG("p"."FloatColumn") AS REAL)
FROM "Prices" AS "p"
""",
            //
            """
SELECT CAST(AVG("p"."NullableFloatColumn") AS REAL)
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."DoubleColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT AVG("p"."NullableDoubleColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."DecimalColumn")
FROM "Prices" AS "p"
""",
            //
            """
SELECT ef_avg("p"."NullableDecimalColumn")
FROM "Prices" AS "p"
""");
    }

    public override async Task Check_inlined_constants_redacting(bool async, bool enableSensitiveDataLogging)
    {
        await base.Check_inlined_constants_redacting(async, enableSensitiveDataLogging);

        if (!enableSensitiveDataLogging)
        {
            AssertSql(
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE "t"."Id" IN (?, ?, ?)
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE EXISTS (
    SELECT 1
    FROM (SELECT ? AS "Value" UNION ALL VALUES (?), (?)) AS "i"
    WHERE "i"."Value" = "t"."Id")
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE ? = "t"."Id"
""");
        }
        else
        {
            AssertSql(
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE "t"."Id" IN (1, 2, 3)
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE EXISTS (
    SELECT 1
    FROM (SELECT 1 AS "Value" UNION ALL VALUES (2), (3)) AS "i"
    WHERE "i"."Value" = "t"."Id")
""",
                //
                """
SELECT "t"."Id", "t"."Name"
FROM "TestEntities" AS "t"
WHERE 1 = "t"."Id"
""");
        }
    }
}
