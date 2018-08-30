using System.IO;

namespace NetTopologySuite.IO.Serialization
{
    internal class Segment
    {
        public SegmentType Type { get; set; }

        public static Segment ReadFrom(BinaryReader reader)
            => new Segment
            {
                Type = (SegmentType)reader.ReadByte()
            };

        public void WriteTo(BinaryWriter writer)
            => writer.Write((byte)Type);
    }
}
