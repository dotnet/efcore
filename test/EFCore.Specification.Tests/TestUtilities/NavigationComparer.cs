// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class NavigationComparer : IEqualityComparer<IReadOnlyNavigation>, IComparer<IReadOnlyNavigation>
{
    private readonly bool _compareAnnotations;

    public NavigationComparer(bool compareAnnotations = true)
    {
        _compareAnnotations = compareAnnotations;
    }

    public int Compare(IReadOnlyNavigation x, IReadOnlyNavigation y)
        => StringComparer.Ordinal.Compare(x.Name, y.Name);

    public bool Equals(IReadOnlyNavigation x, IReadOnlyNavigation y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y == null
            ? false
            : x.Name == y.Name
            && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
    }

    public int GetHashCode(IReadOnlyNavigation obj)
        => obj.Name.GetHashCode();
}
