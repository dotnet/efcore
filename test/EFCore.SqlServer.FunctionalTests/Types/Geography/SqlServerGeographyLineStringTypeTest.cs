// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public class SqlServerGeographyLineStringTypeTest(
    SqlServerGeographyLineStringTypeTest.LineStringTypeFixture fixture,
    ITestOutputHelper testOutputHelper)
    : SqlServerGeographyTypeTestBase<LineString, SqlServerGeographyLineStringTypeTest.LineStringTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0xE61000000114C9772975C9CF4740DCF4673F52965EC0AAD2BB1D86CC47407854...' (Size = 38) (DbType = Object)

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
WHERE [t].[Value].STEquals('LINESTRING (-122.34877 47.6233355, -122.3308366 47.5978429)') = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='0xE610000001040300000033333333337347400000000000605EC0333333333353...' (Size = 80) (DbType = Object)

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
@Fixture_Value='0xE61000000114C9772975C9CF4740DCF4673F52965EC0AAD2BB1D86CC47407854...' (Size = 38) (DbType = Object)

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
@p0='{"OtherValue":"LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)","Value":"LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)"}' (Nullable = false) (Size = 126)
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
@complex_type_Fixture_OtherValue='LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'LINESTRING (-121.5 46.9, -121.2 46.65, -121 46.4)')
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

    public class LineStringTypeFixture : SqlServerGeographyTypeFixture
    {
        public override LineString Value { get; } = new(
        [
            new Coordinate(-122.34877, 47.6233355),
            new Coordinate(-122.3308366, 47.5978429)
        ])
        { SRID = 4326 };

        public override LineString OtherValue { get; } = new(
        [
            new Coordinate(-121.5000, 46.9000),
            new Coordinate(-121.2000, 46.6500),
            new Coordinate(-121.0000, 46.4000)
        ])
        { SRID = 4326 };
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
