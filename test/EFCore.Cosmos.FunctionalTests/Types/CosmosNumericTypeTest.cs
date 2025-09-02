// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture) : TypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture)
{
    public class ByteTypeFixture() : TypeTestFixture(byte.MinValue, byte.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture) : TypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture)
{
    public class ShortTypeFixture() : TypeTestFixture(short.MinValue, short.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture) : TypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture)
{
    public class IntTypeFixture() : TypeTestFixture(int.MinValue, int.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture) : TypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture)
{
    public class LongTypeFixture() : TypeTestFixture(long.MinValue, long.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture) : TypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture)
{
    public class DecimalTypeFixture() : TypeTestFixture(30.5m, 30m)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture) : TypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture)
{
    public class DoubleTypeFixture() : TypeTestFixture(30.5d, 30d)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture) : TypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture)
{
    public class FloatTypeFixture() : TypeTestFixture(30.5f, 30f)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}
