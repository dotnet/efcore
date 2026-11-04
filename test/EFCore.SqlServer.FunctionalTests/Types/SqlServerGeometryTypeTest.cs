// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geometry;

public abstract class GeometryTypeTestBase<T, TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<T, TFixture>(fixture, testOutputHelper)
    where T : NetTopologySuite.Geometries.Geometry
    where TFixture : GeometryTypeTestBase<T, TFixture>.GeometryTypeFixture
{
    // SQL Server doesn't support the equality operator on geometry, override to use EqualsTopologically
    public override async Task Equality_in_query()
    {
        await using var context = Fixture.CreateContext();

        Fixture.TestSqlLoggerFactory.Clear();

        var result = await context.Set<TypeEntity<T>>().Where(e => e.Value.EqualsTopologically(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    // SQL Server doesn't support the equality operator on geometry, override to use EqualsTopologically
    public override async Task Query_property_within_json()
    {
        await using var context = Fixture.CreateContext();

        Fixture.TestSqlLoggerFactory.Clear();

        var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.JsonContainer.Value.EqualsTopologically(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.JsonContainer.Value, Fixture.Comparer);
    }

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
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

    public abstract class GeometryTypeFixture : SqlServerTypeFixture<T>
    {
        public override string? StoreType => "geometry";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseSqlServer(o => o.UseNetTopologySuite());

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class PointTypeTest(PointTypeTest.PointTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<Point, PointTypeTest.PointTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x00000000010C00000000000024400000000000003440' (Size = 22) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class PointTypeFixture() : GeometryTypeFixture
    {
        public override Point Value { get; } = new(10, 20);
        public override Point OtherValue { get; } = new(30, 40);
    }
}

public class LineStringTypeTest(LineStringTypeTest.LineStringTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<LineString, LineStringTypeTest.LineStringTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x000000000114000000000000244000000000000034400000000000002E400000...' (Size = 38) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class LineStringTypeFixture() : GeometryTypeFixture
    {
        public override LineString Value { get; } = new(
        [
            new Coordinate(10, 20),
            new Coordinate(15, 25)
        ]);

        public override LineString OtherValue { get; } = new(
        [
            new Coordinate(30, 40),
            new Coordinate(35, 45),
            new Coordinate(40, 50)
        ]);
    }
}

public class PolygonTypeTest(PolygonTypeTest.PolygonTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<Polygon, PolygonTypeTest.PolygonTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x0000000001040500000000000000000000000000000000000000000000000000...' (Size = 112) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class PolygonTypeFixture() : GeometryTypeFixture
    {
        public override Polygon Value { get; } = new(
            new LinearRing(
            [
                new Coordinate(0, 0),    // NW
                new Coordinate(0, 10),   // SW
                new Coordinate(10, 10),  // SE
                new Coordinate(10, 0),   // NE
                new Coordinate(0, 0)
            ]));

        public override Polygon OtherValue { get; } = new(
            new LinearRing(
            [
                new Coordinate(20, 20),  // NW
                new Coordinate(20, 30),  // SW
                new Coordinate(30, 30),  // SE
                new Coordinate(30, 20),  // NE
                new Coordinate(20, 20)
            ]));
    }
}

public class MultiPointTypeTest(MultiPointTypeTest.MultiPointTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiPoint, MultiPointTypeTest.MultiPointTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x0000000001040200000000000000000014400000000000001440000000000000...' (Size = 87) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class MultiPointTypeFixture() : GeometryTypeFixture
    {
        public override MultiPoint Value { get; } = new MultiPoint(
        [
            new Point(5, 5),
            new Point(10, 10)
        ]);

        public override MultiPoint OtherValue { get; } = new MultiPoint(
        [
            new Point(15, 15),
            new Point(20, 20),
            new Point(25, 25)
        ]);
    }
}

public class MultiLineStringTypeTest(MultiLineStringTypeTest.MultiLineStringTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiLineString, MultiLineStringTypeTest.MultiLineStringTypeFixture>(fixture, testOutputHelper)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0x00000000010404000000000000000000F03F000000000000F03F000000000000...' (Size = 119) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geometry).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class MultiLineStringTypeFixture() : GeometryTypeFixture
    {
        public override MultiLineString Value { get; } = new MultiLineString(
        [
            new LineString([
                new Coordinate(1, 1),
                new Coordinate(2, 2)
            ]),
            new LineString([
                new Coordinate(3, 3),
                new Coordinate(4, 4)
            ])
        ]);

        public override MultiLineString OtherValue { get; } = new MultiLineString(
        [
            new LineString([
                new Coordinate(10, 10),
                new Coordinate(11, 11)
            ]),
            new LineString([
                new Coordinate(12, 12),
                new Coordinate(13, 13)
            ])
        ]);
    }
}

public class MultiPolygonTypeTest(MultiPolygonTypeTest.MultiPolygonTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiPolygon, MultiPolygonTypeTest.MultiPolygonTypeFixture>(fixture, testOutputHelper)
{
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

    public class MultiPolygonTypeFixture() : GeometryTypeFixture
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
}

public class GeometryCollectionTypeTest(GeometryCollectionTypeTest.GeometryCollectionTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<GeometryCollection, GeometryCollectionTypeTest.GeometryCollectionTypeFixture>(fixture, testOutputHelper)
{
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
}
