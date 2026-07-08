// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class DateTimeTypeTest(DateTimeTypeTest.DateTimeTypeFixture fixture)
    : TypeTestBase<DateTime, DateTimeTypeTest.DateTimeTypeFixture>(fixture)
{
    public class DateTimeTypeFixture : CosmosTypeFixtureBase<DateTime>
    {
        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class DateTimeOffsetTypeTest(DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture)
    : TypeTestBase<DateTimeOffset, DateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture)
{
    public class DateTimeOffsetTypeFixture : CosmosTypeFixtureBase<DateTimeOffset>
    {
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2));
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3));

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class DateOnlyTypeTest(DateOnlyTypeTest.DateOnlyTypeFixture fixture) : TypeTestBase<DateOnly, DateOnlyTypeTest.DateOnlyTypeFixture>(fixture)
{
    public class DateOnlyTypeFixture : CosmosTypeFixtureBase<DateOnly>
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class TimeOnlyTypeTest(TimeOnlyTypeTest.TimeOnlyTypeFixture fixture)
    : TypeTestBase<TimeOnly, TimeOnlyTypeTest.TimeOnlyTypeFixture>(fixture)
{
    public class TimeOnlyTypeFixture : CosmosTypeFixtureBase<TimeOnly>
    {
        public override TimeOnly Value { get; } = new TimeOnly(12, 30, 45);
        public override TimeOnly OtherValue { get; } = new TimeOnly(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class TimeSpanTypeTest(TimeSpanTypeTest.TimeSpanTypeFixture fixture) : TypeTestBase<TimeSpan, TimeSpanTypeTest.TimeSpanTypeFixture>(fixture)
{
    public class TimeSpanTypeFixture : CosmosTypeFixtureBase<TimeSpan>
    {
        public override TimeSpan Value { get; } = new TimeSpan(12, 30, 45);
        public override TimeSpan OtherValue { get; } = new TimeSpan(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}
