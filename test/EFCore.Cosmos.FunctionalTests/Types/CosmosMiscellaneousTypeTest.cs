// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Miscellaneous;

public class CosmosBoolTypeTest(CosmosBoolTypeTest.BoolTypeFixture fixture)
    : TypeTestBase<bool, CosmosBoolTypeTest.BoolTypeFixture>(fixture)
{
    public class BoolTypeFixture : CosmosTypeFixtureBase<bool>
    {
        public override bool Value { get; } = true;
        public override bool OtherValue { get; } = false;

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosStringTypeTest(CosmosStringTypeTest.StringTypeFixture fixture)
    : TypeTestBase<string, CosmosStringTypeTest.StringTypeFixture>(fixture)
{
    public class StringTypeFixture : CosmosTypeFixtureBase<string>
    {
        public override string Value { get; } = "foo";
        public override string OtherValue { get; } = "bar";

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosGuidTypeTest(CosmosGuidTypeTest.GuidTypeFixture fixture)
    : TypeTestBase<Guid, CosmosGuidTypeTest.GuidTypeFixture>(fixture)
{
    public class GuidTypeFixture : CosmosTypeFixtureBase<Guid>
    {
        public override Guid Value { get; } = new("8f7331d6-cde9-44fb-8611-81fff686f280");
        public override Guid OtherValue { get; } = new("ae192c36-9004-49b2-b785-8be10d169627");

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
    }
}

public class CosmosByteArrayTypeTest(CosmosByteArrayTypeTest.ByteArrayTypeFixture fixture)
    : TypeTestBase<byte[], CosmosByteArrayTypeTest.ByteArrayTypeFixture>(fixture)
{
    public class ByteArrayTypeFixture : CosmosTypeFixtureBase<byte[]>
    {
        public override byte[] Value { get; } = [1, 2, 3];
        public override byte[] OtherValue { get; } = [4, 5, 6, 7];

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);
    }
}
