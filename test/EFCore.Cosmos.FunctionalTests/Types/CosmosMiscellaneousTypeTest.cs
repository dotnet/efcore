// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Miscellaneous;

public class BoolTypeTest(BoolTypeTest.BoolTypeFixture fixture)
    : TypeTestBase<bool, BoolTypeTest.BoolTypeFixture>(fixture)
{
    public class BoolTypeFixture : TypeTestFixture
    {
        public override bool Value { get; } = true;
        public override bool OtherValue { get; } = false;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class StringTypeTest(StringTypeTest.StringTypeFixture fixture)
    : TypeTestBase<string, StringTypeTest.StringTypeFixture>(fixture)
{
    public class StringTypeFixture : TypeTestFixture
    {
        public override string Value { get; } = "foo";
        public override string OtherValue { get; } = "bar";

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class GuidTypeTest(GuidTypeTest.GuidTypeFixture fixture)
    : TypeTestBase<Guid, GuidTypeTest.GuidTypeFixture>(fixture)
{
    public class GuidTypeFixture : TypeTestFixture
    {
        public override Guid Value { get; } = new("8f7331d6-cde9-44fb-8611-81fff686f280");
        public override Guid OtherValue { get; } = new("ae192c36-9004-49b2-b785-8be10d169627");

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));
    }
}

public class ByteArrayTypeTest(ByteArrayTypeTest.ByteArrayTypeFixture fixture)
    : TypeTestBase<byte[], ByteArrayTypeTest.ByteArrayTypeFixture>(fixture)
{
    public class ByteArrayTypeFixture : TypeTestFixture
    {
        public override byte[] Value { get; } = [1, 2, 3];
        public override byte[] OtherValue { get; } = [4, 5, 6, 7];

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CosmosEventId.NoPartitionKeyDefined));

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);
    }
}
