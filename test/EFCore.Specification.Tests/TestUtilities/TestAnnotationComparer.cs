// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable PossibleNullReferenceException
namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestAnnotationComparer : IEqualityComparer<IAnnotation>, IComparer<IAnnotation>
{
    public static readonly TestAnnotationComparer Instance = new();

    private TestAnnotationComparer()
    {
    }

    public int Compare(IAnnotation? x, IAnnotation? y)
        => (x, y) switch
        {
            (null, null) => 0,
            (not null, null) => 1,
            (null, not null) => -1,
            (not null, not null) => StringComparer.Ordinal.Compare(x.Name, y.Name)
        };

    public bool Equals(IAnnotation? x, IAnnotation? y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y == null
            ? false
            : x.Name == y.Name
            && (x.Name == CoreAnnotationNames.ValueGeneratorFactory
                || Equals(x.Value, y.Value));
    }

    public int GetHashCode(IAnnotation obj)
        => obj.Name.GetHashCode() ^ obj.Value?.GetHashCode() ?? 0;
}
