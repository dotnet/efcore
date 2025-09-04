// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class BoolTypeTest(BoolTypeTest.BoolTypeFixture fixture)
    : TypeTestBase<bool, BoolTypeTest.BoolTypeFixture>(fixture)
{
    public class BoolTypeFixture() : TypeTestFixture(true, false)
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class StringTypeTest(StringTypeTest.StringTypeFixture fixture)
    : TypeTestBase<string, StringTypeTest.StringTypeFixture>(fixture)
{
    public class StringTypeFixture() : TypeTestFixture("foo", "bar")
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class GuidTypeTest(GuidTypeTest.GuidTypeFixture fixture)
    : TypeTestBase<Guid, GuidTypeTest.GuidTypeFixture>(fixture)
{
    public class GuidTypeFixture() : TypeTestFixture(
        new Guid("8f7331d6-cde9-44fb-8611-81fff686f280"),
        new Guid("ae192c36-9004-49b2-b785-8be10d169627"))
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class ByteArrayTypeTest(ByteArrayTypeTest.ByteArrayTypeFixture fixture)
    : TypeTestBase<byte[], ByteArrayTypeTest.ByteArrayTypeFixture>(fixture)
{
    public class ByteArrayTypeFixture() : TypeTestFixture([1, 2, 3], [4, 5, 6])
    {
        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);
    }
}
