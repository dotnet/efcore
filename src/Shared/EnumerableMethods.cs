// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore;

internal static class EnumerableMethods
{
    //public static MethodInfo AggregateWithoutSeed { get; }

    //public static MethodInfo AggregateWithSeedWithoutSelector { get; }

    public static MethodInfo AggregateWithSeedSelector { get; }

    public static MethodInfo All { get; }

    public static MethodInfo AnyWithoutPredicate { get; }

    public static MethodInfo AnyWithPredicate { get; }

    //public static Append { get; }

    public static MethodInfo AsEnumerable { get; }

    public static MethodInfo Cast { get; }

    public static MethodInfo Concat { get; }

    public static MethodInfo Contains { get; }

    //public static MethodInfo ContainsWithComparer { get; }

    public static MethodInfo CountWithoutPredicate { get; }

    public static MethodInfo CountWithPredicate { get; }

    public static MethodInfo DefaultIfEmptyWithoutArgument { get; }

    public static MethodInfo DefaultIfEmptyWithArgument { get; }

    public static MethodInfo Distinct { get; }

    //public static MethodInfo DistinctWithComparer { get; }

    public static MethodInfo ElementAt { get; }

    public static MethodInfo ElementAtOrDefault { get; }

    //public static MethodInfo Empty { get; }

    public static MethodInfo Except { get; }

    //public static MethodInfo ExceptWithComparer { get; }

    public static MethodInfo FirstWithoutPredicate { get; }

    public static MethodInfo FirstWithPredicate { get; }

    public static MethodInfo FirstOrDefaultWithoutPredicate { get; }

    public static MethodInfo FirstOrDefaultWithPredicate { get; }

    public static MethodInfo GroupByWithKeySelector { get; }

    public static MethodInfo GroupByWithKeyElementSelector { get; }

    //public static MethodInfo GroupByWithKeySelectorAndComparer { get; }

    //public static MethodInfo GroupByWithKeyElementSelectorAndComparer { get; }

    public static MethodInfo GroupByWithKeyElementResultSelector { get; }

    public static MethodInfo GroupByWithKeyResultSelector { get; }

    //public static MethodInfo GroupByWithKeyResultSelectorAndComparer { get; }

    //public static MethodInfo GroupByWithKeyElementResultSelectorAndComparer { get; }

    public static MethodInfo GroupJoin { get; }

    //public static MethodInfo GroupJoinWithComparer { get; }

    public static MethodInfo Intersect { get; }

    //public static MethodInfo IntersectWithComparer { get; }

    public static MethodInfo Join { get; }

    public static MethodInfo JoinWithComparer { get; }

    public static MethodInfo LastWithoutPredicate { get; }

    public static MethodInfo LastWithPredicate { get; }

    public static MethodInfo LastOrDefaultWithoutPredicate { get; }

    public static MethodInfo LastOrDefaultWithPredicate { get; }

    public static MethodInfo LongCountWithoutPredicate { get; }

    public static MethodInfo LongCountWithPredicate { get; }

    public static MethodInfo MaxWithoutSelector { get; }

    public static MethodInfo MaxWithSelector { get; }

    public static MethodInfo MinWithoutSelector { get; }

    public static MethodInfo MinWithSelector { get; }

    public static MethodInfo OfType { get; }

    public static MethodInfo OrderBy { get; }

    //public static MethodInfo OrderByWithComparer { get; }

    public static MethodInfo OrderByDescending { get; }

    //public static MethodInfo OrderByDescendingWithComparer { get; }

    //public static MethodInfo Prepend { get; }

    //public static MethodInfo Range { get; }

    //public static MethodInfo Repeat { get; }

    public static MethodInfo Reverse { get; }

    public static MethodInfo Select { get; }

    public static MethodInfo SelectWithOrdinal { get; }

    public static MethodInfo SelectManyWithoutCollectionSelector { get; }

    //public static MethodInfo SelectManyWithoutCollectionSelectorOrdinal { get; }

    public static MethodInfo SelectManyWithCollectionSelector { get; }

    //public static MethodInfo SelectManyWithCollectionSelectorOrdinal { get; }

    public static MethodInfo SequenceEqual { get; }

    //public static MethodInfo SequenceEqualWithComparer { get; }

    public static MethodInfo SingleWithoutPredicate { get; }

