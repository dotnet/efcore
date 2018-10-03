using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoAPI;
using GeoAPI.Geometries;
using GeoAPI.IO;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.Serialization;
using GeoParseException = GeoAPI.IO.ParseException;

namespace NetTopologySuite.IO
{
    /// <summary>
    ///     Reads geography or geometry data in the SQL Server serialization format (described in MS-SSCLRT) into
    ///     <see cref="IGeometry"/> instances.
    /// </summary>
    public class SqlServerSpatialReader : IBinaryGeometryReader
    {
        private readonly IGeometryServices _services;
        private readonly ICoordinateSequenceFactory _sequenceFactory;
        private Ordinates _handleOrdinates;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerSpatialReader"/> class.
        /// </summary>
        public SqlServerSpatialReader()
            : this(GeometryServiceProvider.Instance)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerSpatialReader"/> class.
        /// </summary>
        /// <param name="services"> The geometry services used to create <see cref="IGeometry"/> instances. </param>
        public SqlServerSpatialReader(IGeometryServices services)
        {
            _services = services ?? GeometryServiceProvider.Instance;
            _sequenceFactory = _services.DefaultCoordinateSequenceFactory;
            _handleOrdinates = AllowedOrdinates;
        }

        /// <summary>
        ///     Gets or sets whether invalid linear rings should be fixed. Returns false since invalid rings are
        ///     disallowed. Setting does nothing.
        /// </summary>
        public virtual bool RepairRings
        {
            get => false;
            set { }
        }

        /// <summary>
        ///     Gets or sets whether the SpatialReference ID must be handled. Returns true since it's always handled.
        ///     Setting does nothing.
        /// </summary>
        public virtual bool HandleSRID
        {
            get => true;
            set { }
        }

        /// <summary>
        ///     Gets and <see cref="Ordinates"/> flag that indicate which ordinates can be handled.
        /// </summary>
        public virtual Ordinates AllowedOrdinates
            => Ordinates.XYZM & _sequenceFactory.Ordinates;

