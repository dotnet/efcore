// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class BulkUpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : class, IBulkUpdatesFixtureBase, new()
{
    protected BulkUpdatesTestBase(TFixture fixture)
    {
        Fixture = fixture;
        BulkUpdatesAsserter = new BulkUpdatesAsserter(fixture, RewriteServerQueryExpression);
    }

    protected TFixture Fixture { get; }

    protected BulkUpdatesAsserter BulkUpdatesAsserter { get; }

    protected virtual Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        => serverQueryExpression;

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    public Task AssertDelete<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        int rowsAffectedCount)
        => BulkUpdatesAsserter.AssertDelete(async, query, rowsAffectedCount);

    public Task AssertUpdate<TResult, TEntity>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, TEntity>> entitySelector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        int rowsAffectedCount,
        Action<IReadOnlyList<TEntity>, IReadOnlyList<TEntity>> asserter = null)
        where TResult : class
        => BulkUpdatesAsserter.AssertUpdate(async, query, entitySelector, setPropertyCalls, rowsAffectedCount, asserter);

    protected static async Task AssertTranslationFailed(string details, Func<Task> query)
        => Assert.Contains(
            RelationalStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);
}
