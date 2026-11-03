// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
SELECT "i"."Data" ->> 'Number' AS "Number", "i"."Data" ->> 'ScaledNumber' AS "ScaledNumber", "i"."Data" ->> 'ConvertedNumber' AS "ConvertedNumber", "i"."Data" ->> 'Enabled' AS "Enabled", "i"."Data" ->> 'Disabled' AS "Disabled", "i"."Data" ->> 'OptionalEnabled' AS "OptionalEnabled", "i"."Data" ->> 'LiteralEnabled' AS "LiteralEnabled", "i"."Data" ->> 'NumericEnabled' AS "NumericEnabled", "i"."Data" ->> 'Timestamp' AS "Timestamp", "i"."Data" ->> 'Bytes' AS "Bytes", "i"."ByteArrays" ->> 0 AS "FirstBytes"
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
        var liftedProjection = context.Set<JsonScalarContext.Item>()
            .Select(e => new
            {
                e.Id, e.Data.Number, e.Data.ScaledNumber, e.Data.ConvertedNumber, e.Data.Enabled,
                e.Data.OptionalEnabled, e.Data.LiteralEnabled, e.Data.NumericEnabled
            })
            .OrderBy(e => e.Id)
            .Take(1)
            .Where(e => e.ScaledNumber == number)
            .Select(e => new
            {
                e.Number, e.ScaledNumber, e.ConvertedNumber, e.Enabled, e.OptionalEnabled, e.LiteralEnabled, e.NumericEnabled
            });
        var lifted = async ? await liftedProjection.SingleAsync() : liftedProjection.Single();
        Assert.Equal(43, lifted.Number);
        Assert.Equal(43, lifted.ScaledNumber);
        Assert.Equal(43, lifted.ConvertedNumber);
        Assert.Equal(enabled, lifted.Enabled);
        Assert.Null(lifted.OptionalEnabled);
        Assert.Equal(enabled, lifted.LiteralEnabled);
        Assert.Equal(enabled, lifted.NumericEnabled);
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

    [Theory, InlineData(false), InlineData(true)]
    public async Task Configured_JSON_scalar_projections_preserve_distinct_after_pushdown(bool async)
    {
        var factory = await InitializeNonSharedTest<JsonScalarContext>(
            seed: async context =>
            {
                for (var id = 1; id <= 3; id++)
                {
                    context.Add(
                        new JsonScalarContext.Item
                        {
                            Id = id,
                            Bytes = [1],
                            Data = new JsonScalarContext.JsonData
                            {
                                ScaledNumber = id == 3 ? 143 : 43,
                                Enabled = id == 3,
                                Bytes = [1]
                            }
                        });
                }

                await context.SaveChangesAsync();
            });
        await using var context = factory.CreateDbContext();
        var number = 43;
        var query = context.Set<JsonScalarContext.Item>()
            .Select(e => new { e.Data.ScaledNumber, e.Data.Enabled })
            .Distinct()
            .OrderBy(e => e.ScaledNumber)
            .Take(3)
            .Where(e => e.ScaledNumber >= number);
        var results = async ? await query.ToArrayAsync() : query.ToArray();
        Assert.Collection(
            results,
            v =>
            {
                Assert.Equal(43, v.ScaledNumber);
                Assert.False(v.Enabled);
            },
            v =>
            {
                Assert.Equal(143, v.ScaledNumber);
                Assert.True(v.Enabled);
            });
    }

    private class JsonScalarContext(DbContextOptions options) : DbContext(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonStringReaderWriter>();
            configurationBuilder.DefaultTypeMapping<DateTime>().HasJsonValueReaderWriter<JsonInt64ReaderWriter>();
            configurationBuilder.DefaultTypeMapping<byte[]>().HasJsonValueReaderWriter<JsonStringReaderWriter>();
            configurationBuilder.DefaultTypeMapping<bool>().HasJsonValueReaderWriter<JsonStringReaderWriter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().Property(e => e.Id).ValueGeneratedNever();
            var timestampConverter = new ValueConverter<DateTime, long>(
                v => new DateTimeOffset(v).ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v).UtcDateTime);
            var bytesConverter = new ValueConverter<byte[], string>(
                v => Convert.ToBase64String(v), v => Convert.FromBase64String(v));
            modelBuilder.Entity<Item>().PrimitiveCollection(e => e.Numbers).ElementType().HasConversion<string>();
            modelBuilder.Entity<Item>().PrimitiveCollection(e => e.Timestamps).ElementType().HasConversion(timestampConverter);
            modelBuilder.Entity<Item>().PrimitiveCollection(e => e.ByteArrays).ElementType().HasConversion(bytesConverter);
            modelBuilder.Entity<Item>().OwnsOne(
                e => e.Data, owned =>
                {
                    owned.ToJson();
                    owned.Property(e => e.Number).HasConversion<string>();
                    owned.Property(e => e.ScaledNumber).HasConversion(v => v * 10, v => v / 10)
                        .HasJsonValueReaderWriter<JsonInt32ReaderWriter>();
                    owned.Property(e => e.ConvertedNumber)
                        .HasConversion(v => (v + 1) * 10, v => v / 10 - 1)
                        .HasJsonValueReaderWriter<JsonInt32ReaderWriter>();
                    owned.Property(e => e.Enabled).HasConversion(new BoolToStringConverter("no", "yes"));
                    owned.Property(e => e.Disabled).HasConversion(new BoolToStringConverter("no", "yes"));
                    owned.Property(e => e.OptionalEnabled).HasConversion(new BoolToStringConverter("no", "yes"));
                    owned.Property(e => e.LiteralEnabled).HasJsonValueReaderWriter<LiteralBoolReaderWriter>();
                    owned.Property(e => e.NumericEnabled).HasConversion<int>().HasJsonValueReaderWriter<JsonInt32ReaderWriter>();
                    owned.Property(e => e.Timestamp).HasConversion(timestampConverter);
                    owned.Property(e => e.Bytes).HasConversion(
                        bytesConverter, ChangeTracking.ValueComparer.CreateDefault(typeof(byte[]), favorStructuralComparisons: true));
                    owned.PrimitiveCollection(e => e.ByteArrays).ElementType().HasConversion(bytesConverter);
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

    public sealed class LiteralBoolReaderWriter : JsonValueReaderWriter<bool>
    {
        public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetBoolean();

        public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
            => writer.WriteBooleanValue(value);

        public override Expression ConstructorExpression
            => Expression.New(typeof(LiteralBoolReaderWriter));
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
