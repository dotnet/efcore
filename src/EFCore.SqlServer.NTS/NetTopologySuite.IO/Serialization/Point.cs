using System.IO;

namespace NetTopologySuite.IO.Serialization
{
    internal class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public static Point ReadFrom(BinaryReader reader)
            => new Point
            {
                X = reader.ReadDouble(),
                Y = reader.ReadDouble()
            };

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }
    }
}
