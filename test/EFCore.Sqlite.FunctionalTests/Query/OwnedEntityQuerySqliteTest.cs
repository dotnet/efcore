// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQuerySqliteTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    [Theory, InlineData(false), InlineData(true)]
    public override async Task JSON_reader_writer_with_value_converter_server_comparisons_and_projections(bool async)
    {
        await base.JSON_reader_writer_with_value_converter_server_comparisons_and_projections(async);

        AssertSql(
            """
@number='430'

SELECT "i"."Id"
FROM "Items" AS "i"
WHERE "i"."Data" ->> 'Number' = @number
ORDER BY "i"."Id"
""",
            //
            """
@date='1767225600000'

SELECT "i"."Id"
FROM "Items" AS "i"
WHERE "i"."Data" ->> 'Timestamp' = @date
ORDER BY "i"."Id"
""",
            //
            """
SELECT "i"."Data" ->> 'Number' AS "Number", "i"."Data" ->> 'UpperNumber' AS "UpperNumber", "i"."Data" ->> 'OptionalNumber' AS "OptionalNumber", "i"."Data" ->> 'Timestamp' AS "Timestamp", "i"."Data" ->> 'OptionalTimestamp' AS "OptionalTimestamp"
FROM "Items" AS "i"
ORDER BY "i"."Id"
""");
    }

    [Theory, InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task Configured_JSON_scalar_representations_server_comparisons(bool async, bool enabled)
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        byte[] bytes = [1, 2, 255];
        var factory = await InitializeNonSharedTest<JsonScalarContext>(
            seed: async context =>
            {
                context.Add(
                    new JsonScalarContext.Item
                    {
                        Id = 1, Number = 43, Timestamp = date, Bytes = bytes,
                        Data = new JsonScalarContext.JsonData
                        {
                            Number = 43, ScaledNumber = 43, ConvertedNumber = 43, Enabled = enabled, Disabled = !enabled,
                            LiteralEnabled = enabled, NumericEnabled = enabled, Timestamp = date, Bytes = bytes, ByteArrays = [bytes]
                        },
                        Numbers = [43], Timestamps = [date], ByteArrays = [bytes]
                    });
                await context.SaveChangesAsync();
            });
        await using var context = factory.CreateDbContext();
        var number = 43;
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Number == number));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Number == 43));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.ScaledNumber == number));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.ConvertedNumber == number));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Timestamp == date));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Timestamp == new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Bytes == bytes));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.Bytes == new byte[] { 1, 2, 255 }));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Numbers.Contains(number)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Timestamps.Contains(date)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.ByteArrays.Contains(bytes)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Data.ByteArrays[0] == bytes));
        var comparisonArrays = new[] { bytes };
        var comparisonDates = new[] { date };
        var comparisonNumbers = new[] { number };
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonArrays).Contains(e.Data.Bytes)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonArrays).Contains(e.Bytes)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonDates).Contains(e.Data.Timestamp)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonDates).Contains(e.Timestamp)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonNumbers).Contains(e.Data.Number)));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => EF.Parameter(comparisonNumbers).Contains(e.Number)));
        ClearLog();
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Number == number && e.Data.Number == number));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Timestamp == date && e.Data.Timestamp == date));
        await AssertIds(context.Set<JsonScalarContext.Item>().Where(e => e.Bytes == bytes && e.Data.Bytes == bytes));
        var projection = context.Set<JsonScalarContext.Item>()
            .Select(e => new
            {
                e.Data.Number, e.Data.ScaledNumber, e.Data.ConvertedNumber,
                e.Data.Enabled, e.Data.Disabled, e.Data.OptionalEnabled, e.Data.LiteralEnabled, e.Data.NumericEnabled,
                e.Data.Timestamp, e.Data.Bytes, FirstBytes = e.ByteArrays[0]
            });
        var values = async ? await projection.SingleAsync() : projection.Single();
        Assert.Equal(43, values.Number);
        Assert.Equal(43, values.ScaledNumber);
        Assert.Equal(43, values.ConvertedNumber);
        Assert.Equal(enabled, values.Enabled);
        Assert.Equal(!enabled, values.Disabled);
        Assert.Null(values.OptionalEnabled);
        Assert.Equal(enabled, values.LiteralEnabled);
        Assert.Equal(enabled, values.NumericEnabled);
        Assert.Equal(date, values.Timestamp);
        Assert.Equal(bytes, values.Bytes);
        Assert.Equal(bytes, values.FirstBytes);
        AssertSql(
            """
@number='43'
@number0='43' (Nullable = false) (Size = 2)

SELECT "i"."Id"
FROM "Item" AS "i"
WHERE "i"."Number" = @number AND "i"."Data" ->> 'Number' = @number0
""",
            //
            """
@date='2026-01-01T00:00:00.0000000Z' (DbType = DateTime)
@date0='1767225600000'

SELECT "i"."Id"
FROM "Item" AS "i"
WHERE "i"."Timestamp" = @date AND "i"."Data" ->> 'Timestamp' = @date0
""",
            //
            """
@bytes='0x0102FF' (Size = 3)
@bytes0='AQL/' (Size = 4)

SELECT "i"."Id"
FROM "Item" AS "i"
WHERE "i"."Bytes" = @bytes AND "i"."Data" ->> 'Bytes' = @bytes0
""",
            //
            """
SELECT NULLIF("i"."Data" -> 'Number', 'null') AS "Number", NULLIF("i"."Data" -> 'ScaledNumber', 'null') AS "ScaledNumber", NULLIF("i"."Data" -> 'ConvertedNumber', 'null') AS "ConvertedNumber", NULLIF("i"."Data" -> 'Enabled', 'null') AS "Enabled", NULLIF("i"."Data" -> 'Disabled', 'null') AS "Disabled", NULLIF("i"."Data" -> 'OptionalEnabled', 'null') AS "OptionalEnabled", NULLIF("i"."Data" -> 'LiteralEnabled', 'null') AS "LiteralEnabled", NULLIF("i"."Data" -> 'NumericEnabled', 'null') AS "NumericEnabled", "i"."Data" ->> 'Timestamp' AS "Timestamp", "i"."Data" ->> 'Bytes' AS "Bytes", NULLIF("i"."ByteArrays" -> 0, 'null') AS "FirstBytes"
FROM "Item" AS "i"
LIMIT 2
""");
        var pushedDown = context.Set<JsonScalarContext.Item>()
            .Select(e => new { e.Id, e.Data.ScaledNumber })
            .OrderBy(e => e.Id)
            .Take(1)
            .Where(e => e.ScaledNumber == number)
            .Select(e => e.Id);
        var ids = async ? await pushedDown.ToArrayAsync() : pushedDown.ToArray();
        Assert.Equal([1], ids);
        var query = context.Set<JsonScalarContext.Item>().AsNoTracking();
        var item = async ? await query.SingleAsync() : query.Single();
        Assert.Equal(43, item.Data.Number);
        Assert.Equal(43, item.Data.ScaledNumber);
        Assert.Equal(43, item.Data.ConvertedNumber);
        Assert.Equal(enabled, item.Data.Enabled);
        Assert.Equal(!enabled, item.Data.Disabled);
        Assert.Null(item.Data.OptionalEnabled);
        Assert.Equal(enabled, item.Data.LiteralEnabled);
        Assert.Equal(enabled, item.Data.NumericEnabled);
        Assert.Equal(date, item.Data.Timestamp);
        Assert.Equal(bytes, item.Data.Bytes);
        Assert.Equal(bytes, Assert.Single(item.Data.ByteArrays));

        async Task AssertIds(IQueryable<JsonScalarContext.Item> source)
        {
            var ids = source.Select(e => e.Id);
            var actual = async ? await ids.ToArrayAsync() : ids.ToArray();
            Assert.Equal([1], actual);
        }
    }

    private class JsonScalarContext(DbContextOptions options) : DbContext(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.DefaultTypeMapping<int>().HasJsonValueReaderWriter<IntAsStringReaderWriter>();
            configurationBuilder.DefaultTypeMapping<DateTime>().HasJsonValueReaderWriter<UnixDateTimeReaderWriter>();
            configurationBuilder.DefaultTypeMapping<byte[]>().HasJsonValueReaderWriter<JsonByteArrayReaderWriter>();
            configurationBuilder.DefaultTypeMapping<bool>().HasJsonValueReaderWriter<BoolAsStringReaderWriter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Item>().OwnsOne(
                e => e.Data, owned =>
                {
                    owned.ToJson();
                    owned.Property(e => e.ScaledNumber).HasJsonValueReaderWriter<ScaledIntReaderWriter>();
                    owned.Property(e => e.ConvertedNumber)
                        .HasConversion(v => v + 1, v => v - 1)
                        .HasJsonValueReaderWriter<ScaledIntReaderWriter>();
                    owned.Property(e => e.LiteralEnabled).HasJsonValueReaderWriter<LiteralBoolReaderWriter>();
                    owned.Property(e => e.NumericEnabled).HasJsonValueReaderWriter<NumericBoolReaderWriter>();
                });
        }

        public class Item
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Bytes { get; set; } = null!;
            public List<int> Numbers { get; set; } = [];
            public List<DateTime> Timestamps { get; set; } = [];
            public List<byte[]> ByteArrays { get; set; } = [];
            public JsonData Data { get; set; } = null!;
        }

        public class JsonData
        {
            public int Number { get; set; }
            public int ScaledNumber { get; set; }
            public int ConvertedNumber { get; set; }
            public bool Enabled { get; set; }
            public bool Disabled { get; set; }
            public bool? OptionalEnabled { get; set; }
            public bool LiteralEnabled { get; set; }
            public bool NumericEnabled { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Bytes { get; set; } = null!;
            public List<byte[]> ByteArrays { get; set; } = [];
        }
    }

    public sealed class IntAsStringReaderWriter : JsonValueReaderWriter<int>
    {
        public static IntAsStringReaderWriter Instance { get; } = new();
        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => int.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);
        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(IntAsStringReaderWriter), nameof(Instance));
    }

    public sealed class UnixDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
    {
        public static UnixDateTimeReaderWriter Instance { get; } = new();
        public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => DateTimeOffset.FromUnixTimeMilliseconds(manager.CurrentReader.GetInt64()).UtcDateTime;
        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeMilliseconds());
        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(UnixDateTimeReaderWriter), nameof(Instance));
    }

    public sealed class ScaledIntReaderWriter : JsonValueReaderWriter<int>
    {
        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetInt32() / 10;

        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteNumberValue(value * 10);

        public override Expression ConstructorExpression
            => Expression.New(typeof(ScaledIntReaderWriter));
    }

    public sealed class BoolAsStringReaderWriter : JsonValueReaderWriter<bool>
    {
        public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetString() switch
            {
                "yes" => true,
                "no" => false,
                _ => throw new InvalidOperationException()
            };

        public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
            => writer.WriteStringValue(value ? "yes" : "no");

        public override Expression ConstructorExpression
            => Expression.New(typeof(BoolAsStringReaderWriter));
    }

    public sealed class LiteralBoolReaderWriter : JsonValueReaderWriter<bool>
    {
        public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetBoolean();

        public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
            => writer.WriteBooleanValue(value);

        public override Expression ConstructorExpression
            => Expression.New(typeof(LiteralBoolReaderWriter));
    }

    public sealed class NumericBoolReaderWriter : JsonValueReaderWriter<bool>
    {
        public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetInt32() != 0;

        public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
            => writer.WriteNumberValue(value ? 1 : 0);

        public override Expression ConstructorExpression
            => Expression.New(typeof(NumericBoolReaderWriter));
    }

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

    public override async Task Owned_collection_select_many_without_result_selector(bool async)
    => Assert.Equal(
        SqliteStrings.ApplyNotSupported,
        (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Owned_collection_select_many_without_result_selector(async))).Message);

    public override async Task Owned_collection_select_many_without_result_selector_owned_in_separate_table(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Owned_collection_select_many_without_result_selector_owned_in_separate_table(async))).Message);
}
