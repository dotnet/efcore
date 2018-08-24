using System.IO;

namespace NetTopologySuite.IO.Serialization
{
    internal class Figure
    {
        public FigureAttribute FigureAttribute { get; set; }
        public int PointOffset { get; set; }

        public static Figure ReadFrom(BinaryReader reader)
            => new Figure
            {
                FigureAttribute = (FigureAttribute)reader.ReadByte(),
                PointOffset = reader.ReadInt32()
            };

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte)FigureAttribute);
            writer.Write(PointOffset);
        }
    }
}
