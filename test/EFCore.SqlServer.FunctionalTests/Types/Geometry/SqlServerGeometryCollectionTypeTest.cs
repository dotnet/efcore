// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geometry;

public class SqlServerGeometryCollectionTypeTest(SqlServerGeometryCollectionTypeTest.GeometryCollectionTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : SqlServerGeometryTypeTestBase<GeometryCollection, SqlServerGeometryCollectionTypeTest.GeometryCollectionTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0x00000000010408000000000000000000F03F000000000000F03F000000000000...' (Size = 197) (DbType = Object)

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
WHERE [t].[Value].STEquals('GEOMETRYCOLLECTION (POINT (1 1), LINESTRING (2 2, 3 3), POLYGON ((4 4, 4 6, 6 6, 6 4, 4 4)))') = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges()
    {
        await base.SaveChanges();

        AssertSql(
            """
@p1='1'
@p0='0x0000000001040800000000000000000024400000000000002440000000000000...' (Size = 197) (DbType = Object)

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

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x00000000010408000000000000000000F03F000000000000F03F000000000000...' (Size = 197) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public override async Task SaveChanges_within_json()
    {
        await base.SaveChanges_within_json();

        AssertSql(
            """
@p0='{"OtherValue":"GEOMETRYCOLLECTION (POINT (10 10)
LINESTRING (11 11, 12 12)
POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))","Value":"GEOMETRYCOLLECTION (POINT (10 10)
LINESTRING (11 11, 12 12)
POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))"}' (Nullable = false) (Size = 244)
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
@complex_type_Fixture_OtherValue='GEOMETRYCOLLECTION (POINT (10 10)
LINESTRING (11 11, 12 12)
POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='GEOMETRYCOLLECTION (POINT (10 10)
LINESTRING (11 11, 12 12)
POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'GEOMETRYCOLLECTION (POINT (10 10), LINESTRING (11 11, 12 12), POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'GEOMETRYCOLLECTION (POINT (10 10), LINESTRING (11 11, 12 12), POLYGON ((13 13, 13 15, 15 15, 15 13, 13 13)))')
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

    public class GeometryCollectionTypeFixture() : GeometryTypeFixture
    {
        public override GeometryCollection Value { get; } = new GeometryCollection(
        [
            new Point(1, 1),
            new LineString([
                new Coordinate(2, 2),
                new Coordinate(3, 3)
            ]),
            new Polygon(new LinearRing([
                new Coordinate(4, 4),    // NW
                new Coordinate(4, 6),    // SW
                new Coordinate(6, 6),    // SE
                new Coordinate(6, 4),    // NE
                new Coordinate(4, 4)
            ]))
        ]);

        public override GeometryCollection OtherValue { get; } = new GeometryCollection(
        [
            new Point(10, 10),
            new LineString([
                new Coordinate(11, 11),
                new Coordinate(12, 12)
            ]),
            new Polygon(new LinearRing([
                new Coordinate(13, 13),  // NW
                new Coordinate(13, 15),  // SW
                new Coordinate(15, 15),  // SE
                new Coordinate(15, 13),  // NE
                new Coordinate(13, 13)
            ]))
        ]);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
