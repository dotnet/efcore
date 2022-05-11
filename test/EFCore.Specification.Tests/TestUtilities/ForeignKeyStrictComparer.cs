// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ForeignKeyStrictComparer : IEqualityComparer<IReadOnlyForeignKey>, IComparer<IReadOnlyForeignKey>
{
    private readonly bool _compareAnnotations;
    private readonly bool _compareNavigations;

    public ForeignKeyStrictComparer(bool compareAnnotations = true, bool compareNavigations = true)
    {
        _compareAnnotations = compareAnnotations;
        _compareNavigations = compareNavigations;
    }

    public int Compare(IReadOnlyForeignKey x, IReadOnlyForeignKey y)
        => ForeignKeyComparer.Instance.Compare(x, y);

    public bool Equals(IReadOnlyForeignKey x, IReadOnlyForeignKey y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y == null
            ? false
            : ForeignKeyComparer.Instance.Equals(x, y)
            && (x.IsUnique == y.IsUnique)
            && (x.IsRequired == y.IsRequired)
            && (!_compareNavigations
                || (new NavigationComparer(_compareAnnotations).Equals(x.DependentToPrincipal, y.DependentToPrincipal)
                    && new NavigationComparer(_compareAnnotations).Equals(x.PrincipalToDependent, y.PrincipalToDependent)))
            && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
    }

    public int GetHashCode(IReadOnlyForeignKey obj)
        => ForeignKeyComparer.Instance.GetHashCode(obj);
}