        /// <summary>
        ///     Gets and sets <see cref="Ordinates"/> flag that indicate which ordinates shall be handled.
        /// </summary>
        /// <remarks>
        ///     No matter which <see cref="Ordinates"/> flag you supply, <see cref="Ordinates.XY"/> are always
        ///     processed, the rest is binary and 'ed with <see cref="AllowedOrdinates"/>.
        /// </remarks>
        public virtual Ordinates HandleOrdinates
        {
            get => _handleOrdinates;
            set
            {
                value = Ordinates.XY | (AllowedOrdinates & value);
                _handleOrdinates = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to read geography data. If not, geometry data will be read.
        /// </summary>
        public virtual bool IsGeography { get; set; }

        /// <summary>
        ///     Reads a geometry representation from a <see cref="T:byte[]"/> to a Geometry.
        /// </summary>
        /// <param name="source"> The source to read the geometry from </param>
        /// <returns> A Geometry </returns>
        public virtual IGeometry Read(byte[] source)
        {
            using (var stream = new MemoryStream(source))
            {
                return Read(stream);
            }
        }

        /// <summary>
        ///     Reads a geometry representation from a <see cref="Stream"/> to a Geometry.
        /// </summary>
        /// <param name="stream"> The stream to read from. </param>
        /// <returns> A geometry. </returns>
        public virtual IGeometry Read(Stream stream)
        {
            Geography geography;
            using (var reader = new BinaryReader(stream))
            {
                geography = Geography.ReadFrom(reader);
            }

            return ToGeometry(geography);
        }

        private IGeometry ToGeometry(Geography geography)
        {
            if (geography.SRID == -1)
            {
                return null;
            }

            var handleZ = _handleOrdinates.HasFlag(Ordinates.Z) && geography.ZValues.Count > 0;
            var handleM = _handleOrdinates.HasFlag(Ordinates.M) && geography.MValues.Count > 0;

            var factory = _services.CreateGeometryFactory(geography.SRID);
            var geometries = new Dictionary<int, Stack<IGeometry>>();
            var lastFigureIndex = geography.Figures.Count - 1;
            var lastPointIndex = geography.Points.Count - 1;

            for (var shapeIndex = geography.Shapes.Count - 1; shapeIndex >= 0; shapeIndex--)
            {
                var shape = geography.Shapes[shapeIndex];
                var figures = new Stack<ICoordinateSequence>();

                if (shape.FigureOffset != -1)
                {
                    for (var figureIndex = lastFigureIndex; figureIndex >= shape.FigureOffset; figureIndex--)
                    {
                        var figure = geography.Figures[figureIndex];
                        var pointCount = figure.PointOffset != -1
                            ? lastPointIndex + 1 - figure.PointOffset
                            : 0;
                        var coordinates = _sequenceFactory.Create(pointCount, _handleOrdinates);

                        if (pointCount != 0)
                        {
                            for (var pointIndex = figure.PointOffset; pointIndex <= lastPointIndex; pointIndex++)
                            {
                                var point = geography.Points[pointIndex];
                                var coordinateIndex = pointIndex - figure.PointOffset;

                                coordinates.SetOrdinate(coordinateIndex, Ordinate.X, IsGeography ? point.Y : point.X);
                                coordinates.SetOrdinate(coordinateIndex, Ordinate.Y, IsGeography ? point.X : point.Y);

                                if (handleZ)
                                {
                                    coordinates.SetOrdinate(coordinateIndex, Ordinate.Z, geography.ZValues[pointIndex]);
                                }

                                if (handleM)
                                {
                                    coordinates.SetOrdinate(coordinateIndex, Ordinate.M, geography.MValues[pointIndex]);
                                }
                            }

                            lastPointIndex = figure.PointOffset - 1;
                        }

                        figures.Push(coordinates);
                    }

                    lastFigureIndex = shape.FigureOffset - 1;
                }

                IGeometry geometry;
                switch (shape.Type)
                {
                    case OpenGisType.Point:
                        geometry = factory.CreatePoint(figures.SingleOrDefault());
                        Debug.Assert(!geometries.ContainsKey(shapeIndex));
                        break;

                    case OpenGisType.LineString:
                        geometry = factory.CreateLineString(figures.SingleOrDefault());
                        Debug.Assert(!geometries.ContainsKey(shapeIndex));
                        break;

                    case OpenGisType.Polygon:
                        // NB: For geography (ellipsoidal) data, the actual shell is the figure oriented counter-clockwise
                        geometry = factory.CreatePolygon(
                            factory.CreateLinearRing(figures.FirstOrDefault()),
                            figures.Skip(1).Select(f => factory.CreateLinearRing(f)).ToArray());
                        Debug.Assert(!geometries.ContainsKey(shapeIndex));
                        break;

                    case OpenGisType.MultiPoint:
                        geometry = factory.CreateMultiPoint(
                            geometries.TryGetValue(shapeIndex, out var points)
                                ? points.Cast<IPoint>().ToArray()
                                : null);
                        geometries.Remove(shapeIndex);
                        break;

                    case OpenGisType.MultiLineString:
                        geometry = factory.CreateMultiLineString(
                            geometries.TryGetValue(shapeIndex, out var lineStrings)
                                ? lineStrings.Cast<ILineString>().ToArray()
                                : null);
                        geometries.Remove(shapeIndex);
                        break;

                    case OpenGisType.MultiPolygon:
                        geometry = factory.CreateMultiPolygon(
                                geometries.TryGetValue(shapeIndex, out var polygons)
                                    ? polygons.Cast<IPolygon>().ToArray()
                                    : null);
                        geometries.Remove(shapeIndex);
                        break;

                    case OpenGisType.GeometryCollection:
                        geometry = factory.CreateGeometryCollection(
                                geometries.TryGetValue(shapeIndex, out var children)
                                    ? children.ToArray()
                                    : null);
                        geometries.Remove(shapeIndex);
                        break;

                    default:
                        throw new GeoParseException(SqlServerNTSStrings.UnexpectedGeographyType(shape.Type));
                }

                if (!geometries.ContainsKey(shape.ParentOffset))
                {
                    geometries.Add(shape.ParentOffset, new Stack<IGeometry>());
                }

                geometries[shape.ParentOffset].Push(geometry);
            }

            Debug.Assert(geometries.Keys.Count == 1);

            return geometries[-1].Single();
        }
    }
}
