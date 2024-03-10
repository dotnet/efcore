// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class QueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : class, IQueryFixtureBase, new()
{
    protected QueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
        QueryAsserter = CreateQueryAsserter(fixture);
    }

    protected TFixture Fixture { get; }
    protected QueryAsserter QueryAsserter { get; }

    protected virtual QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new(
            fixture,
            RewriteExpectedQueryExpression,
            RewriteServerQueryExpression,
            IgnoreEntryCount);

    protected virtual bool IgnoreEntryCount
        => false;

    protected virtual Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        => serverQueryExpression;

    protected virtual Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression)
        => new ExpectedQueryRewritingVisitor().Visit(expectedQueryExpression);

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    public Task AssertQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<TResult, object>? elementSorter = null,
        Action<TResult, TResult>? elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        => AssertQuery(async, query, query, elementSorter, elementAsserter, assertOrder, assertEmpty, testMethodName);

    public Task AssertQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<TResult, object>? elementSorter = null,
        Action<TResult, TResult>? elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        => QueryAsserter.AssertQuery(
            actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, assertEmpty, async, testMethodName);

    public Task AssertQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult, TResult>? asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        where TResult : struct
        => AssertQueryScalar(async, query, query, asserter, assertOrder, assertEmpty, testMethodName);

    public Task AssertQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        where TResult : struct
        => QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, asserter, assertOrder, assertEmpty,  async, testMethodName);

    public Task AssertQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult?>> query,
        Action<TResult?, TResult?>? asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        where TResult : struct
        => AssertQueryScalar(async, query, query, asserter, assertOrder, assertEmpty, testMethodName);

    public Task AssertQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult?>> actualQuery,
        Func<ISetSource, IQueryable<TResult?>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = "")
        where TResult : struct
        => QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, asserter, assertOrder, assertEmpty, async, testMethodName);

    protected Task AssertSingleResult<TResult>(
        bool async,
        Expression<Func<ISetSource, TResult>> syncQuery,
        Expression<Func<ISetSource, Task<TResult>>> asyncQuery,
        Action<TResult, TResult>? asserter = null)
        => AssertSingleResult(async, syncQuery, asyncQuery, syncQuery, asserter);

    protected Task AssertSingleResult<TResult>(
        bool async,
        Expression<Func<ISetSource, TResult>> actualSyncQuery,
        Expression<Func<ISetSource, Task<TResult>>> actualAsyncQuery,
        Expression<Func<ISetSource, TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertSingleResult(
            actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, async);

    #region Assert termination operation methods

    protected Task AssertAny<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query)
        => AssertAny(async, query, query);

    protected Task AssertAny<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery)
        => QueryAsserter.AssertAny(
            actualQuery, expectedQuery, async);

    protected Task AssertAny<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate)
        => AssertAny(async, query, query, predicate, predicate);

    protected Task AssertAny<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate)
        => QueryAsserter.AssertAny(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, async);

    protected Task AssertAll<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate)
        => AssertAll(async, query, query, predicate, predicate);

    protected Task AssertAll<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate)
        => QueryAsserter.AssertAll(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, async);

    protected Task AssertElementAt<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<int> index,
        Action<TResult, TResult>? asserter = null)
        => AssertElementAt(async, query, query, index, index, asserter);

    protected Task AssertElementAt<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<int> actualIndex,
        Func<int> expectedIndex,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertElementAt(
            actualQuery, expectedQuery, actualIndex, expectedIndex, asserter, async);

    protected Task AssertElementAtOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<int> index,
        Action<TResult?, TResult?>? asserter = null)
        => AssertElementAtOrDefault(async, query, query, index, index, asserter);

    protected Task AssertElementAtOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<int> actualIndex,
        Func<int> expectedIndex,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertElementAtOrDefault(
            actualQuery, expectedQuery, actualIndex, expectedIndex, asserter, async);

    protected Task AssertFirst<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult, TResult>? asserter = null)
        => AssertFirst(async, query, query, asserter);

    protected Task AssertFirst<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertFirst(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertFirst<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult, TResult>? asserter = null)
        => AssertFirst(async, query, query, predicate, predicate, asserter);

    protected Task AssertFirst<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertFirst(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertFirstOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult?, TResult?>? asserter = null)
        => AssertFirstOrDefault(async, query, query, asserter);

    protected Task AssertFirstOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        int entryCount = 0)
        => QueryAsserter.AssertFirstOrDefault(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertFirstOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult?, TResult?>? asserter = null)
        => AssertFirstOrDefault(async, query, query, predicate, predicate, asserter);

    protected Task AssertFirstOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertFirstOrDefault(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertSingle<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult, TResult>? asserter = null)
        => AssertSingle(async, query, query, asserter);

    protected Task AssertSingle<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertSingle(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertSingle<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult, TResult>? asserter = null)
        => AssertSingle(async, query, query, predicate, predicate, asserter);

    protected Task AssertSingle<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertSingle(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertSingleOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult?, TResult?>? asserter = null)
        => AssertSingleOrDefault(async, query, query, asserter);

    protected Task AssertSingleOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertSingleOrDefault(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertSingleOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult?, TResult?>? asserter = null)
        => AssertSingleOrDefault(async, query, query, predicate, predicate, asserter);

    protected Task AssertSingleOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertSingleOrDefault(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertLast<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult, TResult>? asserter = null)
        => AssertLast(async, query, query, asserter);

    protected Task AssertLast<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertLast(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertLast<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult, TResult>? asserter = null)
        => AssertLast(async, query, query, predicate, predicate, asserter);

    protected Task AssertLast<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null)
        => QueryAsserter.AssertLast(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertLastOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult?, TResult?>? asserter = null)
        => AssertLastOrDefault(async, query, query, asserter);

    protected Task AssertLastOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertLastOrDefault(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertLastOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate,
        Action<TResult?, TResult?>? asserter = null)
        => AssertLastOrDefault(async, query, query, predicate, predicate, asserter);

    protected Task AssertLastOrDefault<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertLastOrDefault(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async);

    protected Task AssertCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query)
        => AssertCount(async, query, query);

    protected Task AssertCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery)
        => QueryAsserter.AssertCount(actualQuery, expectedQuery, async);

    protected Task AssertCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate)
        => AssertCount(async, query, query, predicate, predicate);

    protected Task AssertCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate)
        => QueryAsserter.AssertCount(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, async);

    protected Task AssertLongCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query)
        => AssertLongCount(async, query, query);

    protected Task AssertLongCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, bool>> predicate)
        => AssertLongCount(async, query, query, predicate, predicate);

    protected Task AssertLongCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery)
        => QueryAsserter.AssertLongCount(actualQuery, expectedQuery, async);

    protected Task AssertLongCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate)
        => QueryAsserter.AssertLongCount(
            actualQuery, expectedQuery, actualPredicate, expectedPredicate, async);

    protected Task AssertMin<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult?, TResult?>? asserter = null)
        => AssertMin(async, query, query, asserter);

    protected Task AssertMin<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertMin(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertMin<TResult, TSelector>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, TSelector>> selector,
        Action<TSelector?, TSelector?>? asserter = null)
        => AssertMin(async, query, query, selector, selector, asserter);

    protected Task AssertMin<TResult, TSelector>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, TSelector>> actualSelector,
        Expression<Func<TResult, TSelector>> expectedSelector,
        Action<TSelector?, TSelector?>? asserter = null)
        => QueryAsserter.AssertMin(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertMax<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult?, TResult?>? asserter = null)
        => AssertMax(async, query, query, asserter);

    protected Task AssertMax<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null)
        => QueryAsserter.AssertMax(
            actualQuery, expectedQuery, asserter, async);

    protected Task AssertMax<TResult, TSelector>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, TSelector>> selector,
        Action<TSelector?, TSelector?>? asserter = null)
        => AssertMax(async, query, query, selector, selector, asserter);

    protected Task AssertMax<TResult, TSelector>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, TSelector>> actualSelector,
        Expression<Func<TResult, TSelector>> expectedSelector,
        Action<TSelector?, TSelector?>? asserter = null)
        => QueryAsserter.AssertMax(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<int>> query,
        Action<int, int>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<int>> actualQuery,
        Func<ISetSource, IQueryable<int>> expectedQuery,
        Action<int, int>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<int?>> query,
        Action<int?, int?>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<int?>> actualQuery,
        Func<ISetSource, IQueryable<int?>> expectedQuery,
        Action<int?, int?>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<long>> query,
        Action<long, long>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<long>> actualQuery,
        Func<ISetSource, IQueryable<long>> expectedQuery,
        Action<long, long>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<long?>> query,
        Action<long?, long?>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<long?>> actualQuery,
        Func<ISetSource, IQueryable<long?>> expectedQuery,
        Action<long?, long?>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<decimal>> query,
        Action<decimal, decimal>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<decimal>> actualQuery,
        Func<ISetSource, IQueryable<decimal>> expectedQuery,
        Action<decimal, decimal>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<decimal?>> query,
        Action<decimal?, decimal?>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<decimal?>> actualQuery,
        Func<ISetSource, IQueryable<decimal?>> expectedQuery,
        Action<decimal?, decimal?>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<float>> query,
        Action<float, float>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<float>> actualQuery,
        Func<ISetSource, IQueryable<float>> expectedQuery,
        Action<float, float>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<float?>> query,
        Action<float?, float?>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<float?>> actualQuery,
        Func<ISetSource, IQueryable<float?>> expectedQuery,
        Action<float?, float?>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<double>> query,
        Action<double, double>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<double>> actualQuery,
        Func<ISetSource, IQueryable<double>> expectedQuery,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<double?>> query,
        Action<double?, double?>? asserter = null)
        => AssertSum(async, query, query, asserter);

    protected Task AssertSum(
        bool async,
        Func<ISetSource, IQueryable<double?>> actualQuery,
        Func<ISetSource, IQueryable<double?>> expectedQuery,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, int>> selector,
        Action<int, int>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int>> actualSelector,
        Expression<Func<TResult, int>> expectedSelector,
        Action<int, int>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, int?>> selector,
        Action<int?, int?>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int?>> actualSelector,
        Expression<Func<TResult, int?>> expectedSelector,
        Action<int?, int?>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, long>> selector,
        Action<long, long>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long>> actualSelector,
        Expression<Func<TResult, long>> expectedSelector,
        Action<long, long>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, long?>> selector,
        Action<long?, long?>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long?>> actualSelector,
        Expression<Func<TResult, long?>> expectedSelector,
        Action<long?, long?>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, decimal>> selector,
        Action<decimal, decimal>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal>> actualSelector,
        Expression<Func<TResult, decimal>> expectedSelector,
        Action<decimal, decimal>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, decimal?>> selector,
        Action<decimal?, decimal?>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal?>> actualSelector,
        Expression<Func<TResult, decimal?>> expectedSelector,
        Action<decimal?, decimal?>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, float>> selector,
        Action<float, float>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float>> actualSelector,
        Expression<Func<TResult, float>> expectedSelector,
        Action<float, float>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, float?>> selector,
        Action<float?, float?>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float?>> actualSelector,
        Expression<Func<TResult, float?>> expectedSelector,
        Action<float?, float?>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, double>> selector,
        Action<double, double>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double>> actualSelector,
        Expression<Func<TResult, double>> expectedSelector,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, double?>> selector,
        Action<double?, double?>? asserter = null)
        => AssertSum(async, query, query, selector, selector, asserter);

    protected Task AssertSum<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double?>> actualSelector,
        Expression<Func<TResult, double?>> expectedSelector,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertSum(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<int>> query,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<int>> actualQuery,
        Func<ISetSource, IQueryable<int>> expectedQuery,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<int?>> query,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<int?>> actualQuery,
        Func<ISetSource, IQueryable<int?>> expectedQuery,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<long>> query,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<long>> actualQuery,
        Func<ISetSource, IQueryable<long>> expectedQuery,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<long?>> query,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<long?>> actualQuery,
        Func<ISetSource, IQueryable<long?>> expectedQuery,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<decimal>> query,
        Action<decimal, decimal>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<decimal>> actualQuery,
        Func<ISetSource, IQueryable<decimal>> expectedQuery,
        Action<decimal, decimal>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<decimal?>> query,
        Action<decimal?, decimal?>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<decimal?>> actualQuery,
        Func<ISetSource, IQueryable<decimal?>> expectedQuery,
        Action<decimal?, decimal?>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<float>> query,
        Action<float, float>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<float>> actualQuery,
        Func<ISetSource, IQueryable<float>> expectedQuery,
        Action<float, float>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<float?>> query,
        Action<float?, float?>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<float?>> actualQuery,
        Func<ISetSource, IQueryable<float?>> expectedQuery,
        Action<float?, float?>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<double>> query,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<double>> actualQuery,
        Func<ISetSource, IQueryable<double>> expectedQuery,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<double?>> query,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, asserter);

    protected Task AssertAverage(
        bool async,
        Func<ISetSource, IQueryable<double?>> actualQuery,
        Func<ISetSource, IQueryable<double?>> expectedQuery,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, int>> selector,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int>> actualSelector,
        Expression<Func<TResult, int>> expectedSelector,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, int?>> selector,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int?>> actualSelector,
        Expression<Func<TResult, int?>> expectedSelector,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, long>> selector,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long>> actualSelector,
        Expression<Func<TResult, long>> expectedSelector,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, long?>> selector,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long?>> actualSelector,
        Expression<Func<TResult, long?>> expectedSelector,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, decimal>> selector,
        Action<decimal, decimal>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal>> actualSelector,
        Expression<Func<TResult, decimal>> expectedSelector,
        Action<decimal, decimal>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, decimal?>> selector,
        Action<decimal?, decimal?>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal?>> actualSelector,
        Expression<Func<TResult, decimal?>> expectedSelector,
        Action<decimal?, decimal?>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, float>> selector,
        Action<float, float>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float>> actualSelector,
        Expression<Func<TResult, float>> expectedSelector,
        Action<float, float>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, float?>> selector,
        Action<float?, float?>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float?>> actualSelector,
        Expression<Func<TResult, float?>> expectedSelector,
        Action<float?, float?>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, double>> selector,
        Action<double, double>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double>> actualSelector,
        Expression<Func<TResult, double>> expectedSelector,
        Action<double, double>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, double?>> selector,
        Action<double?, double?>? asserter = null)
        => AssertAverage(async, query, query, selector, selector, asserter);

    protected Task AssertAverage<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double?>> actualSelector,
        Expression<Func<TResult, double?>> expectedSelector,
        Action<double?, double?>? asserter = null)
        => QueryAsserter.AssertAverage(
            actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async);

    #endregion

    #region Helpers

    protected void AssertEqual<T>(T expected, T actual, Action<T, T>? asserter = null)
        => QueryAsserter.AssertEqual(expected, actual, asserter);

    protected void AssertCollection<TElement>(
        IEnumerable<TElement>? expected,
        IEnumerable<TElement>? actual,
        bool ordered = false,
        Func<TElement, object?>? elementSorter = null,
        Action<TElement, TElement>? elementAsserter = null)
        => QueryAsserter.AssertCollection(expected, actual, ordered, elementSorter, elementAsserter);

    protected void AssertInclude<TEntity>(
        TEntity expected,
        TEntity actual,
        params IExpectedInclude[] expectedIncludes)
        => QueryAsserter.AssertInclude(expected, actual, expectedIncludes);

    protected void AssertGrouping<TKey, TElement>(
        IGrouping<TKey, TElement> expected,
        IGrouping<TKey, TElement> actual,
        bool ordered = false,
        Func<TElement, object?>? elementSorter = null,
        Action<TKey, TKey>? keyAsserter = null,
        Action<TElement, TElement>? elementAsserter = null)
    {
        keyAsserter ??= Assert.Equal;
        keyAsserter(expected.Key, actual.Key);
        AssertCollection(expected, actual, ordered, elementSorter, elementAsserter);
    }

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static void AssertTranslationFailed(Action query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            Assert.Throws<InvalidOperationException>(query)
                .Message);

    protected static async Task AssertTranslationFailedWithDetails(Func<Task> query, string details)
        => Assert.Contains(
            CoreStrings.TranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertInvalidMaterializationType(Func<Task> query, string queryableType)
        => Assert.Contains(
            CoreStrings.QueryInvalidMaterializationType("", queryableType)[64..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertIncludeOnNonEntity(Func<Task> query)
        => Assert.Contains(
            CoreStrings.IncludeOnNonEntity("")[64..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertInvalidIncludeExpression(Func<Task> query)
        => Assert.Contains(
            CoreStrings.InvalidIncludeExpression("")[64..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertUnableToTranslateEFProperty(Func<Task> query)
        => Assert.Contains(
            CoreStrings.QueryUnableToTranslateEFProperty("")[64..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertInvalidSetSharedType(Func<Task> query, string typeName)
        => Assert.Equal(
            CoreStrings.InvalidSetSharedType(typeName),
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    #endregion
}