    public static MethodInfo SingleWithPredicate { get; }

    public static MethodInfo SingleOrDefaultWithoutPredicate { get; }

    public static MethodInfo SingleOrDefaultWithPredicate { get; }

    public static MethodInfo Skip { get; }

    public static MethodInfo SkipWhile { get; }

    //public static MethodInfo SkipWhileOrdinal { get; }

    public static MethodInfo Take { get; }

    public static MethodInfo TakeWhile { get; }

    //public static MethodInfo TakeWhileOrdinal { get; }

    public static MethodInfo ThenBy { get; }

    //public static MethodInfo ThenByWithComparer { get; }

    public static MethodInfo ThenByDescending { get; }

    //public static MethodInfo ThenByDescendingWithComparer { get; }

    public static MethodInfo ToArray { get; }

    //public static MethodInfo ToDictionaryWithKeySelector { get; }
    //public static MethodInfo ToDictionaryWithKeySelectorAndComparer { get; }
    //public static MethodInfo ToDictionaryWithKeyElementSelector { get; }
    //public static MethodInfo ToDictionaryWithKeyElementSelectorAndComparer { get; }

    //public static MethodInfo ToHashSet { get; }
    //public static MethodInfo ToHashSetWithComparer { get; }

    public static MethodInfo ToList { get; }

    //public static MethodInfo ToLookupWithKeySelector { get; }
    //public static MethodInfo ToLookupWithKeySelectorAndComparer { get; }
    //public static MethodInfo ToLookupWithKeyElementSelector { get; }
    //public static MethodInfo ToLookupWithKeyElementSelectorAndComparer { get; }

    public static MethodInfo Union { get; }

    //public static MethodInfo UnionWithComparer { get; }

    public static MethodInfo Where { get; }

    //public static MethodInfo WhereOrdinal { get; }

    public static MethodInfo ZipWithSelector { get; }

    // private static Dictionary<Type, MethodInfo> SumWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }

    // private static Dictionary<Type, MethodInfo> AverageWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> MaxWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> MaxWithSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> MinWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> MinWithSelectorMethods { get; }

    // Not currently used
    //
    // public static bool IsSumWithoutSelector(MethodInfo methodInfo)
    //     => SumWithoutSelectorMethods.Values.Contains(methodInfo);
    //
    // public static bool IsSumWithSelector(MethodInfo methodInfo)
    //     => methodInfo.IsGenericMethod
    //         && SumWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());
    //
    // public static bool IsAverageWithoutSelector(MethodInfo methodInfo)
    //     => AverageWithoutSelectorMethods.Values.Contains(methodInfo);
    //
    // public static bool IsAverageWithSelector(MethodInfo methodInfo)
    //     => methodInfo.IsGenericMethod
    //         && AverageWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());
    //
    // public static MethodInfo GetSumWithoutSelector(Type type)
    //     => SumWithoutSelectorMethods[type];

    public static MethodInfo GetSumWithSelector(Type type)
        => SumWithSelectorMethods[type];

    // public static MethodInfo GetAverageWithoutSelector(Type type)
    //     => AverageWithoutSelectorMethods[type];

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
        var queryableMethodGroups = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .GroupBy(mi => mi.Name)
            .ToDictionary(e => e.Key, l => l.ToList());

