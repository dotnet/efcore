// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public class SqlServerGeographyCollectionTypeTest(SqlServerGeographyCollectionTypeTest.GeographyCollectionTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerGeographyTypeTestBase<GeometryCollection, SqlServerGeographyCollectionTypeTest.GeographyCollectionTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0xE61000000104080000008FC2F5285CCF47406666666666965EC08FC2F5285CCF...' (Size = 197) (DbType = Object)

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
WHERE [t].[Value].STEquals('GEOMETRYCOLLECTION (POINT (-122.35 47.62), LINESTRING (-122.35 47.62, -122.345 47.615), POLYGON ((-122.348 47.619, -122.348 47.617, -122.346 47.617, -122.346 47.619, -122.348 47.619)))') = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='0xE61000000104080000009A999999997947409A99999999795EC09A9999999979...' (Size = 197) (DbType = Object)

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
@Fixture_Value='0xE61000000104080000008FC2F5285CCF47406666666666965EC08FC2F5285CCF...' (Size = 197) (DbType = Object)

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
@p0='{"OtherValue":"GEOMETRYCOLLECTION (POINT (-121.9 46.95)
LINESTRING (-121.9 46.95, -121.6 46.82)
POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))","Value":"GEOMETRYCOLLECTION (POINT (-121.9 46.95)
LINESTRING (-121.9 46.95, -121.6 46.82)
POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))"}' (Nullable = false) (Size = 366)
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
@complex_type_Fixture_OtherValue='GEOMETRYCOLLECTION (POINT (-121.9 46.95)
LINESTRING (-121.9 46.95, -121.6 46.82)
POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='GEOMETRYCOLLECTION (POINT (-121.9 46.95)
LINESTRING (-121.9 46.95, -121.6 46.82)
POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'GEOMETRYCOLLECTION (POINT (-121.9 46.95), LINESTRING (-121.9 46.95, -121.6 46.82), POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'GEOMETRYCOLLECTION (POINT (-121.9 46.95), LINESTRING (-121.9 46.95, -121.6 46.82), POLYGON ((-121.88 46.94, -121.88 46.92, -121.86 46.92, -121.86 46.94, -121.88 46.94)))')
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

    public class GeographyCollectionTypeFixture() : GeographyTypeFixture
    {
        public override GeometryCollection Value { get; } = new(
        [
            new Point(-122.3500, 47.6200) { SRID = 4326 },
            new LineString([
                new Coordinate(-122.3500, 47.6200),
                new Coordinate(-122.3450, 47.6150)
            ]) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-122.3480, 47.6190), // NW
                new Coordinate(-122.3480, 47.6170), // SW
                new Coordinate(-122.3460, 47.6170), // SE
                new Coordinate(-122.3460, 47.6190), // NE
                new Coordinate(-122.3480, 47.6190)
            ])) { SRID = 4326 }
        ])
        { SRID = 4326 };

        public override GeometryCollection OtherValue { get; } = new(
        [
            new Point(-121.9000, 46.9500) { SRID = 4326 },
            new LineString([
                new Coordinate(-121.9000, 46.9500),
                new Coordinate(-121.6000, 46.8200)
            ]) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-121.8800, 46.9400), // NW
                new Coordinate(-121.8800, 46.9200), // SW
                new Coordinate(-121.8600, 46.9200), // SE
                new Coordinate(-121.8600, 46.9400), // NE
                new Coordinate(-121.8800, 46.9400)
            ])) { SRID = 4326 }
        ])
        { SRID = 4326 };
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
