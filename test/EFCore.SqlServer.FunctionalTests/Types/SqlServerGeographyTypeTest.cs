// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Types.Geography;

public abstract class GeographyTypeTestBase<T, TFixture>(TFixture fixture) : RelationalTypeTestBase<T, TFixture>(fixture)
    where T : NetTopologySuite.Geometries.Geometry
    where TFixture : GeographyTypeTestBase<T, TFixture>.GeographyTypeFixture
{
    // SQL Server doesn't support the equality operator on geography, override to use EqualsTopologically
    public override async Task Equality_in_query()
    {
        await using var context = Fixture.CreateContext();

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

    public abstract class GeographyTypeFixture : SqlServerTypeFixture<T>
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseSqlServer(o => o.UseNetTopologySuite());

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
    }
}

public class PointTypeTest(PointTypeTest.PointTypeFixture fixture)
    : GeographyTypeTestBase<Point, PointTypeTest.PointTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE6100000010CC9772975C9CF4740DCF4673F52965EC0' (Size = 22) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class PointTypeFixture() : GeographyTypeFixture
    {
        public override Point Value { get; } = new(-122.34877, 47.6233355) { SRID = 4326 };
        public override Point OtherValue { get; } = new(-121.7500, 46.2500) { SRID = 4326 };
    }
}

public class LineStringTypeTest(LineStringTypeTest.LineStringTypeFixture fixture)
    : GeographyTypeTestBase<LineString, LineStringTypeTest.LineStringTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000114C9772975C9CF4740DCF4673F52965EC0AAD2BB1D86CC47407854...' (Size = 38) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class LineStringTypeFixture() : GeographyTypeFixture
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
}

public class PolygonTypeTest(PolygonTypeTest.PolygonTypeFixture fixture)
    : GeographyTypeTestBase<Polygon, PolygonTypeTest.PolygonTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000104050000008FC2F5285CCF47406666666666965EC0AE47E17A14CE...' (Size = 112) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class PolygonTypeFixture() : GeographyTypeFixture
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
}

public class MultiPointTypeTest(MultiPointTypeTest.MultiPointTypeFixture fixture)
    : GeographyTypeTestBase<MultiPoint, MultiPointTypeTest.MultiPointTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000104020000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 87) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class MultiPointTypeFixture() : GeographyTypeFixture
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
}

public class MultiLineStringTypeTest(MultiLineStringTypeTest.MultiLineStringTypeFixture fixture)
    : GeographyTypeTestBase<MultiLineString, MultiLineStringTypeTest.MultiLineStringTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000104040000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 119) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class MultiLineStringTypeFixture() : GeographyTypeFixture
    {
        public override MultiLineString Value { get; } = new([
            new LineString([
                new Coordinate(-122.3500, 47.6200),
                new Coordinate(-122.3450, 47.6150)
            ]) { SRID = 4326 },
            new LineString([
                new Coordinate(-122.3480, 47.6180),
                new Coordinate(-122.3420, 47.6130)
            ]) { SRID = 4326 }
        ])
        { SRID = 4326 };

        public override MultiLineString OtherValue { get; } = new([
            new LineString([
                new Coordinate(-121.9000, 46.9500),
                new Coordinate(-121.6000, 46.8200)
            ]) { SRID = 4326 },
            new LineString([
                new Coordinate(-121.7000, 46.7800),
                new Coordinate(-121.4000, 46.5500)
            ]) { SRID = 4326 }
        ])
        { SRID = 4326 };
    }
}

public class MultiPolygonTypeTest(MultiPolygonTypeTest.MultiPolygonTypeFixture fixture)
    : GeographyTypeTestBase<MultiPolygon, MultiPolygonTypeTest.MultiPolygonTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE610000001040A0000008FC2F5285CCF47406666666666965EC01F85EB51B8CE...' (Size = 215) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class MultiPolygonTypeFixture() : GeographyTypeFixture
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
}

public class GeometryCollectionTypeTest(GeometryCollectionTypeTest.GeometryCollectionTypeFixture fixture)
    : GeographyTypeTestBase<GeometryCollection, GeometryCollectionTypeTest.GeometryCollectionTypeFixture>(fixture)
{
    public override async Task Query_property_within_json()
    {
        await base.Query_property_within_json();

        // Note that the JSON_VALUE RETURNING clause is never used with geometry even on SQL Server 2025, as that type isn't
        // supported (#36627).
        AssertSql(
            """
@Fixture_Value='0xE61000000104080000008FC2F5285CCF47406666666666965EC08FC2F5285CCF...' (Size = 197) (DbType = Object)

SELECT TOP(2) [j].[Id], [j].[OtherValue], [j].[Value], [j].[JsonContainer]
FROM [JsonTypeEntity] AS [j]
WHERE CAST(JSON_VALUE([j].[JsonContainer], '$.Value') AS geography).STEquals(@Fixture_Value) = CAST(1 AS bit)
""");
    }

    public class GeometryCollectionTypeFixture() : GeographyTypeFixture
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
}
