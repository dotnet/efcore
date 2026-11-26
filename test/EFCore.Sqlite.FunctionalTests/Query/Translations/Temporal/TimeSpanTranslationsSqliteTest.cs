// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsSqliteTest : TimeSpanTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeSpanTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hours()
    {
        await base.Hours();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") / 36000000000) % 24 = 3
""");
    }

    public override async Task Minutes()
    {
        await base.Minutes();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") / 600000000) % 60 = 4
""");
    }

    public override async Task Seconds()
    {
        await base.Seconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") / 10000000) % 60 = 5
""");
    }

    public override async Task Milliseconds()
    {
        await base.Milliseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") / 10000) % 1000 = 678
""");
    }

    public override async Task Microseconds()
    {
        await base.Microseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") / 10) % 1000 = 912
""");
    }

    public override async Task Nanoseconds()
    {
        await base.Nanoseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (ef_ticks("b"."TimeSpan") % 10) * 100 = 400
""");
    }

    public override async Task TotalDays()
    {
        await base.TotalDays();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 864000000000.0 > 0.10000000000000001
""");
    }

    public override async Task TotalHours()
    {
        await base.TotalHours();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 36000000000.0 > 2.5
""");
    }

    public override async Task TotalMinutes()
    {
        await base.TotalMinutes();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 600000000.0 > 120.25
""");
    }

    public override async Task TotalSeconds()
    {
        await base.TotalSeconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 10000000.0 > 7215.3999999999996
""");
    }

    public override async Task TotalMilliseconds()
    {
        await base.TotalMilliseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 10000.0 > 7215456.2000000002
""");
    }

    public override async Task TotalMicroseconds()
    {
        await base.TotalMicroseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 10.0 > 7215456200.3000002
""");
    }

    public override async Task TotalNanoseconds()
    {
        await base.TotalNanoseconds();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) * 100.0 > 7215456200300.0
""");
    }

    [Fact]
    public virtual async Task Add()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.TimeSpan + b.TimeSpan).Hours == 6));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ((ef_ticks("b"."TimeSpan") + ef_ticks("b"."TimeSpan")) / 36000000000) % 24 = 6
""");
    }

    [Fact]
    public virtual async Task Subtract()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.TimeSpan - new TimeSpan(1, 0, 0)).Hours == 2));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ((ef_ticks("b"."TimeSpan") - 36000000000) / 36000000000) % 24 = 2
""");
    }

    [Fact]
    public virtual async Task Negate()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (-b.TimeSpan).Hours == -3));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (-ef_ticks("b"."TimeSpan") / 36000000000) % 24 = -3
""");
    }

    [Fact]
    public virtual async Task Negate_method()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Negate().Minutes == -4));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (-ef_ticks("b"."TimeSpan") / 600000000) % 60 = -4
""");
    }

    [Fact]
    public virtual async Task Duration()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (-b.TimeSpan).Duration() == b.TimeSpan));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_timespan(abs(-ef_ticks("b"."TimeSpan"))) = "b"."TimeSpan"
""");
    }

    [Fact]
    public virtual async Task Multiply_by_double()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.TimeSpan * 2.5).Hours == 7));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (CAST(ef_round(CAST(ef_ticks("b"."TimeSpan") AS REAL) * 2.5) AS INTEGER) / 36000000000) % 24 = 7
""");
    }

    [Fact]
    public virtual async Task Divide_by_double()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.TimeSpan / 2.0).Minutes == 32));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (CAST(ef_round(CAST(ef_ticks("b"."TimeSpan") AS REAL) / 2.0) AS INTEGER) / 600000000) % 60 = 32
""");
    }

    [Fact]
    public virtual async Task Multiply_by_double_rounds_half_to_even()
    {
        // .NET rounds the scaled ticks half to even, unlike SQLite's round(): 0.5 and -2.5 ticks become 0 and -2.
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Select(b => new
            {
                b.Id,
                Positive = ((b.TimeSpan - b.TimeSpan + TimeSpan.FromTicks(1)) * 0.5).Ticks,
                Negative = ((b.TimeSpan - b.TimeSpan - TimeSpan.FromTicks(5)) * 0.5).Ticks
            }),
            elementSorter: e => e.Id);

        AssertSql(
            """
SELECT "b"."Id", CAST(ef_round(CAST((ef_ticks("b"."TimeSpan") - ef_ticks("b"."TimeSpan")) + 1 AS REAL) * 0.5) AS INTEGER) AS "Positive", CAST(ef_round(CAST((ef_ticks("b"."TimeSpan") - ef_ticks("b"."TimeSpan")) - 5 AS REAL) * 0.5) AS INTEGER) AS "Negative"
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Divide_by_double_rounds_half_to_even()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Select(b => new
            {
                b.Id,
                Positive = ((b.TimeSpan - b.TimeSpan + TimeSpan.FromTicks(5)) / 2.0).Ticks,
                Negative = ((b.TimeSpan - b.TimeSpan - TimeSpan.FromTicks(1)) / 2.0).Ticks
            }),
            elementSorter: e => e.Id);

        AssertSql(
            """
SELECT "b"."Id", CAST(ef_round(CAST((ef_ticks("b"."TimeSpan") - ef_ticks("b"."TimeSpan")) + 5 AS REAL) / 2.0) AS INTEGER) AS "Positive", CAST(ef_round(CAST((ef_ticks("b"."TimeSpan") - ef_ticks("b"."TimeSpan")) - 1 AS REAL) / 2.0) AS INTEGER) AS "Negative"
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Divide_by_TimeSpan()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan / new TimeSpan(1, 0, 0) > 2.0));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(ef_ticks("b"."TimeSpan") AS REAL) / 36000000000.0 > 2.0
""");
    }

    [Fact]
    public virtual async Task GreaterThan()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan > new TimeSpan(1, 30, 0)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_ticks("b"."TimeSpan") > 54000000000
""");
    }

    [Fact]
    public virtual async Task LessThanOrEqual()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan <= new TimeSpan(1, 0, 15, 456)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_ticks("b"."TimeSpan") <= 877560000000
""");
    }

    [Fact]
    public virtual async Task Add_in_projection()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => b.TimeSpan + new TimeSpan(1, 0, 0, 0)));

        AssertSql(
            """
SELECT ef_timespan(ef_ticks("b"."TimeSpan") + 864000000000)
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Negate_in_projection()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => -b.TimeSpan));

        AssertSql(
            """
SELECT ef_timespan(-ef_ticks("b"."TimeSpan"))
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Max()
    {
        await AssertMax(async: true, ss => ss.Set<BasicTypesEntity>().Select(b => b.TimeSpan));

        AssertSql(
            """
SELECT ef_timespan(max(ef_ticks("b"."TimeSpan")))
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Min()
    {
        await AssertMin(async: true, ss => ss.Set<BasicTypesEntity>().Select(b => b.TimeSpan));

        AssertSql(
            """
SELECT ef_timespan(min(ef_ticks("b"."TimeSpan")))
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Add_nullable()
    {
        await AssertQuery(
            ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.TimeSpan + new TimeSpan(1, 0, 0) > new TimeSpan(4, 0, 0)));

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE ef_ticks("n"."TimeSpan") + 36000000000 > 144000000000
""");
    }

    [Fact]
    public virtual async Task Hours_in_projection_nullable()
    {
        await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Select(b => b.TimeSpan == null ? (int?)null : b.TimeSpan.Value.Hours));

        AssertSql(
            """
SELECT (ef_ticks("n"."TimeSpan") / 36000000000) % 24
FROM "NullableBasicTypesEntities" AS "n"
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