        AggregateWithSeedSelector = GetMethod(
            nameof(Enumerable.Aggregate), 3,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                types[1],
                typeof(Func<,,>).MakeGenericType(types[1], types[0], types[1]),
                typeof(Func<,>).MakeGenericType(types[1], types[2])
            ]);

        All = GetMethod(
            nameof(Enumerable.All), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        AnyWithoutPredicate = GetMethod(
            nameof(Enumerable.Any), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        AnyWithPredicate = GetMethod(
            nameof(Enumerable.Any), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        AsEnumerable = GetMethod(
            nameof(Enumerable.AsEnumerable), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Cast = GetMethod(nameof(Enumerable.Cast), 1, _ => [typeof(IEnumerable)]);

        Concat = GetMethod(
            nameof(Enumerable.Concat), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Contains = GetMethod(
            nameof(Enumerable.Contains), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), types[0]]);

        CountWithoutPredicate = GetMethod(
            nameof(Enumerable.Count), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        CountWithPredicate = GetMethod(
            nameof(Enumerable.Count), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        DefaultIfEmptyWithoutArgument = GetMethod(
            nameof(Enumerable.DefaultIfEmpty), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        DefaultIfEmptyWithArgument = GetMethod(
            nameof(Enumerable.DefaultIfEmpty), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), types[0]]);

        Distinct = GetMethod(nameof(Enumerable.Distinct), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        ElementAt = GetMethod(
            nameof(Enumerable.ElementAt), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(int)]);

        ElementAtOrDefault = GetMethod(
            nameof(Enumerable.ElementAtOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(int)]);

        Except = GetMethod(
            nameof(Enumerable.Except), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        FirstWithoutPredicate = GetMethod(
            nameof(Enumerable.First), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        FirstWithPredicate = GetMethod(
            nameof(Enumerable.First), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        FirstOrDefaultWithoutPredicate = GetMethod(
            nameof(Enumerable.FirstOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        FirstOrDefaultWithPredicate = GetMethod(
            nameof(Enumerable.FirstOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        GroupByWithKeySelector = GetMethod(
            nameof(Enumerable.GroupBy), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        GroupByWithKeyElementSelector = GetMethod(
            nameof(Enumerable.GroupBy), 3,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(Func<,>).MakeGenericType(types[0], types[1]),
                typeof(Func<,>).MakeGenericType(types[0], types[2])
            ]);

        GroupByWithKeyElementResultSelector = GetMethod(
            nameof(Enumerable.GroupBy), 4,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(Func<,>).MakeGenericType(types[0], types[1]),
                typeof(Func<,>).MakeGenericType(types[0], types[2]),
                typeof(Func<,,>).MakeGenericType(
                    types[1], typeof(IEnumerable<>).MakeGenericType(types[2]), types[3])
            ]);

        GroupByWithKeyResultSelector = GetMethod(
            nameof(Enumerable.GroupBy), 3,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(Func<,>).MakeGenericType(types[0], types[1]),
                typeof(Func<,,>).MakeGenericType(
                    types[1], typeof(IEnumerable<>).MakeGenericType(types[0]), types[2])
            ]);

        GroupJoin = GetMethod(
            nameof(Enumerable.GroupJoin), 4,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Func<,>).MakeGenericType(types[0], types[2]),
                typeof(Func<,>).MakeGenericType(types[1], types[2]),
                typeof(Func<,,>).MakeGenericType(
                    types[0], typeof(IEnumerable<>).MakeGenericType(types[1]), types[3])
            ]);

        Intersect = GetMethod(
            nameof(Enumerable.Intersect), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Join = GetMethod(
            nameof(Enumerable.Join), 4,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Func<,>).MakeGenericType(types[0], types[2]),
                typeof(Func<,>).MakeGenericType(types[1], types[2]),
                typeof(Func<,,>).MakeGenericType(types[0], types[1], types[3])
            ]);

        JoinWithComparer = GetMethod(
            nameof(Enumerable.Join), 4,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Func<,>).MakeGenericType(types[0], types[2]),
                typeof(Func<,>).MakeGenericType(types[1], types[2]),
                typeof(Func<,,>).MakeGenericType(types[0], types[1], types[3]),
                typeof(IEqualityComparer<>).MakeGenericType(types[2])
            ]);

        LastWithoutPredicate = GetMethod(
            nameof(Enumerable.Last), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        LastWithPredicate = GetMethod(
            nameof(Enumerable.Last), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        LastOrDefaultWithoutPredicate = GetMethod(
            nameof(Enumerable.LastOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        LastOrDefaultWithPredicate = GetMethod(
            nameof(Enumerable.LastOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        LongCountWithoutPredicate = GetMethod(
            nameof(Enumerable.LongCount), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        LongCountWithPredicate = GetMethod(
            nameof(Enumerable.LongCount), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        MaxWithoutSelector = GetMethod(nameof(Enumerable.Max), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        MaxWithSelector = GetMethod(
            nameof(Enumerable.Max), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        MinWithoutSelector = GetMethod(nameof(Enumerable.Min), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        MinWithSelector = GetMethod(
            nameof(Enumerable.Min), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        OfType = GetMethod(nameof(Enumerable.OfType), 1, _ => [typeof(IEnumerable)]);

        OrderBy = GetMethod(
            nameof(Enumerable.OrderBy), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        OrderByDescending = GetMethod(
            nameof(Enumerable.OrderByDescending), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        Reverse = GetMethod(nameof(Enumerable.Reverse), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Select = GetMethod(
            nameof(Enumerable.Select), 2,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        SelectWithOrdinal = GetMethod(
            nameof(Enumerable.Select), 2,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,,>).MakeGenericType(types[0], typeof(int), types[1])
            ]);

        SelectManyWithoutCollectionSelector = GetMethod(
            nameof(Enumerable.SelectMany), 2,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(Func<,>).MakeGenericType(
                    types[0], typeof(IEnumerable<>).MakeGenericType(types[1]))
            ]);

        SelectManyWithCollectionSelector = GetMethod(
            nameof(Enumerable.SelectMany), 3,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(Func<,>).MakeGenericType(
                    types[0], typeof(IEnumerable<>).MakeGenericType(types[1])),
                typeof(Func<,,>).MakeGenericType(types[0], types[1], types[2])
            ]);

        SequenceEqual = GetMethod(
            nameof(Enumerable.SequenceEqual), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        SingleWithoutPredicate = GetMethod(
            nameof(Enumerable.Single), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        SingleWithPredicate = GetMethod(
            nameof(Enumerable.Single), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        SingleOrDefaultWithoutPredicate = GetMethod(
            nameof(Enumerable.SingleOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        SingleOrDefaultWithPredicate = GetMethod(
            nameof(Enumerable.SingleOrDefault), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        Skip = GetMethod(
            nameof(Enumerable.Skip), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(int)]);

        SkipWhile = GetMethod(
            nameof(Enumerable.SkipWhile), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        ToArray = GetMethod(nameof(Enumerable.ToArray), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        ToList = GetMethod(nameof(Enumerable.ToList), 1, types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Take = GetMethod(
            nameof(Enumerable.Take), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(int)]);

        TakeWhile = GetMethod(
            nameof(Enumerable.TakeWhile), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        ThenBy = GetMethod(
            nameof(Enumerable.ThenBy), 2,
            types => [typeof(IOrderedEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        ThenByDescending = GetMethod(
            nameof(Enumerable.ThenByDescending), 2,
            types => [typeof(IOrderedEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], types[1])]);

        Union = GetMethod(
            nameof(Enumerable.Union), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Where = GetMethod(
            nameof(Enumerable.Where), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], typeof(bool))]);

        ZipWithSelector = GetMethod(
            nameof(Enumerable.Zip), 3,
            types =>
            [
                typeof(IEnumerable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Func<,,>).MakeGenericType(types[0], types[1], types[2])
            ]);

        var numericTypes = new[]
        {
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?)
        };

        // AverageWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>();
        MaxWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        MaxWithSelectorMethods = new Dictionary<Type, MethodInfo>();
        MinWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        MinWithSelectorMethods = new Dictionary<Type, MethodInfo>();
        // SumWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        SumWithSelectorMethods = new Dictionary<Type, MethodInfo>();

        foreach (var type in numericTypes)
        {
            // AverageWithoutSelectorMethods[type] = GetMethod(
            //     nameof(Enumerable.Average), 0, types => new[] { typeof(IEnumerable<>).MakeGenericType(type) });
            AverageWithSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Average), 1,
                types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], type)]);
            MaxWithoutSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Max), 0, _ => [typeof(IEnumerable<>).MakeGenericType(type)]);
            MaxWithSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Max), 1,
                types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], type)]);
            MinWithoutSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Min), 0, _ => [typeof(IEnumerable<>).MakeGenericType(type)]);
            MinWithSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Min), 1,
                types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], type)]);
            // SumWithoutSelectorMethods[type] = GetMethod(
            //     nameof(Enumerable.Sum), 0, types => new[] { typeof(IEnumerable<>).MakeGenericType(type) });
            SumWithSelectorMethods[type] = GetMethod(
                nameof(Enumerable.Sum), 1,
                types => [typeof(IEnumerable<>).MakeGenericType(types[0]), typeof(Func<,>).MakeGenericType(types[0], type)]);
        }

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
            => queryableMethodGroups[name].Single(
                mi => ((genericParameterCount == 0 && !mi.IsGenericMethod)
                        || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount))
                    && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                        parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : [])));
    }
}
