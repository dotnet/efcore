namespace NetTopologySuite.IO.Serialization
{
    internal enum OpenGisType : byte
    {
        Unknown,
        Point,
        LineString,
        Polygon,
        MultiPoint,
        MultiLineString,
        MultiPolygon,
        GeometryCollection,
        CircularString,
        CompoundCurve,
        CurvePolygon,
        FullGlobe // NB: Doesn't align with OgcGeometryType
    };
}
