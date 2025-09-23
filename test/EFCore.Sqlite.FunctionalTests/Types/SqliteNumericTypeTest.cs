// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class SqliteByteTypeTest(SqliteByteTypeTest.ByteTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<byte, SqliteByteTypeTest.ByteTypeFixture>(fixture, testOutputHelper)
{
    public class ByteTypeFixture : RelationalTypeFixtureBase<byte>
    {
        public override byte Value { get; } = byte.MinValue;
        public override byte OtherValue { get; } = byte.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteShortTypeTest(SqliteShortTypeTest.ShortTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<short, SqliteShortTypeTest.ShortTypeFixture>(fixture, testOutputHelper)
{
    public class ShortTypeFixture : RelationalTypeFixtureBase<short>
    {
        public override short Value { get; } = short.MinValue;
        public override short OtherValue { get; } = short.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteIntTypeTest(SqliteIntTypeTest.IntTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<int, SqliteIntTypeTest.IntTypeFixture>(fixture, testOutputHelper)
{
    public class IntTypeFixture : RelationalTypeFixtureBase<int>
    {
        public override int Value { get; } = int.MinValue;
        public override int OtherValue { get; } = int.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteLongTypeTest(SqliteLongTypeTest.LongTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<long, SqliteLongTypeTest.LongTypeFixture>(fixture, testOutputHelper)
{
    public class LongTypeFixture : RelationalTypeFixtureBase<long>
    {
        public override long Value { get; } = long.MinValue;
        public override long OtherValue { get; } = long.MaxValue;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteDecimalTypeTest(SqliteDecimalTypeTest.DecimalTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<decimal, SqliteDecimalTypeTest.DecimalTypeFixture>(fixture, testOutputHelper)
{
    public class DecimalTypeFixture : RelationalTypeFixtureBase<decimal>
    {
        public override decimal Value { get; } = 30.5m;
        public override decimal OtherValue { get; } = 30m;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteDoubleTypeTest(SqliteDoubleTypeTest.DoubleTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<double, SqliteDoubleTypeTest.DoubleTypeFixture>(fixture, testOutputHelper)
{
    public class DoubleTypeFixture : RelationalTypeFixtureBase<double>
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class SqliteFloatTypeTest(SqliteFloatTypeTest.FloatTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<float, SqliteFloatTypeTest.FloatTypeFixture>(fixture, testOutputHelper)
{
    public class FloatTypeFixture : RelationalTypeFixtureBase<float>
    {
        public override float Value { get; } = 30.5f;
        public override float OtherValue { get; } = 30f;

        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}
