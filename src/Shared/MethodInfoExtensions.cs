// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace System.Reflection
{
    internal static class MethodInfoExtensions
    {
        public static bool IsContainsMethod(this MethodInfo method)
            => method.Name == nameof(IList.Contains)
                && method.DeclaringType != null
                && method.DeclaringType.GetInterfaces().Append(method.DeclaringType).Any(
                    t => t == typeof(IList)
                        || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)));
    }
}
