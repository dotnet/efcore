// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
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
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TResult : class
            => AssertFilteredQuery(async, query, query, elementSorter, elementAsserter, assertOrder, entryCount, testMethodName);

        public Task AssertFilteredQuery<TResult>(
            bool async,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Func<TResult, object> elementSorter = null,
            Action<TResult, TResult> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TResult : class
            => QueryAsserter.AssertQuery(
                actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount, async, testMethodName,
                filteredQuery: true);

        public Task AssertFilteredQueryScalar<TResult>(
            bool async,
            Func<ISetSource, IQueryable<TResult>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TResult : struct
            => AssertFilteredQueryScalar(async, query, query, assertOrder, testMethodName);

        public Task AssertFilteredQueryScalar<TResult>(
            bool async,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TResult : struct
            => QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, async, testMethodName, filteredQuery: true);

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
}
