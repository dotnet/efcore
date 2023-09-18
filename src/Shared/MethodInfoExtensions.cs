// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Immutable;

namespace System.Reflection;

internal static class MethodInfoExtensions
{
    public static bool IsContainsMethod(this MethodInfo method)
        => method is { Name: nameof(IList.Contains), DeclaringType: not null }
            && method.DeclaringType.GetInterfaces().Append(method.DeclaringType).Any(
                t => t == typeof(IList)
                    || (t.IsGenericType
                        && t.GetGenericTypeDefinition() is Type genericType
                        && (genericType == typeof(ICollection<>)
                            || genericType == typeof(IReadOnlySet<>)
                            || genericType == typeof(IImmutableSet<>))));
}
