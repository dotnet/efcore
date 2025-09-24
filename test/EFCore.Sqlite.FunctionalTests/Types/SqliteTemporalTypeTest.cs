// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class SqliteDateTimeTypeTest(SqliteDateTimeTypeTest.DateTimeTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTime, SqliteDateTimeTypeTest.DateTimeTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class DateTimeTypeFixture : SqliteTypeFixture<DateTime>
    {
        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);
    }
}

public class SqliteDateTimeOffsetTypeTest(SqliteDateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateTimeOffset, SqliteDateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class DateTimeOffsetTypeFixture : SqliteTypeFixture<DateTimeOffset>
    {
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2));
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3));
    }
}

public class SqliteDateOnlyTypeTest(SqliteDateOnlyTypeTest.DateTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateOnly, SqliteDateOnlyTypeTest.DateTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class DateTypeFixture : SqliteTypeFixture<DateOnly>
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);
    }
}

public class SqliteTimeOnlyTypeTest(SqliteTimeOnlyTypeTest.TimeTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<TimeOnly, SqliteTimeOnlyTypeTest.TimeTypeFixture>(fixture, testOutputHelper)
{
    // TODO: string representation discrepancy between our JSON and M.D.SQLite's string representation, see #36749.
    public override Task Query_property_within_json()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Query_property_within_json());

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class TimeTypeFixture : SqliteTypeFixture<TimeOnly>
    {
        public override TimeOnly Value { get; } = new TimeOnly(12, 30, 45);
        public override TimeOnly OtherValue { get; } = new TimeOnly(14, 0, 0);
    }
}

public class SqliteTimeSpanTypeTest(SqliteTimeSpanTypeTest.TimeSpanTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<TimeSpan, SqliteTimeSpanTypeTest.TimeSpanTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class TimeSpanTypeFixture : SqliteTypeFixture<TimeSpan>
    {
        public override TimeSpan Value { get; } = new TimeSpan(12, 30, 45);
        public override TimeSpan OtherValue { get; } = new TimeSpan(14, 0, 0);
    }
}
