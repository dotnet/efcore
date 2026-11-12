// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#pragma warning disable EF1001 // Internal EF Core API usage.

public class SqlServerOpenJsonExpressionTest
{
    [Fact]
    public void Equals_compares_path()
    {
        var json = new SqlFragmentExpression("[c].[Json]");

        Assert.True(new SqlServerOpenJsonExpression("j", json, [new PathSegment("A")])
            .Equals(new SqlServerOpenJsonExpression("j", json, [new PathSegment("A")])));
        Assert.False(new SqlServerOpenJsonExpression("j", json, [new PathSegment("A")])
            .Equals(new SqlServerOpenJsonExpression("j", json, [new PathSegment("B")])));
        Assert.False(new SqlServerOpenJsonExpression("j", json)
            .Equals(new SqlServerOpenJsonExpression("j", json, [new PathSegment("A")])));
    }
}
