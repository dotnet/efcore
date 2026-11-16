// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class PrecompiledQuerySqliteTest(
    PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQuerySqliteTest.PrecompiledQuerySqliteFixture>
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    public override async Task Class_constant_in_predicate()
    {
        await base.Class_constant_in_predicate();

        AssertSql(
            """
SELECT "b"."Id", "b"."Name", "b"."Json"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 8
LIMIT 2
""");
    }

    [Fact]
    public virtual Task Glob()
        => Test("""_ = context.Blogs.Where(b => EF.Functions.Glob(b.Name, "*foo*")).ToList();""");

    public override async Task TimeOnly_IsBetween()
    {
        await base.TimeOnly_IsBetween();

        AssertSql(
            """
@start='22:00' (DbType = Time)
@end='02:00' (DbType = Time)

SELECT "t"."Id"
FROM "TimeSlots" AS "t"
WHERE "t"."Time" >= @start OR "t"."Time" < @end
ORDER BY "t"."Id"
""",
            //
            """
@start='14:00' (DbType = Time)
@end='16:00' (DbType = Time)

SELECT "t"."Id"
FROM "TimeSlots" AS "t"
WHERE "t"."Time" >= @start AND "t"."Time" < @end
ORDER BY "t"."Id"
""",
            //
            """
@start='14:00' (DbType = Time)
@end='14:00' (DbType = Time)

SELECT "t"."Id"
FROM "TimeSlots" AS "t"
WHERE "t"."Time" >= @start AND "t"."Time" < @end
ORDER BY "t"."Id"
""");
    }

    [Fact]
    public virtual Task Regexp()
        => Test("""_ = context.Blogs.Where(b => Regex.IsMatch(b.Name, "^foo")).ToList();""");

    public class PrecompiledQuerySqliteFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
            => SqlitePrecompiledQueryTestHelpers.Instance;
    }
}
