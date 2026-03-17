// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public class SqlServerGeographyPolygonTypeTest(SqlServerGeographyPolygonTypeTest.PolygonTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerGeographyTypeTestBase<Polygon, SqlServerGeographyPolygonTypeTest.PolygonTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0xE61000000104050000008FC2F5285CCF47406666666666965EC0AE47E17A14CE...' (Size = 112) (DbType = Object)

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
WHERE [t].[Value].STEquals('POLYGON ((-122.35 47.62, -122.35 47.61, -122.34 47.61, -122.34 47.62, -122.35 47.62))') = CAST(1 AS bit)
""");
    }

    public override async Task Primitive_collection_in_query()
    {
        await base.Primitive_collection_in_query();
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='0xE6100000010405000000CDCCCCCCCC4C47403333333333535EC0EC51B81E854B...' (Size = 112) (DbType = Object)

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
@Fixture_Value='0xE61000000104050000008FC2F5285CCF47406666666666965EC0AE47E17A14CE...' (Size = 112) (DbType = Object)

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
@p0='{"OtherValue":"POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))","Value":"POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))"}' (Nullable = false) (Size = 186)
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
@complex_type_Fixture_OtherValue='POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'POLYGON ((-121.3 46.6, -121.3 46.59, -121.28 46.59, -121.28 46.6, -121.3 46.6))')
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

    public class PolygonTypeFixture : SqlServerGeographyTypeFixture
    {
        // Simple rectangle
        public override Polygon Value { get; } = new(
            new LinearRing([
                new Coordinate(-122.3500, 47.6200), // NW
                new Coordinate(-122.3500, 47.6100), // SW
                new Coordinate(-122.3400, 47.6100), // SE
                new Coordinate(-122.3400, 47.6200), // NE
                new Coordinate(-122.3500, 47.6200)  // Close
            ]))
        { SRID = 4326 };

        // Shifted rectangle; different area so not topologically equal
        public override Polygon OtherValue { get; } = new(
            new LinearRing([
                new Coordinate(-121.3000, 46.6000), // NW
                new Coordinate(-121.3000, 46.5900), // SW
                new Coordinate(-121.2800, 46.5900), // SE
                new Coordinate(-121.2800, 46.6000), // NE
                new Coordinate(-121.3000, 46.6000)
            ]))
        { SRID = 4326 };
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
