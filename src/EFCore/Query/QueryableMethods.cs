// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A class that provides reflection metadata for translatable LINQ methods.
    /// </summary>
    public static class QueryableMethods
    {
        public static MethodInfo AsQueryable { get; }
        public static MethodInfo Cast { get; }
        public static MethodInfo OfType { get; }

        public static MethodInfo All { get; }
        public static MethodInfo AnyWithoutPredicate { get; }
        public static MethodInfo AnyWithPredicate { get; }
        public static MethodInfo Contains { get; }

        public static MethodInfo Concat { get; }
        public static MethodInfo Except { get; }
        public static MethodInfo Intersect { get; }
        public static MethodInfo Union { get; }

        public static MethodInfo CountWithoutPredicate { get; }
        public static MethodInfo CountWithPredicate { get; }
        public static MethodInfo LongCountWithoutPredicate { get; }
        public static MethodInfo LongCountWithPredicate { get; }
        public static MethodInfo MinWithSelector { get; }
        public static MethodInfo MinWithoutSelector { get; }
        public static MethodInfo MaxWithSelector { get; }
        public static MethodInfo MaxWithoutSelector { get; }

        public static MethodInfo ElementAt { get; }
        public static MethodInfo ElementAtOrDefault { get; }
        public static MethodInfo FirstWithoutPredicate { get; }
        public static MethodInfo FirstWithPredicate { get; }
        public static MethodInfo FirstOrDefaultWithoutPredicate { get; }
        public static MethodInfo FirstOrDefaultWithPredicate { get; }
        public static MethodInfo SingleWithoutPredicate { get; }
        public static MethodInfo SingleWithPredicate { get; }
        public static MethodInfo SingleOrDefaultWithoutPredicate { get; }
        public static MethodInfo SingleOrDefaultWithPredicate { get; }
        public static MethodInfo LastWithoutPredicate { get; }
        public static MethodInfo LastWithPredicate { get; }
        public static MethodInfo LastOrDefaultWithoutPredicate { get; }
        public static MethodInfo LastOrDefaultWithPredicate { get; }

        public static MethodInfo Distinct { get; }
        public static MethodInfo Reverse { get; }
        public static MethodInfo Where { get; }
        public static MethodInfo Select { get; }
        public static MethodInfo Skip { get; }
        public static MethodInfo Take { get; }
        public static MethodInfo SkipWhile { get; }
        public static MethodInfo TakeWhile { get; }
        public static MethodInfo OrderBy { get; }
        public static MethodInfo OrderByDescending { get; }
        public static MethodInfo ThenBy { get; }
        public static MethodInfo ThenByDescending { get; }
        public static MethodInfo DefaultIfEmptyWithoutArgument { get; }
        public static MethodInfo DefaultIfEmptyWithArgument { get; }

        public static MethodInfo Join { get; }
        public static MethodInfo GroupJoin { get; }
        public static MethodInfo SelectManyWithCollectionSelector { get; }
        public static MethodInfo SelectManyWithoutCollectionSelector { get; }

        public static MethodInfo GroupByWithKeySelector { get; }
        public static MethodInfo GroupByWithKeyElementSelector { get; }
        public static MethodInfo GroupByWithKeyElementResultSelector { get; }
        public static MethodInfo GroupByWithKeyResultSelector { get; }

        public static bool IsSumWithoutSelector(MethodInfo methodInfo)
            => SumWithoutSelectorMethods.Values.Contains(methodInfo);

        public static bool IsSumWithSelector(MethodInfo methodInfo)
            => methodInfo.IsGenericMethod
                && SumWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());

        public static bool IsAverageWithoutSelector(MethodInfo methodInfo)
            => AverageWithoutSelectorMethods.Values.Contains(methodInfo);

        public static bool IsAverageWithSelector(MethodInfo methodInfo)
            => methodInfo.IsGenericMethod
                && AverageWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());

        public static MethodInfo GetSumWithoutSelector(Type type) => SumWithoutSelectorMethods[type];
        public static MethodInfo GetSumWithSelector(Type type) => SumWithSelectorMethods[type];
        public static MethodInfo GetAverageWithoutSelector(Type type) => AverageWithoutSelectorMethods[type];
        public static MethodInfo GetAverageWithSelector(Type type) => AverageWithSelectorMethods[type];

        private static Dictionary<Type, MethodInfo> SumWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> AverageWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }

        static QueryableMethods()
        {
            var queryableMethods = typeof(Queryable).GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            AsQueryable = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.AsQueryable) && mi.IsGenericMethod && mi.GetParameters().Length == 1);
            Cast = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Cast) && mi.GetParameters().Length == 1);
            OfType = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OfType) && mi.GetParameters().Length == 1);

            All = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.All)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            AnyWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any) && mi.GetParameters().Length == 1);
            AnyWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            Contains = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Contains) && mi.GetParameters().Length == 2);

            Concat = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Concat) && mi.GetParameters().Length == 2);
            Except = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Except) && mi.GetParameters().Length == 2);
            Intersect = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Intersect) && mi.GetParameters().Length == 2);
            Union = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Union) && mi.GetParameters().Length == 2);

            CountWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Count) && mi.GetParameters().Length == 1);
            CountWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Count)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LongCountWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LongCount) && mi.GetParameters().Length == 1);
            LongCountWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LongCount)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MinWithSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MinWithoutSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min) && mi.GetParameters().Length == 1);
            MaxWithSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MaxWithoutSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max) && mi.GetParameters().Length == 1);

            ElementAt = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ElementAt) && mi.GetParameters().Length == 2);
            ElementAtOrDefault = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ElementAtOrDefault) && mi.GetParameters().Length == 2);
            FirstWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First) && mi.GetParameters().Length == 1);
            FirstWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            FirstOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault) && mi.GetParameters().Length == 1);
            FirstOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single) && mi.GetParameters().Length == 1);
            SingleWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault) && mi.GetParameters().Length == 1);
            SingleOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last) && mi.GetParameters().Length == 1);
            LastWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault) && mi.GetParameters().Length == 1);
            LastOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            Distinct = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Distinct) && mi.GetParameters().Length == 1);
            Reverse = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Reverse) && mi.GetParameters().Length == 1);
            Where = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Where)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            Select = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Select)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            Skip = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Skip) && mi.GetParameters().Length == 2);
            Take = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Take) && mi.GetParameters().Length == 2);
            SkipWhile = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SkipWhile)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            TakeWhile = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.TakeWhile)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            OrderBy = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OrderBy)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            OrderByDescending = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OrderByDescending)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            ThenBy = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ThenBy)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            ThenByDescending = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ThenByDescending)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            DefaultIfEmptyWithoutArgument = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.DefaultIfEmpty) && mi.GetParameters().Length == 1);
            DefaultIfEmptyWithArgument = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.DefaultIfEmpty) && mi.GetParameters().Length == 2);

            Join = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Join) && mi.GetParameters().Length == 5);
            GroupJoin = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupJoin) && mi.GetParameters().Length == 5);
            SelectManyWithCollectionSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SelectMany)
                    && mi.GetParameters().Length == 3
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SelectManyWithoutCollectionSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SelectMany)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            GroupByWithKeySelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            GroupByWithKeyElementSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy)
                    && mi.GetParameters().Length == 3
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType)
                    && IsExpressionOfFunc(mi.GetParameters()[2].ParameterType));
            GroupByWithKeyElementResultSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy)
                    && mi.GetParameters().Length == 4
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType)
                    && IsExpressionOfFunc(mi.GetParameters()[2].ParameterType)
                    && IsExpressionOfFunc(
                        mi.GetParameters()[3].ParameterType, 3));
            GroupByWithKeyResultSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy)
                    && mi.GetParameters().Length == 3
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType)
                    && IsExpressionOfFunc(
                        mi.GetParameters()[2].ParameterType, 3));

            SumWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithoutSelector<decimal>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long), GetSumOrAverageWithoutSelector<long>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int), GetSumOrAverageWithoutSelector<int>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double), GetSumOrAverageWithoutSelector<double>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float), GetSumOrAverageWithoutSelector<float>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(decimal?), GetSumOrAverageWithoutSelector<decimal?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long?), GetSumOrAverageWithoutSelector<long?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int?), GetSumOrAverageWithoutSelector<int?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double?), GetSumOrAverageWithoutSelector<double?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float?), GetSumOrAverageWithoutSelector<float?>(queryableMethods, nameof(Queryable.Sum)) }
            };

            SumWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(queryableMethods, nameof(Queryable.Sum)) }
            };

            AverageWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithoutSelector<decimal>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long), GetSumOrAverageWithoutSelector<long>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int), GetSumOrAverageWithoutSelector<int>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double), GetSumOrAverageWithoutSelector<double>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float), GetSumOrAverageWithoutSelector<float>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(decimal?), GetSumOrAverageWithoutSelector<decimal?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long?), GetSumOrAverageWithoutSelector<long?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int?), GetSumOrAverageWithoutSelector<int?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double?), GetSumOrAverageWithoutSelector<double?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float?), GetSumOrAverageWithoutSelector<float?>(queryableMethods, nameof(Queryable.Average)) }
            };

            AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(queryableMethods, nameof(Queryable.Average)) }
            };

            static MethodInfo GetSumOrAverageWithoutSelector<T>(List<MethodInfo> queryableMethods, string methodName)
                => queryableMethods.Single(
                    mi => mi.Name == methodName
                        && mi.GetParameters().Length == 1
                        && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(T));

            static MethodInfo GetSumOrAverageWithSelector<T>(List<MethodInfo> queryableMethods, string methodName)
                => queryableMethods.Single(
                    mi => mi.Name == methodName
                        && mi.GetParameters().Length == 2
                        && IsSelector<T>(mi.GetParameters()[1].ParameterType));

            static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Expression<>)
                    && type.GetGenericArguments()[0].IsGenericType
                    && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;

            static bool IsSelector<T>(Type type)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Expression<>)
                    && type.GetGenericArguments()[0].IsGenericType
                    && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
                    && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
        }
    }
}
