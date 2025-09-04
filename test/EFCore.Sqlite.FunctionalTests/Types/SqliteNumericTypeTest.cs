// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class ByteTypeTest(ByteTypeTest.ByteTypeFixture fixture) : RelationalTypeTestBase<byte, ByteTypeTest.ByteTypeFixture>(fixture)
{
    public class ByteTypeFixture() : RelationalTypeTestFixture(byte.MinValue, byte.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class ShortTypeTest(ShortTypeTest.ShortTypeFixture fixture) : RelationalTypeTestBase<short, ShortTypeTest.ShortTypeFixture>(fixture)
{
    public class ShortTypeFixture() : RelationalTypeTestFixture(short.MinValue, short.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class IntTypeTest(IntTypeTest.IntTypeFixture fixture) : RelationalTypeTestBase<int, IntTypeTest.IntTypeFixture>(fixture)
{
    public class IntTypeFixture() : RelationalTypeTestFixture(int.MinValue, int.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class LongTypeTest(LongTypeTest.LongTypeFixture fixture) : RelationalTypeTestBase<long, LongTypeTest.LongTypeFixture>(fixture)
{
    public class LongTypeFixture() : RelationalTypeTestFixture(long.MinValue, long.MaxValue)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DecimalTypeTest(DecimalTypeTest.DecimalTypeFixture fixture) : RelationalTypeTestBase<decimal, DecimalTypeTest.DecimalTypeFixture>(fixture)
{
    public class DecimalTypeFixture() : RelationalTypeTestFixture(30.5m, 30m)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class DoubleTypeTest(DoubleTypeTest.DoubleTypeFixture fixture) : RelationalTypeTestBase<double, DoubleTypeTest.DoubleTypeFixture>(fixture)
{
    public class DoubleTypeFixture() : RelationalTypeTestFixture(30.5d, 30d)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class FloatTypeTest(FloatTypeTest.FloatTypeFixture fixture) : RelationalTypeTestBase<float, FloatTypeTest.FloatTypeFixture>(fixture)
{
    public class FloatTypeFixture() : RelationalTypeTestFixture(30.5f, 30f)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}
