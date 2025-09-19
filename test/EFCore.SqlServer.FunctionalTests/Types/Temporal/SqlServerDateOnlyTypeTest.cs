// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types.Temporal;

public class SqlServerDateOnlyTypeTest(SqlServerDateOnlyTypeTest.DateTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<DateOnly, SqlServerDateOnlyTypeTest.DateTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='01/05/2020' (DbType = Date)

SELECT TOP(2) [t].[Id], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value] = @Fixture_Value
""");
    }

    public override async Task Equality_in_query_with_constant()
    {
        await base.Equality_in_query_with_constant();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value] = '2020-01-05'
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='05/03/2022' (DbType = Date)

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
@Fixture_Value='01/05/2020' (DbType = Date)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE JSON_VALUE([j].[JsonContainer], '$.Value' RETURNING date) = @Fixture_Value
""");
        }
        else
        {
            AssertSql(
                """
@Fixture_Value='01/05/2020' (DbType = Date)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS date) = @Fixture_Value
""");
        }
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"2022-05-03","Value":"2022-05-03"}' (Nullable = false) (Size = 48)
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
@complex_type_Fixture_OtherValue='2022-05-03' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='2022-05-03' (Size = 4000)

UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', @complex_type_Fixture_OtherValue)
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
SET [JsonContainer].modify('$.Value', N'2022-05-03')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'2022-05-03')
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
SET [JsonContainer].modify('$.Value', JSON_VALUE([j].[JsonContainer], '$.OtherValue' RETURNING nvarchar(max)))
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', JSON_VALUE([j].[JsonContainer], '$.OtherValue'))
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

    #endregion JSON

    public class DateTypeFixture : SqlServerTypeFixture<DateOnly>
    {
        public override DateOnly Value { get; } = new DateOnly(2020, 1, 5);
        public override DateOnly OtherValue { get; } = new DateOnly(2022, 5, 3);

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
