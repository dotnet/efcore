// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace System.Reflection
{
    internal static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
            => (memberInfo as PropertyInfo)?.PropertyType ?? ((FieldInfo)memberInfo)?.FieldType;

        public static bool IsSameAs(this MemberInfo propertyInfo, MemberInfo otherPropertyInfo)
        {
            if (propertyInfo == null)
            {
                return otherPropertyInfo == null;
            }

            return otherPropertyInfo == null
                ? false
                : Equals(propertyInfo, otherPropertyInfo)
                   || (propertyInfo.Name == otherPropertyInfo.Name
                       && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                           || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType)
                           || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType)
                           || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType)
                           || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(propertyInfo.DeclaringType)));
        }
    }
}
