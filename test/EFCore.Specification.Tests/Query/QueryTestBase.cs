// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : class, IQueryFixtureBase, new()
    {
        protected QueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

        #region AssertAny

        protected Task AssertAny<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query)
            where TItem1 : class
            => AssertAny(isAsync, query, query);

        protected Task AssertAny<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAny(actualQuery, expectedQuery, isAsync);

        protected Task AssertAny<TItem1, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TResult>> query)
            where TItem1 : class
            => AssertAny(isAsync, query, query);

        protected Task AssertAny<TItem1, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAny(actualQuery, expectedQuery, isAsync);

        protected Task AssertAny<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query)
            where TItem1 : class
            where TItem2 : class
            => AssertAny(isAsync, query, query);

        protected Task AssertAny<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertAny(actualQuery, expectedQuery, isAsync);

        protected Task AssertAny<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertAny(isAsync, query, query);

        protected Task AssertAny<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => Fixture.QueryAsserter.AssertAny(actualQuery, expectedQuery, isAsync);

        protected Task AssertAny<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate)
            where TItem1 : class
            => AssertAny(isAsync, query, query, predicate, predicate);

        protected Task AssertAny<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAny(actualQuery, expectedQuery, actualPredicate, expectedPredicate, isAsync);

        #endregion

        #region AssertAll

        protected Task AssertAll<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate)
            => Fixture.QueryAsserter.AssertAll(
                query, query, predicate, predicate, isAsync);

        protected Task AssertAll<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate)
            => Fixture.QueryAsserter.AssertAll(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, isAsync);

        #endregion

        #region AssertFirst

        protected Task AssertFirst<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirst(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertFirst<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirst(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirst<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirst(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertFirst<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirst(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertFirstOrDefault

        protected Task AssertFirstOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirstOrDefault(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirstOrDefault(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirstOrDefault(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertFirstOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertSingle

        protected Task AssertSingle<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingle(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertSingle<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingle(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertSingle<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingle(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertSingle<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingle(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertSingleOrDefault

        protected Task AssertSingleOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingleOrDefault(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertSingleOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingleOrDefault(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertSingleOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingleOrDefault(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertSingleOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertSingleOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertLast

        protected Task AssertLast<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLast(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertLast<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLast(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertLast<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLast(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertLast<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLast(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertLastOrDefault

        protected Task AssertLastOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLastOrDefault(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertLastOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLastOrDefault(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertLastOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLastOrDefault(
                query, query, predicate, predicate, asserter, entryCount, isAsync);

        protected Task AssertLastOrDefault<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertLastOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertCount

        protected Task AssertCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query)
            => Fixture.QueryAsserter.AssertCount(query, query, isAsync);

        protected Task AssertCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery)
            => Fixture.QueryAsserter.AssertCount(actualQuery, expectedQuery, isAsync);

        protected Task AssertCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, bool>> predicate)
            => Fixture.QueryAsserter.AssertCount(
                query, query, predicate, predicate, isAsync);

        protected Task AssertCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate)
            => Fixture.QueryAsserter.AssertCount(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, isAsync);

        #endregion

        #region AssertLongCount

        protected Task AssertLongCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query)
            => Fixture.QueryAsserter.AssertLongCount(query, query, isAsync);

        protected Task AssertLongCount<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery)
            => Fixture.QueryAsserter.AssertLongCount(actualQuery, expectedQuery, isAsync);

        #endregion

        #region AssertMin

        protected Task AssertMin<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMin(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertMin<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMin(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMin<TResult, TSelector>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, TSelector>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMin(
                query, query, selector, selector, asserter, entryCount, isAsync);

        protected Task AssertMin<TResult, TSelector>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMin(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        #endregion

        #region AssertMax

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                query, query, asserter, entryCount, isAsync);

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, int>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                query, query, selector, selector, asserter, entryCount, isAsync);

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMax<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, decimal>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            => Fixture.QueryAsserter.AssertMax(
                query, query, selector, selector, asserter, entryCount, isAsync);

        #endregion

        #region AssertSum

        protected Task AssertSum(
            bool isAsync,
            Func<ISetSource, IQueryable<int>> query,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(query, query, asserter, isAsync);

        protected Task AssertSum(
            bool isAsync,
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertSum(
            bool isAsync,
            Func<ISetSource, IQueryable<int?>> query,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(query, query, asserter, isAsync);

        protected Task AssertSum(
            bool isAsync,
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, int>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, int?>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, long>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, long?>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, decimal>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, float>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertSum(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        #endregion

        #region AssertAverage

        protected Task AssertAverage(
            bool isAsync,
            Func<ISetSource, IQueryable<int>> query,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(query, query, asserter, isAsync);

        protected Task AssertAverage(
            bool isAsync,
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage(
            bool isAsync,
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage(
            bool isAsync,
            Func<ISetSource, IQueryable<long>> query,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(query, query, asserter, isAsync);

        protected Task AssertAverage(
            bool isAsync,
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, int>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, int?>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, decimal>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Expression<Func<TResult, float>> selector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                query, query, selector, selector, asserter, isAsync);

        protected Task AssertAverage<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<object, object> asserter = null)
            => Fixture.QueryAsserter.AssertAverage(
                actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        #endregion

        #region AssertQuery

        public Task AssertQuery<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> query,
            Func<TResult, object> elementSorter = null,
            Action<TResult, TResult> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TResult : class
            => Fixture.QueryAsserter.AssertQuery(
                query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        public Task AssertQuery<TResult>(
            bool isAsync,
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Func<TResult, object> elementSorter = null,
            Action<TResult, TResult> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TResult : class
            => Fixture.QueryAsserter.AssertQuery(
                actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        #endregion

        #region AssertQueryScalar

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<double>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<uint>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<uint>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<uint>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar<TItem1, uint>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<short>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<bool>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar<TItem1, bool>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<DateTime>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<DateTimeOffset>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TimeSpan>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TResult : struct
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, bool>(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, bool>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, DateTime>(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, DateTime>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        #endregion

        #region AssertQueryScalar - nullable

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<uint?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<double?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<double?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<double?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, double>(isAsync, actualQuery, expectedQuery, assertOrder);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TimeSpan?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<DateTime?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<bool?>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertQueryScalar<TItem1, bool>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> query,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(isAsync, query, query, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(isAsync, actualQuery, expectedQuery, assertOrder, testMethodName);

        public Task AssertQueryScalar<TItem1, TItem2, TResult>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        #endregion

        #region AssertIncludeQuery

        public Task<List<object>> AssertIncludeQuery<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertIncludeQuery(
                isAsync, query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, testMethodName);

        public Task<List<object>> AssertIncludeQuery<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertIncludeQuery(
                actualQuery, expectedQuery, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, isAsync,
                testMethodName);

        public Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => AssertIncludeQuery(
                isAsync, query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, testMethodName);

        public Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertIncludeQuery(
                actualQuery, expectedQuery, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, isAsync,
                testMethodName);

        #endregion

        #region AssertSingleResult

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, object> syncQuery,
            Func<IQueryable<TItem1>, Task<object>> asyncQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertSingleResult(isAsync, syncQuery, asyncQuery, syncQuery, asserter, entryCount, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleResult(
                actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, entryCount, isAsync, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, int> syncQuery,
            Func<IQueryable<TItem1>, Task<int>> asyncQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertSingleResult(isAsync, syncQuery, asyncQuery, syncQuery, asserter, entryCount, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, int> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<int>> actualAsyncQuery,
            Func<IQueryable<TItem1>, int> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleResult(
                actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, entryCount, isAsync, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, long> syncQuery,
            Func<IQueryable<TItem1>, Task<long>> asyncQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertSingleResult(isAsync, syncQuery, asyncQuery, syncQuery, asserter, entryCount, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, long> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<long>> actualAsyncQuery,
            Func<IQueryable<TItem1>, long> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleResult(
                actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, entryCount, isAsync, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, bool> syncQuery,
            Func<IQueryable<TItem1>, Task<bool>> asyncQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => AssertSingleResult(isAsync, syncQuery, asyncQuery, syncQuery, asserter, entryCount, testMethodName);

        protected Task AssertSingleResult<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, bool> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<bool>> actualAsyncQuery,
            Func<IQueryable<TItem1>, bool> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleResult(
                actualSyncQuery, actualAsyncQuery, expectedQuery, asserter, entryCount, isAsync, testMethodName);

        #endregion

        #region Helpers - Sorters

        public static Func<dynamic, dynamic> GroupingSorter<TKey, TElement>()
            => e => ((IGrouping<TKey, TElement>)e).Key + " " + CollectionSorter<TElement>()(e);

        public static Func<dynamic, dynamic> CollectionSorter<TElement>()
            => e => ((IEnumerable<TElement>)e).Count();

        #endregion

        #region Helpers - Asserters

        public void AssertEqual<T>(T expected, T actual, Action<dynamic, dynamic> asserter = null)
            => Fixture.QueryAsserter.AssertEqual(expected, actual, asserter);

        public void AssertCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            bool ordered = false,
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null)
            => Fixture.QueryAsserter.AssertCollection(expected, actual, ordered, elementSorter, elementAsserter);

        public static Action<dynamic, dynamic> GroupingAsserter<TKey, TElement>(
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null)
        {
            return (e, a) =>
            {
                Assert.Equal(((IGrouping<TKey, TElement>)e).Key, ((IGrouping<TKey, TElement>)a).Key);
                CollectionAsserter(elementSorter, elementAsserter)(e, a);
            };
        }

        public static Action<dynamic, dynamic> CollectionAsserter<TElement>(
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null)
        {
            return (e, a) =>
            {
                if (e == null && a == null)
                {
                    return;
                }

                var actual = elementSorter != null
                    ? ((IEnumerable<TElement>)a).OrderBy(elementSorter).ToList()
                    : ((IEnumerable<TElement>)a).ToList();

                var expected = elementSorter != null
                    ? ((IEnumerable<TElement>)e).OrderBy(elementSorter).ToList()
                    : ((IEnumerable<TElement>)e).ToList();

                Assert.Equal(expected.Count, actual.Count);
                elementAsserter ??= Assert.Equal;

                for (var i = 0; i < expected.Count; i++)
                {
                    elementAsserter(expected[i], actual[i]);
                }
            };
        }

        #endregion

        #region Helpers - Maybe

        protected static TResult Maybe<TResult>(object caller, Func<TResult> expression)
            where TResult : class
        {
            return caller == null ? null : expression();
        }

        protected static TResult? MaybeScalar<TResult>(object caller, Func<TResult?> expression)
            where TResult : struct
        {
            return caller == null ? null : expression();
        }

        protected static IEnumerable<TResult> MaybeDefaultIfEmpty<TResult>(IEnumerable<TResult> caller)
            where TResult : class
        {
            return caller == null
                ? new List<TResult> { default }
                : caller.DefaultIfEmpty();
        }

        #endregion
    }
}
