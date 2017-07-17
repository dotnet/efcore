// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryAsserter<TContext> : QueryAsserterBase
        where TContext : DbContext
    {
        private readonly Func<TContext> _contextCreator;
        private readonly Dictionary<Type, Func<dynamic, object>> _entitySorters;
        private readonly Dictionary<Type, Action<dynamic, dynamic>> _entityAsserters;
        private IncludeQueryResultAsserter _includeResultAsserter;

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

        public virtual void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount);

        public override void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(SetExtractor.Set<TItem1>(context)).ToArray();
                var expected = expectedQuery(ExpectedData.Set<TItem1>()).ToArray();

                if (!assertOrder && elementSorter == null && expected.Length > 0 && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // two arguments

        public virtual void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount);

        public override void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(), 
                    ExpectedData.Set<TItem2>()).ToArray();

                if (!assertOrder && elementSorter == null && expected.Length > 0 && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        // three arguments
        public virtual void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount);

        public override void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context),
                    SetExtractor.Set<TItem3>(context)).ToArray();

                var expected = expectedQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>(),
                    ExpectedData.Set<TItem3>()).ToArray();

                if (!assertOrder && elementSorter == null && expected.Length > 0 && expected[0] != null)
                {
                    _entitySorters.TryGetValue(expected[0].GetType(), out elementSorter);
                }

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region AssertQueryScalar

        // one argument

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder);

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<short>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TResult : struct
            => AssertQueryScalar(query, query, assertOrder);

        public override void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(SetExtractor.Set<TItem1>(context)).ToArray();
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

        public virtual void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder);

        public virtual void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder);

        public override void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
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

                TestHelpers.AssertResults(
                    expected,
                    actual,
                    e => e,
                    Assert.Equal,
                    assertOrder);
            }
        }

        // three arguments

        public virtual void AssertQueryScalar<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQueryScalar(query, query, assertOrder);

        public override void AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false)
        {
            using (var context = _contextCreator())
            {
                var actual = actualQuery(
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

        public void AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            Func<dynamic, object> clientProjection = null)
            where TItem1 : class
            => AssertIncludeQuery(query, query, expectedIncludes, elementSorter, clientProjection);

        public override void AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> l2oQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            Func<dynamic, object> clientProjection = null)
        {
            using (var context = _contextCreator())
            {
                var actual = efQuery(SetExtractor.Set<TItem1>(context)).ToList();
                var expected = l2oQuery(ExpectedData.Set<TItem1>()).ToList();

                if (elementSorter != null)
                {
                    actual = actual.OrderBy(elementSorter).ToList();
                    expected = expected.OrderBy(elementSorter).ToList();
                }

                if (clientProjection != null)
                {
                    actual = actual.Select(clientProjection).ToList();
                    expected = expected.Select(clientProjection).ToList();
                }

                _includeResultAsserter.AssertResult(expected, actual, expectedIncludes);
            }
        }

        public void AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            Func<dynamic, object> clientProjection = null)
            where TItem1 : class
            where TItem2 : class
            => AssertIncludeQuery(query, query, expectedIncludes, elementSorter, clientProjection);

        public override void AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> l2oQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            Func<dynamic, object> clientProjection = null)
        {
            using (var context = _contextCreator())
            {
                var actual = efQuery(
                    SetExtractor.Set<TItem1>(context),
                    SetExtractor.Set<TItem2>(context)).ToList();

                var expected = l2oQuery(
                    ExpectedData.Set<TItem1>(),
                    ExpectedData.Set<TItem2>()).ToList();

                if (elementSorter != null)
                {
                    actual = actual.OrderBy(elementSorter).ToList();
                    expected = expected.OrderBy(elementSorter).ToList();
                }

                if (clientProjection != null)
                {
                    actual = actual.Select(clientProjection).ToList();
                    expected = expected.Select(clientProjection).ToList();
                }

                _includeResultAsserter.AssertResult(expected, actual, expectedIncludes);
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
