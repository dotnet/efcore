// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class SqlConstantExpressionTest
{
    [Fact]
    public void Equals_of_IList_uses_deep_equality()
    {
        int[] x = [1, 2, 3];

        var a = new SqlConstantExpression(new[] { x.ToList() }, null);
        var b = new SqlConstantExpression(new[] { x.ToList() }, null);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_of_IList_is_consistent_with_Equals()
    {
        int[] x = [1, 2, 3];

        var a = new SqlConstantExpression(x.ToList(), null);
        var b = new SqlConstantExpression(x.ToList(), null);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
