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
    private readonly IReadOnlyDictionary<Type, object> _entitySorters;

    public BulkUpdatesAsserter(IBulkUpdatesFixtureBase queryFixture, Func<Expression, Expression> rewriteServerQueryExpression)
    {
        _contextCreator = queryFixture.GetContextCreator();
        _useTransaction = queryFixture.GetUseTransaction();
        _setSourceCreator = queryFixture.GetSetSourceCreator();
        _rewriteServerQueryExpression = rewriteServerQueryExpression;
        _entitySorters = queryFixture.EntitySorters ?? new Dictionary<Type, object>();
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
        Expression<Func<SetPropertyStatements<TResult>, SetPropertyStatements<TResult>>> setPropertyStatements,
        int rowsAffectedCount,
        Action<IReadOnlyList<TEntity>, IReadOnlyList<TEntity>> asserter)
        where TResult : class
    {
        _entitySorters.TryGetValue(typeof(TEntity), out var sorter);
        var elementSorter = (Func<TEntity, object>)sorter;
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                _contextCreator, _useTransaction,
                async context =>
                {
                    var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                    var before = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    var result = await processedQuery.ExecuteUpdateAsync(setPropertyStatements);

                    Assert.Equal(rowsAffectedCount, result);

                    var after = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    if (asserter != null)
                    {
                        asserter(before, after);
                    }
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

                    var result = processedQuery.ExecuteUpdate(setPropertyStatements);

                    Assert.Equal(rowsAffectedCount, result);

                    var after = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                    if (asserter != null)
                    {
                        asserter(before, after);
                    }
                });
        }
    }

    private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
        => query.Provider.CreateQuery<T>(_rewriteServerQueryExpression(query.Expression));
}
