// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public static class DataGenerator
{
    private static readonly ConcurrentDictionary<Type, object[]> Values = new();
    private static readonly ConcurrentDictionary<int, object[][]> _boolCombinations = new();

    public static object[][] GetBoolCombinations(int length)
        => _boolCombinations.GetOrAdd(length, l => GetCombinations(Values[typeof(bool)], l));

    static DataGenerator()
    {
        Values[typeof(bool)] = [false, true];
        Values[typeof(bool?)] = [null, false, true];
    }

    public static object[][] GetCombinations(object[] set, int length)
    {
        var sets = new object[length][];
        Array.Fill(sets, set);
        return GetCombinations(sets);
    }

    public static object[][] GetCombinations(params Type[] types)
    {
        var sets = new object[types.Length][];
        for (var i = 0; i < types.Length; i++)
        {
            var type = types[i];
            if (!Values.TryGetValue(type, out var values))
            {
                if (!type.IsDefined(typeof(FlagsAttribute), false))
                {
                    values = Enum.GetValues(type).Cast<object>().ToArray();
                    Values[type] = values;
                }
                else
                {
                    throw new InvalidOperationException($"The set of values for the type {type} is not known.");
                }
            }

            sets[i] = values;
        }

        return GetCombinations(sets);
    }

    public static object[][] GetCombinations(object[][] sets)
    {
        var numberOfCombinations = sets.Aggregate(1L, (current, set) => current * set.Length);
        var combinations = new object[numberOfCombinations][];

        for (var i = 0L; i < numberOfCombinations; i++)
        {
            var combination = new object[sets.Length];
            var temp = i;
            for (var j = 0; j < sets.Length; j++)
            {
                var set = sets[j];
                combination[j] = set[(int)(temp % set.Length)];
                temp /= set.Length;
            }

            combinations[i] = combination;
        }

        return combinations;
    }
}
