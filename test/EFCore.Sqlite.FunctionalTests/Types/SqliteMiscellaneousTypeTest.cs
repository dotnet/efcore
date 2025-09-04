// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class BoolTypeTest(BoolTypeTest.BoolTypeFixture fixture)
    : RelationalTypeTestBase<bool, BoolTypeTest.BoolTypeFixture>(fixture)
{
    public class BoolTypeFixture() : RelationalTypeTestFixture(true, false)
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class StringTypeTest(StringTypeTest.StringTypeFixture fixture)
    : RelationalTypeTestBase<string, StringTypeTest.StringTypeFixture>(fixture)
{
    public class StringTypeFixture() : RelationalTypeTestFixture("foo", "bar")
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class GuidTypeTest(GuidTypeTest.GuidTypeFixture fixture)
    : RelationalTypeTestBase<Guid, GuidTypeTest.GuidTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class GuidTypeFixture() : RelationalTypeTestFixture(
        new Guid("8f7331d6-cde9-44fb-8611-81fff686f280"),
        new Guid("ae192c36-9004-49b2-b785-8be10d169627"))
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
    }
}

public class ByteArrayTypeTest(ByteArrayTypeTest.ByteArrayTypeFixture fixture)
    : RelationalTypeTestBase<byte[], ByteArrayTypeTest.ByteArrayTypeFixture>(fixture)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public class ByteArrayTypeFixture() : RelationalTypeTestFixture([1, 2, 3], [4, 5, 6])
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);
    }
}
