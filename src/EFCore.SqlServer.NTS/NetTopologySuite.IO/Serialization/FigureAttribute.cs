namespace NetTopologySuite.IO.Serialization
{
    internal enum FigureAttribute : byte
    {
        None = 0, // NB: Called "Point" in MS-SSCLRT (v20170816), but never used
        Line = 1,
        Arc = 2,
        Curve = 3
    }
}
