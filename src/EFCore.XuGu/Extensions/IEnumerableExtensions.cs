// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    // ReSharper disable once InconsistentNaming
    internal static class IEnumerableExtensions
    {
        internal static IEnumerable<T> AppendIfTrue<T>(this IEnumerable<T> source, bool condition, Func<T> elementGetter)
            => condition
                ? source.Append(elementGetter.Invoke())
                : source;

        internal static IEnumerable<T> AppendIfTrue<T>(this IEnumerable<T> source, bool condition, T element)
            => condition
                ? source.Append(element)
                : source;
    }
}
