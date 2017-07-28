// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryAsserter<TContext> : QueryAsserterBase
        where TContext : DbContext
    {
        private readonly Func<TContext> _contextCreator;
        private readonly Dictionary<Type, Func<dynamic, object>> _entitySorters;
        private readonly Dictionary<Type, Action<dynamic, dynamic>> _entityAsserters;
        private readonly IncludeQueryResultAsserter _includeResultAsserter;

        public QueryAsserter(
            Func<TContext> contextCreator,
            IExpectedData expectedData,
            Dictionary<Type, Func<dynamic, object>> entitySorters,
            Dictionary<Type, Action<dynamic, dynamic>> entityAsserters)
        {
            _contextCreator = contextCreator;
            ExpectedData = expectedData;

            _entitySorters = entitySorters ?? new Dictionary<Type, Func<dynamic, object>>();
            _entityAsserters = entityAsserters ?? new Dictionary<Type, Action<dynamic, dynamic>>();

            SetExtractor = new DefaultSetExtractor();
            _includeResultAsserter = new IncludeQueryResultAsserter(_entitySorters, _entityAsserters);
        }

        #region AssertSingleResult

        // one argument

        public virtual void AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        public override void AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> actualQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(SetExtractor.Set<TItem1>(context));
                var expected = expectedQuery(ExpectedData.Set<TItem1>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // two arguments

        public virtual void AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        public override void AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context));

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // three arguments

        public virtual void AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        public override void AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context),
                    SetExtractor.Set<TItem3>(context));

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>(),
                    ExpectedData.Set<TItem3>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertSingleResult - async

        // one argument

        public virtual async Task AssertSingleResult<TItem1, TResult>(
            Func<IQueryable<TItem1>, Task<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => await AssertSingleResult(query, query, asserter, entryCount);

        public override async Task AssertSingleResult<TItem1, TResult>(
            Func<IQueryable<TItem1>, Task<TResult>> actualQuery,
            Func<IQueryable<TItem1>, Task<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = await actualQuery(SetExtractor.Set<TItem1>(context));
                var expected = await expectedQuery(ExpectedData.Set<TItem1>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // two arguments

        public virtual async Task AssertSingleResult<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => await AssertSingleResult(query, query, asserter, entryCount);

        public override async Task AssertSingleResult<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = await actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context));

                var expected = await expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // three arguments

        public virtual async Task AssertSingleResult<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => await AssertSingleResult(query, query, asserter, entryCount);

        public override async Task AssertSingleResult<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = await actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context),
                    SetExtractor.Set<TItem3>(context));

                var expected = await expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>(),
                    ExpectedData.Set<TItem3>());

                if (asserter == null
                    && expected != null)
                {
                    _entityAsserters.TryGetValue(expected.GetType(), out asserter);
                }

                if (asserter != null)
                {
                    asserter(expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertQuery

        // one argument

        public virtual Task AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync);

        public override async Task AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetExtractor.Set<TItem1>(context)).ToArrayAsync()
                    : actualQuery(SetExtractor.Set<TItem1>(context)).ToArray();

                var expected = expectedQuery(ExpectedData.Set<TItem1>()).ToArray();

                if (!assertOrder
                    && elementSorter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                if (elementAsserter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entityAsserters.TryGetValue(expected[0].GetType(), out elementAsserter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter,
                    elementAsserter,
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // two arguments

        public virtual Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync);

        public override async Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context)).ToArrayAsync()
                    : actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>()).ToArray();

                if (!assertOrder
                    && elementSorter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                if (elementAsserter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entityAsserters.TryGetValue(expected[0].GetType(), out elementAsserter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter,
                    elementAsserter,
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // three arguments
        public virtual Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount, isAsync);

        public override async Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context),
                        SetExtractor.Set<TItem3>(context)).ToArrayAsync()
                    : actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context),
                        SetExtractor.Set<TItem3>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>(),
                    ExpectedData.Set<TItem3>()).ToArray();

                if (!assertOrder
                    && elementSorter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                if (elementAsserter == null
                    && expected.Length > 0
                    && expected[0] != null)
                {
                    _entityAsserters.TryGetValue(expected[0].GetType(), out elementAsserter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter,
                    elementAsserter,
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertQueryScalar

        // one argument

        public virtual async Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            => await AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<short>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TResult : struct
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public override async Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(SetExtractor.Set<TItem1>(context)).ToArrayAsync()
                    : actualQuery(SetExtractor.Set<TItem1>(context)).ToArray();

                var expected = expectedQuery(ExpectedData.Set<TItem1>()).ToArray();
                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // two arguments

        public virtual async Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public virtual async Task AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            => await AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder, isAsync);

        public override async Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context)).ToArrayAsync()
                    : actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>()).ToArray();

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // three arguments

        public virtual async Task AssertQueryScalar<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => await AssertQueryScalar(query, query, assertOrder, isAsync);

        public override async Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
        {
            using (var context = _contextCreator())
            {
                var actual = isAsync
                    ? await actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context),
                        SetExtractor.Set<TItem3>(context)).ToArrayAsync()
                    : actualQuery(
                        SetExtractor.Set<TItem1>(context),
                        SetExtractor.Set<TItem2>(context),
                        SetExtractor.Set<TItem3>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>(),
                    ExpectedData.Set<TItem3>()).ToArray();

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        #endregion

        #region AssertQueryNullableScalar

        // one argument

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder);

        public override void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(SetExtractor.Set<TItem1>(context)).ToArray();
                var expected = expectedQuery(ExpectedData.Set<TItem1>()).ToArray();
                TestHelpers.AssertResultsNullable(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // two arguments

        public virtual void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder);

        public override void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>()).ToArray();
                TestHelpers.AssertResultsNullable(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        #endregion

        #region AssertIncludeQuery

        public List<object> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => AssertIncludeQuery(query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount);

        public override List<object> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(SetExtractor.Set<TItem1>(context)).ToList();
                var expected = expectedQuery(ExpectedData.Set<TItem1>()).ToList();

                if (!assertOrder)
                {
                    if (elementSorter == null
                        && expected[0] != null)
                    {
                        _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
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

        public List<object> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertIncludeQuery(query, query, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount);

        public override List<object> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context)).ToList();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>()).ToList();

                if (!assertOrder)
                {
                    if (elementSorter == null
                        && expected[0] != null)
                    {
                        _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
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

        #endregion

        private class DefaultSetExtractor : ISetExtractor
        {
            public override IQueryable<TEntity> Set<TEntity>(DbContext context)
                => context.Set<TEntity>();
        }
    }
}
