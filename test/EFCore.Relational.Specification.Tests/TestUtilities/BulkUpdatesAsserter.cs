// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BulkUpdatesAsserter
{
    private readonly Func<DbContext> _contextCreator;
    private readonly Action<DatabaseFacade, IDbContextTransaction> _useTransaction;
    private readonly Func<DbContext, ISetSource> _setSourceCreator;
    private readonly Func<Expression, Expression> _rewriteServerQueryExpression;

    public BulkUpdatesAsserter(IBulkUpdatesFixtureBase queryFixture, Func<Expression, Expression> rewriteServerQueryExpression)
    {
        _contextCreator = queryFixture.GetContextCreator();
        _useTransaction = queryFixture.GetUseTransaction();
        _setSourceCreator = queryFixture.GetSetSourceCreator();
        _rewriteServerQueryExpression = rewriteServerQueryExpression;
    }

    public async Task AssertDelete<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        int rowsAffectedCount)
    {
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                _contextCreator, _useTransaction,
                async context =>
                {
                    var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                    var result = await processedQuery.BulkDeleteAsync();

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
        else
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                _contextCreator, _useTransaction,
                context =>
                {
                    var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                    var result = processedQuery.BulkDelete();

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
    }

    private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
        => query.Provider.CreateQuery<T>(_rewriteServerQueryExpression(query.Expression));
}
