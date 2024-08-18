// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

internal static class ArrayMethods
{
    public static MethodInfo IndexOf { get; }

    public static MethodInfo IndexOfWithStartingPosition { get; }

    static ArrayMethods()
    {
        var arrayGenericMethods = typeof(Array)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.IsGenericMethod)
            .GroupBy(m => m.Name)
            .ToDictionary(m => m.Key, l => l.ToList());

        IndexOf = GetMethod(nameof(Array.IndexOf), 1, (t) =>
        {
            return [t[0].MakeArrayType(), t[0]];
        });

        IndexOfWithStartingPosition = GetMethod(nameof(Array.IndexOf), 1, (t) =>
        {
            return [t[0].MakeArrayType(), t[0], typeof(int)];
        });

        MethodInfo GetMethod(string name, int genericParameterCount, Func<Type[], Type[]> parameterGenerator)
            => arrayGenericMethods[name].Single(
                mi => mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount
                        && mi.GetParameters().Select(e => e.ParameterType).SequenceEqual(
                            parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : [])));
    }
}
