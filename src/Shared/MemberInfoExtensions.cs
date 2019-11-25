// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    internal static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
            => (memberInfo as PropertyInfo)?.PropertyType ?? ((FieldInfo)memberInfo)?.FieldType;

        public static bool IsSameAs(this MemberInfo propertyInfo, MemberInfo otherPropertyInfo)
            => propertyInfo == null
                ? otherPropertyInfo == null
                : (otherPropertyInfo == null
                    ? false
                    : Equals(propertyInfo, otherPropertyInfo)
                    || (propertyInfo.Name == otherPropertyInfo.Name
                        && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                            || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType)
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType)
                            || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType)
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces
                                .Contains(propertyInfo.DeclaringType))));

        public static string GetSimpleMemberName(this MemberInfo member)
        {
            var name = member.Name;
            var index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(index + 1) : name;
        }

        private class MemberInfoComparer : IEqualityComparer<MemberInfo>
        {
            public static readonly MemberInfoComparer Instance = new MemberInfoComparer();

            public bool Equals(MemberInfo x, MemberInfo y)
                => x.IsSameAs(y);

            public int GetHashCode(MemberInfo obj)
                => obj.GetHashCode();
        }
    }
}
