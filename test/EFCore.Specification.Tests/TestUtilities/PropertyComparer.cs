// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class PropertyComparer : IEqualityComparer<IReadOnlyProperty>, IComparer<IReadOnlyProperty>
{
    private readonly bool _compareAnnotations;

    public PropertyComparer(bool compareAnnotations = true)
    {
        _compareAnnotations = compareAnnotations;
    }

    public int Compare(IReadOnlyProperty x, IReadOnlyProperty y)
        => StringComparer.Ordinal.Compare(x.Name, y.Name);

    public bool Equals(IReadOnlyProperty x, IReadOnlyProperty y)
    {
        if (x == null)
        {
            return y == null;
        }

        return y == null
            ? false
            : x.Name == y.Name
            && x.ClrType == y.ClrType
            && x.IsShadowProperty() == y.IsShadowProperty()
            && x.IsNullable == y.IsNullable
            && x.IsConcurrencyToken == y.IsConcurrencyToken
            && x.ValueGenerated == y.ValueGenerated
            && x.GetBeforeSaveBehavior() == y.GetBeforeSaveBehavior()
            && x.GetAfterSaveBehavior() == y.GetAfterSaveBehavior()
            && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
    }

    public int GetHashCode(IReadOnlyProperty obj)
        => obj.Name.GetHashCode();
}
