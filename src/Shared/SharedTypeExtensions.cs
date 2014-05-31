// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;

// ReSharper disable once CheckNamespace

namespace System
{
    [DebuggerStepThrough]
    internal static class SharedTypeExtensions
    {
        public static Type UnwrapNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static bool IsNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || (typeInfo.IsGenericType
                       && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsInteger(this Type type)
        {
            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte);
        }
    }
}
