// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class CosmosByteTypeTest(CosmosByteTypeTest.ByteTypeFixture fixture)
    : TypeTestBase<byte, CosmosByteTypeTest.ByteTypeFixture>(fixture)
{
    public class ByteTypeFixture : CosmosTypeFixtureBase<byte>
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosShortTypeTest(CosmosShortTypeTest.ShortTypeFixture fixture)
    : TypeTestBase<short, CosmosShortTypeTest.ShortTypeFixture>(fixture)
{
    public class ShortTypeFixture : CosmosTypeFixtureBase<short>
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosIntTypeTest(CosmosIntTypeTest.IntTypeFixture fixture) : TypeTestBase<int, CosmosIntTypeTest.IntTypeFixture>(fixture)
{
    public class IntTypeFixture : CosmosTypeFixtureBase<int>
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosLongTypeTest(CosmosLongTypeTest.LongTypeFixture fixture)
    : TypeTestBase<long, CosmosLongTypeTest.LongTypeFixture>(fixture)
{
    public class LongTypeFixture : CosmosTypeFixtureBase<long>
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosDecimalTypeTest(CosmosDecimalTypeTest.DecimalTypeFixture fixture)
    : TypeTestBase<decimal, CosmosDecimalTypeTest.DecimalTypeFixture>(fixture)
{
    public class DecimalTypeFixture : CosmosTypeFixtureBase<decimal>
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosDoubleTypeTest(CosmosDoubleTypeTest.DoubleTypeFixture fixture)
    : TypeTestBase<double, CosmosDoubleTypeTest.DoubleTypeFixture>(fixture)
{
    public class DoubleTypeFixture : CosmosTypeFixtureBase<double>
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosFloatTypeTest(CosmosFloatTypeTest.FloatTypeFixture fixture)
    : TypeTestBase<float, CosmosFloatTypeTest.FloatTypeFixture>(fixture)
{
    public class FloatTypeFixture : CosmosTypeFixtureBase<float>
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}
