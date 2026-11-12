// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

#pragma warning disable EF1001 // Internal EF Core API usage.

public class SelectExpressionTest
{
    [Fact]
    public void Equals_compares_distinct()
    {
        var sqlAliasManager = new SqlAliasManager();

        SelectExpression Create(bool distinct)
            => new(
                alias: "s", tables: [new TableValuedFunctionExpression("f", "F", [])], predicate: null, groupBy: [], having: null,
                projections: [], distinct, orderings: [], offset: null, limit: null, sqlAliasManager);

        Assert.True(Create(distinct: true).Equals(Create(distinct: true)));
        Assert.False(Create(distinct: true).Equals(Create(distinct: false)));
    }
}
