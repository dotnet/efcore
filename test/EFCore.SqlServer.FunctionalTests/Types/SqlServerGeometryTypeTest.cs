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

        var result = await context.Set<TypeEntity>().Where(e => e.Value.EqualsTopologically(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    public override async Task ExecuteUpdate_within_json_to_nonjson_column()
    {
        // See #36688 for supporting this for SQL Server types other than string/numeric/bool
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.ExecuteUpdate_within_json_to_nonjson_column());
        Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
    }

    public abstract class GeometryTypeFixture : RelationalTypeTestFixture
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
    public class PointTypeFixture() : GeometryTypeFixture
    {
        public override Point Value { get; } = new(-122.34877, 47.6233355) { SRID = 4326 };
        public override Point OtherValue { get; } = new(-121.7500, 46.2500) { SRID = 4326 };
    }
}

public class LineStringTypeTest(LineStringTypeTest.LineStringTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<LineString, LineStringTypeTest.LineStringTypeFixture>(fixture, testOutputHelper)
{
    public class LineStringTypeFixture() : GeometryTypeFixture
    {
        public override LineString Value { get; } = new(
        [
            new Coordinate(-122.34877, 47.6233355),
            new Coordinate(-122.3308366, 47.5978429)
        ]) { SRID = 4326 };

        public override LineString OtherValue { get; } = new(
        [
            new Coordinate(-120.5000, 46.9000),
            new Coordinate(-119.8000, 46.7000),
            new Coordinate(-118.6000, 46.4000)
        ]) { SRID = 4326 };
    }
}

public class PolygonTypeTest(PolygonTypeTest.PolygonTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<Polygon, PolygonTypeTest.PolygonTypeFixture>(fixture, testOutputHelper)
{
    public class PolygonTypeFixture() : GeometryTypeFixture
    {
        public override Polygon Value { get; } = new(
            new LinearRing(
            [
                new Coordinate(-122.3500, 47.6200), // NW
                new Coordinate(-122.3500, 47.6100), // SW
                new Coordinate(-122.3400, 47.6100), // SE
                new Coordinate(-122.3400, 47.6200), // NE
                new Coordinate(-122.3500, 47.6200)
            ])) { SRID = 4326 };

        public override Polygon OtherValue { get; } = new(
            new LinearRing(
            [
                new Coordinate(-119.3000, 45.8800), // NW
                new Coordinate(-119.3000, 45.8600), // SW
                new Coordinate(-119.1500, 45.8600), // SE
                new Coordinate(-119.1500, 45.8800), // NE
                new Coordinate(-119.3000, 45.8800)
            ])) { SRID = 4326 };
    }
}

public class MultiPointTypeTest(MultiPointTypeTest.MultiPointTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiPoint, MultiPointTypeTest.MultiPointTypeFixture>(fixture, testOutputHelper)
{
    public class MultiPointTypeFixture() : GeometryTypeFixture
    {
        public override MultiPoint Value { get; } = new MultiPoint(
        [
            new Point(-122.3500, 47.6200) { SRID = 4326 },
            new Point(-122.3450, 47.6150) { SRID = 4326 }
        ]) { SRID = 4326 };

        public override MultiPoint OtherValue { get; } = new MultiPoint(
        [
            new Point(-121.9000, 46.9500) { SRID = 4326 },
            new Point(-121.5000, 46.6000) { SRID = 4326 },
            new Point(-121.2000, 46.3000) { SRID = 4326 }
        ]) { SRID = 4326 };
    }
}

public class MultiLineStringTypeTest(MultiLineStringTypeTest.MultiLineStringTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiLineString, MultiLineStringTypeTest.MultiLineStringTypeFixture>(fixture, testOutputHelper)
{
    public class MultiLineStringTypeFixture() : GeometryTypeFixture
    {
        public override MultiLineString Value { get; } = new MultiLineString(
        [
            new LineString([
                new Coordinate(-122.3500, 47.6200),
                new Coordinate(-122.3450, 47.6150)
            ]) { SRID = 4326 },
            new LineString([
                new Coordinate(-122.3480, 47.6180),
                new Coordinate(-122.3420, 47.6130)
            ]) { SRID = 4326 }
        ]) { SRID = 4326 };

        public override MultiLineString OtherValue { get; } = new MultiLineString(
        [
            new LineString([
                new Coordinate(-120.9000, 46.9500),
                new Coordinate(-120.4000, 46.8200)
            ]) { SRID = 4326 },
            new LineString([
                new Coordinate(-120.7000, 46.7800),
                new Coordinate(-120.2000, 46.5500)
            ]) { SRID = 4326 }
        ]) { SRID = 4326 };
    }
}

public class MultiPolygonTypeTest(MultiPolygonTypeTest.MultiPolygonTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<MultiPolygon, MultiPolygonTypeTest.MultiPolygonTypeFixture>(fixture, testOutputHelper)
{
    public class MultiPolygonTypeFixture() : GeometryTypeFixture
    {
        public override MultiPolygon Value { get; } = new MultiPolygon(
        [
            new Polygon(new LinearRing([
                new Coordinate(-122.3500, 47.6200), // NW
                new Coordinate(-122.3500, 47.6150), // SW
                new Coordinate(-122.3450, 47.6150), // SE
                new Coordinate(-122.3450, 47.6200), // NE
                new Coordinate(-122.3500, 47.6200)
            ])) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-122.3400, 47.6240), // NW
                new Coordinate(-122.3400, 47.6220), // SW
                new Coordinate(-122.3380, 47.6220), // SE
                new Coordinate(-122.3380, 47.6240), // NE
                new Coordinate(-122.3400, 47.6240)
            ])) { SRID = 4326 }
        ]) { SRID = 4326 };

        public override MultiPolygon OtherValue { get; } = new MultiPolygon(
        [
            new Polygon(new LinearRing([
                new Coordinate(-119.8000, 45.9000), // NW
                new Coordinate(-119.8000, 45.8800), // SW
                new Coordinate(-119.6500, 45.8800), // SE
                new Coordinate(-119.6500, 45.9000), // NE
                new Coordinate(-119.8000, 45.9000)
            ])) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-119.6000, 45.8950), // NW
                new Coordinate(-119.6000, 45.8850), // SW
                new Coordinate(-119.5800, 45.8850), // SE
                new Coordinate(-119.5800, 45.8950), // NE
                new Coordinate(-119.6000, 45.8950)
            ])) { SRID = 4326 }
        ]) { SRID = 4326 };
    }
}

