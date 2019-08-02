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

        public static IEnumerable<object[]> IsAsyncData = new[]
        {
            new object[] { false },
            new object[] { true }
        };

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

        protected Task AssertAll<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate)
            where TItem1 : class
            => AssertAll(isAsync, query, query, predicate, predicate);

        protected Task AssertAll<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAll(actualQuery, expectedQuery, actualPredicate, expectedPredicate, isAsync);

        #endregion

        #region AssertFirst

        protected Task AssertFirst<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertFirst(isAsync, query, query, asserter, entryCount);

        protected Task AssertFirst<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertFirst(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirst<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertFirst(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertFirst<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertFirst(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertFirstOrDefault

        protected Task AssertFirstOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertFirstOrDefault(isAsync, query, query, asserter, entryCount);

        protected Task AssertFirstOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertFirstOrDefault(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertFirstOrDefault(isAsync, query, query, asserter, entryCount);

        protected Task AssertFirstOrDefault<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertFirstOrDefault(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertFirstOrDefault(isAsync, query, query, asserter, entryCount);

        protected Task AssertFirstOrDefault<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => Fixture.QueryAsserter.AssertFirstOrDefault(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertFirstOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertFirstOrDefault(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertFirstOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertFirstOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertSingle

        protected Task AssertSingle<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingle(isAsync, query, query, asserter, entryCount);

        protected Task AssertSingle<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingle(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertSingle<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertSingle(isAsync, query, query, asserter, entryCount);

        protected Task AssertSingle<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertSingle(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertSingle<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingle(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertSingle<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingle(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertSingleOrDefault

        protected Task AssertSingleOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingleOrDefault(isAsync, query, query, asserter, entryCount);

        protected Task AssertSingleOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleOrDefault(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertSingleOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingleOrDefault(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertSingleOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertLast

        protected Task AssertLast<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertLast(isAsync, query, query, asserter, entryCount);

        protected Task AssertLast<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertLast(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertLast<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertLast(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertLast<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertLast(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertLastOrDefault

        protected Task AssertLastOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertLastOrDefault(isAsync, query, query, asserter, entryCount);

        protected Task AssertLastOrDefault<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertLastOrDefault(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertLastOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertLastOrDefault(isAsync, query, query, predicate, predicate, asserter, entryCount);

        protected Task AssertLastOrDefault<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertLastOrDefault(
                actualQuery, expectedQuery, actualPredicate, expectedPredicate, asserter, entryCount, isAsync);

        #endregion

        #region AssertCount

        protected Task AssertCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query)
            where TItem1 : class
            => AssertCount(isAsync, query, query);

        protected Task AssertCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertCount(actualQuery, expectedQuery, isAsync);

        protected Task AssertCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool>> query)
            where TItem1 : class
            => AssertCount(isAsync, query, query);

        protected Task AssertCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<bool>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<bool>> expectedQuery)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertCount(actualQuery, expectedQuery, isAsync);

        protected Task AssertCount<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> query,
            Expression<Func<TPredicate, bool>> predicate)
            where TItem1 : class
            => AssertCount(isAsync, query, query, predicate, predicate);

        protected Task AssertCount<TItem1, TPredicate>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertCount(actualQuery, expectedQuery, actualPredicate, expectedPredicate, isAsync);

        protected Task AssertCount<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query)
            where TItem1 : class
            where TItem2 : class
            => AssertCount(isAsync, query, query);

        protected Task AssertCount<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertCount(actualQuery, expectedQuery, isAsync);

        #endregion

        #region AssertLongCount

        protected Task AssertLongCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query)
            where TItem1 : class
            => AssertLongCount(isAsync, query, query);

        protected Task AssertLongCount<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertLongCount(actualQuery, expectedQuery, isAsync);

        protected Task AssertLongCount<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query)
            where TItem1 : class
            where TItem2 : class
            => AssertLongCount(isAsync, query, query);

        protected Task AssertLongCount<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertLongCount(actualQuery, expectedQuery, isAsync);

        #endregion

        #region AssertMin

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, asserter, entryCount);

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, asserter, entryCount);

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, asserter, entryCount);

        protected Task AssertMin<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int?>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, decimal>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMin(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMin<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMin(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        #endregion

        #region AssertMax

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, asserter, entryCount);

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, asserter, entryCount);

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, asserter, entryCount);

        protected Task AssertMax<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, asserter, entryCount, isAsync);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int?>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, decimal>> selector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertMax(isAsync, query, query, selector, selector, asserter, entryCount);

        protected Task AssertMax<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertMax(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, entryCount, isAsync);

        #endregion

        #region AssertSum

        protected Task AssertSum<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertSum(isAsync, query, query, asserter);

        protected Task AssertSum<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertSum<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int?>> query,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertSum(isAsync, query, query, asserter);

        protected Task AssertSum<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int?>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, decimal>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, float>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, float>> actualSelector,
            Expression<Func<TSelector, float>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertSum<TItem1, TItem2, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertSum(query, query, selector, selector, asserter, isAsync);

        protected Task AssertSum<TItem1, TItem2, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertSum(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        #endregion

        #region AssertAverage

        protected Task AssertAverage<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query);

        protected Task AssertAverage<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query);

        protected Task AssertAverage<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<long>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, asserter, isAsync);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query, selector, selector, asserter);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, int?>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query, selector, selector, asserter);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, decimal>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query, selector, selector, asserter);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> query,
            Expression<Func<TSelector, float>> selector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => AssertAverage(isAsync, query, query, selector, selector, asserter);

        protected Task AssertAverage<TItem1, TSelector>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, float>> actualSelector,
            Expression<Func<TSelector, float>> expectedSelector,
            Action<object, object> asserter = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertAverage(actualQuery, expectedQuery, actualSelector, expectedSelector, asserter, isAsync);

        #endregion

        #region AssertQuery

        public Task AssertQuery<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertQuery(
                query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        public Task AssertQuery<TItem1>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertQuery(
                actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        public Task AssertQuery<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertQuery(
                query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        public Task AssertQuery<TItem1, TItem2>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertQuery(
                actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount, isAsync, testMethodName);

        public Task AssertQuery<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQuery(isAsync, query, query, elementSorter, elementAsserter, assertOrder, entryCount, testMethodName);

        public Task AssertQuery<TItem1, TItem2, TItem3>(
            bool isAsync,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            [CallerMemberName] string testMethodName = null)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
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

        public static Action<dynamic, dynamic> GroupingAsserter<TKey, TElement>(
            Func<TElement, object> elementSorter = null, Action<TElement, TElement> elementAsserter = null)
        {
            return (e, a) =>
            {
                Assert.Equal(((IGrouping<TKey, TElement>)e).Key, ((IGrouping<TKey, TElement>)a).Key);
                CollectionAsserter(elementSorter, elementAsserter)(e, a);
            };
        }

        public static Action<dynamic, dynamic> CollectionAsserter<TElement>(
            Func<TElement, object> elementSorter = null, Action<TElement, TElement> elementAsserter = null)
        {
            return (e, a) =>
            {
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
                ? new List<TResult>
                    { default }
                : caller.DefaultIfEmpty();
        }

        #endregion
    }
}
