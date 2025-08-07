// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NullSemanticsQuerySqliteTest : NullSemanticsQueryTestBase<NullSemanticsQuerySqliteFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public NullSemanticsQuerySqliteTest(NullSemanticsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();

    //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    public override async Task Rewrite_compare_int_with_int(bool async)
    {
        await base.Rewrite_compare_int_with_int(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."IntA" = "e"."IntB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" = "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."NullableIntA" = "e"."IntB" AND "e"."NullableIntA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" = "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."IntA" = "e"."NullableIntB" AND "e"."NullableIntB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" = "e"."NullableIntB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableIntA" = "e"."NullableIntB" AND "e"."NullableIntA" IS NOT NULL AND "e"."NullableIntB" IS NOT NULL) OR ("e"."NullableIntA" IS NULL AND "e"."NullableIntB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" = "e"."NullableIntB" OR ("e"."NullableIntA" IS NULL AND "e"."NullableIntB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."IntA" <> "e"."IntB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" <> "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."NullableIntA" <> "e"."IntB" OR "e"."NullableIntA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" <> "e"."IntB" OR "e"."NullableIntA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."IntA" <> "e"."NullableIntB" OR "e"."NullableIntB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" <> "e"."NullableIntB" OR "e"."NullableIntB" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableIntA" <> "e"."NullableIntB" OR "e"."NullableIntA" IS NULL OR "e"."NullableIntB" IS NULL) AND ("e"."NullableIntA" IS NOT NULL OR "e"."NullableIntB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableIntA" <> "e"."NullableIntB" OR "e"."NullableIntA" IS NULL OR "e"."NullableIntB" IS NULL) AND ("e"."NullableIntA" IS NOT NULL OR "e"."NullableIntB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."IntA" <> "e"."IntB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" <> "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."NullableIntA" <> "e"."IntB" OR "e"."NullableIntA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" <> "e"."IntB" OR "e"."NullableIntA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."IntA" <> "e"."NullableIntB" OR "e"."NullableIntB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" <> "e"."NullableIntB" OR "e"."NullableIntB" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableIntA" <> "e"."NullableIntB" OR "e"."NullableIntA" IS NULL OR "e"."NullableIntB" IS NULL) AND ("e"."NullableIntA" IS NOT NULL OR "e"."NullableIntB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableIntA" <> "e"."NullableIntB" OR "e"."NullableIntA" IS NULL OR "e"."NullableIntB" IS NULL) AND ("e"."NullableIntA" IS NOT NULL OR "e"."NullableIntB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."IntA" = "e"."IntB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" = "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."NullableIntA" = "e"."IntB" AND "e"."NullableIntA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" = "e"."IntB"
""",
            //
            """
SELECT "e"."Id", "e"."IntA" = "e"."NullableIntB" AND "e"."NullableIntB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" = "e"."NullableIntB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableIntA" = "e"."NullableIntB" AND "e"."NullableIntA" IS NOT NULL AND "e"."NullableIntB" IS NOT NULL) OR ("e"."NullableIntA" IS NULL AND "e"."NullableIntB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" = "e"."NullableIntB" OR ("e"."NullableIntA" IS NULL AND "e"."NullableIntB" IS NULL)
""");
    }

    public override async Task Rewrite_compare_bool_with_bool(bool async)
    {
        await base.Rewrite_compare_bool_with_bool(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 1 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 1 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."NullableBoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 1 = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 0 = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 0 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 0
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 0 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 0
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 1 <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."NullableBoolB")
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 0 <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 <> "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 1 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 1 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 1 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 1 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 1 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 1 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 0 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 0 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 0 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 0 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 0 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 1 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 1 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 0 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 1 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 1 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 1 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 1 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 1 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 1 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 0 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" = "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 0 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 0 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB" OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 0 OR "e"."NullableBoolA" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 0 OR "e"."NullableBoolA" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 1 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 1 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 0 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 = "e"."NullableBoolB" OR "e"."NullableBoolB" IS NULL
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 1 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 1 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."NullableBoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 1 = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 0 = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" <> "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolA") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolA")
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" = "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = "e"."NullableBoolB"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA"
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" <> "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = 0 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = 0
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" = "e"."BoolB" AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", ("e"."NullableBoolA" = "e"."NullableBoolB" AND "e"."NullableBoolA" IS NOT NULL AND "e"."NullableBoolB" IS NOT NULL) OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL) AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" = "e"."NullableBoolB" OR ("e"."NullableBoolA" IS NULL AND "e"."NullableBoolB" IS NULL)
""",
            //
            """
SELECT "e"."Id", "e"."NullableBoolA" <> 0 AND "e"."NullableBoolA" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" <> 0
""",
            //
            """
SELECT "e"."Id", NOT ("e"."BoolB") AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."BoolB")
""",
            //
            """
SELECT "e"."Id", 1 <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT ("e"."NullableBoolB")
""",
            //
            """
SELECT "e"."Id", "e"."BoolB" AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolB"
""",
            //
            """
SELECT "e"."Id", 0 <> "e"."NullableBoolB" AND "e"."NullableBoolB" IS NOT NULL AS "X"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE 0 <> "e"."NullableBoolB"
""");
    }

    public override async Task Where_coalesce_shortcircuit(bool async)
    {
        await base.Where_coalesce_shortcircuit(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" OR "e"."BoolB"
""");
    }

    public override async Task Where_coalesce_shortcircuit_many(bool async)
    {
        await base.Where_coalesce_shortcircuit_many(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE COALESCE("e"."NullableBoolA", "e"."BoolA" OR "e"."BoolB")
""");
    }

    public override async Task Join_uses_database_semantics(bool async)
    {
        await base.Join_uses_database_semantics(async);

        AssertSql(
            """
SELECT "e"."Id" AS "Id1", "e0"."Id" AS "Id2", "e"."NullableIntA", "e0"."NullableIntB"
FROM "Entities1" AS "e"
INNER JOIN "Entities2" AS "e0" ON "e"."NullableIntA" = "e0"."NullableIntB"
""");
    }

    public override async Task Join_uses_csharp_semantics_for_anon_objects(bool async)
    {
        await base.Join_uses_csharp_semantics_for_anon_objects(async);

        AssertSql(
            """
SELECT "e"."Id" AS "Id1", "e0"."Id" AS "Id2", "e"."NullableIntA", "e0"."NullableIntB"
FROM "Entities1" AS "e"
INNER JOIN "Entities2" AS "e0" ON "e"."NullableIntA" = "e0"."NullableIntB" OR ("e"."NullableIntA" IS NULL AND "e0"."NullableIntB" IS NULL)
""");
    }

    public override async Task Null_semantics_conditional(bool async)
    {
        await base.Null_semantics_conditional(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."BoolA" = CASE
    WHEN "e"."BoolB" THEN "e"."NullableBoolB"
    ELSE "e"."NullableBoolC"
END
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL) THEN "e"."BoolB"
    ELSE "e"."BoolC"
END = "e"."BoolA"
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN CASE
        WHEN "e"."BoolA" THEN ("e"."NullableBoolA" <> "e"."NullableBoolB" OR "e"."NullableBoolA" IS NULL OR "e"."NullableBoolB" IS NULL) AND ("e"."NullableBoolA" IS NOT NULL OR "e"."NullableBoolB" IS NOT NULL)
        ELSE "e"."BoolC"
    END <> "e"."BoolB" THEN "e"."BoolA"
    ELSE "e"."NullableBoolB" = "e"."NullableBoolC" OR ("e"."NullableBoolB" IS NULL AND "e"."NullableBoolC" IS NULL)
END
""",
            //
            """
SELECT CASE
    WHEN CASE
        WHEN "e"."BoolA" THEN "e"."NullableIntA"
        ELSE "e"."IntB"
    END > "e"."IntC" THEN 1
    ELSE 0
END
FROM "Entities1" AS "e"
""");
    }

    public override async Task Null_semantics_contains_non_nullable_item_with_non_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_non_nullable_item_with_non_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" NOT IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""");
    }

    public override async Task Null_semantics_contains_nullable_item_with_non_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_nullable_item_with_non_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableStringA" IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableStringA" NOT IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
) OR "e"."NullableStringA" IS NULL
""");
    }

    public override async Task Null_semantics_contains_non_nullable_item_with_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_non_nullable_item_with_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" IN (
    SELECT "e0"."NullableStringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT (COALESCE("e"."StringA" IN (
    SELECT "e0"."NullableStringA"
    FROM "Entities2" AS "e0"
), 0))
""");
    }

    public override async Task Null_semantics_contains_nullable_item_with_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_nullable_item_with_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE EXISTS (
    SELECT 1
    FROM "Entities2" AS "e0"
    WHERE "e0"."NullableStringA" = "e"."NullableStringB" OR ("e0"."NullableStringA" IS NULL AND "e"."NullableStringB" IS NULL))
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Entities2" AS "e0"
    WHERE "e0"."NullableStringA" = "e"."NullableStringB" OR ("e0"."NullableStringA" IS NULL AND "e"."NullableStringB" IS NULL))
