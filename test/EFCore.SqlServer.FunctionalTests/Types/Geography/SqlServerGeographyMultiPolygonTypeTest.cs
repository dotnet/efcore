// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public class SqlServerGeographyMultiPolygonTypeTest(
    SqlServerGeographyMultiPolygonTypeTest.MultiPolygonTypeFixture fixture,
    ITestOutputHelper testOutputHelper)
    : SqlServerGeographyTypeTestBase<MultiPolygon, SqlServerGeographyMultiPolygonTypeTest.MultiPolygonTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0xE610000001040A0000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 215) (DbType = Object)

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
WHERE [t].[Value].STEquals('MULTIPOLYGON (((-122.35 47.62, -122.35 47.615, -122.345 47.615, -122.345 47.62, -122.35 47.62)), ((-122.3525 47.623, -122.3525 47.6215, -122.351 47.6215, -122.351 47.623, -122.3525 47.623)))') = CAST(1 AS bit)
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
@p0='0xE610000001040A0000000000000000504740D7A3703D0A575EC08FC2F5285C4F...' (Size = 215) (DbType = Object)

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
@Fixture_Value='0xE610000001040A0000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 215) (DbType = Object)

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
@p0='{"OtherValue":"MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625))
((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))","Value":"MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625))
((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))"}' (Nullable = false) (Size = 404)
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
@complex_type_Fixture_OtherValue='MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625))
((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625))
((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625)), ((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'MULTIPOLYGON (((-121.36 46.625, -121.36 46.62, -121.355 46.62, -121.355 46.625, -121.36 46.625)), ((-121.354 46.624, -121.354 46.622, -121.3525 46.622, -121.3525 46.624, -121.354 46.624)))')
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

    public class MultiPolygonTypeFixture : SqlServerGeographyTypeFixture
    {
        public override MultiPolygon Value { get; } = new(
        [
            new Polygon(new LinearRing([
                new Coordinate(-122.3500, 47.6200), // NW
                new Coordinate(-122.3500, 47.6150), // SW
                new Coordinate(-122.3450, 47.6150), // SE
                new Coordinate(-122.3450, 47.6200), // NE
                new Coordinate(-122.3500, 47.6200)
            ])) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-122.3525, 47.6230), // NW
                new Coordinate(-122.3525, 47.6215), // SW
                new Coordinate(-122.3510, 47.6215), // SE
                new Coordinate(-122.3510, 47.6230), // NE
                new Coordinate(-122.3525, 47.6230)
            ])) { SRID = 4326 }
        ])
        { SRID = 4326 };

        public override MultiPolygon OtherValue { get; } = new(
        [
            new Polygon(new LinearRing([
                new Coordinate(-121.3600, 46.6250), // NW
                new Coordinate(-121.3600, 46.6200), // SW
                new Coordinate(-121.3550, 46.6200), // SE
                new Coordinate(-121.3550, 46.6250), // NE
                new Coordinate(-121.3600, 46.6250)
            ])) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-121.3540, 46.6240), // NW
                new Coordinate(-121.3540, 46.6220), // SW
                new Coordinate(-121.3525, 46.6220), // SE
                new Coordinate(-121.3525, 46.6240), // NE
                new Coordinate(-121.3540, 46.6240)
            ])) { SRID = 4326 }
        ])
        { SRID = 4326 };
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
