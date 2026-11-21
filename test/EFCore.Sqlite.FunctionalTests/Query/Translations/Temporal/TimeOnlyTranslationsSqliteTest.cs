// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsSqliteTest : TimeOnlyTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeOnlyTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%H', "b"."TimeOnly") AS INTEGER) = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%M', "b"."TimeOnly") AS INTEGER) = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%S', "b"."TimeOnly") AS INTEGER) = 10
""");
    }

    public override async Task Millisecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Millisecond);

        AssertSql();
    }

    public override async Task Microsecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Microsecond);

        AssertSql();
    }

    public override async Task Nanosecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Nanosecond);

        AssertSql();
    }

    public override async Task AddHours()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.AddHours);

        AssertSql();
    }

    public override async Task AddMinutes()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.AddMinutes);

        AssertSql();
    }

    public override async Task Add_TimeSpan()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Add_TimeSpan);

        AssertSql();
    }

    public override async Task IsBetween()
    {
        await base.IsBetween();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeOnly" >= '14:00:00' AND "b"."TimeOnly" < '16:00:00'
""");
    }

    public override async Task IsBetween_midnight(bool async)
    {
        await base.IsBetween_midnight(async);

        AssertSql(
            """
SELECT "b"."Id"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeOnly" >= '22:00:00' OR "b"."TimeOnly" < '02:00:00'
""");
    }

    public override async Task IsBetween_parameters(bool async, int range)
    {
        await base.IsBetween_parameters(async, range);

        AssertSql(
            $$"""
@start='{{(range == 0 ? "14:00" : range == 3 ? "00:00" : "22:00")}}' (DbType = Time)
@end='{{(range == 0 ? "16:00" : range == 1 ? "02:00" : range == 3 ? "00:00" : "22:00")}}' (DbType = Time)

SELECT "b"."Id"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeOnly" >= rtrim(@start, '0.') {{(range == 1 ? "OR" : "AND")}} "b"."TimeOnly" < rtrim(@end, '0.')
""");
    }

    public override async Task IsBetween_boundaries(bool async)
    {
        await base.IsBetween_boundaries(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."TimeOnly" >= '15:30:10' AND "b"."TimeOnly" < '16:00:00' AS "Start", "b"."TimeOnly" >= '14:00:00' AND "b"."TimeOnly" < '15:30:10' AS "End", "b"."TimeOnly" >= '15:30:10' OR "b"."TimeOnly" < '14:00:00' AS "WrappedStart", "b"."TimeOnly" >= '16:00:00' OR "b"."TimeOnly" < '15:30:10' AS "WrappedEnd", "b"."TimeOnly" >= '15:30:10' AND "b"."TimeOnly" < '15:30:10' AS "Empty"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task IsBetween_precision(bool async)
    {
        await base.IsBetween_precision(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."TimeOnly" >= '00:00:00.0102004' AND "b"."TimeOnly" < '00:00:00.0102005' AS "Start", "b"."TimeOnly" >= '00:00:00.0102003' AND "b"."TimeOnly" < '00:00:00.0102004' AS "End"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task IsBetween_negated(bool async)
    {
        await base.IsBetween_negated(async);

        AssertSql(
            """
SELECT "b"."Id"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeOnly" < '22:00:00' AND "b"."TimeOnly" >= '02:00:00'
""");
    }

    [Theory]
    [InlineData(null, 9)]
    [InlineData(17, null)]
    [InlineData(null, null)]
    [InlineData(17, 9)]
    [InlineData(9, 17)]
    public void Nullable_TimeOnly_parameter_comparisons_in_projection(int? startHour, int? endHour)
    {
        using var context = Fixture.CreateContext();
        var compare = EF.CompileQuery((BasicTypesContext db, TimeOnly? start, TimeOnly? end) =>
            db.BasicTypesEntities.Where(b => b.Id == 0)
                .Select(b => new { After = start > end, AtMost = start <= end, NotAfter = !(start > end) }).Single());

        TimeOnly? start = startHour is int s ? new TimeOnly(s, 0) : null;
        TimeOnly? end = endHour is int e ? new TimeOnly(e, 0) : null;
        var actual = compare(context, start, end);
        Assert.Equal((start > end, start <= end, !(start > end)), (actual.After, actual.AtMost, actual.NotAfter));

        if (start is null || end is null)
        {
            var left = start is null ? "NULL" : "@start";
            var right = end is null ? "NULL" : "@end";
            var sql = Assert.Single(Fixture.TestSqlLoggerFactory.SqlStatements);
            Assert.Contains($"WHEN {left} > {right} THEN 1", sql);
            Assert.Contains($"WHEN {left} <= {right} THEN 1", sql);
            Assert.Contains($"WHEN {left} > {right} THEN 0", sql);
        }
    }

    public override async Task IsBetween_nullable(bool async)
    {
        await base.IsBetween_nullable(async);

        AssertSql(
            """
SELECT "n"."Id", "n"."TimeOnly" IS NOT NULL AND ("n"."TimeOnly" >= '22:00:00' OR "n"."TimeOnly" < '02:00:00') AS "Inside", "n"."TimeOnly" IS NULL OR ("n"."TimeOnly" < '22:00:00' AND "n"."TimeOnly" >= '02:00:00') AS "Outside"
FROM "NullableBasicTypesEntities" AS "n"
""");
    }

    public override async Task IsBetween_column_bounds(bool async)
    {
        await base.IsBetween_column_bounds(async);

        AssertSql(
            """
@time='00:00' (DbType = Time)

SELECT "b"."Id", ('16:00:00' >= rtrim("b"."TimeOnly", '0.') AND @time >= rtrim("b"."TimeOnly", '0.') AND @time < '16:00:00') OR ('16:00:00' < rtrim("b"."TimeOnly", '0.') AND (@time >= rtrim("b"."TimeOnly", '0.') OR @time < '16:00:00')) AS "Start", ("b"."TimeOnly" >= '22:00:00' AND @time >= '22:00:00' AND @time < rtrim("b"."TimeOnly", '0.')) OR ("b"."TimeOnly" < '22:00:00' AND (@time >= '22:00:00' OR @time < rtrim("b"."TimeOnly", '0.'))) AS "End"
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public async Task IsBetween_with_subquery(bool async, int position)
    {
        await AssertTranslationFailed(() => position switch
        {
            0 => AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(b => ss.Set<BasicTypesEntity>()
                .Where(o => o.Id == b.Id).Select(o => o.TimeOnly).FirstOrDefault()
                .IsBetween(new TimeOnly(14, 0), new TimeOnly(16, 0)))),
            1 => AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(
                ss.Set<BasicTypesEntity>().Where(o => o.Id == b.Id).Select(o => o.TimeOnly).FirstOrDefault(), new TimeOnly(16, 0)))),
            _ => AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(
                new TimeOnly(14, 0), ss.Set<BasicTypesEntity>().Where(o => o.Id == b.Id).Select(o => o.TimeOnly).FirstOrDefault())))
        });

        AssertSql();
    }

    public override async Task Subtract()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Subtract);

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromDateTime_compared_to_property);

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromDateTime_compared_to_parameter);

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromDateTime_compared_to_constant);

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromTimeSpan_compared_to_property);

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromTimeSpan_compared_to_parameter);

        AssertSql();
    }

    public override async Task Order_by_FromTimeSpan()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.Order_by_FromTimeSpan);

        AssertSql();
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
