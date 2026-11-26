// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class TableExpressionBaseTest
{
    [Fact]
    public void Equals_compares_annotations()
    {
        var table = new TableValuedFunctionExpression("f", "F", []);

        Assert.True(table.AddAnnotation("A", 1).Equals(table.AddAnnotation("A", 1)));
        Assert.False(table.AddAnnotation("A", 1).Equals(table.AddAnnotation("A", 2)));
        Assert.False(table.AddAnnotation("A", 1).Equals(table.AddAnnotation("B", 1)));
        Assert.False(table.Equals(table.AddAnnotation("A", 1)));
    }
}
