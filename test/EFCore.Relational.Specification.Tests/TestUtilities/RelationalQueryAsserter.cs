// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class RelationalQueryAsserter(
    IQueryFixtureBase queryFixture,
    Func<Expression, Expression> rewriteExpectedQueryExpression,
    Func<Expression, Expression> rewriteServerQueryExpression,
    bool ignoreEntryCount = false) : QueryAsserter(queryFixture, rewriteExpectedQueryExpression, rewriteServerQueryExpression, ignoreEntryCount)
{
    private static int ExecuteReader(DbCommand command)
    {
        var needToOpen = command.Connection?.State == ConnectionState.Closed;
        if (needToOpen)
        {
            command.Connection!.Open();
        }

        var count = 0;

        using (var reader = command.ExecuteReader())
        {
            // Not materializing objects here since automatic creation of objects does not
            // work for some SQL types, such as geometry/geography
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    count++;
                }
            }
        }

        if (needToOpen)
        {
            command.Connection!.Close();
        }

        return count;
    }

    public override async Task AssertSingleResult<TResult>(
        Expression<Func<ISetSource, TResult>> actualSyncQuery,
        Expression<Func<ISetSource, Task<TResult>>> actualAsyncQuery,
        Expression<Func<ISetSource, TResult>> expectedQuery,
        Action<TResult, TResult>? asserter,
        bool async,
        bool filteredQuery = false)
    {
        var outputSql = true;
        try
        {
            await base.AssertSingleResult(actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, async, filteredQuery);
            outputSql = false;
        }
        finally
        {
            if (outputSql)
            {
                ((ITestSqlLoggerFactory)QueryFixture).TestSqlLoggerFactory.OutputSql();
            }
        }
    }

    public override async Task AssertQuery<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<TResult, object>? elementSorter,
        Action<TResult, TResult>? elementAsserter,
        bool assertOrder,
        bool assertEmpty,
        bool async,
        string testMethodName,
        bool filteredQuery = false)
    {
        var outputSql = true;
        try
        {
            await base.AssertQuery(actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, assertEmpty, async, testMethodName, filteredQuery);
            outputSql = false;
        }
        finally
        {
            if (outputSql)
            {
                ((ITestSqlLoggerFactory)QueryFixture).TestSqlLoggerFactory.OutputSql();
            }
        }
    }

    public override async Task AssertQueryScalar<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter,
        bool assertOrder,
        bool assertEmpty,
        bool async,
        string testMethodName,
        bool filteredQuery = false)
    {
        var outputSql = true;
        try
        {
            await base.AssertQueryScalar(actualQuery, expectedQuery, asserter, assertOrder, assertEmpty, async, testMethodName, filteredQuery);
            outputSql = false;
        }
        finally
        {
            if (outputSql)
            {
                ((ITestSqlLoggerFactory)QueryFixture).TestSqlLoggerFactory.OutputSql();
            }
        }
    }

    public override async Task AssertQueryScalar<TResult>(
        Func<ISetSource, IQueryable<TResult?>> actualQuery,
        Func<ISetSource, IQueryable<TResult?>> expectedQuery,
        Action<TResult?, TResult?>? asserter,
        bool assertOrder,
        bool assertEmpty,
        bool async,
        string testMethodName,
        bool filteredQuery = false)
    {
        var outputSql = true;
        try
        {
            await base.AssertQueryScalar(actualQuery, expectedQuery, asserter, assertOrder, assertEmpty, async, testMethodName, filteredQuery);
            outputSql = false;
        }
        finally
        {
            if (outputSql)
            {
                ((ITestSqlLoggerFactory)QueryFixture).TestSqlLoggerFactory.OutputSql();
            }
        }
    }

    private async Task RunAndOutputSqlOnFailure(Func<Task> assertQuery)
    {
        var outputSql = true;
        try
        {
            await assertQuery();
            outputSql = false;
        }
        finally
        {
            if (outputSql)
            {
                ((ITestSqlLoggerFactory)QueryFixture).TestSqlLoggerFactory.OutputSql();
            }
        }
    }

    public override Task AssertAll<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAll(actualQuery, expectedQuery, actualPredicate, expectedPredicate, async, filteredQuery));

    public override Task AssertAny<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAny(actualQuery, expectedQuery, async, filteredQuery));

    public override Task AssertAny<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAny(actualQuery, expectedQuery, actualPredicate, expectedPredicate, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<decimal>> actualQuery,
        Func<ISetSource, IQueryable<decimal>> expectedQuery,
        Action<decimal, decimal>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<decimal?>> actualQuery,
        Func<ISetSource, IQueryable<decimal?>> expectedQuery,
        Action<decimal?, decimal?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<double>> actualQuery,
        Func<ISetSource, IQueryable<double>> expectedQuery,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<double?>> actualQuery,
        Func<ISetSource, IQueryable<double?>> expectedQuery,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<float>> actualQuery,
        Func<ISetSource, IQueryable<float>> expectedQuery,
        Action<float, float>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<float?>> actualQuery,
        Func<ISetSource, IQueryable<float?>> expectedQuery,
        Action<float?, float?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<int>> actualQuery,
        Func<ISetSource, IQueryable<int>> expectedQuery,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<int?>> actualQuery,
        Func<ISetSource, IQueryable<int?>> expectedQuery,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<long>> actualQuery,
        Func<ISetSource, IQueryable<long>> expectedQuery,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage(
        Func<ISetSource, IQueryable<long?>> actualQuery,
        Func<ISetSource, IQueryable<long?>> expectedQuery,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal>> actualSelector,
        Expression<Func<TResult, decimal>> expectedSelector,
        Action<decimal, decimal>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal?>> actualSelector,
        Expression<Func<TResult, decimal?>> expectedSelector,
        Action<decimal?, decimal?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double>> actualSelector,
        Expression<Func<TResult, double>> expectedSelector,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double?>> actualSelector,
        Expression<Func<TResult, double?>> expectedSelector,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float>> actualSelector,
        Expression<Func<TResult, float>> expectedSelector,
        Action<float, float>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float?>> actualSelector,
        Expression<Func<TResult, float?>> expectedSelector,
        Action<float?, float?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int>> actualSelector,
        Expression<Func<TResult, int>> expectedSelector,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int?>> actualSelector,
        Expression<Func<TResult, int?>> expectedSelector,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long>> actualSelector,
        Expression<Func<TResult, long>> expectedSelector,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertAverage<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long?>> actualSelector,
        Expression<Func<TResult, long?>> expectedSelector,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertCount<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertCount(actualQuery, expectedQuery, async, filteredQuery));

    public override Task AssertCount<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertCount(actualQuery, expectedQuery, actualPredicate, expectedPredicate, async, filteredQuery));

    public override Task AssertElementAt<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<int> actualIndex,
        Func<int> expectedIndex,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertElementAt(actualQuery, expectedQuery, actualIndex, expectedIndex, asserter, async, filteredQuery));

    public override Task AssertElementAtOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<int> actualIndex,
        Func<int> expectedIndex,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertElementAtOrDefault(actualQuery, expectedQuery, actualIndex, expectedIndex, asserter, async, filteredQuery));

    public override Task AssertFirst<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertFirst(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertFirst<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertFirst(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertFirstOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertFirstOrDefault(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertFirstOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertFirstOrDefault(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertLast<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertLast(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertLast<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertLast(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertLastOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertLastOrDefault(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertLastOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertLastOrDefault(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertLongCount<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertLongCount(actualQuery, expectedQuery, async, filteredQuery));

    public override Task AssertLongCount<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertLongCount(actualQuery, expectedQuery, actualPredicate, expectedPredicate, async, filteredQuery));

    public override Task AssertMax<TResult, TSelector>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, TSelector>> actualSelector,
        Expression<Func<TResult, TSelector>> expectedSelector,
        Action<TSelector?, TSelector?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TSelector : default
        => RunAndOutputSqlOnFailure(() => base.AssertMax(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertMax<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertMax(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertMin<TResult, TSelector>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, TSelector>> actualSelector,
        Expression<Func<TResult, TSelector>> expectedSelector,
        Action<TSelector?, TSelector?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TSelector : default
        => RunAndOutputSqlOnFailure(() => base.AssertMin(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertMin<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertMin(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSingle<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSingle(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSingle<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult, TResult>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSingle(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertSingleOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertSingleOrDefault(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSingleOrDefault<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, bool>> actualPredicate,
        Expression<Func<TResult, bool>> expectedPredicate,
        Action<TResult?, TResult?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        where TResult : default
        => RunAndOutputSqlOnFailure(() => base.AssertSingleOrDefault(actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<decimal>> actualQuery,
        Func<ISetSource, IQueryable<decimal>> expectedQuery,
        Action<decimal, decimal>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<decimal?>> actualQuery,
        Func<ISetSource, IQueryable<decimal?>> expectedQuery,
        Action<decimal?, decimal?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<double>> actualQuery,
        Func<ISetSource, IQueryable<double>> expectedQuery,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<double?>> actualQuery,
        Func<ISetSource, IQueryable<double?>> expectedQuery,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<float>> actualQuery,
        Func<ISetSource, IQueryable<float>> expectedQuery,
        Action<float, float>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<float?>> actualQuery,
        Func<ISetSource, IQueryable<float?>> expectedQuery,
        Action<float?, float?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<int>> actualQuery,
        Func<ISetSource, IQueryable<int>> expectedQuery,
        Action<int, int>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<int?>> actualQuery,
        Func<ISetSource, IQueryable<int?>> expectedQuery,
        Action<int?, int?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<long>> actualQuery,
        Func<ISetSource, IQueryable<long>> expectedQuery,
        Action<long, long>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum(
        Func<ISetSource, IQueryable<long?>> actualQuery,
        Func<ISetSource, IQueryable<long?>> expectedQuery,
        Action<long?, long?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal>> actualSelector,
        Expression<Func<TResult, decimal>> expectedSelector,
        Action<decimal, decimal>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, decimal?>> actualSelector,
        Expression<Func<TResult, decimal?>> expectedSelector,
        Action<decimal?, decimal?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double>> actualSelector,
        Expression<Func<TResult, double>> expectedSelector,
        Action<double, double>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, double?>> actualSelector,
        Expression<Func<TResult, double?>> expectedSelector,
        Action<double?, double?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float>> actualSelector,
        Expression<Func<TResult, float>> expectedSelector,
        Action<float, float>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, float?>> actualSelector,
        Expression<Func<TResult, float?>> expectedSelector,
        Action<float?, float?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int>> actualSelector,
        Expression<Func<TResult, int>> expectedSelector,
        Action<int, int>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, int?>> actualSelector,
        Expression<Func<TResult, int?>> expectedSelector,
        Action<int?, int?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long>> actualSelector,
        Expression<Func<TResult, long>> expectedSelector,
        Action<long, long>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));

    public override Task AssertSum<TResult>(
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Expression<Func<TResult, long?>> actualSelector,
        Expression<Func<TResult, long?>> expectedSelector,
        Action<long?, long?>? asserter = null,
        bool async = false,
        bool filteredQuery = false)
        => RunAndOutputSqlOnFailure(() => base.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, async, filteredQuery));
}
