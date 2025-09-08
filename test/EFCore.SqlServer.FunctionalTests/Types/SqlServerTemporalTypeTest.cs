// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class DateTimeTypeTest(DateTimeTypeTest.DateTimeTypeFixture fixture)
    : RelationalTypeTestBase<DateTime, DateTimeTypeTest.DateTimeTypeFixture>(fixture)
{
    public class DateTimeTypeFixture() : RelationalTypeTestFixture(
        new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified),
        new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified))
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
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

    public class DateTimeOffsetTypeFixture() : RelationalTypeTestFixture(
        new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2)),
        new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3)))
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
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

    public class DateTypeFixture() : RelationalTypeTestFixture(
        new DateOnly(2020, 1, 5),
        new DateOnly(2022, 5, 3))
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
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

    public class TimeTypeFixture() : RelationalTypeTestFixture(
        new TimeOnly(12, 30, 45),
        new TimeOnly(14, 0, 0))
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
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

    public class TimeSpanTypeFixture() : RelationalTypeTestFixture(
        new TimeSpan(12, 30, 45),
        new TimeSpan(14, 0, 0))
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}
