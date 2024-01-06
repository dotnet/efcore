// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A class that provides reflection metadata for translatable LINQ methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public static class QueryableMethods
{
    //public static MethodInfo AggregateWithoutSeed { get; }

    //public static MethodInfo AggregateWithSeedWithoutSelector { get; }

    //public static MethodInfo AggregateWithSeedSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.All{TResult}" />
    /// </summary>
    public static MethodInfo All { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Any{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo AnyWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.Any{TSource}(IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo AnyWithPredicate { get; }

    //public static MethodInfo AsQueryableNonGeneric { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.AsQueryable{TElement}" />
    /// </summary>
    public static MethodInfo AsQueryable { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Cast{TResult}" />
    /// </summary>
    public static MethodInfo Cast { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Concat{TSource}" />
    /// </summary>
    public static MethodInfo Concat { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Contains{TSource}(IQueryable{TSource},TSource)" />
    /// </summary>
    public static MethodInfo Contains { get; }

    //public static MethodInfo ContainsWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Count{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo CountWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.Count{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo CountWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo DefaultIfEmptyWithoutArgument { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource},TSource)" />
    /// </summary>
    public static MethodInfo DefaultIfEmptyWithArgument { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Distinct{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo Distinct { get; }

    //public static MethodInfo DistinctWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.ElementAt{TSource}(IQueryable{TSource}, int)" />
    /// </summary>
    public static MethodInfo ElementAt { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.ElementAtOrDefault{TSource}(IQueryable{TSource}, int)" />
    /// </summary>
    public static MethodInfo ElementAtOrDefault { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Except{TSource}(IQueryable{TSource},IEnumerable{TSource})" />
    /// </summary>
    public static MethodInfo Except { get; }

    //public static MethodInfo ExceptWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.First{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo FirstWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.First{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo FirstWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo FirstOrDefaultWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo FirstOrDefaultWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.GroupBy{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})" />
    /// </summary>
    public static MethodInfo GroupByWithKeySelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.GroupBy{TSource,TKey,TElement}(IQueryable{TSource},Expression{Func{TSource,TKey}},Expression{Func{TSource,TElement}})" />
    /// </summary>
    public static MethodInfo GroupByWithKeyElementSelector { get; }

    //public static MethodInfo GroupByWithKeySelectorAndComparer { get; }

    //public static MethodInfo GroupByWithKeyElementSelectorAndComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.GroupBy{TSource,TKey,TElement,TResult}(IQueryable{TSource},Expression{Func{TSource,TKey}},Expression{Func{TSource,TElement}},Expression{Func{TKey,IEnumerable{TElement},TResult}})" />
    /// </summary>
    public static MethodInfo GroupByWithKeyElementResultSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.GroupBy{TSource,TKey,TResult}(IQueryable{TSource},Expression{Func{TSource,TKey}},Expression{Func{TKey,IEnumerable{TSource},TResult}})" />
    /// </summary>
    public static MethodInfo GroupByWithKeyResultSelector { get; }

    //public static MethodInfo GroupByWithKeyResultSelectorAndComparer { get; }

    //public static MethodInfo GroupByWithKeyElementResultSelectorAndComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.GroupJoin{TOuter,TInner,TKey,TResult}(IQueryable{TOuter},IEnumerable{TInner},Expression{Func{TOuter,TKey}},Expression{Func{TInner,TKey}},Expression{Func{TOuter,IEnumerable{TInner},TResult}})" />
    /// </summary>
    public static MethodInfo GroupJoin { get; }

    //public static MethodInfo GroupJoinWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Intersect{TSource}(IQueryable{TSource},IEnumerable{TSource})" />
    /// </summary>
    public static MethodInfo Intersect { get; }

    //public static MethodInfo IntersectWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.Join{TOuter,TInner,TKey,TResult}(IQueryable{TOuter},IEnumerable{TInner},Expression{Func{TOuter,TKey}},Expression{Func{TInner,TKey}},Expression{Func{TOuter,TInner,TResult}})" />
    /// </summary>
    public static MethodInfo Join { get; }

    //public static MethodInfo JoinWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Last{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo LastWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Last{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo LastWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.LastOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo LastOrDefaultWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.LastOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo LastOrDefaultWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.LongCount{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo LongCountWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.LongCount{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo LongCountWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Max{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,TResult}})" />
    /// </summary>
    public static MethodInfo MaxWithoutSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Max{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo MaxWithSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Min{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,TResult}})" />
    /// </summary>
    public static MethodInfo MinWithoutSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Min{TSource}(IQueryable{TSource})" />
    /// </summary>
    public static MethodInfo MinWithSelector { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.OfType{TResult}" />
    /// </summary>
    public static MethodInfo OfType { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.OrderBy{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})" />
    /// </summary>
    public static MethodInfo OrderBy { get; }

    //public static MethodInfo OrderByWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.OrderByDescending{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})" />
    /// </summary>
    public static MethodInfo OrderByDescending { get; }

    //public static MethodInfo OrderByDescendingWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Reverse{TSource}" />
    /// </summary>
    public static MethodInfo Reverse { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.Select{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,TResult}})" />
    /// </summary>
    public static MethodInfo Select { get; }

    //public static MethodInfo SelectWithOrdinal { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.SelectMany{TSource,TResult}(IQueryable{TSource},Expression{Func{TSource,IEnumerable{TResult}}})" />
    /// </summary>
    public static MethodInfo SelectManyWithoutCollectionSelector { get; }

    //public static MethodInfo SelectManyWithoutCollectionSelectorOrdinal { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see
    ///         cref="Queryable.SelectMany{TSource,TCollection,TResult}(IQueryable{TSource},Expression{Func{TSource,IEnumerable{TCollection}}},Expression{Func{TSource,TCollection,TResult}})" />
    /// </summary>
    public static MethodInfo SelectManyWithCollectionSelector { get; }

    //public static MethodInfo SelectManyWithCollectionSelectorOrdinal { get; }

    //public static MethodInfo SequenceEqual { get; }

    //public static MethodInfo SequenceEqualWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Single{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo SingleWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Single{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo SingleWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.SingleOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo SingleOrDefaultWithoutPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.SingleOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo SingleOrDefaultWithPredicate { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Skip{TSource}" />
    /// </summary>
    public static MethodInfo Skip { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.SkipWhile{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo SkipWhile { get; }

    //public static MethodInfo SkipWhileOrdinal { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)" />
    /// </summary>
    public static MethodInfo Take { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.TakeWhile{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo TakeWhile { get; }

    //public static MethodInfo TakeWhileOrdinal { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.ThenBy{TSource,TKey}(IOrderedQueryable{TSource},Expression{Func{TSource,TKey}})" />
    /// </summary>
    public static MethodInfo ThenBy { get; }

    //public static MethodInfo ThenByWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for
    ///     <see cref="Queryable.ThenByDescending{TSource,TKey}(IOrderedQueryable{TSource},Expression{Func{TSource,TKey}})" />
    /// </summary>
    public static MethodInfo ThenByDescending { get; }

    //public static MethodInfo ThenByDescendingWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Union{TSource}(IQueryable{TSource},IEnumerable{TSource})" />
    /// </summary>
    public static MethodInfo Union { get; }

    //public static MethodInfo UnionWithComparer { get; }

    /// <summary>
    ///     The <see cref="MethodInfo" /> for <see cref="Queryable.Where{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
    /// </summary>
    public static MethodInfo Where { get; }

    //public static MethodInfo WhereOrdinal { get; }

    //public static MethodInfo Zip { get; }

    /// <summary>
    ///     Checks whether or not the given <see cref="MethodInfo" /> is one of the <see cref="O:Queryable.Average" /> without a selector.
    /// </summary>
    /// <param name="methodInfo">The method to check.</param>
    /// <returns><see langword="true" /> if the method matches; <see langword="false" /> otherwise.</returns>
    public static bool IsAverageWithoutSelector(MethodInfo methodInfo)
        => AverageWithoutSelectorMethods.ContainsValue(methodInfo);

    /// <summary>
    ///     Checks whether or not the given <see cref="MethodInfo" /> is one of the <see cref="O:Queryable.Average" /> with a selector.
    /// </summary>
    /// <param name="methodInfo">The method to check.</param>
    /// <returns><see langword="true" /> if the method matches; <see langword="false" /> otherwise.</returns>
    public static bool IsAverageWithSelector(MethodInfo methodInfo)
        => methodInfo.IsGenericMethod
            && AverageWithSelectorMethods.ContainsValue(methodInfo.GetGenericMethodDefinition());

    /// <summary>
    ///     Checks whether or not the given <see cref="MethodInfo" /> is one of the <see cref="O:Queryable.Sum" /> without a selector.
    /// </summary>
    /// <param name="methodInfo">The method to check.</param>
    /// <returns><see langword="true" /> if the method matches; <see langword="false" /> otherwise.</returns>
    public static bool IsSumWithoutSelector(MethodInfo methodInfo)
        => SumWithoutSelectorMethods.ContainsValue(methodInfo);

    /// <summary>
    ///     Checks whether or not the given <see cref="MethodInfo" /> is one of the <see cref="O:Queryable.Sum" /> with a selector.
    /// </summary>
    /// <param name="methodInfo">The method to check.</param>
    /// <returns><see langword="true" /> if the method matches; <see langword="false" /> otherwise.</returns>
    public static bool IsSumWithSelector(MethodInfo methodInfo)
        => methodInfo.IsGenericMethod
            && SumWithSelectorMethods.ContainsValue(methodInfo.GetGenericMethodDefinition());

    /// <summary>
    ///     Returns the <see cref="MethodInfo" /> for the <see cref="O:Queryable.Average" /> method without a selector for the given type.
    /// </summary>
    /// <param name="type">The generic type of the method to create.</param>
    /// <returns>The <see cref="MethodInfo" />.</returns>
    public static MethodInfo GetAverageWithoutSelector(Type type)
        => AverageWithoutSelectorMethods[type];

    /// <summary>
    ///     Returns the <see cref="MethodInfo" /> for the <see cref="O:Queryable.Average" /> method with a selector for the given type.
    /// </summary>
    /// <param name="type">The generic type of the method to create.</param>
    /// <returns>The <see cref="MethodInfo" />.</returns>
    public static MethodInfo GetAverageWithSelector(Type type)
        => AverageWithSelectorMethods[type];

    /// <summary>
    ///     Returns the <see cref="MethodInfo" /> for the <see cref="O:Queryable.Sum" /> method without a selector for the given type.
    /// </summary>
    /// <param name="type">The generic type of the method to create.</param>
    /// <returns>The <see cref="MethodInfo" />.</returns>
    public static MethodInfo GetSumWithoutSelector(Type type)
        => SumWithoutSelectorMethods[type];

    /// <summary>
    ///     Returns the <see cref="MethodInfo" /> for the <see cref="O:Queryable.Sum" /> method with a selector for the given type.
    /// </summary>
    /// <param name="type">The generic type of the method to create.</param>
    /// <returns>The <see cref="MethodInfo" />.</returns>
    public static MethodInfo GetSumWithSelector(Type type)
        => SumWithSelectorMethods[type];

    private static Dictionary<Type, MethodInfo> AverageWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> SumWithoutSelectorMethods { get; }
    private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }

    static QueryableMethods()
    {
        var queryableMethodGroups = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .GroupBy(mi => mi.Name)
            .ToDictionary(e => e.Key, l => l.ToList());

        All = GetMethod(
            nameof(Queryable.All), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        AnyWithoutPredicate = GetMethod(
            nameof(Queryable.Any), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        AnyWithPredicate = GetMethod(
            nameof(Queryable.Any), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        AsQueryable = GetMethod(
            nameof(Queryable.AsQueryable), 1,
            types => [typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Cast = GetMethod(nameof(Queryable.Cast), 1, types => [typeof(IQueryable)]);

        Concat = GetMethod(
            nameof(Queryable.Concat), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Contains = GetMethod(
            nameof(Queryable.Contains), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), types[0]]);

        CountWithoutPredicate = GetMethod(
            nameof(Queryable.Count), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        CountWithPredicate = GetMethod(
            nameof(Queryable.Count), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        DefaultIfEmptyWithoutArgument = GetMethod(
            nameof(Queryable.DefaultIfEmpty), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        DefaultIfEmptyWithArgument = GetMethod(
            nameof(Queryable.DefaultIfEmpty), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), types[0]]);

        Distinct = GetMethod(nameof(Queryable.Distinct), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        ElementAt = GetMethod(
            nameof(Queryable.ElementAt), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(int)]);

        ElementAtOrDefault = GetMethod(
            nameof(Queryable.ElementAtOrDefault), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(int)]);

        Except = GetMethod(
            nameof(Queryable.Except), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        FirstWithoutPredicate = GetMethod(
            nameof(Queryable.First), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        FirstWithPredicate = GetMethod(
            nameof(Queryable.First), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        FirstOrDefaultWithoutPredicate = GetMethod(
            nameof(Queryable.FirstOrDefault), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        FirstOrDefaultWithPredicate = GetMethod(
            nameof(Queryable.FirstOrDefault), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        GroupByWithKeySelector = GetMethod(
            nameof(Queryable.GroupBy), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        GroupByWithKeyElementSelector = GetMethod(
            nameof(Queryable.GroupBy), 3,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1])),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[2]))
            ]);

        GroupByWithKeyElementResultSelector = GetMethod(
            nameof(Queryable.GroupBy), 4,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1])),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[2])),
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,,>).MakeGenericType(
                        types[1], typeof(IEnumerable<>).MakeGenericType(types[2]), types[3]))
            ]);

        GroupByWithKeyResultSelector = GetMethod(
            nameof(Queryable.GroupBy), 3,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1])),
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,,>).MakeGenericType(
                        types[1], typeof(IEnumerable<>).MakeGenericType(types[0]), types[2]))
            ]);

        GroupJoin = GetMethod(
            nameof(Queryable.GroupJoin), 4,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[2])),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[1], types[2])),
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,,>).MakeGenericType(
                        types[0], typeof(IEnumerable<>).MakeGenericType(types[1]), types[3]))
            ]);

        Intersect = GetMethod(
            nameof(Queryable.Intersect), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Join = GetMethod(
            nameof(Queryable.Join), 4,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(IEnumerable<>).MakeGenericType(types[1]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[2])),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[1], types[2])),
                typeof(Expression<>).MakeGenericType(typeof(Func<,,>).MakeGenericType(types[0], types[1], types[3]))
            ]);

        LastWithoutPredicate = GetMethod(nameof(Queryable.Last), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        LastWithPredicate = GetMethod(
            nameof(Queryable.Last), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        LastOrDefaultWithoutPredicate = GetMethod(
            nameof(Queryable.LastOrDefault), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        LastOrDefaultWithPredicate = GetMethod(
            nameof(Queryable.LastOrDefault), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        LongCountWithoutPredicate = GetMethod(
            nameof(Queryable.LongCount), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        LongCountWithPredicate = GetMethod(
            nameof(Queryable.LongCount), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        MaxWithoutSelector = GetMethod(nameof(Queryable.Max), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        MaxWithSelector = GetMethod(
            nameof(Queryable.Max), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        MinWithoutSelector = GetMethod(nameof(Queryable.Min), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        MinWithSelector = GetMethod(
            nameof(Queryable.Min), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        OfType = GetMethod(nameof(Queryable.OfType), 1, types => [typeof(IQueryable)]);

        OrderBy = GetMethod(
            nameof(Queryable.OrderBy), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        OrderByDescending = GetMethod(
            nameof(Queryable.OrderByDescending), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        Reverse = GetMethod(nameof(Queryable.Reverse), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        Select = GetMethod(
            nameof(Queryable.Select), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        SelectManyWithoutCollectionSelector = GetMethod(
            nameof(Queryable.SelectMany), 2,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(
                        types[0], typeof(IEnumerable<>).MakeGenericType(types[1])))
            ]);

        SelectManyWithCollectionSelector = GetMethod(
            nameof(Queryable.SelectMany), 3,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(
                        types[0], typeof(IEnumerable<>).MakeGenericType(types[1]))),
                typeof(Expression<>).MakeGenericType(typeof(Func<,,>).MakeGenericType(types[0], types[1], types[2]))
            ]);

        SingleWithoutPredicate = GetMethod(
            nameof(Queryable.Single), 1, types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        SingleWithPredicate = GetMethod(
            nameof(Queryable.Single), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        SingleOrDefaultWithoutPredicate = GetMethod(
            nameof(Queryable.SingleOrDefault), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0])]);

        SingleOrDefaultWithPredicate = GetMethod(
            nameof(Queryable.SingleOrDefault), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        Skip = GetMethod(
            nameof(Queryable.Skip), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(int)]);

        SkipWhile = GetMethod(
            nameof(Queryable.SkipWhile), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        Take = GetMethod(
            nameof(Queryable.Take), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(int)]);

        TakeWhile = GetMethod(
            nameof(Queryable.TakeWhile), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
            ]);

        ThenBy = GetMethod(
            nameof(Queryable.ThenBy), 2,
            types =>
            [
                typeof(IOrderedQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        ThenByDescending = GetMethod(
            nameof(Queryable.ThenByDescending), 2,
            types =>
            [
                typeof(IOrderedQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], types[1]))
            ]);

        Union = GetMethod(
            nameof(Queryable.Union), 1,
            types => [typeof(IQueryable<>).MakeGenericType(types[0]), typeof(IEnumerable<>).MakeGenericType(types[0])]);

        Where = GetMethod(
            nameof(Queryable.Where), 1,
            types =>
            [
                typeof(IQueryable<>).MakeGenericType(types[0]),
                typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], typeof(bool)))
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

        AverageWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>();
        SumWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
        SumWithSelectorMethods = new Dictionary<Type, MethodInfo>();

        foreach (var type in numericTypes)
        {
            AverageWithoutSelectorMethods[type] = GetMethod(
                nameof(Queryable.Average), 0, types => [typeof(IQueryable<>).MakeGenericType(type)]);
            AverageWithSelectorMethods[type] = GetMethod(
                nameof(Queryable.Average), 1,
                types =>
                [
                    typeof(IQueryable<>).MakeGenericType(types[0]),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], type))
                ]);
            SumWithoutSelectorMethods[type] = GetMethod(
                nameof(Queryable.Sum), 0, types => [typeof(IQueryable<>).MakeGenericType(type)]);
            SumWithSelectorMethods[type] = GetMethod(
                nameof(Queryable.Sum), 1,
                types =>
                [
                    typeof(IQueryable<>).MakeGenericType(types[0]),
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(types[0], type))
                ]);
        }

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
            => queryableMethodGroups[name].Single(
                mi => ((genericParameterCount == 0 && !mi.IsGenericMethod)
                        || (mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount))
                    && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                        parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : [])));
    }
}
