// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

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

    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

    public Task AssertDelete<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        int rowsAffectedCount)
        => BulkUpdatesAsserter.AssertDelete(async, query, rowsAffectedCount);
}
