// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class FilteredQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : class, IQueryFixtureBase, new()
{
    protected FilteredQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public Task AssertFilteredQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Func<TResult, object> elementSorter = null,
        Action<TResult, TResult> elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : class
        => AssertFilteredQuery(async, query, query, elementSorter, elementAsserter, assertOrder, assertEmpty, testMethodName);

    public Task AssertFilteredQuery<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Func<TResult, object> elementSorter = null,
        Action<TResult, TResult> elementAsserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : class
        => QueryAsserter.AssertQuery(
            actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, assertEmpty, async, testMethodName,
            filteredQuery: true);

    public Task AssertFilteredQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Action<TResult, TResult> asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : struct
        => AssertFilteredQueryScalar(async, query, query, asserter, assertOrder, assertEmpty, testMethodName);

    public Task AssertFilteredQueryScalar<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery,
        Action<TResult, TResult> asserter = null,
        bool assertOrder = false,
        bool assertEmpty = false,
        [CallerMemberName] string testMethodName = null)
        where TResult : struct
        => QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, asserter, assertOrder, assertEmpty, async, testMethodName, filteredQuery: true);

    protected Task AssertFilteredCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query)
        => AssertFilteredCount(async, query, query);

    protected Task AssertFilteredCount<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> actualQuery,
        Func<ISetSource, IQueryable<TResult>> expectedQuery)
        => QueryAsserter.AssertCount(actualQuery, expectedQuery, async, filteredQuery: true);
}
