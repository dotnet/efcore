// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestIndexComparer : IEqualityComparer<IReadOnlyIndex>, IComparer<IReadOnlyIndex>
{
    private readonly bool _compareAnnotations;

    public TestIndexComparer(bool compareAnnotations = true)
    {
        _compareAnnotations = compareAnnotations;
    }

    public int Compare(IReadOnlyIndex x, IReadOnlyIndex y)
        => PropertyListComparer.Instance.Compare(x.Properties, y.Properties);

    public bool Equals(IReadOnlyIndex x, IReadOnlyIndex y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y == null
            ? false
            : PropertyListComparer.Instance.Equals(x.Properties, y.Properties)
            && x.IsUnique == y.IsUnique
            && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
    }

    public int GetHashCode(IReadOnlyIndex obj)
        => PropertyListComparer.Instance.GetHashCode(obj.Properties);
}
