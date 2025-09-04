// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class DateTimeTypeTest(DateTimeTypeTest.DateTimeTypeFixture fixture)
    : TypeTestBase<DateTime, DateTimeTypeTest.DateTimeTypeFixture>(fixture)
{
    public class DateTimeTypeFixture() : TypeTestFixture(
        new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified),
        new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DateTimeOffsetTypeTest(DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture)
    : TypeTestBase<DateTimeOffset, DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture)
{
    public class DateTimeOffsetTypeFixture() : TypeTestFixture(
        new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2)),
        new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3)))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DateOnlyTypeTest(DateOnlyTypeTest.DateTypeFixture fixture) : TypeTestBase<DateOnly, DateOnlyTypeTest.DateTypeFixture>(fixture)
{
    public class DateTypeFixture() : TypeTestFixture(
        new DateOnly(2020, 1, 5),
        new DateOnly(2022, 5, 3))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class TimeOnlyTypeTest(TimeOnlyTypeTest.TimeTypeFixture fixture)
    : TypeTestBase<TimeOnly, TimeOnlyTypeTest.TimeTypeFixture>(fixture)
{
    public class TimeTypeFixture() : TypeTestFixture(
        new TimeOnly(12, 30, 45),
        new TimeOnly(14, 0, 0))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class TimeSpanTypeTest(TimeSpanTypeTest.TimeSpanTypeFixture fixture) : TypeTestBase<TimeSpan, TimeSpanTypeTest.TimeSpanTypeFixture>(fixture)
{
    public class TimeSpanTypeFixture() : TypeTestFixture(
        new TimeSpan(12, 30, 45),
        new TimeSpan(14, 0, 0))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}
