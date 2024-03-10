// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BulkUpdatesAsserter(IBulkUpdatesFixtureBase queryFixture, Func<Expression, Expression> rewriteServerQueryExpression)
{
    private readonly Func<DbContext> _contextCreator = queryFixture.GetContextCreator();
    private readonly Action<DatabaseFacade, IDbContextTransaction> _useTransaction = queryFixture.GetUseTransaction();
    private readonly Func<DbContext, ISetSource> _setSourceCreator = queryFixture.GetSetSourceCreator();
    private readonly Func<Expression, Expression> _rewriteServerQueryExpression = rewriteServerQueryExpression;
    private readonly IReadOnlyDictionary<Type, object> _entitySorters = queryFixture.EntitySorters ?? new Dictionary<Type, object>();

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

                    var result = await processedQuery.ExecuteDeleteAsync();

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

                    var result = processedQuery.ExecuteDelete();

                    Assert.Equal(rowsAffectedCount, result);
                });
        }
    }

    public async Task AssertUpdate<TResult, TEntity>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, TEntity>> entitySelector,
        Expression<Func<SetPropertyCalls<TResult>, SetPropertyCalls<TResult>>> setPropertyCalls,
        int rowsAffectedCount,
        Action<IReadOnlyList<TEntity>, IReadOnlyList<TEntity>> asserter)
        where TResult : class
    {
        var elementSorter = (Func<TEntity, object>)_entitySorters[typeof(TEntity)];
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                _contextCreator, _useTransaction,
                async context =>
                {
                    var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                    var before = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    var result = await processedQuery.ExecuteUpdateAsync(setPropertyCalls);

                    Assert.Equal(rowsAffectedCount, result);

                    var after = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    asserter?.Invoke(before, after);
                });
        }
        else
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                _contextCreator, _useTransaction,
                context =>
                {
                    var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                    var before = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    var result = processedQuery.ExecuteUpdate(setPropertyCalls);

                    Assert.Equal(rowsAffectedCount, result);

                    var after = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    asserter?.Invoke(before, after);
                });
        }
    }

    private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
        => query.Provider.CreateQuery<T>(_rewriteServerQueryExpression(query.Expression));
}
