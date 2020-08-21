// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    internal static class MethodInfoExtensions
    {
        public static bool IsContainsMethod(this MethodInfo method)
            => method.Name == nameof(IList.Contains)
                && method.DeclaringType.GetInterfaces().Append(method.DeclaringType).Any(
                    t => t == typeof(IList)
                        || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)));
    }
}
