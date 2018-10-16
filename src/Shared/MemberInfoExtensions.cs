// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

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
                              || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(propertyInfo.DeclaringType))));

        public static MemberInfo OnInterface(this MemberInfo targetMember, Type interfaceType)
        {
            var declaringType = targetMember.DeclaringType;
            if (declaringType == interfaceType
                || declaringType.IsInterface
                || !declaringType.GetInterfaces().Any(i => i == interfaceType))
            {
                return targetMember;
            }
            if (targetMember is MethodInfo targetMethod)
            {
                return targetMethod.OnInterface(interfaceType);
            }
            if (targetMember is PropertyInfo targetProperty)
            {
                var targetGetMethod = targetProperty.GetMethod;
                var interfaceGetMethod = targetGetMethod.OnInterface(interfaceType);
                if (interfaceGetMethod == targetGetMethod)
                {
                    return targetProperty;
                }

                return interfaceType.GetProperties().First(p => Equals(p.GetMethod, interfaceGetMethod));
            }

            Debug.Fail("Unexpected member type: " + targetMember.MemberType);

            return targetMember;
        }

        public static MethodInfo OnInterface(this MethodInfo targetMethod, Type interfaceType)
        {
            var declaringType = targetMethod.DeclaringType;
            if (declaringType == interfaceType
                || declaringType.IsInterface
                || !declaringType.GetInterfaces().Any(i => i == interfaceType))
            {
                return targetMethod;
            }

            var map = targetMethod.DeclaringType.GetInterfaceMap(interfaceType);
            var index = map.TargetMethods.IndexOf(targetMethod);

            return index != -1
                ? map.InterfaceMethods[index]
                : targetMethod;
        }

        public static string GetSimpleMemberName(this MemberInfo member)
        {
            var name = member.Name;
            var index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(index + 1) : name;
        }
    }
}
