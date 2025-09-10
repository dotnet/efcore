// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture) : RelationalTypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture)
{
    public class ByteTypeFixture : RelationalTypeTestFixture
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture) : RelationalTypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture)
{
    public class ShortTypeFixture : RelationalTypeTestFixture
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture) : RelationalTypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture)
{
    public class IntTypeFixture : RelationalTypeTestFixture
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture) : RelationalTypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture)
{
    public class LongTypeFixture : RelationalTypeTestFixture
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture) : RelationalTypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture)
{
    public class DecimalTypeFixture : RelationalTypeTestFixture
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture) : RelationalTypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture)
{
    public class DoubleTypeFixture : RelationalTypeTestFixture
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture) : RelationalTypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture)
{
    public class FloatTypeFixture : RelationalTypeTestFixture
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}
