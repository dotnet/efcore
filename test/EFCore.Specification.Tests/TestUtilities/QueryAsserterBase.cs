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

        public abstract Task AssertSingleResultTyped<TResult>(
            Func<ISetSource, TResult> actualSyncQuery,
            Func<ISetSource, Task<TResult>> actualAsyncQuery,
            Func<ISetSource, TResult> expectedQuery,
            Action<TResult, TResult> asserter,
            int entryCount,
            bool isAsync,
            string testMethodName);

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

        public abstract Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct;

        public abstract Task AssertQueryScalar<TResult>(
            Func<ISetSource, IQueryable<TResult?>> actualQuery,
            Func<ISetSource, IQueryable<TResult?>> expectedQuery,
            bool assertOrder,
            bool isAsync,
            string testMethodName)
            where TResult : struct;

        public abstract Task<List<TResult>> AssertIncludeQuery<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            List<IExpectedInclude> expectedIncludes,
            Func<TResult, object> elementSorter,
            List<Func<TResult, object>> clientProjections,
            bool assertOrder,
            int entryCount,
            bool isAsync,
            string testMethodName);

        #region Assert termination operation methods

        public abstract Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            bool isAsync = false);

        public abstract Task AssertAny<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        public abstract Task AssertAll<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            bool isAsync = false);

        public abstract Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertFirst<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertFirstOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingle<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSingleOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLast<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertLastOrDefault<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, bool>> actualPredicate,
            Expression<Func<TResult, bool>> expectedPredicate,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

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

        public abstract Task AssertMin<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertMin<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<TSelector, TSelector> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertMax<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Action<TResult, TResult> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertMax<TResult, TSelector>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, TSelector>> actualSelector,
            Expression<Func<TResult, TSelector>> expectedSelector,
            Action<TSelector, TSelector> asserter = null,
            int entryCount = 0,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<int, int> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<int?, int?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<long, long> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<long?, long?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<int, int> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<int?, int?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<long, long> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<long?, long?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertSum<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<int>> actualQuery,
            Func<ISetSource, IQueryable<int>> expectedQuery,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<int?>> actualQuery,
            Func<ISetSource, IQueryable<int?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<long>> actualQuery,
            Func<ISetSource, IQueryable<long>> expectedQuery,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<long?>> actualQuery,
            Func<ISetSource, IQueryable<long?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<decimal>> actualQuery,
            Func<ISetSource, IQueryable<decimal>> expectedQuery,
            Action<decimal, decimal> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<decimal?>> actualQuery,
            Func<ISetSource, IQueryable<decimal?>> expectedQuery,
            Action<decimal?, decimal?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<float>> actualQuery,
            Func<ISetSource, IQueryable<float>> expectedQuery,
            Action<float, float> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<float?>> actualQuery,
            Func<ISetSource, IQueryable<float?>> expectedQuery,
            Action<float?, float?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<double>> actualQuery,
            Func<ISetSource, IQueryable<double>> expectedQuery,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage(
            Func<ISetSource, IQueryable<double?>> actualQuery,
            Func<ISetSource, IQueryable<double?>> expectedQuery,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int>> actualSelector,
            Expression<Func<TResult, int>> expectedSelector,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, int?>> actualSelector,
            Expression<Func<TResult, int?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long>> actualSelector,
            Expression<Func<TResult, long>> expectedSelector,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, long?>> actualSelector,
            Expression<Func<TResult, long?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal>> actualSelector,
            Expression<Func<TResult, decimal>> expectedSelector,
            Action<decimal, decimal> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, decimal?>> actualSelector,
            Expression<Func<TResult, decimal?>> expectedSelector,
            Action<decimal?, decimal?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float>> actualSelector,
            Expression<Func<TResult, float>> expectedSelector,
            Action<float, float> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, float?>> actualSelector,
            Expression<Func<TResult, float?>> expectedSelector,
            Action<float?, float?> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double>> actualSelector,
            Expression<Func<TResult, double>> expectedSelector,
            Action<double, double> asserter = null,
            bool isAsync = false);

        public abstract Task AssertAverage<TResult>(
            Func<ISetSource, IQueryable<TResult>> actualQuery,
            Func<ISetSource, IQueryable<TResult>> expectedQuery,
            Expression<Func<TResult, double?>> actualSelector,
            Expression<Func<TResult, double?>> expectedSelector,
            Action<double?, double?> asserter = null,
            bool isAsync = false);

        #endregion

        #region Helpers

        public abstract void AssertEqual<T>(
            T expected,
            T actual,
            Action<T, T> asserter = null);

        public abstract void AssertEqual<T>(
            T? expected,
            T? actual,
            Action<T?, T?> asserter = null)
            where T : struct;

        public abstract void AssertCollection<TElement>(
            IEnumerable<TElement> expected,
            IEnumerable<TElement> actual,
            bool ordered = false,
            Func<TElement, object> elementSorter = null,
            Action<TElement, TElement> elementAsserter = null);

        #endregion
    }
}
