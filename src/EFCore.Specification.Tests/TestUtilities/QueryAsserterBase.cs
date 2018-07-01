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
        public virtual ISetExtractor SetExtractor { get; set; }
        public virtual IExpectedData ExpectedData { get; set; }

        #region AssertSingleResult

        public abstract Task AssertSingleResult<TItem1>(
            Func<IQueryable<TItem1>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSingleResult<TItem1, TResult>(
            Func<IQueryable<TItem1>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, TResult> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, TResult> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<object>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        public abstract Task AssertSingleResult<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> actualSyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, Task<TResult>> actualAsyncQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, TResult> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQuery

        public abstract Task AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQueryScalar

        public abstract Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        #endregion

        #region AssertQueryScalar - nullable

        public abstract Task AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TResult : struct;

        public abstract Task AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> expectedQuery,
            bool assertOrder = false,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct;

        #endregion

        #region AssertIncludeQuery

        public abstract Task<List<object>> AssertIncludeQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task<List<object>> AssertIncludeQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<dynamic, object> elementSorter = null,
            List<Func<dynamic, object>> clientProjections = null,
            bool assertOrder = false,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        #endregion

        #region AssertAny

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

        public abstract Task AssertAny<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertAll

        public abstract Task AssertAll<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertFirst

        public abstract Task AssertFirst<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertFirst<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertFirstOrDefault

        public abstract Task AssertFirstOrDefault<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertFirstOrDefault<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertFirstOrDefault<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class;

        public abstract Task AssertFirstOrDefault<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertSingle

        public abstract Task AssertSingle<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSingle<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        public abstract Task AssertSingle<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertSingleOrDefault

        public abstract Task AssertSingleOrDefault<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSingleOrDefault<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertLast

        public abstract Task AssertLast<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertLast<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertLastOrDefault

        public abstract Task AssertLastOrDefault<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertLastOrDefault<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertCount

        public abstract Task AssertCount<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertCount<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertCount<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertCount<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        #endregion

        #region AssertLongCount

        public abstract Task AssertLongCount<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertLongCount<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertLongCount<TItem1, TPredicate>(
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TPredicate>> expectedQuery,
            Expression<Func<TPredicate, bool>> actualPredicate,
            Expression<Func<TPredicate, bool>> expectedPredicate,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertLongCount<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> expectedQuery,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        #endregion

        #region AssertMin

        public abstract Task AssertMin<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertMin<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertMin<TItem1, TSelector, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, TResult>> actualSelector,
            Expression<Func<TSelector, TResult>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertMax

        public abstract Task AssertMax<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertMax<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> expectedQuery,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertMax<TItem1, TSelector, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, TResult>> actualSelector,
            Expression<Func<TSelector, TResult>> expectedSelector,
            Action<object, object> asserter = null,
            int entryCount = 0,
            bool isAsync = false)
            where TItem1 : class;

        #endregion

        #region AssertSum

        public abstract Task AssertSum<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int?>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int?>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, float>> actualSelector,
            Expression<Func<TSelector, float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertSum<TItem1, TItem2, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class
            where TItem2 : class;

        #endregion

        #region AssertAverage

        public abstract Task AssertAverage<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<int>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<int>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAverage<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<long>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<long>> expectedQuery,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAverage<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int>> actualSelector,
            Expression<Func<TSelector, int>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAverage<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, int?>> actualSelector,
            Expression<Func<TSelector, int?>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAverage<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, decimal>> actualSelector,
            Expression<Func<TSelector, decimal>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        public abstract Task AssertAverage<TItem1, TSelector>(
            Func<IQueryable<TItem1>, IQueryable<TSelector>> actualQuery,
            Func<IQueryable<TItem1>, IQueryable<TSelector>> expectedQuery,
            Expression<Func<TSelector, float>> actualSelector,
            Expression<Func<TSelector, float>> expectedSelector,
            Action<object, object> asserter = null,
            bool isAsync = false)
            where TItem1 : class;

        #endregion
    }
}
