// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Internal;

public class TypeFullNameComparerTest
{
    [Fact]
    public void Instance_is_singleton()
        => Assert.Same(TypeFullNameComparer.Instance, TypeFullNameComparer.Instance);

    [Fact]
    public void Compare_returns_zero_for_same_reference()
        => Assert.Equal(0, TypeFullNameComparer.Instance.Compare(typeof(int), typeof(int)));

    [Fact]
    public void Compare_returns_zero_when_both_null()
        => Assert.Equal(0, TypeFullNameComparer.Instance.Compare(null, null));

    [Fact]
    public void Compare_treats_null_as_less_than_non_null()
    {
        Assert.True(TypeFullNameComparer.Instance.Compare(null, typeof(int)) < 0);
        Assert.True(TypeFullNameComparer.Instance.Compare(typeof(int), null) > 0);
    }

    [Fact]
    public void Compare_orders_by_ordinal_full_name()
    {
        // "System.Int32" is ordinally less than "System.String".
        Assert.True(TypeFullNameComparer.Instance.Compare(typeof(int), typeof(string)) < 0);
        Assert.True(TypeFullNameComparer.Instance.Compare(typeof(string), typeof(int)) > 0);
    }

    [Fact]
    public void Compare_is_antisymmetric()
    {
        var forward = TypeFullNameComparer.Instance.Compare(typeof(int), typeof(long));
        var backward = TypeFullNameComparer.Instance.Compare(typeof(long), typeof(int));

        Assert.Equal(Math.Sign(forward), -Math.Sign(backward));
    }

    [Fact]
    public void Compare_uses_ordinal_not_culture_aware_comparison()
    {
        // Ordinal comparison is case-sensitive: uppercase letters sort before lowercase ('A' = 65, 'a' = 97).
        var result = TypeFullNameComparer.Instance.Compare(typeof(UpperCased), typeof(lowerCased));

        Assert.True(result < 0);
    }

    [Fact]
    public void Equals_returns_true_for_same_type()
        => Assert.True(TypeFullNameComparer.Instance.Equals(typeof(int), typeof(int)));

    [Fact]
    public void Equals_returns_true_when_both_null()
        => Assert.True(TypeFullNameComparer.Instance.Equals(null, null));

    [Fact]
    public void Equals_returns_false_for_different_types()
        => Assert.False(TypeFullNameComparer.Instance.Equals(typeof(int), typeof(string)));

    [Fact]
    public void Equals_returns_false_when_only_one_is_null()
    {
        Assert.False(TypeFullNameComparer.Instance.Equals(null, typeof(int)));
        Assert.False(TypeFullNameComparer.Instance.Equals(typeof(int), null));
    }

    [Fact]
    public void GetHashCode_is_equal_for_equal_types()
        => Assert.Equal(
            TypeFullNameComparer.Instance.GetHashCode(typeof(int)),
            TypeFullNameComparer.Instance.GetHashCode(typeof(int)));

    // ReSharper disable once ClassNeverInstantiated.Local
    private class UpperCased;

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable once InconsistentNaming
    private class lowerCased;
}
