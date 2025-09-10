// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture) : TypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture)
{
    public class ByteTypeFixture : TypeTestFixture
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture) : TypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture)
{
    public class ShortTypeFixture : TypeTestFixture
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture) : TypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture)
{
    public class IntTypeFixture : TypeTestFixture
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture) : TypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture)
{
    public class LongTypeFixture : TypeTestFixture
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture) : TypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture)
{
    public class DecimalTypeFixture : TypeTestFixture
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture) : TypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture)
{
    public class DoubleTypeFixture : TypeTestFixture
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture) : TypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture)
{
    public class FloatTypeFixture : TypeTestFixture
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}
