// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class QueryAsserterBase
    {
        public virtual Func<DbContext, ISetSource> SetSourceCreator { get; set; } 

        public virtual ISetSource ExpectedData { get; set; }

        public abstract void AssertEqual<T>(
            T expected,
            T actual,
            Action<dynamic, dynamic> asserter = null);

        public abstract void AssertCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            bool ordered = false,
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null);

        #region AssertSingleResult

        public abstract Task AssertSingleResultTyped<TResult>(
            Func<ISetSource, TResult> actualSyncQuery,
            Func<ISetSource, Task<TResult>> actualAsyncQuery,
            Func<ISetSource, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName);

        public abstract Task AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class;

        public abstract Task AssertSingleResult<TItem1, TResult>(
            Func<IQueryable<TItem1>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> expectedQuery,
            Action<object, object> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQuery

        public abstract Task AssertQuery<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Func<TResult, object> elementSorter,
            Action<TResult, TResult> elementAsserter,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TResult : class;

        #endregion

        #region AssertQueryScalar

        public abstract Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TResult : struct;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            where TResult : struct;

        #endregion

        #region AssertQueryScalar - nullable

        public abstract Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TResult : struct;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct;

        #endregion

        #region AssertIncludeQuery

        public abstract Task<List<object>> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class;

        public abstract Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter,
            List<Func<dynamic, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName)
            where TItem1 : class
            where TItem2 : class;

        #endregion

        #region AssertAny

        public abstract Task AssertAnyTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false);

        public abstract Task AssertAny<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAny<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAny<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertAny<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        public abstract Task AssertAnyTyped<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        public abstract Task AssertAny<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertAll

        public abstract Task AssertAll<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        #endregion

        #region AssertFirst

        public abstract Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertFirstOrDefault

        public abstract Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertSingle

        public abstract Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertSingleOrDefault

        public abstract Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertLast

        public abstract Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertLastOrDefault

        public abstract Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertCount

        public abstract Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false);

        public abstract Task AssertCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        #endregion

        #region AssertLongCount

        public abstract Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false);

        public abstract Task AssertLongCount<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        #endregion

        #region AssertMin

        public abstract Task AssertMin<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertMin<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertMax

        public abstract Task AssertMax<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertMax<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        #endregion

        #region AssertSum

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        #endregion

        #region AssertAverage

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false);

        #endregion
    }
}
