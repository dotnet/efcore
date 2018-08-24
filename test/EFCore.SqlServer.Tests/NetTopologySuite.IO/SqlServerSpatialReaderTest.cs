using System.Globalization;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Xunit;
using GeoParseException = GeoAPI.IO.ParseException;

namespace NetTopologySuite.IO
{
    public class SqlServerSpatialReaderTest
    {
        [Theory]
        [InlineData(
            "POINT EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF01")]
        [InlineData(
            "POINT (1 2)",
            "00000000010C000000000000F03F0000000000000040")]
        [InlineData(
            "POINT (1 2 3)",
            "00000000010D000000000000F03F00000000000000400000000000000840")]
        [InlineData(
            "LINESTRING EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF02")]
        [InlineData(
            "LINESTRING (0 0, 0 1)",
            "000000000114000000000000000000000000000000000000000000000000000000000000F03F")]
        [InlineData(
            "LINESTRING (0 0 1, 0 1 2)",
            "000000000115000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F0000000000000040")]
        [InlineData(
            "LINESTRING (0 0, 0 1 2)",
            "000000000115000000000000000000000000000000000000000000000000000000000000F03F000000000000F8FF0000000000000040")]
        [InlineData(
            "LINESTRING (0 0, 0 1, 0 2)",
            "00000000010403000000000000000000000000000000000000000000000000000000000000000000F03F0000000000000000000000000000004001000000010000000001000000FFFFFFFF0000000002")]
        [InlineData(
            "LINESTRING (0 0 1, 0 1 2, 0 2 3)",
            "00000000010503000000000000000000000000000000000000000000000000000000000000000000F03F00000000000000000000000000000040000000000000F03F0000000000000040000000000000084001000000010000000001000000FFFFFFFF0000000002")]
        [InlineData(
            "POLYGON EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF03")]
        [InlineData(
            "POLYGON ((0 0, 0 1, 1 1, 0 0))",
            "00000000010404000000000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000000000000000000000001000000020000000001000000FFFFFFFF0000000003")]
        [InlineData(
            "POLYGON ((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 2, 2 1, 1 1))",
            "0000000001040A0000000000000000000000000000000000000000000000000000000000000000000840000000000000084000000000000008400000000000000840000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000040000000000000004000000000000000400000000000000040000000000000F03F000000000000F03F000000000000F03F020000000200000000000500000001000000FFFFFFFF0000000003")]
        [InlineData(
            "GEOMETRYCOLLECTION EMPTY",
            "000000000104000000000000000001000000FFFFFFFFFFFFFFFF07")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0))",
            "000000000104010000000000000000000000000000000000000001000000010000000002000000FFFFFFFF0000000007000000000000000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0), POINT (0 1))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F020000000100000000010100000003000000FFFFFFFF0000000007000000000000000001000000000100000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (0 1)))",
            "000000000104010000000000000000000000000000000000F03F01000000010000000003000000FFFFFFFF0000000007000000000000000007010000000000000001")]
        [InlineData(
            "GEOMETRYCOLLECTION (POINT (0 0), GEOMETRYCOLLECTION (POINT (0 1)))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F020000000100000000010100000004000000FFFFFFFF0000000007000000000000000001000000000100000007020000000100000001")]
        [InlineData(
            "MULTIPOINT ((0 0))",
            "000000000104010000000000000000000000000000000000000001000000010000000002000000FFFFFFFF0000000004000000000000000001")]
        [InlineData(
            "MULTILINESTRING ((0 0, 0 1))",
            "00000000010402000000000000000000000000000000000000000000000000000000000000000000F03F01000000010000000002000000FFFFFFFF0000000005000000000000000002")]
        [InlineData(
            "MULTIPOLYGON (((0 0, 0 1, 1 1, 0 0)))",
            "00000000010404000000000000000000000000000000000000000000000000000000000000000000F03F000000000000F03F000000000000F03F0000000000000000000000000000000001000000020000000002000000FFFFFFFF0000000006000000000000000003")]
        public void Read_works(string expected, string bytes)
        {
            Assert.Equal(expected, Read(bytes).AsText());
        }

        [Fact]
        public void Read_works_with_SRID()
        {
            var point = Read("E6100000010C00000000000000000000000000000000");

            Assert.Equal(4326, point.SRID);
            Assert.Equal("POINT (0 0)", point.AsText());
        }

        [Fact]
        public void Read_works_when_Point_with_M()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double,
                    dimension: 4),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var point = (IPoint)Read(
                "00000000010F000000000000F03F000000000000004000000000000008400000000000001040",
                geometryServices);

            Assert.Equal(4, point.M);
            Assert.Equal("POINT (1 2 3)", point.AsText());
        }

        [Fact]
        public void Read_works_when_LineString_with_Ms()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double,
                    dimension: 4),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var lineString = (ILineString)Read(
                "000000000117000000000000000000000000000000000000000000000000000000000000F03F00000000000000000000000000000000000000000000F03F0000000000000040",
                geometryServices);

            var mValues = lineString.GetOrdinates(Ordinate.M);
            Assert.Equal(1, mValues[0]);
            Assert.Equal(2, mValues[1]);
            Assert.Equal("LINESTRING (0 0 0, 0 1 0)", lineString.AsText());
        }

        [Fact]
        public void Read_works_when_null()
        {
            Assert.Null(Read("FFFFFFFF"));
        }

        [Fact]
        public void HandleOrdinates_works()
        {
            var geometryServices = new NtsGeometryServices(
                new PackedCoordinateSequenceFactory(
                    PackedCoordinateSequenceFactory.PackedType.Double,
                    dimension: 4),
                new PrecisionModel(PrecisionModels.Floating),
                srid: -1);
            var point = (IPoint)Read(
                "00000000010F000000000000F03F000000000000004000000000000008400000000000001040",
                geometryServices,
                Ordinates.XY);

            Assert.Equal("POINT (1 2)", point.AsText());
        }


        [Fact]
        public void Read_throws_when_circular_string()
        {
            var ex = Assert.Throws<GeoParseException>(
                () => Read("0000000002040300000000000000000000000000000000000000000000000000F03F000000000000F03F0000000000000040000000000000000001000000020000000001000000FFFFFFFF0000000008"));

            Assert.Equal("Unsupported type: CircularString", ex.Message);
        }

        [Fact]
        public void Read_throws_when_compound_curve()
        {
            var ex = Assert.Throws<GeoParseException>(
                () => Read("0000000002040400000000000000000000000000000000000000000000000000F03F00000000000000000000000000000040000000000000F03F0000000000000840000000000000000001000000030000000001000000FFFFFFFF0000000009020000000203"));

            Assert.Equal("Unsupported type: CompoundCurve", ex.Message);
        }

        [Fact]
        public void Read_throws_when_curve_polygon()
        {
            var ex = Assert.Throws<GeoParseException>(
                () => Read("000000000204050000000000000000000040000000000000F03F000000000000F03F00000000000000400000000000000000000000000000F03F000000000000F03F00000000000000000000000000000040000000000000F03F01000000020000000001000000FFFFFFFF000000000A"));

            Assert.Equal("Unsupported type: CurvePolygon", ex.Message);
        }

        [Fact]
        public void Read_throws_when_full_globe()
        {
            var ex = Assert.Throws<GeoParseException>(
                () => Read("E61000000224000000000000000001000000FFFFFFFFFFFFFFFF0B"));

            Assert.Equal("Unsupported type: FullGlobe", ex.Message);
        }

        private IGeometry Read(
            string bytes,
            IGeometryServices geometryServices = null,
            Ordinates handleOrdinates = Ordinates.XYZM)
        {
            var byteArray = new byte[bytes.Length / 2];
            for (var i = 0; i < bytes.Length; i += 2)
            {
                byteArray[i / 2] = byte.Parse(bytes.Substring(i, 2), NumberStyles.HexNumber);
            }

            var reader = new SqlServerSpatialReader(geometryServices ?? NtsGeometryServices.Instance)
            {
                HandleOrdinates = handleOrdinates
            };

            return reader.Read(byteArray);
        }
    }
}
