// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture, testOutputHelper)
{
    public class ByteTypeFixture : RelationalTypeFixtureBase<byte>
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture, testOutputHelper)
{
    public class ShortTypeFixture : RelationalTypeFixtureBase<short>
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture, testOutputHelper)
{
    public class IntTypeFixture : RelationalTypeFixtureBase<int>
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture, testOutputHelper)
{
    public class LongTypeFixture : RelationalTypeFixtureBase<long>
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture, testOutputHelper)
{
    public class DecimalTypeFixture : RelationalTypeFixtureBase<decimal>
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture, testOutputHelper)
{
    public class DoubleTypeFixture : RelationalTypeFixtureBase<double>
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture, testOutputHelper)
{
    public class FloatTypeFixture : RelationalTypeFixtureBase<float>
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}
