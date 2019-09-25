// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryAsserter<TContext> : QueryAsserterBase
        where TContext : DbContext
    {
        private readonly Func<TContext> _contextCreator;
        private readonly Dictionary<Type, object> _entitySorters;
        private readonly Dictionary<Type, object> _entityAsserters;
        private readonly IncludeQueryResultAsserter _includeResultAsserter;

        private const bool ProceduralQueryGeneration = false;

        public QueryAsserter(
            Func<TContext> contextCreator,
            ISetSource expectedData,
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
        {
            _contextCreator = contextCreator;
            ExpectedData = expectedData;
            _entitySorters = entitySorters ?? new Dictionary<Type, object>();
            _entityAsserters = entityAsserters ?? new Dictionary<Type, object>();

            SetSourceCreator = ctx => new DefaultSetSource(ctx);
            _includeResultAsserter = new IncludeQueryResultAsserter(_entitySorters, _entityAsserters);
        }

        public override void AssertEqual<T>(T expected, T actual, Action<dynamic, dynamic> asserter = null)
        {
            if (asserter == null && expected != null)
            {
                _entityAsserters.TryGetValue(expected.GetType(), out var entityAsserter);
                asserter ??= (Action<dynamic, dynamic>)entityAsserter;
            }

            asserter ??= Assert.Equal;
            asserter(expected, actual);
        }

        public override void AssertCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            bool ordered = false)
        {
            if (expected == null !=  (actual == null))
            {
                throw new InvalidOperationException(
                    $"Nullability doesn't match. Expected: {(expected == null ? "NULL" : "NOT NULL")}. Actual: {(actual == null ? "NULL." : "NOT NULL.")}.");
            }

            Func<TElement, object> elementSorter;
            Action<TElement, TElement> elementAsserter;

            _entitySorters.TryGetValue(typeof(TElement), out var sorter);
            _entityAsserters.TryGetValue(typeof(TElement), out var asserter);

            elementSorter = (Func<TElement, object>)sorter;
            elementAsserter = (Action<TElement, TElement>)asserter ?? Assert.Equal;

            if (!ordered)
            {
                if (elementSorter != null)
                {
                    var sortedActual = actual.OrderBy(elementSorter).ToList();
                    var sortedExpected = expected.OrderBy(elementSorter).ToList();
                    
                    Assert.Equal(sortedExpected.Count, sortedActual.Count);
                    for (var i = 0; i < sortedExpected.Count; i++)
                    {
                        elementAsserter(sortedExpected[i], sortedActual[i]);
                    }
                }
                else
                {
                    var sortedActual = actual.OrderBy(e => e).ToList();
                    var sortedExpected = expected.OrderBy(e => e).ToList();

                    Assert.Equal(sortedExpected.Count, sortedActual.Count);
                    for (var i = 0; i < sortedExpected.Count; i++)
                    {
                        elementAsserter(sortedExpected[i], sortedActual[i]);
                    }
                }
            }
            else
            {
                var expectedList = expected.ToList();
                var actualList = actual.ToList();

                Assert.Equal(expectedList.Count, actualList.Count);
                for (var i = 0; i < expectedList.Count; i++)
                {
                    elementAsserter(expectedList[i], actualList[i]);
                }
            }
        }

        #region AssertSingleResult

        public override async Task AssertSingleResultTyped<TResult>(
            Func<ISetSource, TResult> actualSyncQuery,
            Func<ISetSource, Task<TResult>> actualAsyncQuery,
            Func<ISetSource, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            using (var context = _contextCreator())
            {
                TResult actual;

                if (isAsync)
                {
                    actual = await actualAsyncQuery(SetSourceCreator(context));
                }
                else
                {
                    actual = actualSyncQuery(SetSourceCreator(context));
                }

                var expected = expectedQuery(ExpectedData);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override Task AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, object> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>());
            Func<ISetSource, Task<object>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>());
            Func<ISetSource, object> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertSingleResult<TItem1, TResult>(
            Func<IQueryable<TItem1>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, TResult> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>());
            Func<ISetSource, Task<TResult>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>());
            Func<ISetSource, TResult> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, object> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, Task<object>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, object> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertSingleResult<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, TResult> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, Task<TResult>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, TResult> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, object> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, Task<object>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, object> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertSingleResult<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, TResult> setSourceActualSyncQuery = ss => actualSyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, Task<TResult>> setSourceActualAsyncQuery = ss => actualAsyncQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, TResult> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());

            return AssertSingleResultTyped(
                setSourceActualSyncQuery,
                setSourceActualAsyncQuery,
                setSourceExpectedQuery,
                asserter,
                entryCount,
                isAsync,
                testMethodName);
        }

        #endregion

        #region AssertQuery

        private void OrderingSettingsVerifier(bool assertOrder, Type type)
            => OrderingSettingsVerifier(assertOrder, type, elementSorter: null);

        private void OrderingSettingsVerifier(bool assertOrder, Type type, object elementSorter)
        {
            if (!assertOrder
                && type.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>)
                    || type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>)))
            {
                throw new InvalidOperationException(
                    "Query result is OrderedQueryable - you need to set AssertQuery option: 'assertOrder' to 'true'. If the resulting order is non-deterministic by design, add identity projection to the top of the query to disable this check.");
            }

            if (assertOrder && elementSorter != null)
            {
                throw new InvalidOperationException("You shouldn't apply element sorter when 'assertOrder' is set to 'true'.");
            }
        }

        public override async Task AssertQueryTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Func<TResult, object> elementSorter,
            Action<TResult, TResult> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            using (var context = _contextCreator())
            {
                var query = actualQuery(SetSourceCreator(context));
                if (ProceduralQueryGeneration && !isAsync)
                {
                    new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                    return;
                }

                OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter);

                var actual = isAsync
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                var expected = expectedQuery(ExpectedData).ToArray();

                // TODO: temporary, use typeof(TResult) once tests are converted to new pattern
                var firstNonNullableElement = expected.FirstOrDefault(e => e != null);
                if (firstNonNullableElement != null)
                {
                    if (!assertOrder && elementSorter == null)
                    {
                        _entitySorters.TryGetValue(firstNonNullableElement.GetType(), out var sorter);
                        elementSorter = (Func<TResult, object>)sorter;
                    }

                    if (elementAsserter == null)
                    {
                        _entityAsserters.TryGetValue(firstNonNullableElement.GetType(), out var asserter);
                        elementAsserter = (Action<TResult, TResult>)asserter;
                    }
                }

                //if (!assertOrder && elementSorter == null)
                //{
                //    _entitySorters.TryGetValue(typeof(TResult), out var sorter);
                //    elementSorter = sorter;
                //}

                //if (elementAsserter == null)
                //{
                //    _entityAsserters.TryGetValue(typeof(TResult), out var asserter);
                //    elementAsserter = asserter;
                //}

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter,
                    elementAsserter,
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override Task AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter,
            Action<dynamic, dynamic> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertQueryTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                elementSorter,
                elementAsserter,
                assertOrder,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter,
            Action<dynamic, dynamic> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertQueryTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                elementSorter,
                elementAsserter,
                assertOrder,
                entryCount,
                isAsync,
                testMethodName);
        }

        public override Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter,
            Action<dynamic, dynamic> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());

            return AssertQueryTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                elementSorter,
                elementAsserter,
                assertOrder,
                entryCount,
                isAsync,
                testMethodName);
        }

        #endregion

        #region AssertQueryScalar

        public virtual Task AssertQueryScalarTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct
            => AssertQueryScalarTyped(query, query, assertOrder, isAsync, testMethodName);

        public virtual async Task AssertQueryScalarTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct
        {
            using (var context = _contextCreator())
            {
                var query = actualQuery(SetSourceCreator(context));
                if (ProceduralQueryGeneration && !isAsync)
                {
                    new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                    return;
                }

                OrderingSettingsVerifier(assertOrder, query.Expression.Type);

                var actual = isAsync
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                var expected = expectedQuery(ExpectedData).ToArray();

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // one argument

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<short>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalarAsync<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalarAsync<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TResult : struct
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public override Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<TResult>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<TResult>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertQueryScalarTyped(setSourceActualQuery, setSourceExpectedQuery, assertOrder, isAsync, testMethodName);
        }

        // two arguments

        public virtual Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public override Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<TResult>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, IQueryable<TResult>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertQueryScalarTyped(setSourceActualQuery, setSourceExpectedQuery, assertOrder, isAsync, testMethodName);
        }

        // three arguments

        public virtual Task AssertQueryScalar<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public override Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<TResult>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, IQueryable<TResult>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());

            return AssertQueryScalarTyped(setSourceActualQuery, setSourceExpectedQuery, assertOrder, isAsync, testMethodName);
        }

        #endregion

        #region AssertQueryNullableScalar

        public virtual Task AssertQueryScalarTyped<TResult>(
            Func<ISetSource, IQueryable<TResult?>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct
            => AssertQueryScalarTyped(query, query, assertOrder, isAsync, testMethodName);

        public virtual async Task AssertQueryScalarTyped<TResult>(
            Func<ISetSource, IQueryable<TResult?>> actualQuery,
            Func<ISetSource, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct
        {
            using (var context = _contextCreator())
            {
                var query = actualQuery(SetSourceCreator(context));
                if (ProceduralQueryGeneration && !isAsync)
                {
                    new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                    return;
                }

                OrderingSettingsVerifier(assertOrder, query.Expression.Type);

                var actual = isAsync
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                var expected = expectedQuery(ExpectedData).ToArray();

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // one argument

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public override Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<TResult?>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<TResult?>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertQueryScalarTyped(setSourceActualQuery, setSourceExpectedQuery, assertOrder, isAsync, testMethodName);
        }

        // two arguments

        public virtual Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> query,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder, isAsync, testMethodName);

        public virtual Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder, isAsync, testMethodName);

        public override Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<TResult?>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, IQueryable<TResult?>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertQueryScalarTyped(setSourceActualQuery, setSourceExpectedQuery, assertOrder, isAsync, testMethodName);
        }

        #endregion

        #region AssertIncludeQuery

        public Task<List<TResult>> AssertIncludeQueryTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<TResult, object> elementSorter,
            List<Func<TResult, TResult>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TResult : class
            => AssertIncludeQueryTyped(query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, isAsync, testMethodName);

        public async Task<List<TResult>> AssertIncludeQueryTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<TResult, object> elementSorter,
            List<Func<TResult, TResult>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            using (var context = _contextCreator())
            {
                var query = actualQuery(SetSourceCreator(context));
                if (ProceduralQueryGeneration && !isAsync)
                {
                    new ProcedurallyGeneratedQueryExecutor().Execute(query, context, testMethodName);

                    return default;
                }

                OrderingSettingsVerifier(assertOrder, query.Expression.Type, elementSorter);

                var actual = isAsync
                    ? await query.ToListAsync()
                    : query.ToList();

                var expected = expectedQuery(ExpectedData).ToList();

                if (!assertOrder)
                {
                    if (elementSorter == null)
                    {
                        var firstNonNullableElement = expected.FirstOrDefault(e => e != null);
                        if (firstNonNullableElement != null)
                        {
                            _entitySorters.TryGetValue(firstNonNullableElement.GetType(), out var sorter);
                            elementSorter = (Func<TResult, object>)sorter;
                        }
                    }

                    if (elementSorter != null)
                    {
                        actual = actual.OrderBy(elementSorter).ToList();
                        expected = expected.OrderBy(elementSorter).ToList();
                    }
                }

                if (clientProjections != null)
                {
                    foreach (var clientProjection in clientProjections)
                    {
                        var projectedActual = actual.Select(clientProjection).ToList();
                        var projectedExpected = expected.Select(clientProjection).ToList();

                        _includeResultAsserter.AssertResult(projectedExpected, projectedActual, expectedIncludes);
                    }
                }
                else
                {
                    _includeResultAsserter.AssertResult(expected, actual, expectedIncludes);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());

                return actual;
            }
        }

        public Task<List<object>> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            => AssertIncludeQuery(
                query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, isAsync, testMethodName);

        public override Task<List<object>> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertIncludeQueryTyped<object>(
                setSourceActualQuery,
                setSourceExpectedQuery,
                expectedIncludes,
                elementSorter,
                clientProjections,
                assertOrder,
                entryCount,
                isAsync,
                testMethodName);
        }

        public Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            => AssertIncludeQuery(
                query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount, isAsync, testMethodName);

        public override Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertIncludeQueryTyped<object>(
                setSourceActualQuery,
                setSourceExpectedQuery,
                expectedIncludes,
                elementSorter,
                clientProjections,
                assertOrder,
                entryCount,
                isAsync,
                testMethodName);
        }

        #endregion

        #region AssertAny

        public override async Task AssertAnyTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AnyAsync()
                    : actualQuery(SetSourceCreator(context)).Any();

                var expected = expectedQuery(ExpectedData).Any();

                Assert.Equal(expected, actual);
            }
        }

        public override Task AssertAny<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertAnyTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                isAsync);
        }

        public override Task AssertAny<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
        {
            Func<ISetSource, IQueryable<TResult>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<TResult>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertAnyTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                isAsync);
        }

        public override Task AssertAny<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>());

            return AssertAnyTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                isAsync);
        }

        public override Task AssertAny<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
        {
            Func<ISetSource, IQueryable<object>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());
            Func<ISetSource, IQueryable<object>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>(), ss.Set<TItem2>(), ss.Set<TItem3>());

            return AssertAnyTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                isAsync);
        }

        public override async Task AssertAnyTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AnyAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).Any(actualPredicate);

                var expected = expectedQuery(ExpectedData).Any(expectedPredicate);

                Assert.Equal(expected, actual);
            }


            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AnyAsync()
                    : actualQuery(SetSourceCreator(context)).Any();

                var expected = expectedQuery(ExpectedData).Any();

                Assert.Equal(expected, actual);
            }
        }

        public override Task AssertAny<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
        {
            Func<ISetSource, IQueryable<TPredicate>> setSourceActualQuery = ss => actualQuery(ss.Set<TItem1>());
            Func<ISetSource, IQueryable<TPredicate>> setSourceExpectedQuery = ss => expectedQuery(ss.Set<TItem1>());

            return AssertAnyTyped(
                setSourceActualQuery,
                setSourceExpectedQuery,
                actualPredicate,
                expectedPredicate,
                isAsync);
        }

        #endregion

        #region AssertAll

        public override async Task AssertAll<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AllAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).All(actualPredicate);

                var expected = expectedQuery(ExpectedData).All(expectedPredicate);

                Assert.Equal(expected, actual);
            }
        }

        #endregion

        #region AssertFirst

        public override async Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).FirstAsync()
                    : actualQuery(SetSourceCreator(context)).First();

                var expected = expectedQuery(ExpectedData).First();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualFirstPredicate,
            Expression<Func<TResult, bool>> expectedFirstPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).FirstAsync(actualFirstPredicate)
                    : actualQuery(SetSourceCreator(context)).First(actualFirstPredicate);

                var expected = expectedQuery(ExpectedData).First(expectedFirstPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertFirstOrDefault

        public override async Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).FirstOrDefaultAsync()
                    : actualQuery(SetSourceCreator(context)).FirstOrDefault();

                var expected = expectedQuery(ExpectedData).FirstOrDefault();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).FirstOrDefaultAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).FirstOrDefault(actualPredicate);

                var expected = expectedQuery(ExpectedData).FirstOrDefault(expectedPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertSingle

        public override async Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SingleAsync()
                    : actualQuery(SetSourceCreator(context)).Single();

                var expected = expectedQuery(ExpectedData).Single();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualFirstPredicate,
            Expression<Func<TResult, bool>> expectedFirstPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SingleAsync(actualFirstPredicate)
                    : actualQuery(SetSourceCreator(context)).Single(actualFirstPredicate);

                var expected = expectedQuery(ExpectedData).Single(expectedFirstPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertSingleOrDefault

        public override async Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SingleOrDefaultAsync()
                    : actualQuery(SetSourceCreator(context)).SingleOrDefault();

                var expected = expectedQuery(ExpectedData).SingleOrDefault();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SingleOrDefaultAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).SingleOrDefault(actualPredicate);

                var expected = expectedQuery(ExpectedData).SingleOrDefault(expectedPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertLast

        public override async Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LastAsync()
                    : actualQuery(SetSourceCreator(context)).Last();

                var expected = expectedQuery(ExpectedData).Last();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LastAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).Last(actualPredicate);

                var expected = expectedQuery(ExpectedData).Last(expectedPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertLastOrDefault

        public override async Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LastOrDefaultAsync()
                    : actualQuery(SetSourceCreator(context)).LastOrDefault();

                var expected = expectedQuery(ExpectedData).LastOrDefault();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LastOrDefaultAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).LastOrDefault(actualPredicate);

                var expected = expectedQuery(ExpectedData).LastOrDefault(expectedPredicate);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertCount

        public override async Task AssertCount<TResult>(
            Func<ISetSource , IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).CountAsync()
                    : actualQuery(SetSourceCreator(context)).Count();

                var expected = expectedQuery(ExpectedData).Count();

                Assert.Equal(expected, actual);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).CountAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).Count(actualPredicate);

                var expected = expectedQuery(ExpectedData).Count(expectedPredicate);

                Assert.Equal(expected, actual);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        #endregion

        #region AssertLongCount

        public override async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LongCountAsync()
                    : actualQuery(SetSourceCreator(context)).LongCount();

                var expected = expectedQuery(ExpectedData).LongCount();

                Assert.Equal(expected, actual);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).LongCountAsync(actualPredicate)
                    : actualQuery(SetSourceCreator(context)).LongCount(actualPredicate);

                var expected = expectedQuery(ExpectedData).LongCount(expectedPredicate);

                Assert.Equal(expected, actual);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        #endregion

        #region AssertMin

        public override async Task AssertMin<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).MinAsync()
                    : actualQuery(SetSourceCreator(context)).Min();

                var expected = expectedQuery(ExpectedData).Min();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertMin<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).MinAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Min(actualSelector);

                var expected = expectedQuery(ExpectedData).Min(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertMax

        public override async Task AssertMax<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).MaxAsync()
                    : actualQuery(SetSourceCreator(context)).Max();

                var expected = expectedQuery(ExpectedData).Max();

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        public override async Task AssertMax<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).MaxAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Max(actualSelector);

                var expected = expectedQuery(ExpectedData).Max(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertSum

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync()
                    : actualQuery(SetSourceCreator(context)).Sum();

                var expected = expectedQuery(ExpectedData).Sum();

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync()
                    : actualQuery(SetSourceCreator(context)).Sum();

                var expected = expectedQuery(ExpectedData).Sum();

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult,  float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).SumAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Sum(actualSelector);

                var expected = expectedQuery(ExpectedData).Sum(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        #endregion

        #region AssertAverage

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                    : actualQuery(SetSourceCreator(context)).Average();

                var expected = expectedQuery(ExpectedData).Average();

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                    : actualQuery(SetSourceCreator(context)).Average();

                var expected = expectedQuery(ExpectedData).Average();

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync()
                    : actualQuery(SetSourceCreator(context)).Average();

                var expected = expectedQuery(ExpectedData).Average();

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Average(actualSelector);

                var expected = expectedQuery(ExpectedData).Average(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Average(actualSelector);

                var expected = expectedQuery(ExpectedData).Average(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Average(actualSelector);

                var expected = expectedQuery(ExpectedData).Average(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        public override async Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetSourceCreator(context)).AverageAsync(actualSelector)
                    : actualQuery(SetSourceCreator(context)).Average(actualSelector);

                var expected = expectedQuery(ExpectedData).Average(expectedSelector);

                AssertEqual(expected, actual, asserter);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        #endregion

        private class DefaultSetSource : ISetSource
        {
            private readonly DbContext _context;
            public DefaultSetSource(DbContext context)
            {
                _context = context;
            }

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
                => _context.Set<TEntity>();
        }
    }
}