public class GeometryCollectionTypeTest(GeometryCollectionTypeTest.GeometryCollectionTypeFixture fixture, ITestOutputHelper testOutputHelper)
    : GeometryTypeTestBase<GeometryCollection, GeometryCollectionTypeTest.GeometryCollectionTypeFixture>(fixture, testOutputHelper)
{
    public override async Task ExecuteUpdate_within_json_to_constant()
    {
        await base.ExecuteUpdate_within_json_to_constant();

        AssertSql(
            """
UPDATE [j]
SET [j].[JsonContainer] = JSON_MODIFY([j].[JsonContainer], '$.Value', N'GEOMETRYCOLLECTION (POINT (-120.9 46.95), LINESTRING (-120.9 46.95, -120.4 46.82), POLYGON ((-120.8 46.94, -120.8 46.92, -120.78 46.92, -120.78 46.94, -120.8 46.94)))')
FROM [JsonTypeEntity] AS [j]
""");
    }

    public class GeometryCollectionTypeFixture() : GeometryTypeFixture
    {
        public override GeometryCollection Value { get; } = new GeometryCollection(
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

        public override GeometryCollection OtherValue { get; } = new GeometryCollection(
        [
            new Point(-120.9000, 46.9500) { SRID = 4326 },
            new LineString([
                new Coordinate(-120.9000, 46.9500),
                new Coordinate(-120.4000, 46.8200)
            ]) { SRID = 4326 },
            new Polygon(new LinearRing([
                new Coordinate(-120.8000, 46.9400), // NW
                new Coordinate(-120.8000, 46.9200), // SW
                new Coordinate(-120.7800, 46.9200), // SE
                new Coordinate(-120.7800, 46.9400), // NE
                new Coordinate(-120.8000, 46.9400)
            ])) { SRID = 4326 }
        ])
        { SRID = 4326 };
    }
}
