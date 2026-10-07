// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryingEnumerableTest
{
    [Theory]
    [InlineData("Single", false)]
    [InlineData("Single", true)]
    [InlineData("Split", false)]
    [InlineData("Split", true)]
    [InlineData("FromSql", false)]
    [InlineData("FromSql", true)]
    [InlineData("GroupBySingle", false)]
    [InlineData("GroupBySingle", true)]
    [InlineData("GroupBySplit", false)]
    [InlineData("GroupBySplit", true)]
    public async Task GetAsyncEnumerator_only_replaces_cancellation_token_when_provided(string enumerableType, bool provideToken)
    {
        using var context = FakeRelationalTestHelpers.Instance.CreateContext();
        using var originalTokenSource = new CancellationTokenSource();
        using var providedTokenSource = new CancellationTokenSource();
        var queryContext = (RelationalQueryContext)context.GetService<IQueryContextFactory>().Create();
        queryContext.CancellationToken = originalTokenSource.Token;

        IAsyncEnumerable<object> enumerable = enumerableType switch
        {
            "Single" => new SingleQueryingEnumerable<object>(
                queryContext, null!, null, null!, typeof(DbContext), false, false, false),
            "Split" => new SplitQueryingEnumerable<object>(
                queryContext, null!, null, null!, null, null, typeof(DbContext), false, false, false),
            "FromSql" => new FromSqlQueryingEnumerable<object>(
                queryContext, null!, null, [], null!, typeof(DbContext), false, false, false),
            "GroupBySingle" => new GroupBySingleQueryingEnumerable<object, object>(
                queryContext, null!, null, null!, null!, [], null!, typeof(DbContext), false, false, false),
            "GroupBySplit" => new GroupBySplitQueryingEnumerable<object, object>(
                queryContext, null!, null, null!, null!, [], null!, null, null, typeof(DbContext), false, false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(enumerableType))
        };

        await using var enumerator = provideToken
            ? enumerable.GetAsyncEnumerator(providedTokenSource.Token)
            : enumerable.GetAsyncEnumerator();

        Assert.Equal(
            provideToken ? providedTokenSource.Token : originalTokenSource.Token,
            queryContext.CancellationToken);
    }
}
