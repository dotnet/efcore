// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#nullable enable

namespace System.Reflection
{
    internal static class MethodInfoExtensions
    {
        public static bool IsContainsMethod(this MethodInfo method)
        {
            if (method.Name != nameof(IList.Contains))
            {
                return false;
            }

            if (method.DeclaringType is null)
            {
                return false;
            }

            return
                method.DeclaringType
                    .GetInterfaces()
                    .Append(method.DeclaringType)
                    .Any(t => IsCollection(t));
        }

        private static bool IsCollection(Type type)
        {
            if (type == typeof(IList))
            {
                return true;
            }

            if (type.IsGenericType == false)
            {
                return false;
            }

            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(ICollection<>) || genericType == typeof(IImmutableSet<>))
            {
                return true;
            }

            return false;
        }
    }
}
