// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocMiscellaneousQuerySqliteTest : AdHocMiscellaneousQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

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
}
