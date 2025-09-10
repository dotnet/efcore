// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class DateTimeTypeTest(DateTimeTypeTest.DateTimeTypeFixture fixture)
    : RelationalTypeTestBase<DateTime, DateTimeTypeTest.DateTimeTypeFixture>(fixture)
{
    public class DateTimeTypeFixture : RelationalTypeTestFixture
    {
        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }
}

public class DateTimeOffsetTypeTest(DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture)
    : RelationalTypeTestBase<DateTimeOffset, DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class DateTimeOffsetTypeFixture : RelationalTypeTestFixture
    {
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2));
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3));

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DateOnlyTypeTest(DateOnlyTypeTest.DateTypeFixture fixture) : RelationalTypeTestBase<DateOnly, DateOnlyTypeTest.DateTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class DateTypeFixture : RelationalTypeTestFixture
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class TimeOnlyTypeTest(TimeOnlyTypeTest.TimeTypeFixture fixture)
    : RelationalTypeTestBase<TimeOnly, TimeOnlyTypeTest.TimeTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class TimeTypeFixture : RelationalTypeTestFixture
    {
        public override TimeOnly Value { get; } = new TimeOnly(12, 30, 45);
        public override TimeOnly OtherValue { get; } = new TimeOnly(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class TimeSpanTypeTest(TimeSpanTypeTest.TimeSpanTypeFixture fixture) : RelationalTypeTestBase<TimeSpan, TimeSpanTypeTest.TimeSpanTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class TimeSpanTypeFixture : RelationalTypeTestFixture
    {
        public override TimeSpan Value { get; } = new TimeSpan(12, 30, 45);
        public override TimeSpan OtherValue { get; } = new TimeSpan(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}
