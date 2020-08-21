// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    internal static class EnumerableMethods
    {
        public static MethodInfo AsEnumerable { get; }
        public static MethodInfo Cast { get; }
        public static MethodInfo OfType { get; }

        public static MethodInfo All { get; }
        public static MethodInfo AnyWithoutPredicate { get; }
        public static MethodInfo AnyWithPredicate { get; }
        public static MethodInfo Contains { get; }
        public static MethodInfo SequenceEqual { get; }

        public static MethodInfo ToList { get; }
        public static MethodInfo ToArray { get; }

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
        public static MethodInfo SelectWithOrdinal { get; }
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

        private static Dictionary<Type, MethodInfo> SumWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> AverageWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> MaxWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> MaxWithSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> MinWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> MinWithSelectorMethods { get; }

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

        public static MethodInfo GetSumWithoutSelector(Type type)
            => SumWithoutSelectorMethods[type];

        public static MethodInfo GetSumWithSelector(Type type)
            => SumWithSelectorMethods[type];

        public static MethodInfo GetAverageWithoutSelector(Type type)
            => AverageWithoutSelectorMethods[type];

        public static MethodInfo GetAverageWithSelector(Type type)
            => AverageWithSelectorMethods[type];

        public static MethodInfo GetMaxWithoutSelector(Type type)
            => MaxWithoutSelectorMethods.TryGetValue(type, out var method)
                ? method
                : MaxWithoutSelector;

        public static MethodInfo GetMaxWithSelector(Type type)
            => MaxWithSelectorMethods.TryGetValue(type, out var method)
                ? method
                : MaxWithSelector;

        public static MethodInfo GetMinWithoutSelector(Type type)
            => MinWithoutSelectorMethods.TryGetValue(type, out var method)
                ? method
                : MinWithoutSelector;

        public static MethodInfo GetMinWithSelector(Type type)
            => MinWithSelectorMethods.TryGetValue(type, out var method)
                ? method
                : MinWithSelector;

        static EnumerableMethods()
        {
            var enumerableMethods = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .ToList();

            AsEnumerable = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.AsEnumerable) && mi.IsGenericMethod && mi.GetParameters().Length == 1);
            Cast = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Cast) && mi.GetParameters().Length == 1);
            OfType = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.OfType) && mi.GetParameters().Length == 1);

            All = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.All)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            AnyWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);
            AnyWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Any)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            Contains = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Contains) && mi.GetParameters().Length == 2);
            SequenceEqual = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SequenceEqual) && mi.GetParameters().Length == 2);

            ToList = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ToList) && mi.GetParameters().Length == 1);
            ToArray = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ToArray) && mi.GetParameters().Length == 1);

            Concat = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Concat) && mi.GetParameters().Length == 2);
            Except = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Except) && mi.GetParameters().Length == 2);
            Intersect = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Intersect) && mi.GetParameters().Length == 2);
            Union = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Union) && mi.GetParameters().Length == 2);

            CountWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Count) && mi.GetParameters().Length == 1);
            CountWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Count)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            LongCountWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.LongCount) && mi.GetParameters().Length == 1);
            LongCountWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.LongCount)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            MinWithSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Min)
                    && mi.GetParameters().Length == 2
                    && mi.GetGenericArguments().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            MinWithoutSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Min)
                    && mi.GetParameters().Length == 1
                    && mi.IsGenericMethodDefinition);
            MaxWithSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Max)
                    && mi.GetParameters().Length == 2
                    && mi.GetGenericArguments().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            MaxWithoutSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Max)
                    && mi.GetParameters().Length == 1
                    && mi.IsGenericMethodDefinition);

            ElementAt = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ElementAt) && mi.GetParameters().Length == 2);
            ElementAtOrDefault = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ElementAtOrDefault) && mi.GetParameters().Length == 2);
            FirstWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.First) && mi.GetParameters().Length == 1);
            FirstWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.First)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            FirstOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1);
            FirstOrDefaultWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.FirstOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            SingleWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Single) && mi.GetParameters().Length == 1);
            SingleWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Single)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            SingleOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SingleOrDefault) && mi.GetParameters().Length == 1);
            SingleOrDefaultWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SingleOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            LastWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Last) && mi.GetParameters().Length == 1);
            LastWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Last)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            LastOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.LastOrDefault) && mi.GetParameters().Length == 1);
            LastOrDefaultWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.LastOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));

            Distinct = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Distinct) && mi.GetParameters().Length == 1);
            Reverse = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Reverse) && mi.GetParameters().Length == 1);
            Where = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Where)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            Select = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Select)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            SelectWithOrdinal = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Select)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType, funcGenericArgs: 3));
            Skip = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Skip) && mi.GetParameters().Length == 2);
            Take = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Take) && mi.GetParameters().Length == 2);
            SkipWhile = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SkipWhile)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            TakeWhile = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.TakeWhile)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            OrderBy = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.OrderBy)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            OrderByDescending = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.OrderByDescending)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            ThenBy = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ThenBy)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            ThenByDescending = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.ThenByDescending)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            DefaultIfEmptyWithoutArgument = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.DefaultIfEmpty) && mi.GetParameters().Length == 1);
            DefaultIfEmptyWithArgument = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.DefaultIfEmpty) && mi.GetParameters().Length == 2);

            Join = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Join) && mi.GetParameters().Length == 5);
            GroupJoin = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.GroupJoin) && mi.GetParameters().Length == 5);
            SelectManyWithCollectionSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SelectMany)
                    && mi.GetParameters().Length == 3
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            SelectManyWithoutCollectionSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SelectMany)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));

            GroupByWithKeySelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.GroupBy)
                    && mi.GetParameters().Length == 2
                    && IsFunc(mi.GetParameters()[1].ParameterType));
            GroupByWithKeyElementSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.GroupBy)
                    && mi.GetParameters().Length == 3
                    && IsFunc(mi.GetParameters()[1].ParameterType)
                    && IsFunc(mi.GetParameters()[2].ParameterType));
            GroupByWithKeyElementResultSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.GroupBy)
                    && mi.GetParameters().Length == 4
                    && IsFunc(mi.GetParameters()[1].ParameterType)
                    && IsFunc(mi.GetParameters()[2].ParameterType)
                    && IsFunc(
                        mi.GetParameters()[3].ParameterType, 3));
            GroupByWithKeyResultSelector = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.GroupBy)
                    && mi.GetParameters().Length == 3
                    && IsFunc(mi.GetParameters()[1].ParameterType)
                    && IsFunc(
                        mi.GetParameters()[2].ParameterType, 3));

            SumWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithoutSelector<decimal>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(long), GetMethodWithoutSelector<long>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(int), GetMethodWithoutSelector<int>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(double), GetMethodWithoutSelector<double>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(float), GetMethodWithoutSelector<float>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(decimal?), GetMethodWithoutSelector<decimal?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(long?), GetMethodWithoutSelector<long?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(int?), GetMethodWithoutSelector<int?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(double?), GetMethodWithoutSelector<double?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(float?), GetMethodWithoutSelector<float?>(enumerableMethods, nameof(Enumerable.Sum)) }
            };

            SumWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithSelector<decimal>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(long), GetMethodWithSelector<long>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(int), GetMethodWithSelector<int>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(double), GetMethodWithSelector<double>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(float), GetMethodWithSelector<float>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(decimal?), GetMethodWithSelector<decimal?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(long?), GetMethodWithSelector<long?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(int?), GetMethodWithSelector<int?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(double?), GetMethodWithSelector<double?>(enumerableMethods, nameof(Enumerable.Sum)) },
                { typeof(float?), GetMethodWithSelector<float?>(enumerableMethods, nameof(Enumerable.Sum)) }
            };

            AverageWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithoutSelector<decimal>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(long), GetMethodWithoutSelector<long>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(int), GetMethodWithoutSelector<int>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(double), GetMethodWithoutSelector<double>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(float), GetMethodWithoutSelector<float>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(decimal?), GetMethodWithoutSelector<decimal?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(long?), GetMethodWithoutSelector<long?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(int?), GetMethodWithoutSelector<int?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(double?), GetMethodWithoutSelector<double?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(float?), GetMethodWithoutSelector<float?>(enumerableMethods, nameof(Enumerable.Average)) }
            };

            AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithSelector<decimal>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(long), GetMethodWithSelector<long>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(int), GetMethodWithSelector<int>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(double), GetMethodWithSelector<double>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(float), GetMethodWithSelector<float>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(decimal?), GetMethodWithSelector<decimal?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(long?), GetMethodWithSelector<long?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(int?), GetMethodWithSelector<int?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(double?), GetMethodWithSelector<double?>(enumerableMethods, nameof(Enumerable.Average)) },
                { typeof(float?), GetMethodWithSelector<float?>(enumerableMethods, nameof(Enumerable.Average)) }
            };

            MaxWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithoutSelector<decimal>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(long), GetMethodWithoutSelector<long>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(int), GetMethodWithoutSelector<int>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(double), GetMethodWithoutSelector<double>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(float), GetMethodWithoutSelector<float>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(decimal?), GetMethodWithoutSelector<decimal?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(long?), GetMethodWithoutSelector<long?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(int?), GetMethodWithoutSelector<int?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(double?), GetMethodWithoutSelector<double?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(float?), GetMethodWithoutSelector<float?>(enumerableMethods, nameof(Queryable.Max)) }
            };

            MaxWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithSelector<decimal>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(long), GetMethodWithSelector<long>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(int), GetMethodWithSelector<int>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(double), GetMethodWithSelector<double>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(float), GetMethodWithSelector<float>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(decimal?), GetMethodWithSelector<decimal?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(long?), GetMethodWithSelector<long?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(int?), GetMethodWithSelector<int?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(double?), GetMethodWithSelector<double?>(enumerableMethods, nameof(Queryable.Max)) },
                { typeof(float?), GetMethodWithSelector<float?>(enumerableMethods, nameof(Queryable.Max)) }
            };

            MinWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithoutSelector<decimal>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(long), GetMethodWithoutSelector<long>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(int), GetMethodWithoutSelector<int>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(double), GetMethodWithoutSelector<double>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(float), GetMethodWithoutSelector<float>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(decimal?), GetMethodWithoutSelector<decimal?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(long?), GetMethodWithoutSelector<long?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(int?), GetMethodWithoutSelector<int?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(double?), GetMethodWithoutSelector<double?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(float?), GetMethodWithoutSelector<float?>(enumerableMethods, nameof(Queryable.Min)) }
            };

            MinWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetMethodWithSelector<decimal>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(long), GetMethodWithSelector<long>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(int), GetMethodWithSelector<int>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(double), GetMethodWithSelector<double>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(float), GetMethodWithSelector<float>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(decimal?), GetMethodWithSelector<decimal?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(long?), GetMethodWithSelector<long?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(int?), GetMethodWithSelector<int?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(double?), GetMethodWithSelector<double?>(enumerableMethods, nameof(Queryable.Min)) },
                { typeof(float?), GetMethodWithSelector<float?>(enumerableMethods, nameof(Queryable.Min)) }
            };

            static MethodInfo GetMethodWithoutSelector<T>(List<MethodInfo> enumerableMethods, string methodName)
                => enumerableMethods.Single(
                    mi => mi.Name == methodName
                        && mi.GetParameters().Length == 1
                        && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(T));

            static MethodInfo GetMethodWithSelector<T>(List<MethodInfo> enumerableMethods, string methodName)
                => enumerableMethods.Single(
                    mi => mi.Name == methodName
                        && mi.GetParameters().Length == 2
                        && IsSelector<T>(mi.GetParameters()[1].ParameterType));

            static bool IsFunc(Type type, int funcGenericArgs = 2)
                => type.IsGenericType
                    && (funcGenericArgs == 1 && type.GetGenericTypeDefinition() == typeof(Func<>)
                        || funcGenericArgs == 2 && type.GetGenericTypeDefinition() == typeof(Func<,>)
                        || funcGenericArgs == 3 && type.GetGenericTypeDefinition() == typeof(Func<,,>));

            static bool IsSelector<T>(Type type)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Func<,>)
                    && type.GetGenericArguments()[1] == typeof(T);
        }
    }
}
