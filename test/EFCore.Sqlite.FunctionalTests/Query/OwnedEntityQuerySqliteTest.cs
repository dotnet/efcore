// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQuerySqliteTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    [Theory, InlineData(false, DateTimeKind.Utc), InlineData(true, DateTimeKind.Utc),
     InlineData(false, DateTimeKind.Unspecified), InlineData(true, DateTimeKind.Unspecified)]
    public override async Task Legacy_DateTime_reader_writer_server_comparisons(bool async, DateTimeKind kind)
    {
        await base.Legacy_DateTime_reader_writer_server_comparisons(async, kind);

        var suffix = kind == DateTimeKind.Utc ? "Z" : "";
        AssertSql(
            $"""
@boundary='2026-01-01T00:00:00.1230000{suffix}' (DbType = DateTime)

SELECT "i"."Id"
FROM "Items" AS "i"
WHERE "i"."NativeTimestamp" = @boundary
ORDER BY "i"."Id"
""",
            //
            $"""
@boundary='2026-01-01T00:00:00.123{suffix}' (Nullable = false) (Size = {(kind == DateTimeKind.Utc ? 24 : 23)})

SELECT "i"."Id"
FROM "Items" AS "i"
WHERE "i"."Data" ->> 'Timestamp' = @boundary
ORDER BY "i"."Id"
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));
}
