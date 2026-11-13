// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

public class JsonDateTimeReaderWriterTest
{
    private const string PreviousFormat = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK";

    public static TheoryData<DateTime> Values
        => new()
        {
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 0, 0, 0, 120, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 12, 34, 56, DateTimeKind.Unspecified).AddTicks(1),
            DateTime.MinValue,
            DateTime.MaxValue
        };

    [Fact]
    public void Instance_writes_fixed_width_format()
    {
        Assert.Equal(
            "\"2026-01-01T00:00:00.0000000Z\"",
            JsonDateTimeReaderWriter.Instance.ToJsonString(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(
            "\"2026-01-01T00:00:00.1200000\"",
            JsonDateTimeReaderWriter.Instance.ToJsonString(new DateTime(2026, 1, 1, 0, 0, 0, 120)));
    }

    [Theory, MemberData(nameof(Values))]
    public void Custom_format_can_restore_previous_behavior(DateTime value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStringValue(value);
        }

        Assert.Equal(
            Encoding.UTF8.GetString(stream.ToArray()),
            new JsonDateTimeReaderWriter(PreviousFormat).ToJsonString(value));
    }

    [Theory, MemberData(nameof(Values))]
    public void Values_round_trip(DateTime value)
    {
        foreach (var readerWriter in new[] { JsonDateTimeReaderWriter.Instance, new JsonDateTimeReaderWriter(PreviousFormat) })
        {
            var actual = (DateTime)readerWriter.FromJsonString(readerWriter.ToJsonString(value));

            Assert.Equal(value, actual);
            Assert.Equal(value.Kind, actual.Kind);
        }
    }

    [Fact]
    public void ConstructorExpression_creates_equivalent_reader_writer()
    {
        Assert.Same(
            JsonDateTimeReaderWriter.Instance,
            Expression.Lambda<Func<JsonDateTimeReaderWriter>>(JsonDateTimeReaderWriter.Instance.ConstructorExpression).Compile()());

        var custom = Expression.Lambda<Func<JsonDateTimeReaderWriter>>(
            new JsonDateTimeReaderWriter(PreviousFormat).ConstructorExpression).Compile()();

        Assert.Equal("\"2026-01-01T00:00:00Z\"", custom.ToJsonString(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
    }
}
