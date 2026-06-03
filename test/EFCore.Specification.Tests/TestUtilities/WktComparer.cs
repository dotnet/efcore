// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class WktComparer : IEqualityComparer<string>
{
    private static readonly WKTReader _reader = new();

    public static WktComparer Instance { get; } = new();

    private WktComparer()
    {
    }

    public bool Equals(string? x, string? y)
        => x == y
            || Normalize(x) == Normalize(y);

    public static string? Normalize(string? text)
        => text != null
            ? _reader.Read(text).AsText()
            : null;

    public int GetHashCode(string obj)
        => throw new NotImplementedException();
}
