// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Numeric;

public class SqlServerDoubleTypeTest(SqlServerDoubleTypeTest.DoubleTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<double, SqlServerDoubleTypeTest.DoubleTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='30.5'

SELECT TOP(2) [t].[Id], [t].[ArrayValue], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value] = @Fixture_Value
""");
    }

    public override async Task Equality_in_query_with_constant()
    {
        await base.Equality_in_query_with_constant();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[ArrayValue], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value] = 30.5E0
""");
    }

    public override async Task Primitive_collection_in_query()
    {
        await base.Primitive_collection_in_query();

        AssertSql(
            """
@value='30.5'

SELECT TOP(2) [t].[Id], [t].[ArrayValue], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[ArrayValue]) WITH ([value] float '$') AS [a]
    WHERE [a].[value] = @value) = 2
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='30'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [TypeEntity] SET [Value] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    #region JSON

    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@Fixture_Value='30.5'

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE JSON_VALUE([j].[JsonContainer], '$.Value' RETURNING float) = @Fixture_Value
""");
        }
        else
        {
            AssertSql(
                """
@Fixture_Value='30.5'

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS float) = @Fixture_Value
""");
        }
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":30,"Value":30}' (Nullable = false) (Size = 28)
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [JsonTypeEntity] SET [JsonContainer] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    public override async Task ExecuteUpdate_within_json_to_parameter()
    {
        await base.ExecuteUpdate_within_json_to_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@Fixture_OtherValue='30'

UPDATE [j]
SET [JsonContainer].modify('$.Value', @Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@Fixture_OtherValue='30'

UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', @Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', 30.0E0)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', 30.0E0)
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    public override async Task ExecuteUpdate_within_json_to_another_json_property()
    {
        await base.ExecuteUpdate_within_json_to_another_json_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
UPDATE [j]
SET [JsonContainer].modify('$.Value', JSON_VALUE([j].[JsonContainer], '$.OtherValue' RETURNING float))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', CAST(JSON_VALUE([j].[JsonContainer], '$.OtherValue') AS float))
FROM [JsonTypeEntity] AS [j]
""");
        }
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
SET [JsonContainer].modify('$.Value', [j].[OtherValue])
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', [j].[OtherValue])
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    #endregion JSON

    public class DoubleTypeFixture : SqlServerTypeFixture<double>
    {
        public override double Value { get; } = 30.5d;
        public override double OtherValue { get; } = 30d;
    }
}
