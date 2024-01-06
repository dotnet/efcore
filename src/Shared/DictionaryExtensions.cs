// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Utilities;

[DebuggerStepThrough]
internal static class DictionaryExtensions
{
    public static TValue GetOrAddNew<TKey, TValue>(
        this IDictionary<TKey, TValue> source,
        TKey key)
        where TValue : new()
    {
        if (!source.TryGetValue(key, out var value))
        {
            value = new TValue();
            source.Add(key, value);
        }

        return value;
    }

    public static TValue? Find<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source,
        TKey key)
        => !source.TryGetValue(key, out var value) ? default : value;

    public static bool TryGetAndRemove<TKey, TValue, TReturn>(
        this IDictionary<TKey, TValue> source,
        TKey key,
        [NotNullWhen(true)] out TReturn value)
    {
        if (source.TryGetValue(key, out var item)
            && item != null)
        {
            source.Remove(key);
            value = (TReturn)(object)item;
            return true;
        }

        value = default!;
        return false;
    }

    public static void Remove<TKey, TValue>(
        this IDictionary<TKey, TValue> source,
        Func<TKey, TValue, bool> predicate)
        => source.Remove((k, v, p) => p!(k, v), predicate);

    public static void Remove<TKey, TValue, TState>(
        this IDictionary<TKey, TValue> source,
        Func<TKey, TValue, TState?, bool> predicate,
        TState? state)
    {
        var found = false;
        var firstRemovedKey = default(TKey);
        List<KeyValuePair<TKey, TValue>>? pairsRemainder = null;
        foreach (var pair in source)
        {
            if (found)
            {
                pairsRemainder ??= [];

                pairsRemainder.Add(pair);
                continue;
            }

            if (!predicate(pair.Key, pair.Value, state))
            {
                continue;
            }

            if (!found)
            {
                found = true;
                firstRemovedKey = pair.Key;
            }
        }

        if (found)
        {
            source.Remove(firstRemovedKey!);
            if (pairsRemainder == null)
            {
                return;
            }

            foreach (var (key, value) in pairsRemainder)
            {
                if (predicate(key, value, state))
                {
                    source.Remove(key);
                }
            }
        }
    }
}
