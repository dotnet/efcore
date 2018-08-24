using System.IO;

namespace NetTopologySuite.IO.Serialization
{
    internal class Shape
    {
        public int ParentOffset { get; set; }
        public int FigureOffset { get; set; }
        public OpenGisType Type { get; set; }

        public bool IsCollection()
            => Type == OpenGisType.MultiPoint
                || Type == OpenGisType.MultiLineString
                || Type == OpenGisType.MultiPolygon
                || Type == OpenGisType.GeometryCollection;

        public static Shape ReadFrom(BinaryReader reader)
            => new Shape
            {
                ParentOffset = reader.ReadInt32(),
                FigureOffset = reader.ReadInt32(),
                Type = (OpenGisType)reader.ReadByte()
            };

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(ParentOffset);
            writer.Write(FigureOffset);
            writer.Write((byte)Type);
        }
    }
}
