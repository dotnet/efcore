// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Miscellaneous;

public class SqliteBoolTypeTest(SqliteBoolTypeTest.BoolTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<bool, SqliteBoolTypeTest.BoolTypeFixture>(fixture, testOutputHelper)
{
    public class BoolTypeFixture : SqliteTypeFixture<bool>
    {
        public override bool Value { get; } = true;
        public override bool OtherValue { get; } = false;
    }
}

public class SqliteStringTypeTest(SqliteStringTypeTest.StringTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<string, SqliteStringTypeTest.StringTypeFixture>(fixture, testOutputHelper)
{
    public class StringTypeFixture : SqliteTypeFixture<string>
    {
        public override string Value { get; } = "foo";
        public override string OtherValue { get; } = "bar";
    }
}

public class SqliteGuidTypeTest(SqliteGuidTypeTest.GuidTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<Guid, SqliteGuidTypeTest.GuidTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class GuidTypeFixture : SqliteTypeFixture<Guid>
    {
        public override Guid Value { get; } = new("8f7331d6-cde9-44fb-8611-81fff686f280");
        public override Guid OtherValue { get; } = new("ae192c36-9004-49b2-b785-8be10d169627");
    }
}

public class SqliteByteArrayTypeTest(SqliteByteArrayTypeTest.ByteArrayTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<byte[], SqliteByteArrayTypeTest.ByteArrayTypeFixture>(fixture, testOutputHelper)
{
    public override Task Query_property_within_json() => base.Query_property_within_json();

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for Sqlite types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.InnerException!.Message);
    }

    public class ByteArrayTypeFixture : SqliteTypeFixture<byte[]>
    {
        public override byte[] Value { get; } = [1, 2, 3];
        public override byte[] OtherValue { get; } = [4, 5, 6, 7];

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);
    }
}
