// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class CosmosDateTimeTypeTest(CosmosDateTimeTypeTest.DateTimeTypeFixture fixture)
    : TypeTestBase<DateTime, CosmosDateTimeTypeTest.DateTimeTypeFixture>(fixture)
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/287 (Aggregates over subqueries return null result set)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Primitive_collection_in_query()
        => base.Primitive_collection_in_query();

    public class DateTimeTypeFixture : CosmosTypeFixtureBase<DateTime>
    {
        public override DateTime Value { get; } = new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified);
        public override DateTime OtherValue { get; } = new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosDateTimeOffsetTypeTest(CosmosDateTimeOffsetTypeTest.DateTimeOffsetTypeFixture fixture)
    : TypeTestBase<DateTimeOffset, CosmosDateTimeOffsetTypeTest.DateTimeOffsetTypeFixture>(fixture)
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/287 (Aggregates over subqueries return null result set)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Primitive_collection_in_query()
        => base.Primitive_collection_in_query();

    public class DateTimeOffsetTypeFixture : CosmosTypeFixtureBase<DateTimeOffset>
    {
        public override DateTimeOffset Value { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2));
        public override DateTimeOffset OtherValue { get; } = new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3));

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosDateOnlyTypeTest(CosmosDateOnlyTypeTest.DateOnlyTypeFixture fixture) : TypeTestBase<DateOnly, CosmosDateOnlyTypeTest.DateOnlyTypeFixture>(fixture)
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/287 (Aggregates over subqueries return null result set)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Primitive_collection_in_query()
        => base.Primitive_collection_in_query();

    public class DateOnlyTypeFixture : CosmosTypeFixtureBase<DateOnly>
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosTimeOnlyTypeTest(CosmosTimeOnlyTypeTest.TimeOnlyTypeFixture fixture)
    : TypeTestBase<TimeOnly, CosmosTimeOnlyTypeTest.TimeOnlyTypeFixture>(fixture)
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/287 (Aggregates over subqueries return null result set)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Primitive_collection_in_query()
        => base.Primitive_collection_in_query();

    public class TimeOnlyTypeFixture : CosmosTypeFixtureBase<TimeOnly>
    {
        public override TimeOnly Value { get; } = new TimeOnly(12, 30, 45);
        public override TimeOnly OtherValue { get; } = new TimeOnly(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosTimeSpanTypeTest(CosmosTimeSpanTypeTest.TimeSpanTypeFixture fixture) : TypeTestBase<TimeSpan, CosmosTimeSpanTypeTest.TimeSpanTypeFixture>(fixture)
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/287 (Aggregates over subqueries return null result set)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Primitive_collection_in_query()
        => base.Primitive_collection_in_query();

    public class TimeSpanTypeFixture : CosmosTypeFixtureBase<TimeSpan>
    {
        public override TimeSpan Value { get; } = new TimeSpan(12, 30, 45);
        public override TimeSpan OtherValue { get; } = new TimeSpan(14, 0, 0);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}
