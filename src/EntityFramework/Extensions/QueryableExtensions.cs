// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace System.Linq
{
    public static class QueryableExtensions
    {
        #region Any/All

        private static readonly MethodInfo _any = GetMethod("Any");

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return AnyAsync(source, default(CancellationToken));
        }

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, bool>(_any, source, cancellationToken);
        }

        private static readonly MethodInfo _anyPredicate = GetMethod("Any", 1);

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return AnyAsync(source, predicate, default(CancellationToken));
        }

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, bool>(_anyPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _allPredicate = GetMethod("All", 1);

        public static Task<bool> AllAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return AllAsync(source, predicate, default(CancellationToken));
        }

        public static Task<bool> AllAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, bool>(_allPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Count/LongCount

        private static readonly MethodInfo _count = GetMethod("Count");

        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return CountAsync(source, default(CancellationToken));
        }

        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, int>(_count, source, cancellationToken);
        }

        private static readonly MethodInfo _countPredicate = GetMethod("Count", 1);

        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return CountAsync(source, predicate, default(CancellationToken));
        }

        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, int>(_countPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _longCount = GetMethod("LongCount");

        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return LongCountAsync(source, default(CancellationToken));
        }

        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, long>(_longCount, source, cancellationToken);
        }

        private static readonly MethodInfo _longCountPredicate = GetMethod("LongCount", 1);

        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return LongCountAsync(source, predicate, default(CancellationToken));
        }

        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, long>(_longCountPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region First/FirstOrDefault

        private static readonly MethodInfo _first = GetMethod("First");

        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return FirstAsync(source, default(CancellationToken));
        }

        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_first, source, cancellationToken);
        }

        private static readonly MethodInfo _firstPredicate = GetMethod("First", 1);

        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return FirstAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_firstPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault");

        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return FirstOrDefaultAsync(source, default(CancellationToken));
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_firstOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _firstOrDefaultPredicate = GetMethod("FirstOrDefault", 1);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return FirstOrDefaultAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_firstOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Last/LastOrDefault

        private static readonly MethodInfo _last = GetMethod("Last");

        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return LastAsync(source, default(CancellationToken));
        }

        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_last, source, cancellationToken);
        }

        private static readonly MethodInfo _lastPredicate = GetMethod("Last", 1);

        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return LastAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_lastPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _lastOrDefault = GetMethod("LastOrDefault");

        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return LastOrDefaultAsync(source, default(CancellationToken));
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_lastOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _lastOrDefaultPredicate = GetMethod("LastOrDefault", 1);

        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return LastOrDefaultAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_lastOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Single/SingleOrDefault

        private static readonly MethodInfo _single = GetMethod("Single");

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return SingleAsync(source, default(CancellationToken));
        }

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_single, source, cancellationToken);
        }

        private static readonly MethodInfo _singlePredicate = GetMethod("Single", 1);

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return SingleAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_singlePredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault");

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return SingleOrDefaultAsync(source, default(CancellationToken));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_singleOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _singleOrDefaultPredicate = GetMethod("SingleOrDefault", 1);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate)
        {
            return SingleOrDefaultAsync(source, predicate, default(CancellationToken));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            return ExecuteAsync<TSource, TSource>(_singleOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Min

        private static readonly MethodInfo _min = GetMethod("Min", predicate: mi => mi.IsGenericMethod);

        public static Task<TSource> MinAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return MinAsync(source, default(CancellationToken));
        }

        public static Task<TSource> MinAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_min, source, cancellationToken);
        }

        private static readonly MethodInfo _minSelector = GetMethod("Min", 1, mi => mi.IsGenericMethod);

        public static Task<TResult> MinAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector)
        {
            return MinAsync(source, selector, default(CancellationToken));
        }

        public static Task<TResult> MinAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, TResult>(_minSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Max

        private static readonly MethodInfo _max = GetMethod("Max", predicate: mi => mi.IsGenericMethod);

        public static Task<TSource> MaxAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return MaxAsync(source, default(CancellationToken));
        }

        public static Task<TSource> MaxAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, TSource>(_max, source, cancellationToken);
        }

        private static readonly MethodInfo _maxSelector = GetMethod("Max", 1, mi => mi.IsGenericMethod);

        public static Task<TResult> MaxAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector)
        {
            return MaxAsync(source, selector, default(CancellationToken));
        }

        public static Task<TResult> MaxAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, TResult>(_maxSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Sum

        private static readonly MethodInfo _sumDecimal = GetMethod<decimal>("Sum");

        public static Task<decimal> SumAsync(
            [NotNull] this IQueryable<decimal> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<decimal> SumAsync(
            [NotNull] this IQueryable<decimal> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<decimal, decimal>(_sumDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDecimal = GetMethod<decimal?>("Sum");

        public static Task<decimal?> SumAsync(
            [NotNull] this IQueryable<decimal?> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<decimal?> SumAsync(
            [NotNull] this IQueryable<decimal?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<decimal?, decimal?>(_sumNullableDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _sumDecimalSelector = GetMethod<decimal>("Sum", 1);

        public static Task<decimal> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<decimal> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, decimal>(_sumDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDecimalSelector = GetMethod<decimal?>("Sum", 1);

        public static Task<decimal?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<decimal?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, decimal?>(_sumNullableDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumInt = GetMethod<int>("Sum");

        public static Task<int> SumAsync(
            [NotNull] this IQueryable<int> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<int> SumAsync(
            [NotNull] this IQueryable<int> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<int, int>(_sumInt, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableInt = GetMethod<int?>("Sum");

        public static Task<int?> SumAsync(
            [NotNull] this IQueryable<int?> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<int?> SumAsync(
            [NotNull] this IQueryable<int?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<int?, int?>(_sumNullableInt, source, cancellationToken);
        }

        private static readonly MethodInfo _sumIntSelector = GetMethod<int>("Sum", 1);

        public static Task<int> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<int> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, int>(_sumIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableIntSelector = GetMethod<int?>("Sum", 1);

        public static Task<int?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<int?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, int?>(_sumNullableIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumLong = GetMethod<long>("Sum");

        public static Task<long> SumAsync(
            [NotNull] this IQueryable<long> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<long> SumAsync(
            [NotNull] this IQueryable<long> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<long, long>(_sumLong, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableLong = GetMethod<long?>("Sum");

        public static Task<long?> SumAsync(
            [NotNull] this IQueryable<long?> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<long?> SumAsync(
            [NotNull] this IQueryable<long?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<long?, long?>(_sumNullableLong, source, cancellationToken);
        }

        private static readonly MethodInfo _sumLongSelector = GetMethod<long>("Sum", 1);

        public static Task<long> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<long> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, long>(_sumLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableLongSelector = GetMethod<long?>("Sum", 1);

        public static Task<long?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<long?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, long?>(_sumNullableLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumDouble = GetMethod<double>("Sum");

        public static Task<double> SumAsync(
            [NotNull] this IQueryable<double> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<double> SumAsync(
            [NotNull] this IQueryable<double> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<double, double>(_sumDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDouble = GetMethod<double?>("Sum");

        public static Task<double?> SumAsync(
            [NotNull] this IQueryable<double?> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<double?> SumAsync(
            [NotNull] this IQueryable<double?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<double?, double?>(_sumNullableDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _sumDoubleSelector = GetMethod<double>("Sum", 1);

        public static Task<double> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<double> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double>(_sumDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDoubleSelector = GetMethod<double?>("Sum", 1);

        public static Task<double?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<double?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double?>(_sumNullableDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumFloat = GetMethod<float>("Sum");

        public static Task<float> SumAsync(
            [NotNull] this IQueryable<float> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<float> SumAsync(
            [NotNull] this IQueryable<float> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<float, float>(_sumFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableFloat = GetMethod<float?>("Sum");

        public static Task<float?> SumAsync(
            [NotNull] this IQueryable<float?> source)
        {
            return SumAsync(source, default(CancellationToken));
        }

        public static Task<float?> SumAsync(
            [NotNull] this IQueryable<float?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<float?, float?>(_sumNullableFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _sumFloatSelector = GetMethod<float>("Sum", 1);

        public static Task<float> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<float> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, float>(_sumFloatSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableFloatSelector = GetMethod<float?>("Sum", 1);

        public static Task<float?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector)
        {
            return SumAsync(source, selector, default(CancellationToken));
        }

        public static Task<float?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, float?>(_sumNullableFloatSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Average

        private static MethodInfo GetAverageMethod<TOperand, TResult>(int parameterCount = 0)
        {
            return GetMethod<TResult>(
                "Average",
                parameterCount,
                mi => (parameterCount == 0
                       && mi.GetParameters()[0].ParameterType == typeof(IQueryable<TOperand>))
                      || (mi.GetParameters().Length == 2
                          && mi.GetParameters()[1]
                              .ParameterType.GenericTypeArguments[0]
                              .GenericTypeArguments[1] == typeof(TOperand)));
        }

        private static readonly MethodInfo _averageDecimal = GetAverageMethod<decimal, decimal>();

        public static Task<decimal> AverageAsync(
            [NotNull] this IQueryable<decimal> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<decimal> AverageAsync(
            [NotNull] this IQueryable<decimal> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<decimal, decimal>(_averageDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDecimal = GetAverageMethod<decimal?, decimal?>();

        public static Task<decimal?> AverageAsync(
            [NotNull] this IQueryable<decimal?> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<decimal?> AverageAsync(
            [NotNull] this IQueryable<decimal?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<decimal?, decimal?>(_averageNullableDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _averageDecimalSelector = GetAverageMethod<decimal, decimal>(1);

        public static Task<decimal> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<decimal> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, decimal>(_averageDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDecimalSelector = GetAverageMethod<decimal?, decimal?>(1);

        public static Task<decimal?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<decimal?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, decimal?>(_averageNullableDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageInt = GetAverageMethod<int, double>();

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<int> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<int> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<int, double>(_averageInt, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableInt = GetAverageMethod<int?, double?>();

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<int?> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<int?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<int?, double?>(_averageNullableInt, source, cancellationToken);
        }

        private static readonly MethodInfo _averageIntSelector = GetAverageMethod<int, double>(1);

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double>(_averageIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableIntSelector = GetAverageMethod<int?, double?>(1);

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double?>(_averageNullableIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageLong = GetAverageMethod<long, double>();

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<long> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<long> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<long, double>(_averageLong, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableLong = GetAverageMethod<long?, double?>();

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<long?> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<long?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<long?, double?>(_averageNullableLong, source, cancellationToken);
        }

        private static readonly MethodInfo _averageLongSelector = GetAverageMethod<long, double>(1);

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double>(_averageLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableLongSelector = GetAverageMethod<long?, double?>(1);

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double?>(_averageNullableLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageDouble = GetAverageMethod<double, double>();

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<double> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<double> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<double, double>(_averageDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDouble = GetAverageMethod<double?, double?>();

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<double?> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<double?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<double?, double?>(_averageNullableDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _averageDoubleSelector = GetAverageMethod<double, double>(1);

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double>(_averageDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDoubleSelector = GetAverageMethod<double?, double?>(1);

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, double?>(_averageNullableDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageFloat = GetAverageMethod<float, float>();

        public static Task<float> AverageAsync(
            [NotNull] this IQueryable<float> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<float> AverageAsync(
            [NotNull] this IQueryable<float> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<float, float>(_averageFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableFloat = GetAverageMethod<float?, float?>();

        public static Task<float?> AverageAsync(
            [NotNull] this IQueryable<float?> source)
        {
            return AverageAsync(source, default(CancellationToken));
        }

        public static Task<float?> AverageAsync(
            [NotNull] this IQueryable<float?> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<float?, float?>(_averageNullableFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _averageFloatSelector = GetAverageMethod<float, float>(1);

        public static Task<float> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<float> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, float>(_averageFloatSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableFloatSelector = GetAverageMethod<float?, float?>(1);

        public static Task<float?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector)
        {
            return AverageAsync(source, selector, default(CancellationToken));
        }

        public static Task<float?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return ExecuteAsync<TSource, float?>(_averageNullableFloatSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Contains

        private static readonly MethodInfo _contains = GetMethod("Contains", 1);

        public static Task<bool> ContainsAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] TSource item)
        {
            return ContainsAsync(source, item, default(CancellationToken));
        }

        public static Task<bool> ContainsAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] TSource item,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return ExecuteAsync<TSource, bool>(
                _contains,
                source,
                Expression.Constant(item, typeof(TSource)),
                cancellationToken);
        }

        #endregion

        #region ToList/Array

        public static Task<List<TSource>> ToListAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return ToListAsync(source, default(CancellationToken));
        }

        public static Task<List<TSource>> ToListAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.AsAsyncEnumerable().ToList(cancellationToken);
        }

        public static Task<TSource[]> ToArrayAsync<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            return ToArrayAsync(source, default(CancellationToken));
        }

        public static Task<TSource[]> ToArrayAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            return source.AsAsyncEnumerable().ToArray(cancellationToken);
        }

        #endregion

        #region Include

        public static IQueryable<T> Include<T, TProperty>(
            [NotNull] this IQueryable<T> source,
            [NotNull] Expression<Func<T, TProperty>> path)
        {
            Check.NotNull(source, "source");

            throw new NotImplementedException();
        }

        #endregion

        #region AsAsyncEnumerable

        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>([NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            var enumerable = source as IAsyncEnumerable<TSource>;

            if (enumerable != null)
            {
                return enumerable;
            }

            var entityQueryableAccessor = source as IAsyncEnumerableAccessor<TSource>;

            if (entityQueryableAccessor != null)
            {
                return entityQueryableAccessor.AsyncEnumerable;
            }

            throw new InvalidOperationException(Strings.FormatIQueryableNotAsync(typeof(TSource)));
        }

        #endregion

        #region Load

        public static void Load<TSource>([NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                }
            }
        }

        public static Task LoadAsync<TSource>([NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, "source");

            return source.LoadAsync(default(CancellationToken));
        }

        public static async Task LoadAsync<TSource>(
            [NotNull] this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");

            // ReSharper disable once NotAccessedVariable
            var _ = default(TSource);

            var asyncEnumerable = source.AsAsyncEnumerable();

            using (var enumerator = asyncEnumerable.GetEnumerator())
            {
                while (await enumerator.MoveNext())
                {
                    _ = enumerator.Current;
                }
            }
        }

        #endregion

        #region ToDictionary

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsAsyncEnumerable().ToDictionary(keySelector);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, cancellationToken);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(comparer, "comparer");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, comparer);
        }

        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(comparer, "comparer");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, comparer, cancellationToken);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector, cancellationToken);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");
            Check.NotNull(elementSelector, "comparer");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector, comparer);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");
            Check.NotNull(elementSelector, "elementSelector");
            Check.NotNull(comparer, "comparer");

            return source.AsAsyncEnumerable().ToDictionary(keySelector, elementSelector, comparer, cancellationToken);
        }

        #endregion

        #region ForEach

        public static Task ForEachAsync<T>([NotNull] this IQueryable<T> source, [NotNull] Action<T> action)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsAsyncEnumerable().ForEachAsync(action, CancellationToken.None);
        }

        public static Task ForEachAsync<T>(
            [NotNull] this IQueryable<T> source,
            [NotNull] Action<T> action,
            CancellationToken cancellationToken)
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            return source.AsAsyncEnumerable().ForEachAsync(action, cancellationToken);
        }

        #endregion

        #region Impl.

        private static Task<TResult> ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            var provider = source.Provider as IAsyncQueryProvider;

            if (provider != null)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo = operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                return provider.ExecuteAsync<TResult>(
                    Expression.Call(null, operatorMethodInfo, new[] { source.Expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(Strings.FormatIQueryableProviderNotAsync());
        }

        private static Task<TResult> ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken cancellationToken)
        {
            return ExecuteAsync<TSource, TResult>(
                operatorMethodInfo, source, Expression.Quote(expression), cancellationToken);
        }

        private static Task<TResult> ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken cancellationToken)
        {
            var provider = source.Provider as IAsyncQueryProvider;

            if (provider != null)
            {
                operatorMethodInfo
                    = operatorMethodInfo.GetGenericArguments().Length == 2
                        ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult))
                        : operatorMethodInfo.MakeGenericMethod(typeof(TSource));

                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        null,
                        operatorMethodInfo,
                        new[] { source.Expression, expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(Strings.FormatIQueryableProviderNotAsync());
        }

        private static MethodInfo GetMethod<TResult>(
            string name, int parameterCount = 0, Func<MethodInfo, bool> predicate = null)
        {
            return GetMethod(
                name,
                parameterCount,
                mi => mi.ReturnType == typeof(TResult)
                      && (predicate == null || predicate(mi)));
        }

        private static MethodInfo GetMethod(
            string name, int parameterCount = 0, Func<MethodInfo, bool> predicate = null)
        {
            return typeof(Queryable).GetTypeInfo().GetDeclaredMethods(name)
                .Single(mi => mi.GetParameters().Length == parameterCount + 1
                              && (predicate == null || predicate(mi)));
        }

        #endregion
    }
}
