// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace System.Reflection
{
    [DebuggerStepThrough]
    internal static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo property)
            => (property.GetMethod ?? property.SetMethod).IsStatic;

        public static bool IsCandidateProperty(this PropertyInfo propertyInfo)
            => !propertyInfo.IsStatic()
                && propertyInfo.GetIndexParameters().Length == 0
                && propertyInfo.CanRead
                && propertyInfo.CanWrite
                && !propertyInfo.GetTargetType().GetTypeInfo().IsInterface;

        public static Type FindCandidateNavigationPropertyType(this PropertyInfo propertyInfo, Func<Type, bool> isPrimitiveProperty)
        {
            if (!propertyInfo.IsCandidateProperty())
            {
                return null;
            }

            var targetType = propertyInfo.GetTargetType();

            if (isPrimitiveProperty(targetType)
                || targetType.GetTypeInfo().IsValueType)
            {
                return null;
            }

            return targetType;
        }

        public static Type GetTargetType(this PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.PropertyType;
            targetType = targetType.TryGetSequenceType() ?? targetType;
            targetType = targetType.UnwrapNullableType();

            return targetType;
        }
    }
}
