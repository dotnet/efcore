using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace NetTopologySuite.IO.Serialization
{
    internal class Geography
    {
        public int SRID { get; set; }
        public byte Version { get; set; } = 1;
        public IList<Point> Points { get; } = new List<Point>();
        public IList<double> ZValues { get; } = new List<double>();
        public IList<double> MValues { get; } = new List<double>();
        public IList<Figure> Figures { get; } = new List<Figure>();
        public IList<Shape> Shapes { get; } = new List<Shape>();
        public IList<Segment> Segments { get; } = new List<Segment>();
        public bool IsValid { get; set; } = true;
        public bool IsLargerThanAHemisphere { get; set; }

        public static Geography ReadFrom(BinaryReader reader)
        {
            try
            {
                var geography = new Geography
                {
                    SRID = reader.ReadInt32()
                };

                if (geography.SRID == -1)
                {
                    return geography;
                }

                geography.Version = reader.ReadByte();
                if (geography.Version != 1 && geography.Version != 2)
                {
                    throw new FormatException(SqlServerNTSStrings.UnexpectedGeographyVersion(geography.Version));
                }

                var properties = (SerializationProperties)reader.ReadByte();
                geography.IsValid = properties.HasFlag(SerializationProperties.IsValid);
                geography.IsLargerThanAHemisphere = properties.HasFlag(SerializationProperties.IsLargerThanAHemisphere);

                var numberOfPoints = properties.HasFlag(SerializationProperties.IsSinglePoint)
                    ? 1
                    : properties.HasFlag(SerializationProperties.IsSingleLineSegment)
                        ? 2
                        : reader.ReadInt32();

                for (var i = 0; i < numberOfPoints; i++)
                {
                    geography.Points.Add(Point.ReadFrom(reader));
                }

                if (properties.HasFlag(SerializationProperties.HasZValues))
                {
                    for (var i = 0; i < numberOfPoints; i++)
                    {
                        geography.ZValues.Add(reader.ReadDouble());
                    }
                }

                if (properties.HasFlag(SerializationProperties.HasMValues))
                {
                    for (var i = 0; i < numberOfPoints; i++)
                    {
                        geography.MValues.Add(reader.ReadDouble());
                    }
                }

                var hasSegments = false;

                if (properties.HasFlag(SerializationProperties.IsSinglePoint)
                    || properties.HasFlag(SerializationProperties.IsSingleLineSegment))
                {
                    geography.Figures.Add(
                        new Figure
                        {
                            FigureAttribute = FigureAttribute.Line,
                            PointOffset = 0
                        });
                }
                else
                {
                    var numberOfFigures = reader.ReadInt32();

                    for (var i = 0; i < numberOfFigures; i++)
                    {
                        var figure = Figure.ReadFrom(reader);

                        if (geography.Version == 1)
                        {
                            // NB: The legacy value is ignored. Exterior rings are always first
                            figure.FigureAttribute = FigureAttribute.Line;
                        }
                        else if (figure.FigureAttribute == FigureAttribute.Curve)
                        {
                            hasSegments = true;
                        }

                        geography.Figures.Add(figure);
                    }
                }

                if (properties.HasFlag(SerializationProperties.IsSinglePoint)
                    || properties.HasFlag(SerializationProperties.IsSingleLineSegment))
                {
                    geography.Shapes.Add(
                        new Shape
                        {
                            ParentOffset = -1,
                            FigureOffset = 0,
                            Type = properties.HasFlag(SerializationProperties.IsSinglePoint)
                                ? OpenGisType.Point
                                : OpenGisType.LineString
                        });
                }
                else
                {
                    var numberOfShapes = reader.ReadInt32();

                    for (var i = 0; i < numberOfShapes; i++)
                    {
                        geography.Shapes.Add(Shape.ReadFrom(reader));
                    }
                }

                if (hasSegments)
                {
                    var numberOfSegments = reader.ReadInt32();

                    for (var i = 0; i < numberOfSegments; i++)
                    {
                        geography.Segments.Add(Segment.ReadFrom(reader));
                    }
                }

                return geography;
            }
            catch (EndOfStreamException ex)
            {
                throw new FormatException(SqlServerNTSStrings.UnexpectedEndOfStream, ex);
            }
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(SRID);

            if (SRID == -1)
            {
                return;
            }

            writer.Write(Version);

            var properties = SerializationProperties.None;
            if (ZValues.Any())
            {
                properties |= SerializationProperties.HasZValues;
            }
            if (MValues.Any())
            {
                properties |= SerializationProperties.HasMValues;
            }
            if (IsValid)
            {
                properties |= SerializationProperties.IsValid;
            }
            if (Shapes.First().Type == OpenGisType.Point && Points.Any())
            {
                properties |= SerializationProperties.IsSinglePoint;
            }
            if (Shapes.First().Type == OpenGisType.LineString && Points.Count == 2)
            {
                properties |= SerializationProperties.IsSingleLineSegment;
            }
            if (IsLargerThanAHemisphere)
            {
                properties |= SerializationProperties.IsLargerThanAHemisphere;
            }
            writer.Write((byte)properties);

            if (!properties.HasFlag(SerializationProperties.IsSinglePoint)
                && !properties.HasFlag(SerializationProperties.IsSingleLineSegment))
            {
                writer.Write(Points.Count);
            }

            foreach (var point in Points)
            {
                point.WriteTo(writer);
            }

            foreach (var z in ZValues)
            {
                writer.Write(z);
            }

            foreach (var m in MValues)
            {
                writer.Write(m);
            }

            if (properties.HasFlag(SerializationProperties.IsSinglePoint)
                || properties.HasFlag(SerializationProperties.IsSingleLineSegment))
            {
                return;
            }

            writer.Write(Figures.Count);

            if (Version == 1)
            {
                for (var shapeIndex = 0; shapeIndex < Shapes.Count; shapeIndex++)
                {
                    var shape = Shapes[shapeIndex];
                    if (shape.FigureOffset == -1 || shape.IsCollection())
                    {
                        continue;
                    }

                    var nextShapeIndex = shapeIndex + 1;
                    while (nextShapeIndex < Shapes.Count && Shapes[nextShapeIndex].FigureOffset == -1)
                    {
                        nextShapeIndex++;
                    }

                    var lastFigureIndex = nextShapeIndex >= Shapes.Count
                        ? Figures.Count - 1
                        : Shapes[nextShapeIndex].FigureOffset - 1;

                    if (Shapes[shapeIndex].Type == OpenGisType.Polygon)
                    {
                        // NB: Although never mentioned in MS-SSCLRT (v20170816), exterior rings must be first
                        writer.Write((byte)LegacyFigureAttribute.ExteriorRing);
                        writer.Write(Figures[shape.FigureOffset].PointOffset);

                        for (var figureIndex = shape.FigureOffset + 1; figureIndex <= lastFigureIndex; figureIndex++)
                        {
                            writer.Write((byte)LegacyFigureAttribute.InteriorRing);
                            writer.Write(Figures[figureIndex].PointOffset);
                        }

                        continue;
                    }

                    for (var figureIndex = shape.FigureOffset; figureIndex <= lastFigureIndex; figureIndex++)
                    {
                        writer.Write((byte)LegacyFigureAttribute.Stroke);
                        writer.Write(Figures[figureIndex].PointOffset);
                    }
                }
            }
            else
            {
                foreach (var figure in Figures)
                {
                    figure.WriteTo(writer);
                }
            }

            writer.Write(Shapes.Count);

            foreach (var shape in Shapes)
            {
                shape.WriteTo(writer);
            }

            if (Segments.Any())
            {
                writer.Write(Segments.Count);

                foreach (var segment in Segments)
                {
                    segment.WriteTo(writer);
                }
            }
        }
    }
}
