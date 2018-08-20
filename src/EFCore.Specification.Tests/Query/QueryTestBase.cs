// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : class, IQueryFixtureBase, new()
    {
        protected QueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        #region AssertSingleResult

        protected void AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        protected void AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> actualQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertSingleResult(actualQuery, expectedQuery, asserter, entryCount);

        public void AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        public void AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertSingleResult(actualQuery, expectedQuery, asserter, entryCount);

        public void AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> query,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertSingleResult(query, query, asserter, entryCount);

        public void AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => Fixture.QueryAsserter.AssertSingleResult(actualQuery, expectedQuery, asserter, entryCount);

        #endregion

        #region AssertQuery

        public void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        public void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertQuery(actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        public void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        public void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertQuery(actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        public void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount);

        public void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => Fixture.QueryAsserter.AssertQuery(actualQuery, expectedQuery, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        #endregion

        #region AssertQueryScalar

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<double>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<uint>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<uint>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<uint>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, uint>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<long>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<short>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<bool>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, bool>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<DateTime>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<DateTimeOffset>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<TimeSpan>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TResult : struct
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder).GetAwaiter().GetResult();

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, bool>(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<bool>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, bool>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, DateTime>(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
           Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> actualQuery,
           Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<DateTime>> expectedQuery,
           bool assertOrder = false)
           where TItem1 : class
           where TItem2 : class
           => AssertQueryScalar<TItem1, TItem2, DateTime>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder).GetAwaiter().GetResult();

        public void AssertQueryScalar<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder).GetAwaiter().GetResult();

        #endregion

        #region AssertQueryScalar - nullable

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, int>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<TimeSpan?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<DateTime?>> query,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<bool?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<bool?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            => AssertQueryScalar<TItem1, bool>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar(query, query, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<int?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQueryScalar<TItem1, TItem2, int>(actualQuery, expectedQuery, assertOrder);

        public void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => Fixture.QueryAsserter.AssertQueryScalar(actualQuery, expectedQuery, assertOrder);

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

        public List<object> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertIncludeQuery(actualQuery, expectedQuery, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount).GetAwaiter().GetResult();

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

        public List<object> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            where TItem2 : class
            => Fixture.QueryAsserter.AssertIncludeQuery(actualQuery, expectedQuery, expectedIncludes, elementSorter, clientProjections, assertOrder, entryCount).GetAwaiter().GetResult();

        #endregion

        #region Helpers - Sorters

        public static Func<dynamic, dynamic> GroupingSorter<TKey, TElement>()
            => e => ((IGrouping<TKey, TElement>)e).Key + " " + CollectionSorter<TElement>()(e);

        public static Func<dynamic, dynamic> CollectionSorter<TElement>()
            => e => ((IEnumerable<TElement>)e).Count();

        #endregion

        #region Helpers - Asserters

        public static Action<dynamic, dynamic> GroupingAsserter<TKey, TElement>(Func<TElement, object> elementSorter = null, Action<TElement, TElement> elementAsserter = null)
        {
            return (e, a) =>
                {
                    Assert.Equal(((IGrouping<TKey, TElement>)e).Key, ((IGrouping<TKey, TElement>)a).Key);
                    CollectionAsserter(elementSorter, elementAsserter)(e, a);
                };
        }

        public static Action<dynamic, dynamic> CollectionAsserter<TElement>(Func<TElement, object> elementSorter = null, Action<TElement, TElement> elementAsserter = null)
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
                    elementAsserter = elementAsserter ?? Assert.Equal;

                    for (var i = 0; i < expected.Count; i++)
                    {
                        elementAsserter(expected[i], actual[i]);
                    }
                };
        }

        #endregion

        #region Helpers - Maybe

        public static TResult Maybe<TResult>(object caller, Func<TResult> expression)
            where TResult : class
        {
            if (caller == null)
            {
                return null;
            }

            return expression();
        }

        public static TResult? MaybeScalar<TResult>(object caller, Func<TResult?> expression)
            where TResult : struct
        {
            if (caller == null)
            {
                return null;
            }

            return expression();
        }

        #endregion
    }
}
