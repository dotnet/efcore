// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

[SpatialiteRequired]
public class SpatialQuerySqliteTest : SpatialQueryRelationalTestBase<SpatialQuerySqliteFixture>
{
    public SpatialQuerySqliteTest(SpatialQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task SimpleSelect(bool async)
    {
        await base.SimpleSelect(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Geometry", "p"."Group", "p"."Point", "p"."PointM", "p"."PointZ", "p"."PointZM"
FROM "PointEntity" AS "p"
""",
            //
            """
SELECT "l"."Id", "l"."LineString"
FROM "LineStringEntity" AS "l"
""",
            //
            """
SELECT "p"."Id", "p"."Polygon"
FROM "PolygonEntity" AS "p"
""",
            //
            """
SELECT "m"."Id", "m"."MultiLineString"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task Distance_on_converted_geometry_type(bool async)
    {
        await base.Distance_on_converted_geometry_type(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Nullable = false) (Size = 60) (DbType = String)

SELECT "g"."Id", Distance("g"."Location", @__point_0) AS "Distance"
FROM "GeoPointEntity" AS "g"
""");
    }

    public override async Task Distance_on_converted_geometry_type_lhs(bool async)
    {
        await base.Distance_on_converted_geometry_type_lhs(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Nullable = false) (Size = 60) (DbType = String)

SELECT "g"."Id", Distance(@__point_0, "g"."Location") AS "Distance"
FROM "GeoPointEntity" AS "g"
""");
    }

    public override async Task Distance_on_converted_geometry_type_constant(bool async)
    {
        await base.Distance_on_converted_geometry_type_constant(async);

        AssertSql(
            """
SELECT "g"."Id", Distance("g"."Location", GeomFromText('POINT (0 1)')) AS "Distance"
FROM "GeoPointEntity" AS "g"
""");
    }

    public override async Task Distance_on_converted_geometry_type_constant_lhs(bool async)
    {
        await base.Distance_on_converted_geometry_type_constant_lhs(async);

        AssertSql(
            """
SELECT "g"."Id", Distance(GeomFromText('POINT (0 1)'), "g"."Location") AS "Distance"
FROM "GeoPointEntity" AS "g"
""");
    }

    public override async Task WithConversion(bool async)
    {
        await base.WithConversion(async);

        AssertSql(
            """
SELECT "g"."Id", "g"."Location"
FROM "GeoPointEntity" AS "g"
""");
    }

    public override async Task Area(bool async)
    {
        await base.Area(async);

        AssertSql(
            """
SELECT "p"."Id", Area("p"."Polygon") AS "Area"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task AsBinary(bool async)
    {
        await base.AsBinary(async);

        AssertSql(
            """
SELECT "p"."Id", AsBinary("p"."Point") AS "Binary"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task AsBinary_with_null_check(bool async)
    {
        await base.AsBinary_with_null_check(async);

        AssertSql(
            """
SELECT "p"."Id", CASE
    WHEN "p"."Point" IS NULL THEN NULL
    ELSE AsBinary("p"."Point")
END AS "Binary"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task AsText(bool async)
    {
        await base.AsText(async);

        AssertSql(
            """
SELECT "p"."Id", AsText("p"."Point") AS "Text"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Boundary(bool async)
    {
        await base.Boundary(async);

        AssertSql(
            """
SELECT "p"."Id", Boundary("p"."Polygon") AS "Boundary"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Buffer(bool async)
    {
        await base.Buffer(async);

        AssertSql(
            """
SELECT "p"."Id", Buffer("p"."Polygon", 1.0) AS "Buffer"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Buffer_quadrantSegments(bool async)
    {
        await base.Buffer_quadrantSegments(async);

        AssertSql(
            """
SELECT "p"."Id", Buffer("p"."Polygon", 1.0, 8) AS "Buffer"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Centroid(bool async)
    {
        await base.Centroid(async);

        AssertSql(
            """
SELECT "p"."Id", Centroid("p"."Polygon") AS "Centroid"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Combine_aggregate(bool async)
    {
        await base.Combine_aggregate(async);

        AssertSql(
            """
SELECT "p"."Group" AS "Id", Collect("p"."Point") AS "Combined"
FROM "PointEntity" AS "p"
WHERE "p"."Point" IS NOT NULL
GROUP BY "p"."Group"
""");
    }

    public override async Task EnvelopeCombine_aggregate(bool async)
    {
        await base.EnvelopeCombine_aggregate(async);

        AssertSql(
            """
SELECT "p"."Group" AS "Id", Extent("p"."Point") AS "Combined"
FROM "PointEntity" AS "p"
WHERE "p"."Point" IS NOT NULL
GROUP BY "p"."Group"
""");
    }

    public override async Task Contains(bool async)
    {
        await base.Contains(async);

        AssertSql(
            """
@__point_0='0x000100000000000000000000D03F000000000000D03F000000000000D03F0000...' (Size = 60) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Contains("p"."Polygon", @__point_0)
END AS "Contains"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task ConvexHull(bool async)
    {
        await base.ConvexHull(async);

        AssertSql(
            """
SELECT "p"."Id", ConvexHull("p"."Polygon") AS "ConvexHull"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task ConvexHull_aggregate(bool async)
    {
        await base.ConvexHull_aggregate(async);

        AssertSql(
            """
SELECT "p"."Group" AS "Id", ConvexHull(Collect("p"."Point")) AS "ConvexHull"
FROM "PointEntity" AS "p"
WHERE "p"."Point" IS NOT NULL
GROUP BY "p"."Group"
""");
    }

    public override async Task IGeometryCollection_Count(bool async)
    {
        await base.IGeometryCollection_Count(async);

        AssertSql(
            """
SELECT "m"."Id", NumGeometries("m"."MultiLineString") AS "Count"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task LineString_Count(bool async)
    {
        await base.LineString_Count(async);

        AssertSql(
            """
SELECT "l"."Id", NumPoints("l"."LineString") AS "Count"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task CoveredBy(bool async)
    {
        await base.CoveredBy(async);

        AssertSql(
            """
@__polygon_0='0x000100000000000000000000F0BF000000000000F0BF00000000000000400000...' (Size = 132) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Point" IS NOT NULL THEN CoveredBy("p"."Point", @__polygon_0)
END AS "CoveredBy"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Covers(bool async)
    {
        await base.Covers(async);

        AssertSql(
            """
@__point_0='0x000100000000000000000000D03F000000000000D03F000000000000D03F0000...' (Size = 60) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Covers("p"."Polygon", @__point_0)
END AS "Covers"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Crosses(bool async)
    {
        await base.Crosses(async);

        AssertSql(
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id", CASE
    WHEN "l"."LineString" IS NOT NULL THEN Crosses("l"."LineString", @__lineString_0)
END AS "Crosses"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task Difference(bool async)
    {
        await base.Difference(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", Difference("p"."Polygon", @__polygon_0) AS "Difference"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Dimension(bool async)
    {
        await base.Dimension(async);

        AssertSql(
            """
SELECT "p"."Id", Dimension("p"."Point") AS "Dimension"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Disjoint_with_cast_to_nullable(bool async)
    {
        await base.Disjoint_with_cast_to_nullable(async);

        AssertSql(
            """
@__point_0='0x000100000000000000000000F03F000000000000F03F000000000000F03F0000...' (Size = 60) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Disjoint("p"."Polygon", @__point_0)
END AS "Disjoint"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Disjoint_with_null_check(bool async)
    {
        await base.Disjoint_with_null_check(async);

        AssertSql(
            """
@__point_0='0x000100000000000000000000F03F000000000000F03F000000000000F03F0000...' (Size = 60) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NULL THEN NULL
    WHEN "p"."Polygon" IS NOT NULL THEN Disjoint("p"."Polygon", @__point_0)
END AS "Disjoint"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Distance_with_null_check(bool async)
    {
        await base.Distance_with_null_check(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

SELECT "p"."Id", Distance("p"."Point", @__point_0) AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Distance_with_cast_to_nullable(bool async)
    {
        await base.Distance_with_cast_to_nullable(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

SELECT "p"."Id", Distance("p"."Point", @__point_0) AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Distance_geometry(bool async)
    {
        await base.Distance_geometry(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

SELECT "p"."Id", Distance("p"."Geometry", @__point_0) AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Distance_constant(bool async)
    {
        await base.Distance_constant(async);

        AssertSql(
            """
SELECT "p"."Id", Distance("p"."Point", GeomFromText('POINT (0 1)')) AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Distance_constant_srid_4326(bool async)
    {
        await base.Distance_constant_srid_4326(async);

        AssertSql(
            """
SELECT "p"."Id", Distance("p"."Point", GeomFromText('POINT (1 1)', 4326)) AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Distance_constant_lhs(bool async)
    {
        await base.Distance_constant_lhs(async);

        AssertSql(
            """
SELECT "p"."Id", Distance(GeomFromText('POINT (0 1)'), "p"."Point") AS "Distance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task EndPoint(bool async)
    {
        await base.EndPoint(async);

        AssertSql(
            """
SELECT "l"."Id", EndPoint("l"."LineString") AS "EndPoint"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task Envelope(bool async)
    {
        await base.Envelope(async);

        AssertSql(
            """
SELECT "p"."Id", Envelope("p"."Polygon") AS "Envelope"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task EqualsTopologically(bool async)
    {
        await base.EqualsTopologically(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000000000000000000000000000...' (Size = 60) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Point" IS NOT NULL THEN Equals("p"."Point", @__point_0)
END AS "EqualsTopologically"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task ExteriorRing(bool async)
    {
        await base.ExteriorRing(async);

        AssertSql(
            """
SELECT "p"."Id", ExteriorRing("p"."Polygon") AS "ExteriorRing"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task GeometryType(bool async)
    {
        await base.GeometryType(async);

        AssertSql(
            """
SELECT "p"."Id", CASE rtrim(GeometryType("p"."Point"), ' ZM')
    WHEN 'POINT' THEN 'Point'
    WHEN 'LINESTRING' THEN 'LineString'
    WHEN 'POLYGON' THEN 'Polygon'
    WHEN 'MULTIPOINT' THEN 'MultiPoint'
    WHEN 'MULTILINESTRING' THEN 'MultiLineString'
    WHEN 'MULTIPOLYGON' THEN 'MultiPolygon'
    WHEN 'GEOMETRYCOLLECTION' THEN 'GeometryCollection'
END AS "GeometryType"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task GetGeometryN(bool async)
    {
        await base.GetGeometryN(async);

        AssertSql(
            """
SELECT "m"."Id", GeometryN("m"."MultiLineString", 0 + 1) AS "Geometry0"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task GetGeometryN_with_null_argument(bool async)
    {
        await base.GetGeometryN_with_null_argument(async);

        AssertSql(
            """
SELECT "m"."Id", GeometryN("m"."MultiLineString", (
    SELECT MAX("m0"."Id")
    FROM "MultiLineStringEntity" AS "m0"
    WHERE 0) + 1) AS "Geometry0"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task GetInteriorRingN(bool async)
    {
        await base.GetInteriorRingN(async);

        AssertSql(
            """
SELECT "p"."Id", CASE
    WHEN NumInteriorRing("p"."Polygon") = 0 THEN NULL
    ELSE InteriorRingN("p"."Polygon", 0 + 1)
END AS "InteriorRing0"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task GetPointN(bool async)
    {
        await base.GetPointN(async);

        AssertSql(
            """
SELECT "l"."Id", PointN("l"."LineString", 0 + 1) AS "Point0"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task InteriorPoint(bool async)
    {
        await base.InteriorPoint(async);

        AssertSql(
            """
SELECT "p"."Id", PointOnSurface("p"."Polygon") AS "InteriorPoint", "p"."Polygon"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Intersection(bool async)
    {
        await base.Intersection(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", Intersection("p"."Polygon", @__polygon_0) AS "Intersection"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Intersects(bool async)
    {
        await base.Intersects(async);

        AssertSql(
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id", CASE
    WHEN "l"."LineString" IS NOT NULL THEN Intersects("l"."LineString", @__lineString_0)
END AS "Intersects"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task ICurve_IsClosed(bool async)
    {
        await base.ICurve_IsClosed(async);

        AssertSql(
            """
SELECT "l"."Id", CASE
    WHEN "l"."LineString" IS NOT NULL THEN IsClosed("l"."LineString")
END AS "IsClosed"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task IMultiCurve_IsClosed(bool async)
    {
        await base.IMultiCurve_IsClosed(async);

        AssertSql(
            """
SELECT "m"."Id", CASE
    WHEN "m"."MultiLineString" IS NOT NULL THEN IsClosed("m"."MultiLineString")
END AS "IsClosed"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task IsEmpty(bool async)
    {
        await base.IsEmpty(async);

        AssertSql(
            """
SELECT "m"."Id", CASE
    WHEN "m"."MultiLineString" IS NOT NULL THEN IsEmpty("m"."MultiLineString")
END AS "IsEmpty"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task IsRing(bool async)
    {
        await base.IsRing(async);

        AssertSql(
            """
SELECT "l"."Id", CASE
    WHEN "l"."LineString" IS NOT NULL THEN IsRing("l"."LineString")
END AS "IsRing"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task IsSimple(bool async)
    {
        await base.IsSimple(async);

        AssertSql(
            """
SELECT "l"."Id", CASE
    WHEN "l"."LineString" IS NOT NULL THEN IsSimple("l"."LineString")
END AS "IsSimple"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task IsValid(bool async)
    {
        await base.IsValid(async);

        AssertSql(
            """
SELECT "p"."Id", CASE
    WHEN "p"."Point" IS NOT NULL THEN IsValid("p"."Point")
END AS "IsValid"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task IsWithinDistance(bool async)
    {
        await base.IsWithinDistance(async);

        AssertSql(
            """
@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

SELECT "p"."Id", Distance("p"."Point", @__point_0) <= 1.0 AS "IsWithinDistance"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Item(bool async)
    {
        await base.Item(async);

        AssertSql(
            """
SELECT "m"."Id", GeometryN("m"."MultiLineString", 0 + 1) AS "Item0"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
            """
SELECT "l"."Id", GLength("l"."LineString") AS "Length"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task M(bool async)
    {
        await base.M(async);

        AssertSql(
            """
SELECT "p"."Id", M("p"."Point") AS "M"
FROM "PointEntity" AS "p"
""");
    }

    // No Sqlite Translation
    public override Task Normalized(bool async)
        => Task.CompletedTask;

    public override async Task NumGeometries(bool async)
    {
        await base.NumGeometries(async);

        AssertSql(
            """
SELECT "m"."Id", NumGeometries("m"."MultiLineString") AS "NumGeometries"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task NumInteriorRings(bool async)
    {
        await base.NumInteriorRings(async);

        AssertSql(
            """
SELECT "p"."Id", NumInteriorRing("p"."Polygon") AS "NumInteriorRings"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task NumPoints(bool async)
    {
        await base.NumPoints(async);

        AssertSql(
            """
SELECT "l"."Id", NumPoints("l"."LineString") AS "NumPoints"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task OgcGeometryType(bool async)
    {
        await base.OgcGeometryType(async);

        AssertSql(
            """
SELECT "p"."Id", CASE rtrim(GeometryType("p"."Point"), ' ZM')
    WHEN 'POINT' THEN 1
    WHEN 'LINESTRING' THEN 2
    WHEN 'POLYGON' THEN 3
    WHEN 'MULTIPOINT' THEN 4
    WHEN 'MULTILINESTRING' THEN 5
    WHEN 'MULTIPOLYGON' THEN 6
    WHEN 'GEOMETRYCOLLECTION' THEN 7
END AS "OgcGeometryType"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Overlaps(bool async)
    {
        await base.Overlaps(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Overlaps("p"."Polygon", @__polygon_0)
END AS "Overlaps"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task PointOnSurface(bool async)
    {
        await base.PointOnSurface(async);

        AssertSql(
            """
SELECT "p"."Id", PointOnSurface("p"."Polygon") AS "PointOnSurface", "p"."Polygon"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Relate(bool async)
    {
        await base.Relate(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Relate("p"."Polygon", @__polygon_0, '212111212')
END AS "Relate"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Reverse(bool async)
    {
        await base.Reverse(async);

        AssertSql(
            """
SELECT "l"."Id", ST_Reverse("l"."LineString") AS "Reverse"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task SRID(bool async)
    {
        await base.SRID(async);

        AssertSql(
            """
SELECT "p"."Id", SRID("p"."Point") AS "SRID"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task SRID_geometry(bool async)
    {
        await base.SRID_geometry(async);

        AssertSql(
            """
SELECT "p"."Id", SRID("p"."Geometry") AS "SRID"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task StartPoint(bool async)
    {
        await base.StartPoint(async);

        AssertSql(
            """
SELECT "l"."Id", StartPoint("l"."LineString") AS "StartPoint"
FROM "LineStringEntity" AS "l"
""");
    }

    public override async Task SymmetricDifference(bool async)
    {
        await base.SymmetricDifference(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", SymDifference("p"."Polygon", @__polygon_0) AS "SymmetricDifference"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task ToBinary(bool async)
    {
        await base.ToBinary(async);

        AssertSql(
            """
SELECT "p"."Id", AsBinary("p"."Point") AS "Binary"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task ToText(bool async)
    {
        await base.ToText(async);

        AssertSql(
            """
SELECT "p"."Id", AsText("p"."Point") AS "Text"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Touches(bool async)
    {
        await base.Touches(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Polygon" IS NOT NULL THEN Touches("p"."Polygon", @__polygon_0)
END AS "Touches"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Union(bool async)
    {
        await base.Union(async);

        AssertSql(
            """
@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

SELECT "p"."Id", GUnion("p"."Polygon", @__polygon_0) AS "Union"
FROM "PolygonEntity" AS "p"
""");
    }

    public override async Task Union_aggregate(bool async)
    {
        await base.Union_aggregate(async);

        AssertSql(
            """
SELECT "p"."Group" AS "Id", GUnion("p"."Point") AS "Union"
FROM "PointEntity" AS "p"
WHERE "p"."Point" IS NOT NULL
GROUP BY "p"."Group"
""");
    }

    public override async Task Union_void(bool async)
    {
        await base.Union_void(async);

        AssertSql(
            """
SELECT "m"."Id", UnaryUnion("m"."MultiLineString") AS "Union"
FROM "MultiLineStringEntity" AS "m"
""");
    }

    public override async Task Within(bool async)
    {
        await base.Within(async);

        AssertSql(
            """
@__polygon_0='0x000100000000000000000000F0BF000000000000F0BF00000000000000400000...' (Size = 132) (DbType = String)

SELECT "p"."Id", CASE
    WHEN "p"."Point" IS NOT NULL THEN Within("p"."Point", @__polygon_0)
END AS "Within"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task X(bool async)
    {
        await base.X(async);

        AssertSql(
            """
SELECT "p"."Id", X("p"."Point") AS "X"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Y(bool async)
    {
        await base.Y(async);

        AssertSql(
            """
SELECT "p"."Id", Y("p"."Point") AS "Y"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task Z(bool async)
    {
        await base.Z(async);

        AssertSql(
            """
SELECT "p"."Id", Z("p"."Point") AS "Z"
FROM "PointEntity" AS "p"
""");
    }

    public override async Task IsEmpty_equal_to_null(bool async)
    {
        await base.IsEmpty_equal_to_null(async);

        AssertSql(
            """
SELECT "p"."Id"
FROM "PointEntity" AS "p"
WHERE CASE
    WHEN "p"."Point" IS NOT NULL THEN IsEmpty("p"."Point")
END IS NULL
""");
    }

    public override async Task IsEmpty_not_equal_to_null(bool async)
    {
        await base.IsEmpty_not_equal_to_null(async);

        AssertSql(
            """
SELECT "p"."Id"
FROM "PointEntity" AS "p"
WHERE CASE
    WHEN "p"."Point" IS NOT NULL THEN IsEmpty("p"."Point")
END IS NOT NULL
""");
    }

    public override async Task Intersects_equal_to_null(bool async)
    {
        await base.Intersects_equal_to_null(async);

        AssertSql(
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id"
FROM "LineStringEntity" AS "l"
WHERE CASE
    WHEN "l"."LineString" IS NOT NULL THEN Intersects("l"."LineString", @__lineString_0)
END IS NULL
""",
            //
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id"
FROM "LineStringEntity" AS "l"
WHERE CASE
    WHEN "l"."LineString" IS NOT NULL THEN Intersects(@__lineString_0, "l"."LineString")
END IS NULL
""");
    }

    public override async Task Intersects_not_equal_to_null(bool async)
    {
        await base.Intersects_not_equal_to_null(async);

        AssertSql(
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id"
FROM "LineStringEntity" AS "l"
WHERE CASE
    WHEN "l"."LineString" IS NOT NULL THEN Intersects("l"."LineString", @__lineString_0)
END IS NOT NULL
""",
            //
            """
@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

SELECT "l"."Id"
FROM "LineStringEntity" AS "l"
WHERE CASE
    WHEN "l"."LineString" IS NOT NULL THEN Intersects(@__lineString_0, "l"."LineString")
END IS NOT NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
