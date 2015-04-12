// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    internal static class ConventionsPropertyInfoExtensions
    {
        public static bool IsCandidateProperty(this PropertyInfo propertyInfo)
            => !propertyInfo.IsStatic()
               && propertyInfo.GetIndexParameters().Length == 0
               && propertyInfo.CanRead
               && propertyInfo.CanWrite;

        public static bool IsCandidatePrimitiveProperty(this PropertyInfo propertyInfo)
            => IsCandidateProperty(propertyInfo) && propertyInfo.PropertyType.IsPrimitive();

        public static bool IsCandidateNavigationProperty(this PropertyInfo propertyInfo, out Type targetType)
        {
            if (!IsCandidateProperty(propertyInfo))
            {
                targetType = null;
                return false;
            }

            targetType = propertyInfo.PropertyType;
            targetType = targetType.TryGetSequenceType() ?? targetType;
            targetType = targetType.UnwrapNullableType();

            var typeInfo = targetType.GetTypeInfo();
            if (targetType.IsPrimitive()
                || typeInfo.IsValueType
                || typeInfo.IsAbstract
                || typeInfo.IsInterface)
            {
                return false;
            }

            return true;
        }
    }
}
