// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture, testOutputHelper)
{
    public class ByteTypeFixture : SqliteTypeFixture<byte>
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture, testOutputHelper)
{
    public class ShortTypeFixture : SqliteTypeFixture<short>
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture, testOutputHelper)
{
    public class IntTypeFixture : SqliteTypeFixture<int>
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture, testOutputHelper)
{
    public class LongTypeFixture : SqliteTypeFixture<long>
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture, testOutputHelper)
{
    public class DecimalTypeFixture : SqliteTypeFixture<decimal>
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture, testOutputHelper)
{
    public class DoubleTypeFixture : SqliteTypeFixture<double>
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture, testOutputHelper)
{
    public class FloatTypeFixture : SqliteTypeFixture<float>
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;
    }
}
