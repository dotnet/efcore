// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable PossibleNullReferenceException
namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestValueConverterComparer : IEqualityComparer<ValueConverter>
{
    public static readonly TestValueConverterComparer Instance = new();

    private TestValueConverterComparer()
    {
    }

    public bool Equals(ValueConverter? x, ValueConverter? y)
        => x == null
            ? y == null
            : y == null
                ? false
                : ExpressionEqualityComparer.Instance.Equals(x.ConvertFromProviderExpression, y.ConvertFromProviderExpression)
                    && ExpressionEqualityComparer.Instance.Equals(x.ConvertToProviderExpression, y.ConvertToProviderExpression);

    public int GetHashCode(ValueConverter obj)
        => ExpressionEqualityComparer.Instance.GetHashCode(obj.ConvertFromProviderExpression)
            ^ ExpressionEqualityComparer.Instance.GetHashCode(obj.ConvertToProviderExpression);
}
