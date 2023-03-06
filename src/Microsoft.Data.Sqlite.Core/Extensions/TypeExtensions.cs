// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TypeExtensions
    {
        public static Type UnwrapEnumType(this Type type)
            => type.IsEnum ? Enum.GetUnderlyingType(type) : type;

        public static Type UnwrapNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;
    }
}
