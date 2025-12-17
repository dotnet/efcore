// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public class SqlServerGeographyMultiPointTypeTest(SqlServerGeographyMultiPointTypeTest.MultiPointTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerGeographyTypeTestBase<MultiPoint, SqlServerGeographyMultiPointTypeTest.MultiPointTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0xE61000000104020000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 87) (DbType = Object)

SELECT TOP(2) [t].[Id], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value].STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public override async Task Equality_in_query_with_constant()
    {
        await base.Equality_in_query_with_constant();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[OtherValue], [t].[Value]
FROM [TypeEntity] AS [t]
WHERE [t].[Value].STEquals('MULTIPOINT ((-122.35 47.62), (-122.345 47.615))') = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='0xE61000000104030000009A999999997947409A99999999795EC0CDCCCCCCCC4C...' (Size = 117) (DbType = Object)

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

        // Note that the JSON_VALUE RETURNING clause is never used with geography even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000104020000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 87) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"MULTIPOINT ((-121.9 46.95)
(-121.5 46.6)
(-121.2 46.3))","Value":"MULTIPOINT ((-121.9 46.95)
(-121.5 46.6)
(-121.2 46.3))"}' (Nullable = false) (Size = 142)
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
@complex_type_Fixture_OtherValue='MULTIPOINT ((-121.9 46.95)
(-121.5 46.6)
(-121.2 46.3))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='MULTIPOINT ((-121.9 46.95)
(-121.5 46.6)
(-121.2 46.3))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'MULTIPOINT ((-121.9 46.95), (-121.5 46.6), (-121.2 46.3))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'MULTIPOINT ((-121.9 46.95), (-121.5 46.6), (-121.2 46.3))')
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
SET [JsonContainer].modify('$.Value', [j].[OtherValue].STAsText())
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', [j].[OtherValue].STAsText())
FROM [JsonTypeEntity] AS [j]
""");
        }
    }

    #endregion JSON

    public class MultiPointTypeFixture : SqlServerGeographyTypeFixture
    {
        public override MultiPoint Value { get; } = new([
            new Point(-122.3500, 47.6200) { SRID = 4326 },
            new Point(-122.3450, 47.6150) { SRID = 4326 }
        ])
        { SRID = 4326 };

        public override MultiPoint OtherValue { get; } = new([
            new Point(-121.9000, 46.9500) { SRID = 4326 },
            new Point(-121.5000, 46.6000) { SRID = 4326 },
            new Point(-121.2000, 46.3000) { SRID = 4326 }
        ])
        { SRID = 4326 };
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
