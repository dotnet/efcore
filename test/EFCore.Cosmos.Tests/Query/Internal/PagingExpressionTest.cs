// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class PagingExpressionTest
{
    [Fact]
    public void Update_preserves_paging_parameters()
    {
        var expression = Expression.Constant(1);
        var maxItemCount = new SqlParameterExpression("count", typeof(int), null);
        var continuationToken = new SqlParameterExpression("token", typeof(string), null);
        var tokenLimit = new SqlParameterExpression("limit", typeof(int?), null);
        var paging = new PagingExpression(expression, maxItemCount, continuationToken, tokenLimit, typeof(int));
        var replacement = Expression.Constant(2);

        Assert.Same(paging, paging.Update(expression));
        var updated = paging.Update(replacement);

        Assert.Same(replacement, updated.Expression);
        Assert.Same(maxItemCount, updated.MaxItemCount);
        Assert.Same(continuationToken, updated.ContinuationToken);
        Assert.Same(tokenLimit, updated.ResponseContinuationTokenLimitInKb);
        Assert.Equal(paging.Type, updated.Type);
    }
}
