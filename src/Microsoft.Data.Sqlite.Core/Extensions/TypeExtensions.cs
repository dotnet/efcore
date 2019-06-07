// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
            => !type.IsValueType
               || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));

        public static Type UnwrapEnumType(this Type type)
            => type.GetTypeInfo().IsEnum ? Enum.GetUnderlyingType(type) : type;

        public static Type UnwrapNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;
    }
}
