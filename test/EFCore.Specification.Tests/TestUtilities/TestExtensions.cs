// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class TestExtensions
{
    public static TResult? Maybe<TSource, TResult>(this TSource caller, Func<TSource, TResult> result)
        where TResult : class?
        => caller is null ? null : result(caller);

    public static TResult? MaybeScalar<TSource, TResult>(this TSource caller, Func<TSource, TResult> result)
        where TResult : struct
        => caller is not null ? result(caller) : null;

    public static TResult? MaybeScalar<TSource, TResult>(this TSource caller, Func<TSource, TResult?> result)
        where TResult : struct
        => caller is not null ? result(caller) : null;

    public static IEnumerable<TResult?> MaybeDefaultIfEmpty<TResult>(this IEnumerable<TResult>? caller)
        => caller is null ? new List<TResult?> { default } : caller.DefaultIfEmpty();

    public static void ZipAssert<T>(
        this IReadOnlyCollection<T> expected,
        IReadOnlyCollection<T> actual,
        Action<T, T> elementAsserter)
    {
        Assert.Equal(expected.Count, actual.Count);
        Assert.All(expected.Zip(actual), t => elementAsserter(t.First, t.Second));
    }

    public static void ZipAssert<T>(
        this IEnumerable<T> expected,
        IEnumerable<T> actual,
        Action<T, T> elementAsserter)
    {
        Assert.Equal(expected.Count(), actual.Count());
        Assert.All(expected.Zip(actual), t => elementAsserter(t.First, t.Second));
    }
}
