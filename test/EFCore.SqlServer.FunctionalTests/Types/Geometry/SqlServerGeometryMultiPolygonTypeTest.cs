// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geometry;

public class SqlServerGeometryMultiPolygonTypeTest(
    SqlServerGeometryMultiPolygonTypeTest.MultiPolygonTypeFixture fixture,
    ITestOutputHelper testOutputHelper)
    : SqlServerGeometryTypeTestBase<MultiPolygon, SqlServerGeometryMultiPolygonTypeTest.MultiPolygonTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Equality_in_query_with_parameter()
    {
        await base.Equality_in_query_with_parameter();

        AssertSql(
            """
@Fixture_Value='0x0000000001040A00000000000000000000000000000000000000000000000000...' (Size = 215) (DbType = Object)

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
WHERE [t].[Value].STEquals('MULTIPOLYGON (((0 0, 0 5, 5 5, 5 0, 0 0)), ((10 10, 10 15, 15 15, 15 10, 10 10)))') = CAST(1 AS bit)
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
@p0='0x0000000001040A00000000000000000034400000000000003440000000000000...' (Size = 215) (DbType = Object)

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
@Fixture_Value='0x0000000001040A00000000000000000000000000000000000000000000000000...' (Size = 215) (DbType = Object)

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
@p0='{"OtherValue":"MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20))
((30 30, 30 35, 35 35, 35 30, 30 30)))","Value":"MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20))
((30 30, 30 35, 35 35, 35 30, 30 30)))"}' (Nullable = false) (Size = 210)
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
@complex_type_Fixture_OtherValue='MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20))
((30 30, 30 35, 35 35, 35 30, 30 30)))' (Size = 4000)

UPDATE [j]
SET [JsonContainer].modify('$.Value', @complex_type_Fixture_OtherValue)
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
@complex_type_Fixture_OtherValue='MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20))
((30 30, 30 35, 35 35, 35 30, 30 30)))' (Size = 4000)

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
SET [JsonContainer].modify('$.Value', N'MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20)), ((30 30, 30 35, 35 35, 35 30, 30 30)))')
FROM [JsonTypeEntity] AS [j]
""");
        }
        else
        {
            AssertSql(
                """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'MULTIPOLYGON (((20 20, 20 25, 25 25, 25 20, 20 20)), ((30 30, 30 35, 35 35, 35 30, 30 30)))')
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

    public class MultiPolygonTypeFixture : GeometryTypeFixture
    {
        public override MultiPolygon Value { get; } = new MultiPolygon(
        [
            new Polygon(new LinearRing([
                new Coordinate(0, 0),    // NW
                new Coordinate(0, 5),    // SW
                new Coordinate(5, 5),    // SE
                new Coordinate(5, 0),    // NE
                new Coordinate(0, 0)
            ])),
            new Polygon(new LinearRing([
                new Coordinate(10, 10),  // NW
                new Coordinate(10, 15),  // SW
                new Coordinate(15, 15),  // SE
                new Coordinate(15, 10),  // NE
                new Coordinate(10, 10)
            ]))
        ]);

        public override MultiPolygon OtherValue { get; } = new MultiPolygon(
        [
            new Polygon(new LinearRing([
                new Coordinate(20, 20),  // NW
                new Coordinate(20, 25),  // SW
                new Coordinate(25, 25),  // SE
                new Coordinate(25, 20),  // NE
                new Coordinate(20, 20)
            ])),
            new Polygon(new LinearRing([
                new Coordinate(30, 30),  // NW
                new Coordinate(30, 35),  // SW
                new Coordinate(35, 35),  // SE
                new Coordinate(35, 30),  // NE
                new Coordinate(30, 30)
            ]))
        ]);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
