using System;

namespace NetTopologySuite.IO.Serialization
{
    [Flags]
    internal enum SerializationProperties : byte
    {
        None = 0,
        HasZValues = 1,
        HasMValues = 2,
        IsValid = 4,
        IsSinglePoint = 8,
        IsSingleLineSegment = 0x10,
        IsLargerThanAHemisphere = 0x20
    }
}
