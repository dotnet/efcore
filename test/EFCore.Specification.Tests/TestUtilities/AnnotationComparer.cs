// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable PossibleNullReferenceException
namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class AnnotationComparer : IEqualityComparer<IAnnotation>, IComparer<IAnnotation>
{
    public static readonly AnnotationComparer Instance = new();

    private AnnotationComparer()
    {
    }

    public int Compare(IAnnotation x, IAnnotation y)
        => StringComparer.Ordinal.Compare(x.Name, y.Name);

    public bool Equals(IAnnotation x, IAnnotation y)
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
