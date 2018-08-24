using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.IO.Serialization;
using NetTopologySuite.Utilities;
using SqlShape = NetTopologySuite.IO.Serialization.Shape;

namespace NetTopologySuite.IO
{
    /// <summary>
    ///     Writes <see cref="IGeometry"/> instances into geography or geometry data in the SQL Server serialization
    ///     format (described in MS-SSCLRT).
    /// </summary>
    public class SqlServerSpatialWriter : IBinaryGeometryWriter
    {
        // TODO: Default these to true?
        private bool _emitZ;
        private bool _emitM;

        /// <summary>
        ///     Gets or sets the desired <see cref="ByteOrder"/>. Returns <see cref="ByteOrder.LittleEndian"/> since
        ///     it's required. Setting does nothing.
        /// </summary>
        public virtual ByteOrder ByteOrder
        {
            get => ByteOrder.LittleEndian;
            set { }
        }

        /// <summary>
        ///     Gets or sets whether the SpatialReference ID must be handled. Returns true since it's required. Setting
        ///     does nothing.
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
            => Ordinates.XYZM;

        /// <summary>
        ///     Gets and sets <see cref="Ordinates"/> flag that indicate which ordinates shall be handled.
        /// </summary>
        /// <remarks>
        ///     No matter which <see cref="Ordinates"/> flag you supply, <see cref="Ordinates.XY"/> are always
        ///     processed, the rest is binary and 'ed with <see cref="AllowedOrdinates"/>.
        /// </remarks>
        public virtual Ordinates HandleOrdinates
        {
            get
            {
                var value = Ordinates.XY;
                if (_emitZ)
                {
                    value |= Ordinates.Z;
                }
                if (_emitM)
                {
                    value |= Ordinates.M;
                }

                return value;
            }
            set
            {
                _emitZ = value.HasFlag(Ordinates.Z);
                _emitM = value.HasFlag(Ordinates.M);
            }
        }

        /// <summary>
        ///     Writes a binary representation of a given geometry.
        /// </summary>
        /// <param name="geometry"> The geometry </param>
        /// <returns> The binary representation of geometry </returns>
        public virtual byte[] Write(IGeometry geometry)
        {
            using (var stream = new MemoryStream())
            {
                Write(geometry, stream);

                return stream.ToArray();
            }
        }

        /// <summary>
        ///     Writes a binary representation of a given geometry.
        /// </summary>
        /// <param name="geometry"> The geometry </param>
        /// <param name="stream"> The stream to write to. </param>
        public virtual void Write(IGeometry geometry, Stream stream)
        {
            var geography = ToGeography(geometry);

            using (var writer = new BinaryWriter(stream))
            {
                geography.WriteTo(writer);
            }
        }

        private Geography ToGeography(IGeometry geometry)
        {
            if (geometry == null)
            {
                return new Geography { SRID = -1 };
            }

            var geometries = new Queue<(IGeometry, int)>();
            geometries.Enqueue((geometry, -1));

            // TODO: For geography (ellipsoidal) data, set IsLargerThanAHemisphere.
            var geography = new Geography
            {
                SRID = Math.Max(0, geometry.SRID),
                IsValid = geometry.IsValid
            };

            while (geometries.Any())
            {
                var (currentGeometry, parentOffset) = geometries.Dequeue();

                var figureOffset = geography.Figures.Count;
                var figureAdded = false;

                switch (currentGeometry)
                {
                    case IPoint point:
                    case ILineString lineString:
                        figureAdded = addFigure(currentGeometry, FigureAttribute.Line);
                        break;

                    case IPolygon polygon:
                        // TODO: For geography (ellipsoidal) data, the shell must be oriented counter-clockwise
                        figureAdded = addFigure(polygon.Shell, FigureAttribute.Line);
                        foreach (var hole in polygon.Holes)
                        {
                            // TODO: For geography (ellipsoidal) data, the holes must be oriented clockwise
                            figureAdded |= addFigure(hole, FigureAttribute.Line);
                        }
                        break;

                    case IGeometryCollection geometryCollection:
                        foreach (var item in geometryCollection.Geometries)
                        {
                            geometries.Enqueue((item, geography.Shapes.Count));
                            figureAdded = true;
                        }
                        break;

                    default:
                        Assert.ShouldNeverReachHere("Unsupported Geometry implementation: " + geometry.GetType());
                        break;
                }

                geography.Shapes.Add(
                    new SqlShape
                    {
                        ParentOffset = parentOffset,
                        FigureOffset = figureAdded ? figureOffset : -1,
                        Type = ToOpenGisType(currentGeometry.OgcGeometryType)
                    });

                bool addFigure(IGeometry g, FigureAttribute figureAttribute)
                {
                    var pointOffset = geography.Points.Count;
                    var pointsAdded = false;

                    foreach (var coordinate in g.Coordinates)
                    {
                        // TODO: For geography (ellipsoidal) data, the point's X value must be Latitude, and the Y
                        //       value Longitude.
                        geography.Points.Add(
                            new Point
                            {
                                X = coordinate.X,
                                Y = coordinate.Y
                            });
                        pointsAdded = true;

                        if (_emitZ)
                        {
                            geography.ZValues.Add(coordinate.Z);
                        }
                    }

                    if (!pointsAdded)
                    {
                        return false;
                    }

                    if (_emitM)
                    {
                        foreach (var m in g.GetOrdinates(Ordinate.M))
                        {
                            geography.MValues.Add(m);
                        }
                    }

                    geography.Figures.Add(
                        new Figure
                        {
                            FigureAttribute = figureAttribute,
                            PointOffset = pointOffset
                        });

                    return true;
                }
            }

            if (geography.ZValues.All(double.IsNaN))
            {
                geography.ZValues.Clear();
            }

            if (geography.MValues.All(double.IsNaN))
            {
                geography.MValues.Clear();
            }

            return geography;
        }

        private OpenGisType ToOpenGisType(OgcGeometryType type)
        {
            if (type < OgcGeometryType.Point
                || type > OgcGeometryType.CurvePolygon)
            {
                Assert.ShouldNeverReachHere("Unsupported type: " + type);
            }

            return (OpenGisType)type;
        }
    }
}