""");
    }

    public override async Task CaseWhen_equal_to_second_filter(bool async)
    {
        await base.CaseWhen_equal_to_second_filter(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END = 2
""");
    }

    public override async Task CaseWhen_equal_to_first_or_third_filter(bool async)
    {
        await base.CaseWhen_equal_to_first_or_third_filter(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END = 3
""");
    }

    public override async Task CaseWhen_equal_to_second_select(bool async)
    {
        await base.CaseWhen_equal_to_second_select(async);

        AssertSql(
            """
SELECT CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END = 2 AND CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END IS NOT NULL
FROM "Entities1" AS "e"
ORDER BY "e"."Id"
""");
    }

    public override async Task CaseWhen_equal_to_first_or_third_select(bool async)
    {
        await base.CaseWhen_equal_to_first_or_third_select(async);

        AssertSql(
            """
SELECT CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END = 3 AND CASE
    WHEN "e"."StringA" = 'Foo' THEN 3
    WHEN "e"."StringB" = 'Foo' THEN 2
    WHEN "e"."StringC" = 'Foo' THEN 3
END IS NOT NULL
FROM "Entities1" AS "e"
ORDER BY "e"."Id"
""");
    }

    public override async Task CaseOpWhen_projection(bool async)
    {
        await base.CaseOpWhen_projection(async);

        AssertSql(
            """
SELECT CASE "e"."StringA" = 'Foo'
    WHEN 1 THEN 3
    WHEN 0 THEN 2
END
FROM "Entities1" AS "e"
ORDER BY "e"."Id"
""");
    }

    public override async Task CaseOpWhen_predicate(bool async)
    {
        await base.CaseOpWhen_predicate(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE CASE "e"."StringA" = 'Foo'
    WHEN 1 THEN 3
    WHEN 0 THEN 2
END = 2
""");
    }

    public override async Task Bool_equal_nullable_bool_HasValue(bool async)
    {
        await base.Bool_equal_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NOT NULL
""",
            //
            """
@prm='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @prm = ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_equal_nullable_bool_compared_to_null(bool async)
    {
        await base.Bool_equal_nullable_bool_compared_to_null(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NULL
""",
            //
            """
@prm='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @prm = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_bool_HasValue(bool async)
    {
        await base.Bool_not_equal_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NULL
""",
            //
            """
@prm='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @prm <> ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" <> ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_int_HasValue(bool async)
    {
        await base.Bool_not_equal_nullable_int_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" IS NULL
""",
            //
            """
@prm='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @prm <> ("e"."NullableIntA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" <> ("e"."NullableIntA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_bool_compared_to_null(bool async)
    {
        await base.Bool_not_equal_nullable_bool_compared_to_null(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NOT NULL
""",
            //
            """
@prm='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @prm <> ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_logical_operation_with_nullable_bool_HasValue(bool async)
    {
        await base.Bool_logical_operation_with_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE 0
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" OR "e"."NullableBoolA" IS NOT NULL
""");
    }

    public override async Task Negated_order_comparison_on_non_nullable_arguments_gets_optimized(bool async)
    {
        await base.Negated_order_comparison_on_non_nullable_arguments_gets_optimized(async);

        AssertSql(
            """
@i='1'

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" <= @i
""",
            //
            """
@i='1'

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" < @i
""",
            //
            """
@i='1'

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" >= @i
""",
            //
            """
@i='1'

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."IntA" > @i
""");
    }

    public override async Task Negated_order_comparison_on_nullable_arguments_doesnt_get_optimized(bool async)
    {
        await base.Negated_order_comparison_on_nullable_arguments_doesnt_get_optimized(async);

        AssertSql(
            """
@i='1' (Nullable = true)

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."NullableIntA" > @i THEN 0
    ELSE 1
END
""",
            //
            """
@i='1' (Nullable = true)

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."NullableIntA" >= @i THEN 0
    ELSE 1
END
""",
            //
            """
@i='1' (Nullable = true)

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."NullableIntA" < @i THEN 0
    ELSE 1
END
""",
            //
            """
@i='1' (Nullable = true)

SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE CASE
    WHEN "e"."NullableIntA" <= @i THEN 0
    ELSE 1
END
""");
    }

    public override async Task Comparison_compared_to_null_check_on_bool(bool async)
    {
        await base.Comparison_compared_to_null_check_on_bool(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE ("e"."IntA" = "e"."IntB") <> ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE ("e"."IntA" <> "e"."IntB") = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
    {
        var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
        if (useRelationalNulls)
        {
            new SqliteDbContextOptionsBuilder(options).UseRelationalNulls();
        }

        var context = new NullSemanticsContext(options.Options);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return context;
    }
}
