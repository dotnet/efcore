// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Miscellaneous;

public class BoolTypeTest(BoolTypeTest.BoolTypeFixture fixture)
    : RelationalTypeTestBase<bool, BoolTypeTest.BoolTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@Fixture_Value='True'

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE JSON_VALUE([j].[JsonContainer], '$.Value' RETURNING bit) = @Fixture_Value
""");
        }
        else
        {
            AssertSql(
                """
@Fixture_Value='True'

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS bit) = @Fixture_Value
""");
        }
    }

    public class BoolTypeFixture : SqlServerTypeFixture<bool>
    {
        public override bool Value { get; } = true;
        public override bool OtherValue { get; } = false;

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class StringTypeTest(StringTypeTest.StringTypeFixture fixture)
    : RelationalTypeTestBase<string, StringTypeTest.StringTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@Fixture_Value='foo' (Size = 4000)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE JSON_VALUE([j].[JsonContainer], '$.Value' RETURNING nvarchar(max)) = @Fixture_Value
""");
        }
        else
        {
            AssertSql(
                """
@Fixture_Value='foo' (Size = 4000)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE JSON_VALUE([j].[JsonContainer], '$.Value') = @Fixture_Value
""");
        }
    }

    public class StringTypeFixture : SqlServerTypeFixture<string>
    {
        public override string Value { get; } = "foo";
        public override string OtherValue { get; } = "bar";

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class GuidTypeTest(GuidTypeTest.GuidTypeFixture fixture)
    : RelationalTypeTestBase<Guid, GuidTypeTest.GuidTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with uniqueidentifier even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='8f7331d6-cde9-44fb-8611-81fff686f280'

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS uniqueidentifier) = @Fixture_Value
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class GuidTypeFixture : SqlServerTypeFixture<Guid>
    {
        public override Guid Value { get; } = new("8f7331d6-cde9-44fb-8611-81fff686f280");
        public override Guid OtherValue { get; } = new("ae192c36-9004-49b2-b785-8be10d169627");

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => TestEnvironment.SetCompatibilityLevelFromEnvironment(base.AddOptions(builder));
    }
}

public class ByteArrayTypeTest(ByteArrayTypeTest.ByteArrayTypeFixture fixture)
    : RelationalTypeTestBase<byte[], ByteArrayTypeTest.ByteArrayTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with varbinary even on SQL Server 2025, as that type isn't supported
        // (#36627).
        // We also can't just wrap JSON_VALUE() with CAST(... AS varbinary(max)), as that would apply a SQL Server binary format
        // conversion, and not base64. So we use OPENJSON which does perform base64 conversion.
        AssertSql(
            """
@Fixture_Value='0x010203' (Size = 8000)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
OUTER APPLY OPENJSON([j].[JsonContainer]) WITH ([Value] varbinary(max) '$.Value') AS [v]
WHERE [v].[Value] = @Fixture_Value
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // TODO: Currently failing on Helix only, see #36746
        if (Environment.GetEnvironmentVariable("HELIX_WORKITEM_ROOT") is not null)
        {
            return;
        }

        await base.ExecuteUpdate_within_json_to_nonjson_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE(JSON_OBJECT('v': [j].[OtherValue]), '$.v'))
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public class ByteArrayTypeFixture() : SqlServerTypeFixture<byte[]>
    {
        public override byte[] Value { get; } = [1, 2, 3];
        public override byte[] OtherValue { get; } = [4, 5, 6, 7];

        public override Func<byte[], byte[], bool> Comparer { get; } = (a, b) => a.SequenceEqual(b);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => TestEnvironment.SetCompatibilityLevelFromEnvironment(base.AddOptions(builder));
    }
}
